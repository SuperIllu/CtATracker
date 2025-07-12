using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CtATracker.characters
{
    public class CharacterEntry
    {
        public string Name;
        public List<Skills.SkillConfig> Skills;

        public CharacterEntry()
        {
            Name = "NewCharacter";
            Skills = new List<Skills.SkillConfig>();
        }

        public void AddSkill(Skills.SkillConfig skill)
        {
            Skills.SkillConfig? skillConfig = Skills.Find(s => skill.Name == s.Name);
            if (skillConfig is null)
            {
                // is a new skill
                Skills.Add(skill);
            }
            else
            {
                // skill might be already there as a pure synergy skill
                if (skillConfig.TotalPoints > 0)
                {
                    throw new InvalidOperationException($"Skill '{skill.Name}' already exists in character '{Name}'.");
                }
                skillConfig.TotalPoints = skill.TotalPoints;
            }
        }

        public void RemoveSkill(string skillName)
        {
            var skillToRemove = Skills.FirstOrDefault(s => s.Name == skillName);
            if (skillToRemove != null)
            {
                Skills.Remove(skillToRemove);
            }
            else
            {
                throw new KeyNotFoundException($"Skill '{skillName}' not found in character '{Name}'.");
            }
        }

        public void UpdateSkillLevel(string skillName, int hardPoints, int totalPoints)
        {
            var existingSkill = Skills.FirstOrDefault(s => s.Name == skillName);
            if (existingSkill != null)
            {
                existingSkill.HardPoints = hardPoints;
                existingSkill.TotalPoints = totalPoints;
            }
            else
            {
                throw new KeyNotFoundException($"Skill '{skillName}' not found in character '{Name}'.");
            }
        }

        internal void SetTotalSkillPoints(string skillName, int skillLevel)
        {
            var skillEntry = Skills.Find(s => s.Name == skillName);
            if (skillEntry != null)
            {
                skillEntry.TotalPoints = skillLevel;
            }
            else
            {
                throw new KeyNotFoundException($"Skill '{skillName}' not found in character '{Name}'.");
            }
        }

        /// <summary>
        /// Aka synergy points.
        /// </summary>
        /// <param name="skillName"></param>
        /// <param name="hardPoints"></param>
        /// <exception cref="KeyNotFoundException"></exception>
        internal void SetHardSkillPoints(string skillName, int hardPoints)
        {
            var skillEntry = Skills.Find(s => s.Name == skillName);
            if (skillEntry != null)
            {
                skillEntry.HardPoints = hardPoints;
            }
            else
            {
                // if the synergy skill does not exist, create it with 0 total points
                AddSkill(new Skills.SkillConfig() { Name = skillName, HardPoints = hardPoints, TotalPoints = 0 });
            }
        }
    }
}
