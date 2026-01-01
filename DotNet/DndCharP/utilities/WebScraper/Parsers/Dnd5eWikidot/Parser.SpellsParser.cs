using AngleSharp.Dom;
using AngleSharp.Parser.Html;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebScraper.Models;

namespace WebScraper.Parsers.Dnd5eWikidot
{
    static partial class Parser
    {
        private static class SpellsParser
        {
            public static List<Spell> ParseAllSpells()
            {
                var spells = new List<Spell>();

                var di = new DirectoryInfo($"{_downloadedPagesDir}/spells/all_spells_pages");
                foreach (var fi in di.GetFiles("*.html.txt", SearchOption.TopDirectoryOnly))
                {
                    if (CommonHelpers.ShouldBeFiltered(fi.Name))
                        continue;

                    string html = File.ReadAllText(fi.FullName);
                    var spell = ParseSpellPage(html);

                    if (CommonHelpers.ShouldBeFiltered(spell.name))
                        continue;
                    spells.Add(spell);
                }

                return spells;
            }


            private static Spell ParseSpellPage(string html)
            {
                var parser = new HtmlParser();
                var document = parser.Parse(html);

                var nameElem = document.QuerySelector(".page-header");
                string name = nameElem.TextContent.Trim();

                var mainDivElem = document.QuerySelector("#page-content");

                var firstParagraphElem = GetFirstChildParagraph(mainDivElem);
                var (firstDescriptionElem, levelAndSchool, combinedInfoParagraphText) =
                    GetInfoElements(firstParagraphElem);

                (int level, string school) = ParseLevelAndSchool(levelAndSchool);

                string time, range, components, duration;
                GetInfo(combinedInfoParagraphText, out time, out range, out components, out duration);

                string description = GetDescription(firstDescriptionElem, out IElement classesElem);

                var classes = GetClasses(classesElem, ref school);

                return new Spell
                {
                    name = name,
                    level = level,
                    school = school,
                    time = time,
                    range = range,
                    components = components,
                    duration = duration,
                    description = description,
                    classes = classes,
                };
            }

            private static void GetInfo(string combinedInfoParagraphText, out string time, out string range, out string components, out string duration)
            {
                string[] combinedInfoParagraphLines = combinedInfoParagraphText
                                    .Split("\n");

                time = combinedInfoParagraphLines[0]
                    .Replace("Casting Time:", "").Trim();
                range = combinedInfoParagraphLines[1]
                    .Replace("Range:", "").Trim();
                components = combinedInfoParagraphLines[2]
                    .Replace("Components:", "").Trim();
                duration = combinedInfoParagraphLines[3]
                    .Replace("Duration:", "").Trim();
            }

            private static (IElement, string, string) GetInfoElements(IElement firstParagraphElem)
            {
                var firstDescriptionElem = firstParagraphElem.NextElementSibling;
                string levelAndSchool = firstParagraphElem.TextContent.Trim();
                string combinedInfoParagraphText = firstParagraphElem.NextElementSibling
                    .TextContent.Trim();

                if (levelAndSchool.Contains("Casting Time:"))
                {
                    combinedInfoParagraphText = "Casting Time:"
                        + levelAndSchool.Split("Casting Time:")[1];

                    levelAndSchool = levelAndSchool.Split("Casting Time:")[0];
                }
                else
                {
                    firstDescriptionElem = firstDescriptionElem.NextElementSibling;
                }

                return (firstDescriptionElem, levelAndSchool, combinedInfoParagraphText);
            }


            private static List<string> GetClasses(IElement classesElem, ref string school)
            {
                string classesStr = classesElem.TextContent.Trim()
                            .Replace("Spell Lists.", "");


                List<string> classes = classesStr.Split(",").ToList();

                bool addDunamancy = false;
                classes = classes.Select(cls =>
                {
                    if (cls.Contains("Dunamancy"))
                        addDunamancy = true;
                    return cls.Replace("(Dunamancy)", "");
                }).ToList();

                classes = classes.Select(cls => cls.Trim()).ToList();


                if (addDunamancy && !school.ToLower().Contains("dunamancy"))
                    school = school + " (Dunamancy)";

                return classes;
            }


            private static string GetDescription(
                IElement firstDescriptionElem,
                out IElement classesElem)
            {
                var elem = firstDescriptionElem;
                string description = "";
                while (elem.NextElementSibling.NodeName != "DIV")
                {
                    description = description + CommonHelpers.ReadArbitraryElement(elem);
                    elem = elem.NextElementSibling;
                }

                classesElem = elem;
                return description;
            }


            private static (int level, string school) ParseLevelAndSchool(string levelAndSchool)
            {
                if (levelAndSchool.ToLower().Contains("cantrip"))
                {
                    int level = 0;
                    string school = levelAndSchool
                        .Replace(" cantrip", "")
                        .Replace(" Cantrip", "")
                        .Replace("cantrip ", "")
                        .Replace("Cantrip ", "");

                    return (level, school);
                }
                else
                {
                    int level = int.Parse(levelAndSchool.Substring(0, 1));
                    string school = levelAndSchool
                            .Split("level ")[1];

                    return (level, school);
                }
            }


            private static IElement GetFirstChildParagraph(IElement mainDivElem)
            {
                var currentElem = mainDivElem.Children[0];
                while (currentElem.NodeName != "P")
                    currentElem = currentElem.NextElementSibling;

                return currentElem;
            }
        }
    }
}
