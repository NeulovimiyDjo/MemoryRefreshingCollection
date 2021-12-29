using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SharedProjects.SharedHelpers.Common
{
    public static class OSInteractor
    {
        public static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo source = new(sourceDirectory);
            DirectoryInfo target = new(targetDirectory);

            Directory.CreateDirectory(target.FullName);
            foreach (FileInfo fi in source.GetFiles())
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyDirectory(diSourceSubDir.FullName, nextTargetSubDir.FullName);
            }
        }

        public static IEnumerable<string> GetFilesByRegex(
            string path,
            string searchPatternExpression = "",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Regex regex = new Regex(searchPatternExpression, RegexOptions.IgnoreCase);
            return Directory.EnumerateFiles(path, "*", searchOption)
                .Where(file => regex.IsMatch(Path.GetFileName(file)));
        }
    }
}
