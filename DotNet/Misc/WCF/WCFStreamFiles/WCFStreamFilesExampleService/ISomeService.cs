using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCFStreamFilesExampleService
{
    [ServiceContract]
    interface ISomeService
    {
        [OperationContract]
        string TestMethod1(string param);

        [OperationContract]
        UploadResponse TestMethod2(UploadRequest request);
    }
}
