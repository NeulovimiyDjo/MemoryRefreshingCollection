using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebScraper.Models;

namespace WebScraper.Parsers.Dnd5eWikidot
{
    static partial class Parser
    {
        private static class SubclassPageParser
        {
            public static Dictionary<string, Subclass> ParseAllSubclassPages(string className)
            {
                var subclasses = new Dictionary<string, Subclass>();

                var sanitizedClassName = className.Replace(" ", "_");
                if (sanitizedClassName.Contains("Rune_Scribe"))
                    return subclasses;

                var parentDi = new DirectoryInfo($"{_downloadedPagesDir}/classes");
                var subclassDirectoryName = parentDi.GetDirectories($"*{sanitizedClassName}*")
                    .FirstOrDefault().FullName;
                
                var di = new DirectoryInfo(subclassDirectoryName);
                foreach (var fi in di.GetFiles("*.html.txt", SearchOption.TopDirectoryOnly))
                {
                    if (CommonHelpers.ShouldBeFiltered(fi.Name))
                        continue;

                    string html = File.ReadAllText(fi.FullName);
                    var subclass = ParseSubclassPage(html);

                    if (CommonHelpers.ShouldBeFiltered(subclass.name))
                        continue;
                    subclasses.Add(subclass.name, subclass);
                }

                return subclasses;
            }


            private static Subclass ParseSubclassPage(string html)
            {
                var parser = new HtmlParser();
                var document = parser.Parse(html);

                var nameElem = document.QuerySelector(".page-title");
                string name = nameElem.TextContent.Trim();
                if (name.Contains(":"))
                    name = name.Split(":")[1].Trim();

                var mainDiv = document.QuerySelector("#page-content");


                string description = "";
                var elem = mainDiv.Children[0];
                while (elem.NodeName != "DIV" && elem.NodeName != "H3")
                {
                    description = description + CommonHelpers.ReadArbitraryElement(elem);

                    elem = elem.NextElementSibling;
                }

                var cantripsIncreases = new List<int>();
                var spellsIncreases = new List<int>();
                // if subclass has spellcasting
                if (document.QuerySelector("#toc0").TextContent.Trim() == "Spellcasting")
                {
                    var spellcastingTable = mainDiv.QuerySelector("table");

                    (cantripsIncreases, spellsIncreases) = ReadSpellcastingTable(spellcastingTable);
                }



                var abilitiesList = GetAbilitiesList(mainDiv);


                return new Subclass
                {
                    name = name,
                    description = description,
                    abilities = abilitiesList,
                    cantrips = cantripsIncreases,
                    spells = spellsIncreases
                };
            }


            private static (List<int>, List<int>) ReadSpellcastingTable(AngleSharp.Dom.IElement table)
            {
                if (table.Children.Length == 1) table = table.Children[0]; // get tbody instead

                int cantripsColNum = 0;
                int spellsColNum = 0;
                int counter = 0;
                foreach (var col in table.Children[1].Children)
                {
                    if (col.TextContent.Trim() == "Cantrips Known")
                        cantripsColNum = counter;
                    else if (col.TextContent.Trim() == "Spells Known")
                        spellsColNum = counter;

                    ++counter;
                }

                int prevCantripsAmount = 0;
                int prevSpellsAmount = 0;


                var cantripsIncreases = new List<int>();
                var spellsIncreases = new List<int>();
                for (int rowNum = 2; rowNum < table.Children.Length; ++rowNum)
                {
                    int level = rowNum - 1;

                    if (cantripsColNum > 0)
                    {
                        string s = table.Children[rowNum].Children[cantripsColNum].TextContent.Trim();
                        if (s == "" || s == "-") continue;

                        int extra = 0;
                        if (s.Contains("Mage Hand + "))
                        {
                            s = s.Replace("Mage Hand + ", "");
                            extra = 1;
                        }

                        int cantripsAmount = Int32.Parse(s) + extra;
                        for (int i = 0; i < cantripsAmount - prevCantripsAmount; ++i)
                        {
                            cantripsIncreases.Add(level);
                        }
                        prevCantripsAmount = cantripsAmount;
                    }

                    if (spellsColNum > 0)
                    {
                        string s = table.Children[rowNum].Children[spellsColNum].TextContent.Trim();
                        if (s == "" || s == "-") continue;

                        int spellsAmount = Int32.Parse(s);
                        for (int i = 0; i < spellsAmount - prevSpellsAmount; ++i)
                        {
                            spellsIncreases.Add(level);
                        }
                        prevSpellsAmount = spellsAmount;
                    }
                }

                return (cantripsIncreases, spellsIncreases);
            }


            private static List<Ability> GetAbilitiesList(AngleSharp.Dom.IElement mainDiv)
            {
                var abilitiesList = new List<Ability>();

                var abilitiesHeaders = mainDiv.QuerySelectorAll("h3");

                foreach (var header in abilitiesHeaders)
                {
                    string abilityName = header.TextContent.Trim();

                    var ability = new Ability { name = abilityName };

                    string description = "";
                    var elem = header.NextElementSibling;
                    while (elem != null && elem.NodeName != "H3")
                    {
                        description = description + CommonHelpers.ReadArbitraryElement(elem);

                        elem = elem.NextElementSibling;
                    }

                    ability.description = description;

                    ClassPageParser.FillOptions(ability, header);

                    abilitiesList.Add(ability);
                }


                return abilitiesList;
            }
        }
    }
}
