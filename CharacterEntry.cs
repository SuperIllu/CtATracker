using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CtATracker
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
            if (!Skills.Any(s => s.Name == skill.Name))
            {
                Skills.Add(skill);
            }
            else
            {
                throw new InvalidOperationException($"Skill '{skill.Name}' already exists in character '{Name}'.");
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
    }
}
