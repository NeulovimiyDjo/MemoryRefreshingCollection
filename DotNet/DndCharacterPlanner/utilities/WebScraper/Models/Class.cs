using Newtonsoft.Json;
using System.Collections.Generic;


namespace WebScraper.Models
{
    public class Class
    {
        public int id;

        public string name;
        public string description;
        public string requirement;
        public List<int> feats;
        public List<int> cantrips;
        public List<int> spells;

        public string subclassAbilityName;

        public List<Ability> abilities;
        public Dictionary<string, Subclass> subclasses;


        [JsonIgnore]
        public List<int> subclassAbilityLevels;


        public void FillSubclassAbilitiesLevels()
        {
            Ability subclassAbility = abilities.Find(a => a.name == subclassAbilityName);

            foreach (var sub in subclasses.Values)
            {
                // Exception for this monk subclass. Only has one ability which can be taken multiple times.
                if (sub.name == "Way of the Four Elements")
                {
                    sub.abilities[0].level = subclassAbility.level;
                    continue;
                }

                // subclassAbilityLevels only contains levels for abilities 
                // which level is higher than the level when subclass becomes available.
                for (int i = 0; i < subclassAbilityLevels.Count; ++i)
                {
                    sub.abilities[sub.abilities.Count - 1 - i].level = subclassAbilityLevels[subclassAbilityLevels.Count - 1 - i];
                }

                // Fill the ones not accounted in subclassAbilityLevels.
                foreach (var ab in sub.abilities)
                {
                    if (ab.level == 0) ab.level = subclassAbility.level;
                }
            }
        }
    }
}
