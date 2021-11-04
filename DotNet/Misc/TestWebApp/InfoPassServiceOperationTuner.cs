using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SOAP.Simulator.Endpoints;
using SoapCore.Extensibility;
using SoapCore.ServiceModel;

namespace TestWeb
{
    public class InfoPassServiceOperationTuner : IServiceOperationTuner
    {
        public void Tune(
            HttpContext httpContext,
            object serviceInstance,
            OperationDescription operation)
        {
            if (serviceInstance is SimulatorBaseService service)
                service.HttpRequestHeaders = httpContext.Request.Headers;

            if (!operation.IsOneWay && DontWrapResponse(operation))
                httpContext.Request.Headers.Add("DontWrapResponse", "ignoredValue");
        }

        private static bool DontWrapResponse(OperationDescription operation)
        {
            bool isSimulatorService = operation.Contract.Service.ServiceType.Assembly.GetName().Name
                == typeof(SomeBaseService).Assembly.GetName().Name;

            bool noReturn = operation.ReturnType == typeof(void)
                || !operation.ReturnType.IsGenericType && operation.ReturnType == typeof(Task);

            bool hasNoWrapOption = operation.IsMessageContractRequest
                && operation.InParameters
                    .Single().Parameter.ParameterType
                    .GetCustomAttributes(typeof(MessageContractAttribute), false)
                    .Any(x => ((MessageContractAttribute)x).IsWrapped == false);

            return isSimulatorService && noReturn && hasNoWrapOption;
        }
    }
}
