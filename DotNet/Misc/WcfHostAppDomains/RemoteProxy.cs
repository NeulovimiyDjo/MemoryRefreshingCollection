using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace WcfHost
{
    public class RemoteProxy : MarshalByRefObject
    {
        public static void CreateAndStartAllServices(string assemblyFile, string configPath)
        {
            AppDomainSetup info = new AppDomainSetup
            {
                ShadowCopyFiles = "false",
                ConfigurationFile = configPath// assemblyFile + ".config"
            };

            AppDomain appDomain = AppDomain.CreateDomain(assemblyFile, null, info);

            RemoteProxy proxy = (RemoteProxy)appDomain.CreateInstanceAndUnwrap(
                Assembly.GetExecutingAssembly().FullName,
                typeof(RemoteProxy).FullName);

            try
            {
                proxy.LoadServices(assemblyFile);
            }
            catch (Exception)
            {
                AppDomain.Unload(appDomain);
                throw;
            }
        }


        public void LoadServices(string assemblyFile)
        {
            AssemblyName assemblyRef = new AssemblyName();
            assemblyRef.CodeBase = assemblyFile;
            Assembly assembly = Assembly.Load(assemblyRef);

            ProcessGlobalAsax(assembly);

            Type[] serviceTypes = LocateServices(assembly.GetTypes());
            foreach (Type serviceType in serviceTypes)
                CreateAndStartService(serviceType);
        }

        private void ProcessGlobalAsax(Assembly assembly)
        {
            Type globalType = assembly.GetTypes().Where(x => x.Name == "Global").First();
            object global = Activator.CreateInstance(globalType);
            globalType
                .GetMethod("Application_Start", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(global, new object[] { this, new EventArgs() });
        }

        private Type[] LocateServices(Type[] types)
        {
            List<Type> services = new List<Type>();
            foreach (Type type in types)
            {
                if (type.IsClass && TypeIsServiceContract(type))
                    services.Add(type);
            }
            return services.ToArray();
        }

        private void CreateAndStartService(Type serviceType)
        {
            ServiceHost host = new ServiceHost(serviceType, new Uri[]
            {
                new Uri($"http://localhost:{5555}/{serviceType.Name}")
            });
            host.Open();
        }

        private bool TypeIsServiceContract(Type type)
        {
            return HasServiceContractAttribute(type)
                || AnyTypeIsServiceContract(type.GetInterfaces());
        }

        private bool HasServiceContractAttribute(Type type)
        {
            return type.IsDefined(typeof(ServiceContractAttribute), false);
        }

        private bool AnyTypeIsServiceContract(Type[] types)
        {
            foreach (Type type in types)
            {
                if (TypeIsServiceContract(type))
                    return true;
            }
            return false;
        }
    }
}
