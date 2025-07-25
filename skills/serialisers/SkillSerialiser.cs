﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static CtATracker.skills.SkillHandler;

namespace CtATracker.skills.serialisers
{
    internal class SkillSerialiser
    {


        internal static Dictionary<string, Skill> ParseSkills(string skillFileContent)
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

        public static Func<int, Dictionary<string, SkillConfig>, int> GenerateDurationFunction(string durationString)
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

        private static string SanitizeInput(string input)
        {
            // Very basic sanitizing: disallow certain dangerous characters
            // Disallow: semicolons, backticks, quotes, etc.
            string pattern = @"[;`""']";
            if (Regex.IsMatch(input, pattern))
                throw new ArgumentException($"Input '{input}' contains potentially dangerous characters.");

            return input.Trim();
        }
    }
}
