using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;

namespace SOAPCoreService
{
    public static class ServicesLocator
    {
        public static Type[] LocateServices(Assembly[] assemblies)
        {
            List<Type> services = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsClass && TypeIsServiceContract(type))
                        services.Add(type);
                }
            }
            return services.ToArray();
        }

        private static bool TypeIsServiceContract(Type type)
        {
            return HasServiceContractAttribute(type)
                || AnyTypeIsServiceContract(type.GetInterfaces());
        }

        private static bool HasServiceContractAttribute(Type type)
        {
            return type.IsDefined(typeof(ServiceContractAttribute), false);
        }

        private static bool AnyTypeIsServiceContract(Type[] types)
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
