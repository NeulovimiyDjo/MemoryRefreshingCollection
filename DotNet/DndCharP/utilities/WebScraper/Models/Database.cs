using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;


namespace WebScraper.Models
{
    public class Database
    {
        public List<Option> feats;
        public Dictionary<string, Race> races;

        public List<Spell> cantrips;
        public List<Spell> spells;
        public Dictionary<string, Class> classes;


        [JsonIgnore]
        public List<Spell> spellsAndCantrips;


        public void DoPostParsing(bool useOldSpellParser = false)
        {
            int featIdCounter = 200;
            int spellIdCounter = 400;

            int raceIdCounter = 1000;
            int subraceIdCounter = 1200;

            int classIdCounter = 1500;
            int subclassIdCounter = 1600;

            int abilityIdCounter = 2000;
            int optionIdCounter = 5000;


            foreach (var feat in feats)
            {
                feat.id = featIdCounter++;
                feat.FillStats();
            }


            foreach (var spell in spellsAndCantrips)
            {
                spell.id = spellIdCounter++;

                if (useOldSpellParser)
                {
                    spell.SetLevel();
                    spell.SetClasses();
                    spell.SetSchool();
                    spell.SetSource();
                }
            }

            cantrips = (from s in spellsAndCantrips where s.level == 0 select s).ToList();
            spells = (from s in spellsAndCantrips where s.level > 0 select s).ToList();



            foreach (var race in races.Values)
            {
                race.id = raceIdCounter++;
                race.FillSubracesDescriptions();

                foreach (var a in race.abilities)
                {
                    a.id = abilityIdCounter++;
                    a.FillStats();

                    a.increases = new List<int>();
                    switch (a.name)
                    {
                        case "Ability Score Increase":
                            if (race.name == "Half-Elf")
                            {
                                a.increases.AddRange(new int[] { 0 });
                            }
                            break;
                        case "Half-Elf Versatility":
                            a.increases.AddRange(new int[] { 0 });
                            break;
                        default:
                            break;
                    }


                    foreach (var o in a.options)
                    {
                        o.id = optionIdCounter++;
                        o.FillStats();
                    }
                }



                foreach (var sub in race.subraces.Values)
                {
                    sub.id = subraceIdCounter++;

                    foreach (var a in sub.abilities)
                    {
                        a.id = abilityIdCounter++;
                        a.FillStats();

                        a.increases = new List<int>();
                        switch (a.name)
                        {
                            case "Ability Score Increase":
                                if (sub.name == "Variant Human")
                                {
                                    a.increases.AddRange(new int[] { 0 });
                                }
                                break;
                            //case "Skills":
                            //  a.increases.AddRange(new int[] { 0 });
                            //  break;
                            case "Feat":
                                a.increases.AddRange(new int[] { 0 });
                                break;
                            default:
                                break;
                        }


                        foreach (var o in a.options)
                        {
                            o.id = optionIdCounter++;
                            o.FillStats();
                        }
                    }
                }
            }


            foreach (var cls in classes.Values)
            {
                cls.id = classIdCounter++;
                cls.FillSubclassAbilitiesLevels();


                foreach (var a in cls.abilities)
                {
                    a.id = abilityIdCounter++;
                    a.FillStats();

                    a.increases = new List<int>();
                    switch (a.name)
                    {
                        case "Metamagic":
                            a.increases.AddRange(new int[] { 0, 0, 10, 17 });
                            break;
                        case "Fighting Style":
                            a.increases.AddRange(new int[] { 0 });
                            break;
                        case "Eldritch Invocations":
                            a.increases.AddRange(new int[] { 0, 0, 5, 7, 9, 12, 15, 18 });
                            break;
                        default:
                            break;
                    }


                    foreach (var o in a.options)
                    {
                        o.id = optionIdCounter++;
                        o.FillStats();
                    }
                }


                foreach (var sub in cls.subclasses.Values)
                {
                    sub.id = subclassIdCounter++;

                    var subAbility = cls.abilities.Find(a => a.name == cls.subclassAbilityName);
                    sub.level = subAbility.level;

                    foreach (var a in sub.abilities)
                    {
                        a.id = abilityIdCounter++;
                        a.FillStats();

                        a.increases = new List<int>();
                        switch (a.name)
                        {
                            case "Divine Magic":
                                a.increases.AddRange(new int[] { 0 });
                                break;
                            case "Fighting Style":
                                a.increases.AddRange(new int[] { 0 });
                                break;
                            case "Additional Fighting Style":
                                a.increases.AddRange(new int[] { 0 });
                                break;
                            case "Combat Superiority":
                                a.increases.AddRange(new int[] { 0, 0, 0, 7, 7, 10, 10, 15, 15 });
                                break;
                            default:
                                break;
                        }


                        foreach (var o in a.options)
                        {
                            o.id = optionIdCounter++;
                            o.FillStats();
                        }
                    }
                }
            }
        }
    }
}
