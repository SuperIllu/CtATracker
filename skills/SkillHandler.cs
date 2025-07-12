using CtATracker.skills.serialisers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;


[assembly: InternalsVisibleTo("CtATracker.Tests")]

namespace CtATracker.skills
{
    public class SkillHandler
    {
        [Serializable]
        internal struct Skill
        {
            public string Name;
            public string ShortName;
            public HashSet<string> Synergies;
            public Func<int, Dictionary<string, SkillConfig>, int> DurationFunc;
        }

        [Serializable]
        public class SkillConfig
        {
            public string Name;
            public int TotalPoints;
            public int HardPoints;

            public override string ToString() => $"{Name} (Total: {TotalPoints}, Hard: {HardPoints})";
        }

        private Dictionary<string, Skill> _allSkills;


        internal SkillHandler(ISkillFileHandler fileHandler)
        {
            _allSkills = fileHandler.LoadSkills();
            return;

            string basePath = AppContext.BaseDirectory;
            string filePath = Path.Combine(basePath, "");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Skill configuration file '{filePath}' not found.");
            }

            string fileContent = File.ReadAllText(filePath);
            _allSkills = ParseSkills(fileContent);

        }

        private static Dictionary<string, Skill> ParseSkills(string skillFileContent)
        {
            Dictionary<string, Skill> skills = new();

            var deserializer = new DeserializerBuilder()
               .WithNamingConvention(CamelCaseNamingConvention.Instance)
               .Build();
            var rawSkillList = deserializer.Deserialize<List<Dictionary<string, string>>>(skillFileContent);
            foreach (var rawSkill in rawSkillList)
            {
                try
                {
                    string allKeys = string.Join(", ", rawSkill.Keys);
                    string allValues = string.Join(", ", rawSkill.Values);
                    //Console.WriteLine($"keys: {allKeys}, values: {allValues}");

                    string skillName = rawSkill["name"];
                    string shortName = rawSkill["shortName"];
                    string durationString = rawSkill["calculation"];

                    Func<int, Dictionary<string, SkillConfig>, int> durationFunction = GenerateDurationFunction(durationString);

                    Skill skill = new Skill
                    {
                        Name = skillName,
                        ShortName = shortName,
                        Synergies = GetSynergies(durationString),
                        DurationFunc = durationFunction
                    };

                    skills.Add(skillName, skill);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing skill '{rawSkill}': {ex.Message}");
                    continue;
                }
            }

            return skills;
        }

        private static Func<int, Dictionary<string, SkillConfig>, int> GenerateDurationFunction(string durationString)
        {
            string sanitized = SanitizeInput(durationString);

            Func<int, Dictionary<string, SkillConfig>, int> durationFunc = (level, charSkills) =>
            {
                string durationStrInternal = sanitized.Replace("{level}", level.ToString());

                string replaced = Regex.Replace(durationStrInternal, @"\{(.*?)\}", match =>
                {
                    string key = match.Groups[1].Value;
                    if (charSkills.TryGetValue(key, out var skillConfig))
                        return skillConfig.HardPoints.ToString();
                    else
                        // If the skill is not found, assume no points for synergy
                        return "0";
                });

                object result = new NCalc.Expression(replaced).Evaluate();
                return result is int i ? i : Convert.ToInt32(result);
            };

            return durationFunc;
        }

        public static HashSet<string> GetSynergies(string calculationString)
        {
            HashSet<string> synergies = new HashSet<string>();
            // Match all {skillName} patterns in the calculation string
            var matches = Regex.Matches(calculationString, @"\{(.*?)\}");
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string skillName = match.Groups[1].Value.Trim();
                    if (!string.IsNullOrEmpty(skillName) && skillName != "level")
                    {
                        synergies.Add(skillName);
                    }
                }
            }
            return synergies;
        }

        private static string SanitizeInput(string input)
        {
            // Very basic sanitizing: disallow certain dangerous characters
            // Disallow: semicolons, backticks, quotes, etc.
            string pattern = @"[;`""']";
            if (Regex.IsMatch(input, pattern))
                throw new ArgumentException($"Input '{input}' contains potentially dangerous characters.");

            return input.Trim();
        }

        public int CalculateSkillDuration(string skillName, int level, Dictionary<string, SkillConfig> charConfig)
        {
            if (_allSkills.TryGetValue(skillName, out var durationFunc))
            {
                return durationFunc.DurationFunc(level, charConfig);
            }
            return -1;
        }

        internal List<string> GetAllSkills()
        {
            return _allSkills.Keys.ToList();
        }

        internal Skill GetSkill(string skillName)
        {
            if (_allSkills.TryGetValue(skillName, out var skill))
            {
                return skill;
            }
            // pure synergy skills do not have skill entries
            return default;
        }

        internal bool TryGetSkill(string skillName, out Skill skill)
        {
            return _allSkills.TryGetValue(skillName, out skill);
        }
    }

}

