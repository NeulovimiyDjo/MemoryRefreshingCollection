using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace WebScraper.Models
{
    public class Spell
    {
        public int id;

        public string name;

        public int level;
        public List<string> classes;

        public string school;

        public string time;
        public string range;
        public string components;
        public string duration;

        public string description;

        public string save; // was only used in old spell parser?
        public bool ritual; // was only used in old spell parser?
        public bool concentration; // was only used in old spell parser?

        public string source; // was only used in old spell parser?

        [JsonIgnore]
        public List<string> categories; // needed for old spells parser only


        public void SetLevel()
        {
            string levelStr = categories.Find(s => s.Contains("Level "));

            if (levelStr != null)
            {
                string subString = levelStr.Substring(6, 1);
                Int32.TryParse(subString, out level);
            }
            else // cantrip
            {
                level = 0;
            }
        }

        public void SetClasses()
        {
            classes = new List<string>();

            List<string> strings = categories.FindAll(s => s.Contains(" Spells"));
            foreach (var str in strings)
            {
                if (str != null)
                {
                    string className = str.Substring(0, str.Length - 7);
                    classes.Add(className);
                }
            }
        }

        public void SetSchool()
        {
            school = categories.Find(s =>
            {
                switch (s)
                {
                    case "Abjuration":
                    case "Conjuration":
                    case "Divination":
                    case "Enchantment":
                    case "Evocation":
                    case "Illusion":
                    case "Necromancy":
                    case "Transmutation":
                        return true;
                    default:
                        return false;
                }
            });
        }

        public void SetSource()
        {
            string src = categories.Find(s =>
            {
                switch (s)
                {
                    case "Player's Handbook":
                    case "Xanathar's Guide to Everything":
                    case "Unearthed Arcana":
                        return true;
                    default:
                        return false;
                }
            });

            switch (src)
            {
                case "Player's Handbook":
                    source = "PHB";
                    break;
                case "Xanathar's Guide to Everything":
                    source = "XGTE";
                    break;
                case "Unearthed Arcana":
                    source = "UA";
                    break;
                default:
                    break;
            }
        }
    }
}
