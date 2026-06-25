using CtATracker.characters;
using CtATracker.characters.serialisers;
using CtATracker.secondary_windows;
using CtATracker.window_elements;
using CtATracker.skills;
using CtATracker.skills.serialisers;
using System.Linq;
using System.Windows;
using System.Windows.Input;


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
            _characterHandler = new CharacterHandler(new CharacterFileHandler(CharacterFileName));
            try
            {
                _skillHandler = new SkillHandler(new SkillFileHandler(SkillFileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load skills: {ex.Message}\n\nThe application will now close.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            InitializeComponent();

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

            var keyDupes = _characterHandler.CurrentChar.Skills
                .Where(s => s.HotKey != Key.None && s.TotalPoints > 0)
                .GroupBy(s => s.HotKey)
                .Where(g => g.Count() > 1)
                .ToList();
            if (keyDupes.Any())
            {
                var msg = string.Join("\n", keyDupes.Select(g => $"  {g.Key}: {string.Join(", ", g.Select(s => s.Name))}"));
                MessageBox.Show($"Duplicate hotkeys found:\n{msg}\n\nFix them before starting the overlay.", "Hotkey Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gpDupes = _characterHandler.CurrentChar.Skills
                .Where(s => s.GamepadButton != GamepadButton.None && s.TotalPoints > 0)
                .GroupBy(s => s.GamepadButton)
                .Where(g => g.Count() > 1)
                .ToList();
            if (gpDupes.Any())
            {
                var msg = string.Join("\n", gpDupes.Select(g => $"  {g.Key}: {string.Join(", ", g.Select(s => s.Name))}"));
                MessageBox.Show($"Duplicate gamepad buttons found:\n{msg}\n\nFix them before starting the overlay.", "Gamepad Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                _overlayWindow = new SummaryWindow(_characterHandler.CurrentChar, _skillHandler, _characterHandler.CurrentControlScheme);
                _overlayWindow.Owner = this;
                _overlayWindow.Show();
                StartOverlayButton.Content = "Stop overlay";
            }

        }


        private void InputMode_Changed(object sender, RoutedEventArgs e)
        {
            _characterHandler.CurrentControlScheme = KeyboardModeRadio.IsChecked == true
                ? ControlScheme.Keyboard
                : ControlScheme.Controller;
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