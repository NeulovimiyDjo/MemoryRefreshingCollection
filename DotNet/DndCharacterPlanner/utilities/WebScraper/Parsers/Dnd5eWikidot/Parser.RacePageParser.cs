using AngleSharp.Parser.Html;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebScraper.Models;

namespace WebScraper.Parsers.Dnd5eWikidot
{
    static partial class Parser
    {
        private static class RacePageParser
        {
            public static Dictionary<string, Race> ParseAllRacePages()
            {
                var races = new Dictionary<string, Race>();

                var di = new DirectoryInfo($"{_downloadedPagesDir}/races");
                foreach (var fi in di.GetFiles("*.html.txt", SearchOption.AllDirectories))
                {
                    if (CommonHelpers.ShouldBeFiltered(fi.Name))
                        continue;

                    string html = File.ReadAllText(fi.FullName);
                    var race = ParseRacePage(html);

                    if (CommonHelpers.ShouldBeFiltered(race.name))
                        continue;
                    races.Add(race.name, race);
                }

                return races;
            }


            private static Race ParseRacePage(string html)
            {
                var parser = new HtmlParser();
                var document = parser.Parse(html);

                var nameElem = document.QuerySelector(".page-title");
                string name = nameElem.TextContent.Trim();

                var mainDiv = document.QuerySelector("#page-content");

                string description = "";
                var elem = mainDiv.Children[0];
                while (elem.NodeName != "DIV")
                {
                    description = description + CommonHelpers.ReadArbitraryElement(elem);

                    elem = elem.NextElementSibling;
                }


                var abilitiesList = GetAbilities(mainDiv);

                Dictionary<string, Subrace> subraces = GetSubraces(mainDiv);

                return new Race { name = name, description = description, abilities = abilitiesList, subraces = subraces };
            }


            private static List<Ability> GetAbilities(AngleSharp.Dom.IElement mainDiv)
            {
                var header = mainDiv.QuerySelector("#toc0");

                List<Ability> abilities = ReadAbilities(header);

                return abilities;
            }

            private static List<Ability> ReadAbilities(AngleSharp.Dom.IElement header)
            {
                var abilities = new List<Ability>();


                string description = "";
                var div = header.NextElementSibling;
                foreach (var elem in div.Children)
                {
                    if (elem.Children.Length > 0 && elem.TagName == "P")
                    {
                        string abilityName = elem.Children[0].TextContent.Trim();
                        abilityName = abilityName.Replace(".", "");

                        elem.Children[0].Remove();
                        description = elem.TextContent.Trim();

                        var ability = new Ability { name = abilityName, description = description, level = 0, options = new List<Option>() };
                        ability.options = new List<Option>();
                        abilities.Add(ability);

                        if (ability.name == "Half-Elf Versatility")
                        {
                            var p = elem.NextElementSibling;
                            while (p != null)
                            {
                                string optionName = p.Children[0].TextContent.Trim();
                                optionName = optionName.Replace(".", "");

                                p.Children[0].Remove();
                                string optionDescription = p.TextContent.Trim();

                                p = p.NextElementSibling;

                                var option = new Option { name = optionName, description = optionDescription };
                                if (CommonHelpers.ShouldBeFiltered(option.name))
                                    continue;
                                ability.options.Add(option);
                            }

                            break;
                        }
                    }
                    else // another p (or maybe table) continuing description
                    {
                        abilities.Last().description += CommonHelpers.ReadArbitraryElement(elem);
                    }
                }

                return abilities;
            }


            private static Dictionary<string, Subrace> GetSubraces(AngleSharp.Dom.IElement mainDiv)
            {
                var subraces = new Dictionary<string, Subrace>();

                var headers = mainDiv.QuerySelectorAll("H3");

                foreach (var header in headers)
                {
                    string subraceName = header.TextContent.Trim();

                    if (subraceName == "Arbandi Elf Clans") break;

                    string description = ReadSubraceDescription(header);
                    List<Ability> abilities = ReadAbilities(header);

                    var subrace = new Subrace { name = subraceName, description = description, abilities = abilities };
                    subraces.Add(subrace.name, subrace);
                }

                return subraces;
            }


            private static string ReadSubraceDescription(AngleSharp.Dom.IElement header)
            {
                string description = "";
                var div = header.NextElementSibling;
                foreach (var elem in div.Children)
                {
                    if (elem.Children.Length > 0 && elem.Children[0].TextContent.Trim() == "Ability Score Increase.")
                    {
                        break;
                    }
                    else
                    {
                        description = description + "\n" + CommonHelpers.ReadArbitraryElement(elem);
                        elem.Remove();
                    }
                }

                return description;
            }
        }
    }
}
