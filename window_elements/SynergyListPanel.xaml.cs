using CtATracker.characters;
using CtATracker.secondary_windows;
using CtATracker.skills;
using CtATracker.UI_element_prefabs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static CtATracker.skills.SkillHandler;

namespace CtATracker.window_elements
{
    /// <summary>
    /// Interaction logic for SynergyListPanel.xaml
    /// </summary>
    public partial class SynergyListPanel : UserControl
    {
        private SkillHandler _skillHandler;
        private CharacterHandler _characterHandler;

        public SynergyListPanel()
        {
            InitializeComponent();
        }

        private void _characterHandler_OnCharacterSelected(CharacterEntry character)
        {
            ShowSynergiesForChar(character);
        }

        public void LinkHandlers(SkillHandler skillHandler, CharacterHandler characterHandler)
        {
            _skillHandler = skillHandler;
            _characterHandler = characterHandler;
            _characterHandler.OnSkillAddedOrRemoved += () =>
            {
                if (_characterHandler.CurrentChar != null)
                    _characterHandler_OnCharacterSelected(_characterHandler.CurrentChar);
            };
            _characterHandler.OnCharacterSelected += _characterHandler_OnCharacterSelected;
        }


        internal void ShowSynergiesForChar(CharacterEntry? currentChar)
        {
            if (currentChar is null)
            {
                SynergyPanel.Children.Clear();
                return;
            }
            HashSet<string> synergies = currentChar.GetSynergies(_skillHandler);
            SynergyPanel.Children.Clear();

            List<SkillConfig> synergiesToList = new();
            foreach (string synergyName in synergies)
            {
                SkillConfig entry = GetOrCreatePlaceholder(currentChar, synergyName);
                AddSynergySkillToUI(entry);
            }
        }

        private static SkillConfig GetOrCreatePlaceholder(CharacterEntry currentChar, string skillName)
        {
            return currentChar.Skills.Find(s => s.Name == skillName)
                ?? new SkillConfig { Name = skillName, HardPoints = 0 };
        }

        private void AddSynergySkillToUI(SkillConfig synergySkill)
        {
            SynergySkill synergyEntry = new SynergySkill();
            synergyEntry.LinkHandler(_characterHandler);
            synergyEntry.SkillName = synergySkill.Name;
            synergyEntry.SkillLevel = synergySkill.HardPoints.ToString();
            SynergyPanel.Children.Add(synergyEntry);
        }
    }
}
