using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmbClientProject
{
    public class SimpleFolderReader : IFolderReader
    {
        private readonly string _rootPath;

        public SimpleFolderReader(string rootPath)
        {
            _rootPath = rootPath;
        }

        public void Dispose()
        {
        }

        public List<string> ListDirs(string path)
        {
            return Directory
                .GetDirectories(GetFullPath(path))
                .Select(x => Path.GetFileName(x))
                .ToList();
        }

        public List<string> ListFiles(string path)
        {
            return Directory
                .GetFiles(GetFullPath(path))
                .Select(x => Path.GetFileName(x))
                .ToList();
        }

        public byte[] GetFileContent(string path)
        {
            return File.ReadAllBytes(GetFullPath(path));
        }

        private string GetFullPath(string path) => Path.Combine(_rootPath, path);
    }
}
