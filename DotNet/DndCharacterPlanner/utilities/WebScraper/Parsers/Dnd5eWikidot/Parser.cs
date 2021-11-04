using System;
using System.Collections.Generic;
using System.Linq;
using WebScraper.Models;

namespace WebScraper.Parsers.Dnd5eWikidot
{
    public static partial class Parser
    {
        public static string _downloadedPagesDir;

        public static Database CreateDbFromDownloadedPages()
        {
            var feats = FeatsParserOther.ParseFeats();
            var races = RacePageParser.ParseAllRacePages();
            var classes = ClassPageParser.ParseAllClassPages();
            var spells = SpellsParser.ParseAllSpells();

            var db = new Database
            {
                feats = feats,
                races = races,
                classes = classes,
                spellsAndCantrips = spells
            };

            AddMissingSpellsAndFillSourceFromOldSpellList(feats, races, classes, spells);

            db.DoPostParsing();
            return db;
        }


        private static void AddMissingSpellsAndFillSourceFromOldSpellList(List<Option> feats, Dictionary<string, Race> races, Dictionary<string, Class> classes, List<Spell> spells)
        {
            var spells2 = SpellListPageParserOther.ScrapeAllSpells();
            var db2 = new Database
            {
                feats = feats,
                races = races,
                classes = classes,
                spellsAndCantrips = spells2
            };
            db2.DoPostParsing(true);



            string TransformForCompare(string name)
            {
                return name
                    .ToLower()
                    .Replace("ll", "l")
                    .Replace(":", "")
                    .Replace(" ", "")
                    ;
            }
            bool SpellNamesAreEqual(string name1, string name2)
            {
                return TransformForCompare(name1) == TransformForCompare(name2);
            };

            spells2
                .Where(s => s.source != "UA").ToList()
                .ForEach(s2 =>
                {
                    var spell = spells.FirstOrDefault(s => SpellNamesAreEqual(s.name, s2.name));

                    if (spell is null)
                        spells.Add(s2);
                    else
                        spell.source = s2.source;
                });

            spells.Sort((bigger, smaller) => bigger.name.CompareTo(smaller.name));

            spells.ForEach(s =>
            {
                if (s.school.ToLower().Contains("dunamancy"))
                    s.source = "Wildemount";
                else if (s.source is null)
                    s.source = "UNKNOWN";
            });
        }
    }
}
