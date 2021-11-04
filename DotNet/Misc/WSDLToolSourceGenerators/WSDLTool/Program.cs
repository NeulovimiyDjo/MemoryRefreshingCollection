using System;
using System.IO;
using WSDLTool.Logic;
using WSDLToolGenerators;

namespace WSDLTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Type serviceType = typeof(DoSomethingService);
            string serviceName = serviceType.Name;

            Directory.CreateDirectory(@$"..\..\tool_out");

            string wsdl = WSDLCreator.GetWSDLFromService(serviceType);
            File.WriteAllText(@$"..\..\tool_out\{serviceName}.wsdl", wsdl);

            string interfaceCsCode = CSCreator.CreateServiceInterfaceClassFromWSDL($"{serviceName}", @$"..\..\tool_out");
            File.WriteAllText(@$"..\..\tool_out\I{serviceName}.cs", interfaceCsCode);

            string serviceDummyCode = CSCreator.CreateServiceDummyImplementationClass(interfaceCsCode);
            File.WriteAllText(@$"..\..\tool_out\{serviceName}.cs", serviceDummyCode);

            string proxyCsCode = CSCreator.CreateServiceProxyClassFromWSDL($"{serviceName}", @$"..\..\tool_out");
            File.WriteAllText(@$"..\..\tool_out\{serviceName}Proxy.cs", proxyCsCode);
        }
    }
}
