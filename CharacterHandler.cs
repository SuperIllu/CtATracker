using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static CtATracker.Skills;

namespace CtATracker
{
    internal class CharacterHandler
    {
        public const string CharacterFileName = "Characters.yml";


        private Dictionary<string, CharacterEntry> _characters;
        private Action<CharacterEntry> _onCharSelectedCallback;

        public CharacterEntry? CurrentChar { get; private set; } = null;




        public CharacterHandler(Action<CharacterEntry> onCharSelectedCallback)
        {
            _onCharSelectedCallback = onCharSelectedCallback;

            string basePath = AppContext.BaseDirectory;
            string filePath = Path.Combine(basePath, SkillFileName);

            if (!File.Exists(CharacterFileName))
            {
                // no chars existing yet - start from scratch
                _characters = new();
            }
            else
            {
                string fileContent = File.ReadAllText(CharacterFileName);
                _characters = LoadCharacters(fileContent);
                if (_characters.Count > 0)
                {
                    // load the first character as current
                    CurrentChar = _characters.First().Value;
                }
            }
        }

        public void AddNewCharacter(string charName)
        {
            if (_characters.ContainsKey(charName))
            {
                MessageBox.Show($"Character '{charName}' already exists. Please choose a different name.");
                return; 
            }

            CharacterEntry newChar  = new CharacterEntry
            {
                Name = charName,
                Skills = new List<Skills.SkillConfig>()
            };
            _characters.Add(charName, newChar);
        }

        internal void SetCurrentCharacter(string newCharName)
        {
            if (_characters.TryGetValue(newCharName, out var character))
            {
                CurrentChar = character;
                _onCharSelectedCallback?.Invoke(CurrentChar);
            }
            else
            {
                throw new KeyNotFoundException($"Character '{newCharName}' not found.");
            }
        }

        public List<string> GetCharacterList()
        {
            return _characters.Keys.ToList();
        }


        public void DeleteCurrentCharacter()
        {
            if (CurrentChar == null || !_characters.ContainsKey(CurrentChar.Name))
            {
                throw new InvalidOperationException("No current character to delete.");
            }
            _characters.Remove(CurrentChar.Name);
            CurrentChar = null;
            SaveCharactersToFile();
        }

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
                }).ToList()
            }).ToList();
            return serializer.Serialize(serializableList);
        }

        private void SaveCharactersToFile()
        {
            string yaml = SerialiseCharacters(_characters);

            File.WriteAllText(CharacterFileName, yaml);
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

        private static void DeserializeCharacterEntry(Dictionary<string, CharacterEntry> characters, Dictionary<string, object> rawCharEntry)
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
