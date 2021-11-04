namespace WebScraper.Parsers.Dnd5eWikidot
{
    static partial class Parser
    {
        private static class CommonHelpers
        {
            public static bool ShouldBeFiltered(string name)
            {
                if (name.Contains("(UA)")
                    || name.Contains("(UA:")
                    || name.Contains("(UA 20")
                    || name.Contains("(UA_20")
                    || name.Contains("(HB)")
                    || name.Contains(" HB")
                    || name.Contains("_HB")
                    || name.Contains("(Homebrew)")
                    || name.Contains("(Amonkhet)")
                    || name.Contains("(D&D Beyond)")
                    || name.Contains("(D&D_Beyond)")
                    || name.Contains("(Kaladesh)"))
                    return true;

                return false;
            }


            public static string ReadArbitraryElement(AngleSharp.Dom.IElement elem)
            {
                string result = "";

                if (elem.TagName == "TABLE")
                {
                    var tbody = elem.Children[0];
                    result = ReadTable(tbody);
                }
                else if (elem.TagName == "UL" || elem.TagName == "OL")
                {
                    result = ReadList(elem);
                }
                else if (elem.TagName == "P")
                {
                    result = "<p>" + elem.TextContent.Trim() + "</p>";
                }
                else
                {
                    result = elem.TextContent.Trim();
                }

                return result;
            }

            private static string ReadTable(AngleSharp.Dom.IElement tbody)
            {
                string result = "";

                result = result + "<table>";
                foreach (var row in tbody.Children)
                {
                    result = result + "<tr>";
                    foreach (var item in row.Children)
                    {
                        string text = item.TextContent.Trim();
                        if (text == "") text = "X";

                        if (item.HasAttribute("colspan"))
                        {
                            var colspan = item.GetAttribute("colspan");
                            string val = colspan.ToString();

                            result = result + "<td colspan=\"" + val + "\">" + text + "</td>";
                        }
                        else
                        {
                            result = result + "<td>" + text + "</td>";
                        }
                    }
                    result = result + "</tr>";
                }
                result = result + "</table>";

                return result;
            }


            private static string ReadList(AngleSharp.Dom.IElement ul)
            {
                string result = "";

                result = result + "<ul style=\"list-style-type: square; padding-left: 2em;\">";
                foreach (var li in ul.Children)
                {
                    string text = li.TextContent.Trim();
                    result = result + "<li>" + text + "</li>";
                }
                result = result + "</ul>";

                return result;
            }
        }
    }
}
