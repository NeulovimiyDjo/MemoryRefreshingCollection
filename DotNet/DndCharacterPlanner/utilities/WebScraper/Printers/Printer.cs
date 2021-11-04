using System;
using System.Collections.Generic;
using WebScraper.Models;

namespace WebScraper.Printers
{
    public static class Printer
    {
        public static void Print(Database db)
        {
            PrintFeats(db.feats);
            PrintRaces(db.races);
            PrintClasses(db.classes);
            PrintSpells(db.cantrips);
            PrintSpells(db.spells);
        }


        private static void PrintFeats(List<Option> feats)
        {
            foreach (var feat in feats)
            {
                Console.WriteLine("Name: " + feat.name);
                Console.WriteLine("Requirement: " + feat.requirement);
                Console.WriteLine("Description: " + feat.description);

                Console.WriteLine();
            }
        }


        private static void PrintRaces(Dictionary<string, Race> races)
        {
            foreach (var race in races.Values)
            {
                Console.WriteLine("=======================================================================");

                Console.WriteLine("Race Name: " + race.name);
                Console.WriteLine("Description: " + race.description);


                foreach (var ability in race.abilities)
                {
                    Console.WriteLine();
                    Console.WriteLine("Ability: " + ability.name + " (" + ability.level + ")");
                    Console.WriteLine(ability.description);

                    foreach (var option in ability.options)
                    {
                        Console.WriteLine("Option: " + option.name);
                        Console.WriteLine(option.description);
                    }
                }

                foreach (var subrace in race.subraces.Values)
                {
                    Console.WriteLine("------------------------------------------------------------------");

                    Console.WriteLine("Subrace Name: " + subrace.name);
                    Console.WriteLine("Subrace Description: " + subrace.description);

                    foreach (var ability in subrace.abilities)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Subrace Ability: " + ability.name + " (" + ability.level + ")");
                        Console.WriteLine(ability.description);

                        foreach (var option in ability.options)
                        {
                            Console.WriteLine("Option: " + option.name);
                            Console.WriteLine(option.description);
                        }
                    }

                    Console.WriteLine("------------------------------------------------------------------");
                    Console.WriteLine();
                }

                Console.WriteLine("=======================================================================");
                Console.WriteLine();
                Console.WriteLine();
            }
        }


        private static void PrintClasses(Dictionary<string, Class> classes)
        {
            foreach (var cls in classes.Values)
            {
                Console.WriteLine("=======================================================================");

                Console.WriteLine("Class Name: " + cls.name);
                Console.WriteLine("Description: " + cls.description);
                Console.WriteLine("Requirement: " + cls.requirement);

                Console.Write("Feats Increases: [");
                bool first = true;
                foreach (var lvl in cls.feats)
                {
                    if (first) first = false; else Console.Write(",");
                    Console.Write(lvl);
                }
                Console.WriteLine("]");

                foreach (var ability in cls.abilities)
                {
                    Console.WriteLine();
                    Console.WriteLine("Ability: " + ability.name + " (" + ability.level + ")");
                    Console.WriteLine(ability.description);

                    foreach (var option in ability.options)
                    {
                        Console.WriteLine("Option: " + option.name);
                        Console.WriteLine(option.description);
                    }
                }

                foreach (var subclass in cls.subclasses.Values)
                {
                    Console.WriteLine("------------------------------------------------------------------");

                    Console.WriteLine("Subclass Name: " + subclass.name);
                    Console.WriteLine("Subclass Description: " + subclass.description);

                    foreach (var ability in subclass.abilities)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Subclass Ability: " + ability.name + " (" + ability.level + ")");
                        Console.WriteLine(ability.description);

                        foreach (var option in ability.options)
                        {
                            Console.WriteLine("Option: " + option.name);
                            Console.WriteLine(option.description);
                        }
                    }

                    Console.WriteLine("------------------------------------------------------------------");
                    Console.WriteLine();
                }

                Console.WriteLine("=======================================================================");
                Console.WriteLine();
                Console.WriteLine();
            }
        }


        private static void PrintSpells(List<Spell> spells)
        {
            foreach (var spell in spells)
            {
                Console.WriteLine("Name: " + spell.name);

                //foreach (var category in spell.categories)
                //{
                //    Console.WriteLine("Category: " + category);
                //}

                Console.WriteLine("Time: " + spell.time);
                Console.WriteLine("Range: " + spell.range);
                Console.WriteLine("Components: " + spell.components);
                Console.WriteLine("Duration: " + spell.duration);

                Console.WriteLine("Description: " + spell.description);

                //Console.WriteLine("Save: " + spell.save);
                //Console.WriteLine("Ritual: " + spell.ritual);
                //Console.WriteLine("Concentration: " + spell.concentration);

                Console.WriteLine();
            }
        }
    }
}
