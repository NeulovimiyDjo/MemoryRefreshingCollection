using System.IO;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace WebScraper.Downloaders.Dnd5eWikidot
{
    public static partial class Downloader
    {
        private static class SpellPagesHelper
        {
            public static void DownloadAllSpellPages()
            {
                var spellListHtml = File.ReadAllText(
                    $"{_downloadedPagesDir}/spells/All_Spells.html.txt"
                );

                var parser = new HtmlParser();
                var document = parser.Parse(spellListHtml);

                var tableElemsByLvl = document.QuerySelectorAll(".wiki-content-table");
                foreach (var tableElem in tableElemsByLvl)
                {
                    var allSpellLinkElems = tableElem.QuerySelectorAll("a");
                    foreach (var linkElem in allSpellLinkElems)
                    {
                        DownloadPageFromLinkElem(linkElem);
                    }
                }
            }


            private static void DownloadPageFromLinkElem(IElement linkElem)
            {
                string relativeUrl = ((IHtmlAnchorElement)linkElem).Href
                    .Replace("about://", "");

                string fullUrl = _mainPageUrl + relativeUrl;


                string fileName = linkElem.TextContent.Trim()
                    .Replace("spell:", "")
                    .Replace(" ", "_")
                    .Replace("/", "_")
                    .Replace(":", "_");

                string filePath =
                    $"{_downloadedPagesDir}/spells/all_spells_pages/{fileName}.html.txt";


                CommonHelpers.DownloadPage(fullUrl, filePath);
            }
        }
    }
}
