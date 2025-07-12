using CtATracker.skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CtATracker.characters
{
    public class CharacterEntry
    {
        public string Name;
        public List<SkillHandler.SkillConfig> Skills;

        public CharacterEntry()
        {
            Name = "NewCharacter";
            Skills = new List<SkillHandler.SkillConfig>();
        }

        public void AddSkill(SkillHandler.SkillConfig skill)
        {
            SkillHandler.SkillConfig? skillConfig = Skills.Find(s => skill.Name == s.Name);
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

        public void RemoveSkill(string skillName, SkillHandler skillHandler)
        {
            var skillToRemove = Skills.FirstOrDefault(s => s.Name == skillName);
            if (skillToRemove != null)
            {
                if (skillToRemove.HardPoints > 0)
                {
                    // also acts as a synergy, keep hard points
                    skillToRemove.TotalPoints = 0;
                }
                else
                {
                    // has no synergy points, so we can remove it
                    Skills.Remove(skillToRemove);
                }

                CleanSyngergies(skillToRemove, skillHandler);
            }
            else
            {
                throw new KeyNotFoundException($"Skill '{skillName}' not found in character '{Name}'.");
            }
        }

        /// <summary>
        /// Remove all synergies related to the skill if no other skill is using it.
        /// </summary>
        /// <param name="skillName"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void CleanSyngergies(SkillHandler.SkillConfig skillConfig, SkillHandler skillHandler)
        {
            if (skillHandler.TryGetSkill(skillConfig.Name, out var skill))
            {
                HashSet<string> uniqueSynergies = FindUniqueSynergies(skillConfig, skillHandler, skill.Synergies);
                foreach (var synergy in uniqueSynergies)
                {
                    RemoveSynergySkill(synergy, skillHandler);
                }
            }
        }

        /// <summary>
        /// Gets the synergy skill which are not used by any other skill.
        /// </summary>
        /// <param name="skillConfig"></param>
        /// <param name="skillHandler"></param>
        /// <param name="ownSynergies"></param>
        /// <returns></returns>
        private HashSet<string> FindUniqueSynergies(SkillHandler.SkillConfig skillConfig, SkillHandler skillHandler, HashSet<string> ownSynergies)
        {
            HashSet<string> ownSynergiesCpy = new(ownSynergies);
            HashSet<string> otherSynergies = new();
            // go through each other skill and find their synergies
            foreach (var charSkill in Skills)
            {
                if (charSkill.Name == skillConfig.Name) continue;
                if (charSkill.TotalPoints <= 0) continue; // skip pure synergy skills

                HashSet<string> skillSynergies = skillHandler.GetSkill(charSkill.Name).Synergies;
                if (skillSynergies is null) continue;
                otherSynergies.UnionWith(skillSynergies);
            }

            ownSynergiesCpy.RemoveWhere(s => otherSynergies.Contains(s));

            return ownSynergiesCpy;
        }

        private void RemoveSynergySkill(string synergySkillName, SkillHandler skillHandler)
        {
            var synergySkill = Skills.FirstOrDefault(s => s.Name == synergySkillName);
            // don't remove synergy skill if it is used itself as a main skill
            if (synergySkill.TotalPoints == 0)
            {
                // synergy skill is not used as a main skill
                Skills.Remove(synergySkill);
            }
            else
            {
                // also used a main skill, so just reset synergy points
                synergySkill.HardPoints = 0;
            }
        }

        public HashSet<string> GetSynergies(SkillHandler skillHandler)
        {
            HashSet<string> synergies = new();
            foreach (var skill in Skills)
            {
                if (skill.TotalPoints <= 0) continue; // skip pure synergy skills
                if (skillHandler.TryGetSkill(skill.Name, out var skillEntry))
                {
                    synergies.UnionWith(skillEntry.Synergies);
                }
            }
            return synergies;
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
                AddSkill(new SkillHandler.SkillConfig() { Name = skillName, HardPoints = hardPoints, TotalPoints = 0, HotKey = System.Windows.Input.Key.None });
            }
        }

        internal void SetHotkey(string skillName, Key key)
        {
            var skillEntry = Skills.Find(s => s.Name == skillName);
            skillEntry.HotKey = key;
        }
    }
}
