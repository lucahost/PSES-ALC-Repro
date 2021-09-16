using System.Management.Automation;
using PSES.ALC.Dependency;

namespace PSES.ALC.Repro
{
    [Cmdlet(VerbsDiagnostic.Test, "PSESALC")]
    public class TestCommand : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            WriteObject("Hello before creating DBContext Instance");
            var context = new TestDBContext();
            WriteObject("Hello after creating DBContext Instance");
        }
    }
}
