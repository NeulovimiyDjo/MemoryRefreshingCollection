using System;
using System.Collections.Generic;

namespace WebScraper.Models
{
    public class Ability
    {
        public int id;

        public int level;
        public string name;
        public string description;

        public List<Option> options;
        public List<int> increases;

        public Dictionary<string, int> bonusStats;


        public void FillStats()
        {
            bonusStats = new Dictionary<string, int>();

            if (name == "Ability Score Increase")
            {
                string[] parts = description.Split(" score increases by ");

                if (parts.Length > 1)
                {
                    string[] words = parts[0].Split(' ');
                    string shortStatName = words[words.Length - 1].Substring(0, 3).ToLower();
                    bonusStats.Add(shortStatName, Int32.Parse(parts[1].Substring(0, 1)));
                }

                if (parts.Length > 2)
                {
                    string[] words = parts[1].Split(' ');
                    string shortStatName = words[words.Length - 1].Substring(0, 3).ToLower();
                    bonusStats.Add(shortStatName, Int32.Parse(parts[2].Substring(0, 1)));
                }
            }
        }
    }
}
