using System.ServiceModel;
using System.Threading.Tasks;

namespace WCFServices
{
    [ServiceContract(Namespace = Constants.XmlNamespace)]
    interface ITest_TestService
    {
        [OperationContract]
        string TestMethod1(string param);

        [OperationContract]
        Task<string> TestMethod2(string param);

        [OperationContract]
        Task<string> TestMethod3(Test_TestServiceRequest3 Test_TestServiceRequest3);
    }
}
