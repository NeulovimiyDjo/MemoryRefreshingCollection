using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace WCFStreamFilesExampleService
{
    [MessageContract]
    public class UploadRequest
    {
        [MessageHeader(MustUnderstand = true)]
        public string FileName { get; set; }

        [MessageBodyMember(Order = 1)]
        public Stream Stream { get; set; }
    }

    [MessageContract]
    public class UploadResponse
    {
        [MessageBodyMember(Order = 1)]
        public bool UploadSucceeded { get; set; }
    }





    public class SomeService : ISomeService
    {
        public string TestMethod1(string param)
        {
            return "Hello " + param;
        }

        public UploadResponse TestMethod2(UploadRequest request)
        {
            using (var fileStream = File.Create("c:/tmp/fileXX.7z"))
            {
                request.Stream.CopyTo(fileStream);
            }

            return new UploadResponse { UploadSucceeded = true };
        }
    }
}
