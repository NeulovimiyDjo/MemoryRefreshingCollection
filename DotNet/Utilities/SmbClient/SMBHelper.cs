using System;
using System.Collections.Generic;
using SMBLibrary;
using SMBLibrary.Client;
using SMBLibrary.SMB1;

namespace SmbClientProject
{
    public class SMBHelper : IDisposable
    {
        private readonly string _shareHost;
        private readonly string _shareName;
        private readonly string _domain;
        private readonly string _user;
        private readonly string _password;

        private readonly SMB2Client _client;
        private bool _connected;
        private bool _loggedIn;

        public SMBHelper(
            string shareHost, string shareName,
            string domain, string user, string password)
        {
            _shareHost = shareHost;
            _shareName = shareName;
            _domain = domain;
            _user = user;
            _password = password;

            _client = new();
        }

        public void Dispose()
        {
            LogoffAndDisconnect();
        }

        public void ConnectAndLogin()
        {
            _connected = _client.Connect(_shareHost, SMBTransportType.DirectTCPTransport);
            if (_connected)
            {
                NTStatus status = _client.Login(_domain, _user, _password);
                if (status == NTStatus.STATUS_SUCCESS)
                {
                    _loggedIn = true;

                    List<string> shares = _client.ListShares(out status);
                    if (!shares.Contains(_shareName))
                        throw new Exception($"Share with name '{_shareName}' was not found.");
                }
                else
                {
                    throw new Exception($"Failed to login with user '{_domain}\\{_user}'. Status: '{status}'.");
                }
            }
            else
            {
                throw new Exception($"Failed to connect to share host '{_shareHost}'.");
            }
        }

        public void LogoffAndDisconnect()
        {
            if (_loggedIn)
            {
                try { _client.Logoff(); }
                catch { }
                finally { _loggedIn = false; }
            }

            if (_connected)
            {
                try { _client.Disconnect(); }
                catch { }
                finally { _connected = false; }
            }
        }

        public List<FileEntry> ListFiles(string path)
        {
            return ExecuteInTreeConnect(
                path,
                (ISMBFileStore store, string smbPath, out object handle, out FileStatus fStatus) =>
                {
                    NTStatus status = store.CreateFile(
                        out object handleX,
                        out FileStatus fStatusX,
                        smbPath,
                        AccessMask.GENERIC_READ,
                        FileAttributes.Directory,
                        ShareAccess.Read,
                        CreateDisposition.FILE_OPEN,
                        CreateOptions.FILE_DIRECTORY_FILE,
                        null);
                    handle = handleX;
                    fStatus = fStatusX;
                    return status;
                },
                (ISMBFileStore store, object handle) =>
                {
                    List<FileEntry> files = new();
                    if (store is SMB1FileStore)
                    {
                        NTStatus status = ((SMB1FileStore)store).QueryDirectory(
                            out List<FindInformation> fileList,
                            @"\*",
                            FindInformationLevel.SMB_FIND_FILE_DIRECTORY_INFO);

                        if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_NO_MORE_FILES)
                            throw new Exception($"Failed to query directory. Path: '{path}'. Status: '{status}'.");

                        foreach (FindFileDirectoryInfo fdi in fileList)
                        {
                            string name = fdi.FileName.ToString();
                            bool isDir = (fdi.ExtFileAttributes & ExtendedFileAttributes.Directory) == ExtendedFileAttributes.Directory;
                            AddFile(files, name, isDir);
                        }
                    }
                    else
                    {
                        NTStatus status = store.QueryDirectory(
                            out List<QueryDirectoryFileInformation> fileList,
                            handle,
                            "*",
                            FileInformationClass.FileDirectoryInformation);

                        if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_NO_MORE_FILES)
                            throw new Exception($"Failed to query directory. Path: '{path}'. Status: '{status}'.");

                        foreach (FileDirectoryInformation fdi in fileList)
                        {
                            string name = fdi.FileName.ToString();
                            bool isDir = (fdi.FileAttributes & FileAttributes.Directory) == FileAttributes.Directory;
                            AddFile(files, name, isDir);
                        }
                    }
                    return files;
                }
            );

            static void AddFile(List<FileEntry> files, string name, bool isDir)
            {
                if (isDir && (name == "." || name == ".."))
                    return;

                files.Add(new FileEntry()
                {
                    Name = name,
                    IsDirectory = isDir,
                });
            }
        }

        public System.IO.Stream ReadFile(string path)
        {
            return ExecuteInTreeConnect(
                path,
                (ISMBFileStore store, string smbPath, out object handle, out FileStatus fStatus) =>
                {
                    NTStatus status = store.CreateFile(
                        out object handleX,
                        out FileStatus fStatusX,
                        smbPath,
                        AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE,
                        FileAttributes.Normal,
                        ShareAccess.Read,
                        CreateDisposition.FILE_OPEN,
                        CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                        null);
                    handle = handleX;
                    fStatus = fStatusX;
                    return status;
                },
                (ISMBFileStore store, object handle) =>
                {
                    System.IO.MemoryStream stream = new();
                    byte[] data;
                    long bytesRead = 0;
                    int maxReadSize = Math.Min(65536, (int)_client.MaxReadSize);
                    while (true)
                    {
                        NTStatus status = store.ReadFile(out data, handle, bytesRead, maxReadSize);
                        if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_END_OF_FILE)
                            throw new Exception($"Failed to read from file. Path: '{path}'. Status: '{status}'.");

                        if (status == NTStatus.STATUS_END_OF_FILE || data.Length == 0)
                            break;

                        bytesRead += data.Length;
                        stream.Write(data, 0, data.Length);
                    }
                    stream.Position = 0;
                    return stream;
                }
            );
        }

        public void WriteFile(string path, byte[] fileContent)
        {
            ExecuteInTreeConnect(
                path,
                (ISMBFileStore store, string smbPath, out object handle, out FileStatus fStatus) =>
                {
                    NTStatus status = store.CreateFile(
                        out object handleX,
                        out FileStatus fStatusX,
                        smbPath,
                        AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE,
                        FileAttributes.Normal,
                        ShareAccess.None,
                        CreateDisposition.FILE_CREATE,
                        CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                        null);
                    handle = handleX;
                    fStatus = fStatusX;
                    return status;
                },
                (ISMBFileStore store, object handle) =>
                {
                    var localFileStream = new System.IO.MemoryStream(fileContent);
                    int writeOffset = 0;
                    int maxWriteSize = Math.Min(65536, (int)store.MaxWriteSize);
                    while (localFileStream.Position < localFileStream.Length)
                    {
                        byte[] buffer = new byte[maxWriteSize];
                        int bytesRead = localFileStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead < maxWriteSize)
                            Array.Resize(ref buffer, bytesRead);

                        NTStatus status = store.WriteFile(out int numberOfBytesWritten, handle, writeOffset, buffer);
                        if (status != NTStatus.STATUS_SUCCESS)
                            throw new Exception($"Failed to write to file. Path: '{path}'. Status: '{status}'.");
                        writeOffset += bytesRead;
                    }
                    return true;
                }
            );
        }

        private TRes ExecuteInTreeConnect<TRes>(
            string path,
            CreateHandle createHandleFunc,
            Func<ISMBFileStore, object, TRes> doWorkFunc)
        {
            string smbPath = path.Replace(@"/", @"\");
            ISMBFileStore store = _client.TreeConnect(_shareName, out NTStatus status);
            if (status == NTStatus.STATUS_SUCCESS)
            {
                try
                {
                    if (store is SMB1FileStore)
                        smbPath = @"\\" + smbPath;

                    status = createHandleFunc(store, smbPath, out object handle, out FileStatus fileStatus);
                    if (status == NTStatus.STATUS_SUCCESS)
                    {
                        try
                        {
                            return doWorkFunc(store, handle);
                        }
                        finally
                        {
                            try { status = store.CloseFile(handle); } catch { }
                        }
                    }
                    else
                    {
                        throw new Exception($"Failed to create handle. Path: '{path}'. Status: '{status}'.");
                    }
                }
                finally
                {
                    try { status = store.Disconnect(); } catch { }
                }
            }
            else
            {
                throw new Exception($"Failed to tree-connect to share host '{_shareHost}'. Status: '{status}'.");
            }
        }

        private delegate NTStatus CreateHandle(ISMBFileStore store, string path, out object handle, out FileStatus fStatus);
    }

    public class FileEntry
    {
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
    }
}
