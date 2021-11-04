using SomeProject.SomeMethod;
using System.Threading.Tasks;

namespace SomeProject
{
    public class DoSomethingService : IDoSomethingService
    {
        public async Task SomeMethod(DoSomethingServiceRequest DoSomethingServiceRequest)
        {
            //MockImplemenation
        }
    }
}
