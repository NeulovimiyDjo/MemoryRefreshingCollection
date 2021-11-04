using System;
using System.Collections.Generic;
using SMBLibrary;
using SMBLibrary.Client;

namespace SMB
{
    public static class SMBHelper
    {
        public static void WriteFileToShareOnHost(
            string shareHost, string shareName,
            string filePath, byte[] fileContent,
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

        private static void WriteFileToShare(SMB2Client client, string shareName, string filePath, byte[] fileContent)
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

        private static void WriteToFileStore(ISMBFileStore fileStore, string filePath, byte[] fileContent)
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
                    status = fileStore.WriteFile(out int numberOfBytesWritten, fileHandle, 0, fileContent);
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
