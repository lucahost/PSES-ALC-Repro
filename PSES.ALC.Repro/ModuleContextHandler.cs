using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.Loader;

namespace PSES.ALC.Repro
{
    public class ModuleContextHandler: IModuleAssemblyInitializer, IModuleAssemblyCleanup
    {
        private static readonly string SBinBasePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, ".."));

        private static readonly string SBinCommonPath = Path.Combine(SBinBasePath, "Common");

#if NETFRAMEWORK
        private static string SBinFrameworkPath = Path.Combine(SBinBasePath, "win-x64");
#endif

        public void OnImport()
        {
#if NETFRAMEWORK
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly_NetFramework;
#else
            AssemblyLoadContext.Default.Resolving += ResolveAssembly_NetCore;
#endif
        }


#if NETFRAMEWORK

        private static Assembly ResolveAssembly_NetFramework(object sender, ResolveEventArgs args)
        {
            // In .NET Framework, we must try to resolve ALL assemblies under the dependency dir here
            // This essentially means we are combining the .NET Core ALC and resolve events into one here
            // Note that:
            //   - This is not a recommended usage of Assembly.LoadFile()
            //   - Even doing this will not bypass the GAC

            // Parse the assembly name to get the file name
            var asmName = new AssemblyName(args.Name);
            var dllFileName = $"{asmName.Name}.dll";

            // Look for the DLL in our .NET Framework directory
            string frameworkAsmPath = Path.Combine(SBinFrameworkPath, dllFileName);
            if (File.Exists(frameworkAsmPath))
            {
                return LoadAssemblyFile_NetFramework(frameworkAsmPath);
            }

            // Now look in the dependencies directory to resolve .NET Standard dependencies
            string commonAsmPath = Path.Combine(SBinCommonPath, dllFileName);
            if (File.Exists(commonAsmPath))
            {
                return LoadAssemblyFile_NetFramework(commonAsmPath);
            }

            // We've run out of places to look
            return null;
        }

        private static Assembly LoadAssemblyFile_NetFramework(string assemblyPath)
        {
            return Assembly.LoadFile(assemblyPath);
        }

#else

        private static Assembly ResolveAssembly_NetCore(
            AssemblyLoadContext assemblyLoadContext,
            AssemblyName assemblyName)
        {
            // In .NET Core, PowerShell deals with assembly probing so our logic is much simpler
            // We only care about our Engine assembly
            if (assemblyName?.Name?.Equals("Test.Core.ALC") != true)
            {
                return null;
            }

            // Now load the Engine assembly through the dependency ALC, and let it resolve further dependencies automatically
            return DependencyAssemblyLoadContext.GetForDirectory(SBinCommonPath).LoadFromAssemblyName(assemblyName);
        }

#endif
        public void OnRemove(PSModuleInfo psModuleInfo)
        {
#if NETFRAMEWORK
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly_NetFramework;
#else
            AssemblyLoadContext.Default.Resolving -= ResolveAssembly_NetCore;
#endif
        }
    }
}
