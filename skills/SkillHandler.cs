using CtATracker.skills.serialisers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;


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
            public Key HotKey;

            public override string ToString() => $"{Name} (Total: {TotalPoints}, Hard: {HardPoints})";
        }

        private Dictionary<string, Skill> _allSkills;


        internal SkillHandler(ISkillFileHandler fileHandler)
        {
            _allSkills = fileHandler.LoadSkills();
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

