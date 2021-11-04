using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedProjects.SharedHelpers.Common;
using System.IO;

namespace UnitTests.WSDLTool
{
    [TestClass]
    public class AssemblySetup
    {
        private const string RelativeTmpWSDLToolPath = @".\tmp_wsdlTool";

        [AssemblyInitialize()]
        public static void AssemblyInitialize(TestContext testContext)
        {
            string builtWSDLToolPath = Path.GetFullPath(@$"..\Tools\WSDLTool\bin\out");
            string tmpWSDLToolPath = Path.GetFullPath(RelativeTmpWSDLToolPath);
            if (Directory.Exists(tmpWSDLToolPath))
                Directory.Delete(tmpWSDLToolPath, true);
            OSInteractor.CopyDirectory(builtWSDLToolPath, tmpWSDLToolPath);

            ResourcesManager.ExtractResourcesToDir(tmpWSDLToolPath);

            Directory.SetCurrentDirectory(tmpWSDLToolPath);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
        }
    }
}
