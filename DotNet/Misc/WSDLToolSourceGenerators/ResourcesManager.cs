using System;
using System.IO;
using System.Reflection;

namespace SharedProjects.SharedHelpers.Common
{
    public static class ResourcesManager
    {
        public static Stream GetStream(string resourceName)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            string aName = assembly.GetName().Name;
            Stream stream = assembly.GetManifestResourceStream($"{aName}.Resources.{resourceName}");
            return stream;
        }

        public static string GetString(string resourceName)
        {
            using Stream stream = GetStream(resourceName);
            using StreamReader sr = new(stream);
            return sr.ReadToEnd();
        }

        public static void ExtractResourcesToDir(string destDir)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            string[] resNames = assembly.GetManifestResourceNames();
            foreach (string resName in resNames)
            {
                using Stream resStream = assembly.GetManifestResourceStream(resName);
                string fileName = resName.Split(new string[] { ".Resources." }, StringSplitOptions.RemoveEmptyEntries)[1];
                using Stream fileStream = File.Open(@$"{destDir}\{fileName}", FileMode.OpenOrCreate, FileAccess.Write);
                resStream.CopyTo(fileStream);
            }
        }
    }
}
