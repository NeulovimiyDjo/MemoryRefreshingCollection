using AngleSharp.Parser.Html;
using System.Collections.Generic;
using System.IO;
using WebScraper.Models;

namespace WebScraper.Parsers.Dnd5eWikidot
{
    static partial class Parser
    {
        private static class SeparateOptionsPageParser
        {
            public static List<Option> ParseOptions(string fileName)
            {
                string html = File.ReadAllText(_downloadedPagesDir + "/" + fileName);


                var parser = new HtmlParser();
                var document = parser.Parse(html);


                var options = new List<Option>();

                var elem = document.QuerySelector("#toc0");
                while (elem != null)
                {
                    string name = elem.TextContent.Trim();

                    elem = elem.NextElementSibling;
                    string description = "";
                    while (elem != null && elem.NodeName != "H2")
                    {
                        description = description + CommonHelpers.ReadArbitraryElement(elem);

                        elem = elem.NextElementSibling;
                    }

                    var option = new Option { name = name, description = description };

                    if (CommonHelpers.ShouldBeFiltered(option.name))
                        continue;
                    options.Add(option);
                }



                return options;
            }
        }
    }
}
