using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SMBLibrary;
using SMBLibrary.Client;
using System;
using System.Collections.Generic;

namespace TestApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class TestNetShareController : ControllerBase
    {
        private readonly ILogger<TestLdapController> _logger;
        private readonly IConfiguration _config;

        public TestNetShareController(ILogger<TestLdapController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route(nameof(TestNetShareOld))]
        public string TestNetShareOld()
        {
            string shareHost = _config.GetValue<string>("NetShareHost");
            string shareName = _config.GetValue<string>("NetShareName");
            string filePath = "test.txt";
            string fileContent = "test data";
            WriteFileToShareOnHost(
                shareHost, shareName,
                filePath, fileContent,
                "", "", ""
            );
            return "Write successfull";
        }

        [AllowAnonymous]
        [HttpGet]
        [Route(nameof(TestNetShareNew))]
        public string TestNetShareNew()
        {
            string shareHost = System.IO.File.ReadAllText("/app/netsharecreds/sharehost.txt");
            string shareName = System.IO.File.ReadAllText("/app/netsharecreds/sharename.txt");
            string domain = System.IO.File.ReadAllText("/app/netsharecreds/domain.txt");
            string user = System.IO.File.ReadAllText("/app/netsharecreds/user.txt");
            string password = System.IO.File.ReadAllText("/app/netsharecreds/password.txt");
            string filePath = "test.txt";
            string fileContent = "test data";
            WriteFileToShareOnHost(
                shareHost, shareName,
                filePath, fileContent,
                domain, user, password
            );
            return "Write successfull";
        }

        private void WriteFileToShareOnHost(
            string shareHost, string shareName,
            string filePath, string fileContent,
            string domain, string user, string password)
        {
            SMB2Client client = new();
            bool isConnected = client.Connect(shareHost, SMBTransportType.DirectTCPTransport);
            if (isConnected)
            {
                try
                {
                    NTStatus status = client.Login(domain, user, password);
                    if (status == NTStatus.STATUS_SUCCESS)
                    {
                        try
                        {
                            WriteFileToShare(client, shareName, filePath, fileContent);
                        }
                        finally
                        {
                            client.Logoff();
                        }
                    }
                    else
                    {
                        throw new Exception($"Failed to login with empty credentials.");
                    }
                }
                finally
                {
                    client.Disconnect();
                }
            }
            else
            {
                throw new Exception($"Failed to connect to share host {shareHost}.");
            }
        }

        private void WriteFileToShare(SMB2Client client, string shareName, string filePath, string fileContent)
        {
            List<string> shares = client.ListShares(out NTStatus status);
            if (!shares.Contains(shareName))
                throw new Exception($"Share with name {shareName} does was not found.");

            ISMBFileStore fileStore = client.TreeConnect(shareName, out status);
            try
            {
                WriteToFileStore(fileStore, filePath, fileContent);
            }
            finally
            {
                fileStore.Disconnect();
            }
        }

        private void WriteToFileStore(ISMBFileStore fileStore, string filePath, string fileContent)
        {
            if (fileStore is SMB1FileStore)
                filePath = @"\\" + filePath;

            NTStatus status = fileStore.CreateFile(
                out object fileHandle,
                out FileStatus fileStatus,
                filePath,
                AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE,
                FileAttributes.Normal,
                ShareAccess.None,
                CreateDisposition.FILE_CREATE,
                CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                null);

            if (status == NTStatus.STATUS_SUCCESS)
            {
                try
                {
                    byte[] data = System.Text.Encoding.ASCII.GetBytes(fileContent);
                    status = fileStore.WriteFile(out int numberOfBytesWritten, fileHandle, 0, data);
                    if (status != NTStatus.STATUS_SUCCESS)
                        throw new Exception("Failed to write to file");
                }
                finally
                {
                    fileStore.CloseFile(fileHandle);
                }
            }
            else
            {
                throw new Exception($"Failed to create file {filePath}.");
            }
        }
    }
}
