namespace WebScraper.Downloaders.Dnd5eWikidot
{
    public static partial class Downloader
    {
        private static class SpecialPagesHelper
        {
            public static void DownloadAllSpecialPages()
            {
                DownloadSpecialPage("http://dnd5e.wikidot.com/warlock:eldritch-invocations");
                DownloadSpecialPage("http://dnd5e.wikidot.com/fighter:battle-master:maneuvers");
                DownloadSpecialPage("http://dnd5e.wikidot.com/artificer-infusions");
                DownloadSpecialPage("http://dnd5e.wikidot.com/monk:four-elements:disciplines");
            }


            private static void DownloadSpecialPage(string url)
            {
                string fileName = url
                    .Replace(_mainPageUrl, "")
                    .Replace(":", "_");

                string filePath = $"{_downloadedPagesDir}/special_pages/{fileName}.html.txt";

                CommonHelpers.DownloadPage(url, filePath);
            }
        }
    }
}
