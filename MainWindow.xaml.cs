using System.Text;
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

namespace CtATracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NewSkillInput? _newSkillWindow;
        private Skills _skillHandler;
        private CharacterHandler _characterHandler;


        public MainWindow()
        {
            InitializeComponent();
            _skillHandler = new Skills();
            _characterHandler = new CharacterHandler(CharacterSelected);
            CheckForCharacter();
        }

        private void CheckForCharacter()
        {
            bool charIsAvaliable = _characterHandler.CurrentChar != null;
            AddSkillButton.IsEnabled = charIsAvaliable;
            DeleteCharButton.IsEnabled = charIsAvaliable;
        }

        private void DeletePreset_Click(object sender, RoutedEventArgs e) { }
        private void UpdateConfig_Click(object sender, RoutedEventArgs e) { }
        private void NewChar_Click(object sender, RoutedEventArgs e)
        {
            string newCharName = SavePresetNameBox.Text;
            if (string.IsNullOrWhiteSpace(newCharName))
            {
                MessageBox.Show("Please enter a character name.");
                return;
            }

            _characterHandler.AddNewCharacter(newCharName);
            _characterHandler.SetCurrentCharacter(newCharName);

            PresetComboBox.Items.Clear();
            foreach (var character in _characterHandler.GetCharacterList())
            {
                PresetComboBox.Items.Add(character);
            }
            PresetComboBox.SelectedItem = newCharName;
        }

        /// <summary>
        /// Callback to update the UI when a character is selected.
        /// </summary>
        /// <param name="character"></param>
        private void CharacterSelected(CharacterEntry character)
        {
            CheckForCharacter();

            if (_characterHandler.CurrentChar is null) return;

            UpdateSkillList();

        }

        private void AddSynergySkillToUI(SkillConfig synergySkill)
        {
            throw new NotImplementedException();
        }

        private void AddSkillToUI(SkillConfig skill)
        {
            if (skill.TotalPoints <= 0) return;
            var newSkillUIEntry = new SkillEntryControl();
            newSkillUIEntry.SkillName = skill.Name;
            newSkillUIEntry.SkillLevel = skill.TotalPoints.ToString();
            // TODO load key binding
            SkillsPanel.Children.Add(newSkillUIEntry);
        }

        private void SetKey1_Click(object sender, RoutedEventArgs e) { }
        private void SetKey2_Click(object sender, RoutedEventArgs e) { }

        private void AddSkill_Click(object sender, RoutedEventArgs e)
        {
            if (_newSkillWindow != null) return;
            if (_characterHandler.CurrentChar == null) return;
            
            List<string> availableSkills = _skillHandler.GetAllSkills();
            List<string> alreadyKnownSkills = _characterHandler.CurrentChar.Skills.Select(skill => skill.Name).ToList();

            // Filter out already known skills from available skills
            availableSkills = availableSkills.Where(skill => !alreadyKnownSkills.Contains(skill)).ToList();
            // show the unavailable skills in gray
            alreadyKnownSkills = alreadyKnownSkills.Where(skill => !availableSkills.Contains(skill)).ToList();

            _newSkillWindow = new NewSkillInput(availableSkills, alreadyKnownSkills);
            _newSkillWindow.LinkSelectedCallback(NewSkillCallback, UnlockUi);
            _newSkillWindow.Owner = this;
            _newSkillWindow.Show();

            //disable other buttons while the new skill window is open
            SaveChangesButton.IsEnabled = false;
            PresetComboBox.IsEnabled = false;
            SavePresetNameBox.IsEnabled = false;

        }

        private void NewSkillCallback(string skillName, int skillLvl)
        {
            if (_characterHandler.CurrentChar == null)
                            {
                MessageBox.Show("No character selected. Please create or select a character first.");
                return;
            }

            
            Console.WriteLine($"New skill selected: {skillName} with level {skillLvl}");
            _characterHandler.CurrentChar.AddSkill(new Skills.SkillConfig() { Name = skillName, TotalPoints = skillLvl, HardPoints = 0 });

            UpdateSkillList();

            UnlockUi();
        }

        private void UpdateSkillList()
        {
            HashSet<string> synergies = new HashSet<string>();
            SkillsPanel.Children.Clear();

            foreach (var skill in _characterHandler.CurrentChar.Skills)
            {
                AddSkillToUI(skill);
                synergies.UnionWith(_skillHandler.GetSkill(skill.Name).Synergies);
            }


            List<SkillConfig> filteredAndSortedSkills = _characterHandler.CurrentChar.Skills
                .Where(skill => synergies.Contains(skill.Name))
                .OrderByDescending(skill => skill.HardPoints)
                .ToList();

            SynergyPanel.Children.Clear();
            foreach (var synergySkill in filteredAndSortedSkills)
            {
                AddSynergySkillToUI(synergySkill);
            }
        }

        private void UnlockUi()
        {
            // re-enable buttons after the new skill window is closed
            SaveChangesButton.IsEnabled = true;
            PresetComboBox.IsEnabled = true;
            SavePresetNameBox.IsEnabled = true;
            _newSkillWindow = null;
        }


        private void DeleteSkillEntry(object sender, RoutedEventArgs e)
        {
            // Logic to delete a skill entry
            Button button = sender as Button;
            if (button != null)
            {
                StackPanel parentPanel = button.Parent as StackPanel;
                //TODO delete entry
            }
        }
    }
}