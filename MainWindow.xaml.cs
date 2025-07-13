using CtATracker.characters;
using CtATracker.characters.serialisers;
using CtATracker.secondary_windows;
using CtATracker.window_elements;
using CtATracker.skills;
using CtATracker.skills.serialisers;
using System.Windows;


namespace CtATracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string CharacterFileName = "Characters.yml";
        public const string SkillFileName = "skills.yml";

        private SkillHandler _skillHandler;
        private CharacterHandler _characterHandler;
        private SummaryWindow? _overlayWindow;

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
            // enable / disable sub panels based on character availability
            bool charIsAvaliable = _characterHandler.CurrentChar != null;
            StartOverlayButton.IsEnabled = charIsAvaliable;
        }

        private void StartOverlay_Click(object sender, RoutedEventArgs e) 
        {
            if (_characterHandler.CurrentChar == null) return;

            if (_overlayWindow != null)
            {
                // overlay already running - stop it
                _overlayWindow.Close();
                _overlayWindow = null;
                StartOverlayButton.Content = "Start overlay";
            }
            else
            {
                // start overlay
                _overlayWindow = new SummaryWindow(_characterHandler.CurrentChar, _skillHandler);
                _overlayWindow.Owner = this;
                _overlayWindow.Show();
                StartOverlayButton.Content = "Stop overlay";
            }

        }


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
    }
}