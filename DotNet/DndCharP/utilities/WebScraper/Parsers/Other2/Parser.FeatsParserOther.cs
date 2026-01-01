using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using System.Collections.Generic;
using System.IO;
using WebScraper.Models;

namespace WebScraper.Parsers.Dnd5eWikidot
{
    static partial class Parser
    {
        private static class FeatsParserOther
        {
            public static List<Option> ParseFeats()
            {
                string html = File.ReadAllText(
                    _downloadedPagesDir
                    + "/../other_downloaded_pages"
                    + "/FeatsPage.html.txt"
                );


                var parser = new HtmlParser();
                var document = parser.Parse(html);


                var feats = new List<Option>();

                var elem = document.QuerySelector("#toc2");
                while (elem != null && elem.NodeName != "H1")
                {
                    elem = AddOptionAndGetNextElem(feats, elem);
                }

                ParseAndAddRacialFeats(feats, elem);

                return feats;
            }

            private static void ParseAndAddRacialFeats(List<Option> feats, IElement h1RacialFeatsElem)
            {
                var divRacialFeatsElem = h1RacialFeatsElem.NextElementSibling;
                var racialFeatsElems = divRacialFeatsElem.QuerySelectorAll("H2");

                foreach (var h2FeatNameElem in racialFeatsElems)
                {
                    string name = h2FeatNameElem.TextContent.Trim();

                    var requirementElem = h2FeatNameElem.NextElementSibling;
                    string requirement = requirementElem.TextContent.Trim()
                        .Replace("Prerequisite: ", "");

                    var descriptionElem = requirementElem.NextElementSibling;
                    string description = "";
                    while (descriptionElem != null
                        && descriptionElem.NodeName != "H2"
                        && descriptionElem.NodeName != "H1")
                    {
                        description = description
                            + CommonHelpers.ReadArbitraryElement(descriptionElem);

                        descriptionElem = descriptionElem.NextElementSibling;
                    }

                    var option = new Option { name = name, requirement = requirement, description = description };
                    if (CommonHelpers.ShouldBeFiltered(option.name))
                        continue;
                    feats.Add(option);
                }
            }

            private static IElement AddOptionAndGetNextElem(List<Option> feats, IElement elem)
            {
                string name = elem.TextContent.Trim();

                elem = elem.NextElementSibling;
                string description = "";
                while (elem != null && elem.NodeName != "H2" && elem.NodeName != "H1")
                {
                    description = description + CommonHelpers.ReadArbitraryElement(elem);

                    elem = elem.NextElementSibling;
                }

                var option = new Option { name = name, description = description };
                if (CommonHelpers.ShouldBeFiltered(option.name))
                    return elem;
                feats.Add(option);
                return elem;
            }
        }
    }
}
