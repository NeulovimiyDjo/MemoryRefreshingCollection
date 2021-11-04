using SomeProject.SomeMethod;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SomeProject
{
    [ServiceContract(Namespace = "http://testsrv")]
    public interface IDoSomethingService
    {
        [OperationContract]
        Task SomeMethod(DoSomethingServiceRequest DoSomethingServiceRequest);
    }
}
