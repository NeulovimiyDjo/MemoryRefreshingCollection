using System;
using System.IO;

namespace WebScraper.Downloaders.Dnd5eWikidot
{
    public static partial class Downloader
    {
        private static class CommonHelpers
        {
            public static void DownloadPage(string url, string filePath)
            {
                var html = GetHtmlFromUrl(url);
                SavePage(html, filePath);
            }


            public static void SavePage(string html, string filePath)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, html);
            }


            public static string GetHtmlFromUrl(string url)
            {
                Console.WriteLine($"Downloading {url}");

                var request = _httpClient.GetAsync(url);
                var response = request.Result.Content.ReadAsStringAsync();

                return response.Result;
            }
        }
    }
}
