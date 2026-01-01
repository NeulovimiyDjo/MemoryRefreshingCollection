using System;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;

namespace WebScraper.Downloaders.Dnd5eWikidot
{
    public static partial class Downloader
    {
        private static class MainPageLinksHelper
        {
            public static void DownloadAllPagesInDocument(IHtmlDocument document)
            {
                DownloadPagesInTocRange(document, "spells", 0, 0);
                DownloadPagesInTocRange(document, "schools", 1, 1);
                DownloadPagesInTocRange(document, "races", 2, 4, true);
                DownloadPagesInTocRange(document, "backgrounds", 5, 5);
                DownloadPagesInTocRange(document, "classes", 6, 20, true);
                DownloadPagesInTocRange(document, "items", 21, 21);
                DownloadPagesInTocRange(document, "feats", 22, 23, true);
            }


            private static void DownloadPagesInTocRange(
                IHtmlDocument document,
                string baseDirectoryName,
                int firstToc, int lastToc,
                bool useSubdirectories = false)
            {
                for (int tocNum = firstToc; tocNum <= lastToc; ++tocNum)
                {
                    var headerElem = document.QuerySelector("#toc" + tocNum);

                    DownloadTocHeaderLinkIfAny(baseDirectoryName, headerElem);
                    DownloadTocInnerLinks(baseDirectoryName, headerElem, useSubdirectories);
                }
            }

            private static void DownloadTocHeaderLinkIfAny(string baseDirectoryName, IElement headerElem)
            {
                var headerLinkElem = headerElem.QuerySelector("a");
                if (headerLinkElem != null)
                    DownloadPageFromTocLinkElem(baseDirectoryName, headerLinkElem);
            }

            private static void DownloadTocInnerLinks(
                string baseDirectoryName,
                IElement headerElem,
                bool useSubdirectories)
            {
                if (headerElem.TextContent.Trim() == "The Rune Scribe (UA)")
                    return;

                var paragraphElem = GetFirstSiblingParagraph(headerElem);
                var allLinkElems = paragraphElem.QuerySelectorAll("a");

                string subdirectoryName = headerElem.TextContent.Trim().Replace(" ", "_");
                var directoryName = useSubdirectories
                    ? $"{baseDirectoryName}/{subdirectoryName}"
                    : baseDirectoryName;

                foreach (var linkElem in allLinkElems)
                {
                    DownloadPageFromTocLinkElem(directoryName, linkElem);
                }
            }

            private static IElement GetFirstSiblingParagraph(IElement headerElem)
            {
                var currentElem = headerElem.NextElementSibling;
                while (currentElem.NodeName != "P")
                    currentElem = currentElem.NextElementSibling;

                return currentElem;
            }

            private static void DownloadPageFromTocLinkElem(string directoryName, IElement linkElem)
            {
                string url = ((IHtmlAnchorElement)linkElem).Href;
                string fileName = linkElem.TextContent.Trim().Replace(" ", "_");

                string filePath = $"{_downloadedPagesDir}/{directoryName}/{fileName}.html.txt";
                CommonHelpers.DownloadPage(url, filePath);
            }
        }
    }
}
