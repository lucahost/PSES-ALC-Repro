# Run with Windows PowerShell (5.1) direct
    # Shoud Print Hello before creating DBContext Instance
    # Should create new DBContext Instance
    # Shoud Print Hello after creating DBContext Instance
# Run with PSES (Windows PowerShell)
    # Should throw "Method 'DisposeAsync' in type 'Microsoft.EntityFrameworkCore.DbContext' from assembly... does not have an implementation
    # If you attach VisualStudio to PowerShell Process and show modules, it will show that PSES has loaded newer Microsoft.Extension.Dlls

$libPath = "$($PSScriptRoot)\PSES.ALC.Repro\bin\Debug\net461\win-x64\PSES.ALC.Repro.dll"

if (-not (Test-Path $libPath)) {
    dotnet build ".\PSES.ALC.Repro"
}

Import-Module $libPath


Test-PSESALC