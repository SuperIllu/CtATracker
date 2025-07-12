using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CtATracker.characters.serialisers
{
    internal static class CharacterSerialiser
    {

        public static string SerialiseCharacters(Dictionary<string, CharacterEntry> characters)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            // Prepare a serializable structure matching the YAML format
            var serializableList = characters.Select(kvp => new
            {
                name = kvp.Key,
                skills = kvp.Value.Skills.Select(skill => new
                {
                    name = skill.Name,
                    hardPoints = skill.HardPoints,
                    totalPoints = skill.TotalPoints
                }).Where(skill => skill.hardPoints > 0 || skill.totalPoints > 0).ToList()
            }).ToList();
            return serializer.Serialize(serializableList);
        }

        public static Dictionary<string, CharacterEntry> LoadCharacters(string fileContent)
        {
            Dictionary<string, CharacterEntry> characters = new();
            CharacterEntry charEntry = new CharacterEntry();


            var deserializer = new DeserializerBuilder()
               .WithNamingConvention(CamelCaseNamingConvention.Instance)
               .Build();
            var rawCharList = deserializer.Deserialize<List<Dictionary<string, object>>>(fileContent);
            foreach (var rawCharEntry in rawCharList)
            {
                try
                {
                    DeserializeCharacterEntry(characters, rawCharEntry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing character '{rawCharEntry}': {ex.Message}");
                    continue;
                }
            }
            return characters;
        }

        public static void DeserializeCharacterEntry(Dictionary<string, CharacterEntry> characters, Dictionary<string, object> rawCharEntry)
        {
            string charName = (string)rawCharEntry["name"];
            CharacterEntry charEntry = new() { Name = charName, Skills = new List<Skills.SkillConfig>() };
            characters[charName] = charEntry;
            object rawSkills = rawCharEntry["skills"];
            if (rawSkills is IEnumerable<object> skills)
            {
                foreach (var skillObj in skills)
                {
                    if (skillObj is Dictionary<object, object> skillDict)
                    {
                        string skillName = skillDict["name"].ToString();
                        string hardPointsStr = skillDict["hardPoints"].ToString();
                        int hardPoints = int.Parse(hardPointsStr);
                        string totalPointsStr = skillDict["totalPoints"].ToString();
                        int totalPoints = int.Parse(totalPointsStr);

                        charEntry.Skills.Add(new Skills.SkillConfig
                        {
                            Name = skillName,
                            HardPoints = hardPoints,
                            TotalPoints = totalPoints
                        });
                    }
                }
            }
        }

    }
}
