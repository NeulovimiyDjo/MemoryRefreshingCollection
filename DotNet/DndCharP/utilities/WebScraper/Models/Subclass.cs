using System.Collections.Generic;

namespace WebScraper.Models
{
    public class Subclass
    {
        public int id;

        public int level;

        public string name;
        public string description;
        public List<Ability> abilities;

        public List<int> cantrips;
        public List<int> spells;
    }
}
