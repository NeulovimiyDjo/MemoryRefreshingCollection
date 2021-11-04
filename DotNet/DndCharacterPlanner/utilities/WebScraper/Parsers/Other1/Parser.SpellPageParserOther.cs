using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System.Collections.Generic;
using WebScraper.Models;

namespace WebScraper.Parsers.Dnd5eWikidot
{
    static partial class Parser
    {
        private static class SpellPageParserOther
        {
            public static Spell ParseSpellPage(string html)
            {
                var parser = new HtmlParser();
                var document = parser.Parse(html);


                var elem = document.QuerySelector(".page-header__title");
                string name = elem.TextContent.Trim();

                List<string> categories = ParseCategories(document);

                (string time, string range, string components, string duration, string description) = ParseAttributesAndDescription(document);


                var spell = new Spell { categories = categories, name = name, time = time, range = range, components = components, duration = duration, description = description };

                return spell;
            }


            private static List<string> ParseCategories(IHtmlDocument document)
            {
                List<string> categories = new List<string>();

                var header = document.QuerySelector("#PageHeader");

                foreach (var child in header.Children[0].Children[0].Children[1].Children)
                {
                    if (child.TagName == "A")
                    {
                        categories.Add(child.TextContent.Trim());
                    }
                    else // DIV
                    {
                        foreach (var liElem in child.Children[1].Children[0].Children) // child.Children[1].Children[0] = ul
                        {
                            categories.Add(liElem.TextContent.Trim());
                        }
                    }
                }

                return categories;
            }


            private static (string, string, string, string, string) ParseAttributesAndDescription(IHtmlDocument document)
            {
                string time, range, components, duration;
                string description = "";


                var elem = document.QuerySelector("#mw-content-text");

                if (elem.Children[0].TagName == "ASIDE")
                {
                    (time, range, components, duration) = ReadAttributes(2, elem.Children[0], "usual");


                    if (elem.Children[0].Children[0].TextContent.Trim() == "Word of Radiance" ||
                        elem.Children[0].Children[0].TextContent.Trim() == "Fireball") // an exceptionional pages
                    {

                        elem.Children[0].Remove();
                        description = elem.TextContent.Trim();
                    }
                    else
                    {
                        description = ReadDescription(1, elem, false);
                    }
                }
                else if (elem.Children[1].TagName == "BLOCKQUOTE") // idk why n1 child aside is actually n3 and n1 is blockquote
                {
                    // e.g. https://dnd5e.fandom.com/wiki/Plane_Shift

                    (time, range, components, duration) = ReadAttributes(2, elem.Children[3], "usual");

                    description = ReadDescription(4, elem, false);
                }
                else if (elem.Children[0].TagName == "TABLE")
                {
                    (time, range, components, duration) = ReadAttributes(0, elem.Children[0].Children[1], "usual");

                    description = ReadDescription(1, elem, true);
                }
                else if (elem.Children[1].TagName == "TABLE")
                {
                    (time, range, components, duration) = ReadAttributes(0, elem.Children[1].Children[0], "usual");

                    description = ReadDescription(2, elem, true);
                }
                else if (elem.Children[0].Children.Length > 4) // it's <p> and time, range, components and duration are children of p0
                {
                    // e.g. https://dnd5e.fandom.com/wiki/Mordenkainen%27s_Sword
                    // or https://dnd5e.fandom.com/wiki/Power_Word_Heal

                    (time, range, components, duration) = ReadAttributes(0, elem, "pFirst");

                    description = ReadDescription(1, elem, true);
                }
                else // it's <p> and time, range, components and duration are inside p1, p2, p3, p4
                {
                    (time, range, components, duration) = ReadAttributes(0, elem, "pSecond");

                    description = ReadDescription(5, elem, true);
                }

                description = description.Trim();
                description = System.Text.RegularExpressions.Regex.Replace(description, @"\[.\]", ""); // remove [1] e.t.c. references/links


                return (time, range, components, duration, description);
            }


            private static (string, string, string, string) ReadAttributes(int start, AngleSharp.Dom.IElement root, string type)
            {
                string time, range, components, duration;
                AngleSharp.Dom.IElement currentElem;

                if (type == "pFirst")
                {
                    currentElem = root.Children[0];

                    currentElem.Children[0].Remove();

                    string text = currentElem.TextContent.Trim();
                    string[] attributes = text.Split(':');

                    time = attributes[1];
                    time = System.Text.RegularExpressions.Regex.Replace(time, @"Range", "");
                    time = time.Trim();

                    range = attributes[2];
                    range = System.Text.RegularExpressions.Regex.Replace(range, @"Components", "");
                    range = range.Trim();

                    components = attributes[3];
                    components = System.Text.RegularExpressions.Regex.Replace(components, @"Duration", "");
                    components = components.Trim();

                    duration = attributes[4].Trim();
                }
                else if (type == "pSecond")
                {
                    currentElem = root.Children[1];
                    time = currentElem.TextContent;
                    time = System.Text.RegularExpressions.Regex.Replace(time, @"Casting time:", "");
                    time = time.Trim();


                    currentElem = root.Children[2];
                    range = currentElem.TextContent;
                    range = System.Text.RegularExpressions.Regex.Replace(range, @"Range:", "");
                    range = range.Trim();


                    currentElem = root.Children[3];
                    components = currentElem.TextContent;
                    components = System.Text.RegularExpressions.Regex.Replace(components, @"Components:", "");
                    components = components.Trim();


                    currentElem = root.Children[4];
                    duration = currentElem.TextContent;
                    duration = System.Text.RegularExpressions.Regex.Replace(duration, @"Duration:", "");
                    duration = duration.Trim();
                }
                else
                {
                    currentElem = root.Children[start + 0].Children[1];
                    time = currentElem.TextContent.Trim();


                    currentElem = root.Children[start + 1].Children[1];
                    range = currentElem.TextContent.Trim();


                    currentElem = root.Children[start + 2].Children[1];
                    components = currentElem.TextContent.Trim();


                    currentElem = root.Children[start + 3].Children[1];
                    duration = currentElem.TextContent.Trim();
                }



                return (time, range, components, duration);
            }


            private static string ReadDescription(int start, AngleSharp.Dom.IElement root, bool isPureDescription)
            {
                string result = "";


                if (root.QuerySelectorAll("#Description").Length == 0) isPureDescription = true; // all ahead is <p>s containing description       

                for (int i = start; i < root.Children.Length; i++)
                {
                    var currentElem = root.Children[i];


                    if (currentElem.TextContent.Trim() == "Description") // skip until after Description header
                    {
                        isPureDescription = true;
                        continue;
                    }
                    if (currentElem.TextContent.Trim() == "Leveling") // skip Leveling Header
                    {
                        continue;
                    }
                    if (currentElem.TextContent.Trim() == "References") // references header ends the description section
                    {
                        isPureDescription = false;
                    }


                    if (currentElem.ClassName == "references") continue;


                    if (isPureDescription)
                    {
                        result = result + CommonHelpers.ReadArbitraryElement(currentElem);
                    }
                }

                return result;
            }
        }
    }
}
