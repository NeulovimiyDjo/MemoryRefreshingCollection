using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmbClientProject
{
    public class SmbFolderReader : IFolderReader
    {
        private readonly string _rootPath;
        private readonly SMBHelper _smbHelper;

        public SmbFolderReader(string rootPath, SMBHelper smbHelper)
        {
            _rootPath = rootPath;
            _smbHelper = smbHelper;
            _smbHelper.ConnectAndLogin();
        }

        public void Dispose()
        {
            _smbHelper.Dispose();
        }

        public List<string> ListDirs(string path)
        {
            return _smbHelper
                .ListFiles(GetFullPath(path))
                .Where(x => x.IsDirectory == true)
                .Select(x => x.Name)
                .ToList();
        }

        public List<string> ListFiles(string path)
        {
            return _smbHelper
                .ListFiles(GetFullPath(path))
                .Where(x => x.IsDirectory == false)
                .Select(x => x.Name)
                .ToList();
        }

        public byte[] GetFileContent(string path)
        {
            return _smbHelper.ReadFile(GetFullPath(path)).ReadAllBytes();
        }

        private string GetFullPath(string path) => Path.Combine(_rootPath, path);
    }
}
