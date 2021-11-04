using System;
using System.IO;
using System.Net.Http;
using AngleSharp.Parser.Html;

namespace WebScraper.Downloaders.Dnd5eWikidot
{
    public static partial class Downloader
    {
        private const string _mainPageUrl = "http://dnd5e.wikidot.com";
        private static readonly string _downloadedPagesDir;

        private static readonly HttpClient _httpClient = new HttpClient();

        static Downloader()
        {
            string webScraperProjectDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            _downloadedPagesDir = webScraperProjectDir + "/../dnd5e_downloaded_pages";
        }


        public static void DownloadAllWikidotPages()
        {
            string mainPageHtml = CommonHelpers.GetHtmlFromUrl(_mainPageUrl);

            var parser = new HtmlParser();
            var document = parser.Parse(mainPageHtml);

            CommonHelpers.SavePage(mainPageHtml, $"{_downloadedPagesDir}/MainPage.html.txt");
            MainPageLinksHelper.DownloadAllPagesInDocument(document);
            SpellPagesHelper.DownloadAllSpellPages();
            SpecialPagesHelper.DownloadAllSpecialPages();
        }
    }
}
