using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharedProjects.SharedHelpers.Common;
using System.IO;

namespace UnitTests.WSDLTool
{
    [TestClass]
    public class WSDLToolTests
    {
        private const string ServiceName = "DoSomethingService";
        private static readonly string ServiceNamespace = $"{ServiceName}Namespace";

        private static readonly string WCFGendWSDLPath = $"WCF_GENERATED_{ServiceName}.wsdl";
        private static readonly string ToolGendWSDLPath = $"TOOL_GENERATED_{ServiceName}.wsdl";
        private static readonly string WCFGendDllPath = $"WCF_GENERATED_{ServiceName}.dll";
        private static readonly string ToolGendDllPath = $"TOOL_GENERATED_{ServiceName}.dll";
        private static readonly string WCFGendInterfacePath = $"WCF_GENERATED_I{ServiceName}.cs";
        private static readonly string ToolGendInterfacePath = $"TOOL_GENERATED_I{ServiceName}.cs";
        private static readonly string WCFGendProxyPath = $"WCF_GENERATED_{ServiceName}Proxy.cs";
        private static readonly string ToolGendProxyPath = $"TOOL_GENERATED_{ServiceName}Proxy.cs";

        private static readonly string ManualParameterPath = $"MANUALLY_CREATED_{ServiceName}Parameter.cs";
        private static readonly string ManualInterfacePath = $"MANUALLY_CREATED_I{ServiceName}.cs";
        private static readonly string ManualServicePath = $"MANUALLY_CREATED_{ServiceName}.cs";
        private static readonly string ManualDllPath = $"MANUALLY_CREATED_{ServiceName}.dll";

        [TestCleanup()]
        public void TestCleanup()
        {
            string pattern = @"_GENERATED_.*\.cs$|TOOL_GENERATED_.*\.wsdl$|_GENERATED_.*\.dll$|_CREATED_.*\.dll$";
            string workDir = Directory.GetCurrentDirectory();
            foreach (string file in OSInteractor.GetFilesByRegex(workDir, pattern))
                File.Delete(file);
        }

        [TestMethod]
        public void CSCodeGeneratedFromWCFWSDL_IsSameAs_FromToolCreatedWSDL()
        {
            string WCFGendInterfaceCode = CreateServiceInterface(WCFGendInterfacePath, WCFGendWSDLPath);
            string WCFGendServiceCode = TestHelper.CreateServiceDummyImplementationCode(WCFGendInterfaceCode);
            string WCFGendProxyCode = CreateServiceProxy(WCFGendProxyPath, WCFGendWSDLPath);
            TestHelper.CreateAssemblyFromSources(WCFGendDllPath, WCFGendInterfaceCode, WCFGendServiceCode);
            string ToolGendWSDL_FromWCFDll = CreateServiceWSDL(ToolGendWSDLPath, WCFGendDllPath);

            string ToolGendInterfaceCode = CreateServiceInterface(ToolGendInterfacePath, ToolGendWSDLPath);
            string ToolGendServiceCode = TestHelper.CreateServiceDummyImplementationCode(ToolGendInterfaceCode);
            string ToolGendProxyCode = CreateServiceProxy(ToolGendProxyPath, ToolGendWSDLPath);
            TestHelper.CreateAssemblyFromSources(ToolGendDllPath, ToolGendInterfaceCode, ToolGendServiceCode);
            string ToolGendWSDL_FromToolDll = CreateServiceWSDL(ToolGendWSDLPath, ToolGendDllPath);

            Assert.AreEqual(WCFGendInterfaceCode, ToolGendInterfaceCode);
            Assert.AreEqual(WCFGendServiceCode, ToolGendServiceCode);
            Assert.AreEqual(WCFGendProxyCode, ToolGendProxyCode);
            Assert.AreEqual(ToolGendWSDL_FromWCFDll, ToolGendWSDL_FromToolDll);
        }

        [TestMethod]
        public void WSDLGeneratedFromManualCSCode_IsSameAs_FromToolCreatedCSCode()
        {
            string ManualParameterCode = File.ReadAllText(ManualParameterPath);
            string ManualInterfaceCode = File.ReadAllText(ManualInterfacePath);
            string ManualServiceCode = File.ReadAllText(ManualServicePath);
            TestHelper.CreateAssemblyFromSources(ManualDllPath, ManualParameterCode, ManualInterfaceCode, ManualServiceCode);
            string ToolGendWSDL_FromManualDll = CreateServiceWSDL(ToolGendWSDLPath, ManualDllPath);

            string ToolGendInterfaceCode = CreateServiceInterface(ToolGendInterfacePath, ToolGendWSDLPath);
            string ToolGendServiceCode = TestHelper.CreateServiceDummyImplementationCode(ToolGendInterfaceCode);
            TestHelper.CreateAssemblyFromSources(ToolGendDllPath, ToolGendInterfaceCode, ToolGendServiceCode);
            string ToolGendWSDL_FromToolDll = CreateServiceWSDL(ToolGendWSDLPath, ToolGendDllPath);

            Assert.AreEqual(ToolGendWSDL_FromManualDll, ToolGendWSDL_FromToolDll);
        }

        private static string CreateServiceWSDL(string outPath, string serviceDllPath)
        {
            string cmd = "wsdl";
            TestHelper.RunWSDLTool($"{cmd} {outPath} {ServiceName} {serviceDllPath}");
            return File.ReadAllText(outPath);
        }

        private static string CreateServiceInterface(string outPath, string wsdlPath)
        {
            string cmd = "interface";
            TestHelper.RunWSDLTool($"{cmd} {outPath} {ServiceNamespace} {wsdlPath}");
            return File.ReadAllText(outPath);
        }

        private static string CreateServiceProxy(string outPath, string wsdlPath)
        {
            string cmd = "proxy";
            TestHelper.RunWSDLTool($"{cmd} {outPath} {ServiceNamespace} {wsdlPath}");
            return File.ReadAllText(outPath);
        }
    }
}
