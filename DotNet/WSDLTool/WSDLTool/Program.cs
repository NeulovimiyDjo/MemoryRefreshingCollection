using System;
using System.IO;
using WSDLTool.Logic;

namespace WSDLTool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string cmd = args[0];
            string outPath = args[1];

            string outContent;
            switch (cmd)
            {
                case "wsdl":
                    {
                        string serviceName = args[2];
                        string assemblyPath = args[3];
                        outContent = WSDLCreator.GetWSDLFromService(serviceName, assemblyPath);
                        break;
                    }
                case "interface":
                    {
                        string serviceNamespace = args[2];
                        string wsdlPath = args[3];
                        string wsdl = File.ReadAllText(wsdlPath);
                        outContent = CSCreator.CreateServiceInterfaceClassFromWSDL(serviceNamespace, wsdl);
                        break;
                    }
                case "proxy":
                    {
                        string serviceNamespace = args[2];
                        string wsdlPath = args[3];
                        string wsdl = File.ReadAllText(wsdlPath);
                        outContent = CSCreator.CreateServiceProxyClassFromWSDL(serviceNamespace, wsdl);
                        break;
                    }
                default:
                    throw new ArgumentException($"Invalid command [{cmd}]");
            }

            File.WriteAllText(outPath, outContent);
        }
    }
}
