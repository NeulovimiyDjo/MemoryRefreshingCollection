using System.Collections.Generic;

namespace WebScraper.Models
{
    public class Option
    {
        public int id;

        public string name;
        public string requirement;
        public string description;

        public Dictionary<string, int> bonusStats;


        public void FillStats()
        {
            bonusStats = new Dictionary<string, int>();

            switch (name)
            {
                case "Actor":
                    bonusStats.Add("cha", 1);
                    break;
                case "Durable":
                case "Dwarven Fortitude":
                case "Infernal Constitution":
                    bonusStats.Add("con", 1);
                    break;
                case "Heavily Armored":
                case "Heavy Armor Master":
                    bonusStats.Add("str", 1);
                    break;
                case "Keen Mind":
                case "Linguist":
                    bonusStats.Add("int", 1);
                    break;
                default:
                    break;
            }
        }
    }
}
