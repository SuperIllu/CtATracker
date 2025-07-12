using CtATracker.characters;
using CtATracker.characters.serialisers;
using CtATracker.skills;
using CtATracker.skills.serialisers;
using CtATracker.window_elements;
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
using static CtATracker.skills.SkillHandler;

namespace CtATracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string CharacterFileName = "Characters.yml";
        public const string SkillFileName = "skills.yml";

        private NewSkillInput? _newSkillWindow;
        private SkillHandler _skillHandler;
        private CharacterHandler _characterHandler;


        public MainWindow()
        {
            InitializeComponent();
            _skillHandler = new SkillHandler(new SkillFileHandler(SkillFileName));
            _characterHandler = new CharacterHandler(new CharacterFileHandler(CharacterFileName));

            _characterHandler.OnCharacterSelected += CharacterSelected;
            SkillList.LinkHandler(_skillHandler, _characterHandler);
            SynergyList.LinkHandlers(_skillHandler, _characterHandler);
            CharacterSelection.Initialize(_characterHandler);
            CheckForCharacter();
        }

        private void CheckForCharacter()
        {
            bool charIsAvaliable = _characterHandler.CurrentChar != null;
            // TODO enable / disable sub panels based on character availability
        }

        private void DeletePreset_Click(object sender, RoutedEventArgs e) { }
        private void UpdateConfig_Click(object sender, RoutedEventArgs e) { }


        /// <summary>
        /// Callback to update the UI when a character is selected.
        /// </summary>
        /// <param name="character"></param>
        private void CharacterSelected(CharacterEntry character)
        {
            CheckForCharacter();

            if (_characterHandler.CurrentChar is null) return;

            SkillList.SelectCharacter(_characterHandler.CurrentChar);

        }

        private void AddSynergySkillToUI(SkillConfig synergySkill)
        {
            throw new NotImplementedException();
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
            // TODO disable sub panels until skill window is closed
            //PresetComboBox.IsEnabled = false;
            //SavePresetNameBox.IsEnabled = false;

        }

        private void NewSkillCallback(string skillName, int skillLvl)
        {
            if (_characterHandler.CurrentChar == null)
                            {
                MessageBox.Show("No character selected. Please create or select a character first.");
                return;
            }

            
            Console.WriteLine($"New skill selected: {skillName} with level {skillLvl}");
            _characterHandler.CurrentChar.AddSkill(new SkillHandler.SkillConfig() { Name = skillName, TotalPoints = skillLvl, HardPoints = 0, HotKey=Key.None });

            // TODO call update sskill list in sub panel
            SkillList.SelectCharacter(_characterHandler.CurrentChar);
            SynergyList.ShowSynergiesForChar(_characterHandler.CurrentChar);

            UnlockUi();
        }



        private void UnlockUi()
        {
            // re-enable buttons after the new skill window is closed
            SaveChangesButton.IsEnabled = true;
            //PresetComboBox.IsEnabled = true;
            //SavePresetNameBox.IsEnabled = true;
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