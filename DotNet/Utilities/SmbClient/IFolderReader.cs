using System;
using System.Collections.Generic;

namespace SmbClientProject
{
    public interface IFolderReader : IDisposable
    {
        public List<string> ListDirs(string path);
        public List<string> ListFiles(string path);
        public byte[] GetFileContent(string path);
    }
}
