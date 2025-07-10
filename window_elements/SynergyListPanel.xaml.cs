using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static CtATracker.Skills;

namespace CtATracker.window_elements
{
    /// <summary>
    /// Interaction logic for SynergyListPanel.xaml
    /// </summary>
    public partial class SynergyListPanel : UserControl
    {
        private Skills _skillHandler;

        public SynergyListPanel()
        {
            InitializeComponent();
        }

        public void LinkSkillHandler(Skills skillHandler)
        {
            _skillHandler = skillHandler;
        }



        public void UpdateSynergyList(CharacterEntry characterEntry, IEnumerable<string> synergies)
        {
            List<SkillConfig> filteredAndSortedSkills = characterEntry.Skills
    .Where(skill => synergies.Contains(skill.Name))
    .OrderByDescending(skill => skill.HardPoints)
    .ToList();

            SynergyPanel.Children.Clear();
            foreach (var synergySkill in filteredAndSortedSkills)
            {
                AddSynergySkillToUI(synergySkill);
            }
        }

        internal void ShowSynergiesForChar(CharacterEntry currentChar)
        {
            HashSet<string> synergies = new HashSet<string>();
            foreach (var skill in currentChar.Skills)
            {
                synergies.UnionWith(_skillHandler.GetSkill(skill.Name).Synergies);
            }

            SynergyPanel.Children.Clear();

            List<SkillConfig> synergiesToList = new();
            foreach (string synergyName in synergies)
            {
                if (_skillHandler.TryGetSkill(synergyName, out Skill synergySkill))
                {
                    // the synergy skill is a normal skill 
                    SkillConfig? potentialSkill = currentChar.Skills.Find(s => s.Name == synergyName);
                    if (potentialSkill is null)
                    {
                        // char doesn't have this skill - use temporary
                        synergiesToList.Add(new SkillConfig
                        {
                            Name = synergyName,
                            HardPoints = 0 // or some default value, if applicable
                        });
                        continue;
                    }
                    synergiesToList.Add(potentialSkill);
                }
                else
                {
                    // this is a pure synergy skill - create a placeholder
                    synergiesToList.Add(new SkillConfig
                    {
                        Name = synergyName,
                        HardPoints = 0 // or some default value, if applicable
                    });
                }


            }

            foreach (var synergySkill in synergiesToList)
            {
                AddSynergySkillToUI(synergySkill);
            }
        }

        private void AddSynergySkillToUI(SkillConfig synergySkill)
        {
            SynergySkill synergyEntry = new SynergySkill();
            synergyEntry.SkillName = synergySkill.Name;
            synergyEntry.SkillLevel = synergySkill.HardPoints.ToString();
            SynergyPanel.Children.Add(synergyEntry);
        }
    }
}
