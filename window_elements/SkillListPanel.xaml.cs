using CtATracker.characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for SkillPanel.xaml
    /// </summary>
    public partial class SkillListPanel : UserControl
    {
        private Skills _skillHandler;
        private CharacterHandler _characterHandler;

        public SkillListPanel()
        {
            InitializeComponent();
        }

        internal void LinkHandler(Skills skillHandler, CharacterHandler charHandler)
        {
            _skillHandler = skillHandler;
            _characterHandler = charHandler;
        }




        internal void SelectCharacter(CharacterEntry? currentChar)
        {
            SkillsPanel.Children.Clear();

            if (currentChar == null) return;

            foreach (var skill in currentChar.Skills)
            {
                AddSkillToUI(skill);
            }


            UpdateSkillSelection();
        }


        private void AddSkillToUI(SkillConfig skill)
        {
            if (skill.TotalPoints <= 0) return;
            var newSkillUIEntry = new SkillEntryControl();
            newSkillUIEntry.LinkHandlers(_characterHandler);
            newSkillUIEntry.SkillName = skill.Name;
            newSkillUIEntry.SkillLevel = skill.TotalPoints.ToString();
            newSkillUIEntry.LinkSkillRemovalCallback((skillName) =>
            {
                _characterHandler.CurrentChar?.RemoveSkill(skillName);
                SelectCharacter(_characterHandler.CurrentChar);
                _characterHandler.SkillsUpdated();
            });
            // TODO load key binding
            SkillsPanel.Children.Add(newSkillUIEntry);
        }

        private void AddSkill_Click(object sender, RoutedEventArgs e)
        {

            if (SkillComboBox.SelectedItem is not ComboBoxItem cbi) return;
            if (!cbi.IsEnabled) return;

            string? skillName = cbi.Content.ToString();
            if (skillName is null)
            {
                return; // Handle null skill name gracefully
            }
            var currentChar = _characterHandler.CurrentChar;
            if (currentChar == null)
            {
                return;
            }
            currentChar.AddSkill(new SkillConfig() { Name = skillName, HardPoints = 0, TotalPoints = 1 });
            _characterHandler.SkillsUpdated();

            //update ui after adding the skill
            SelectCharacter(currentChar);
            UpdateSkillSelection();

        }

        private void UpdateSkillSelection()
        {
            List<string> availableSkills = _skillHandler.GetAllSkills();
            List<string> alreadyKnownSkills = _characterHandler.CurrentChar.Skills.Select(skill => skill.Name).ToList();

            // Filter out already known skills from available skills
            availableSkills = availableSkills.Where(skill => !alreadyKnownSkills.Contains(skill)).ToList();
            // show the unavailable skills in gray
            alreadyKnownSkills = alreadyKnownSkills.Where(skill => !availableSkills.Contains(skill)).ToList();

            UpdateSkillSelection(availableSkills, alreadyKnownSkills);
        }

        private void UpdateSkillSelection(List<string> enabledItems, List<string> disabledItems = null)
        {
            SkillComboBox.Items.Clear(); // Clear existing items
            // Add enabled items
            foreach (var item in enabledItems)
            {
                SkillComboBox.Items.Add(new ComboBoxItem
                {
                    Content = item,
                    IsEnabled = true
                });
            }

            // Add disabled (grayed out) items if provided
            if (disabledItems != null)
            {
                foreach (var item in disabledItems)
                {
                    SkillComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = item,
                        IsEnabled = false,
                        Foreground = SystemColors.GrayTextBrush
                    });
                }
            }

            SkillComboBox.SelectedIndex = 0; // Optional default selection
        }
    }
}
