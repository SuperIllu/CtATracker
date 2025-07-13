using CtATracker.characters;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CtATracker.UI_element_prefabs
{
    /// <summary>
    /// Prefab for a skill entry in the main window.
    /// </summary>
    public partial class SkillEntryControl : UserControl
    {
        private static bool _isCapturingKey;
        private static SkillEntryControl? _listeningButton;
        private Key _lastCapturedKey;

        public event Action<string, Key> OnHotKeySelected;
        private Action<string> _removeSkillCallback;
        private Action _onStartListening;
        private Action _onStopListening;
        private CharacterHandler _characterHandler;

        public SkillEntryControl()
        {
            InitializeComponent();
        }

        public void LinkSkillRemovalCallback(Action<string> callback)
        {
            _removeSkillCallback = callback;
        }

        public void LinkHandlers(CharacterHandler characterHandler)
        {
            _characterHandler = characterHandler;
        }

        public string SkillName
        {
            get => SkillNameBlock.Text;
            set => SkillNameBlock.Text = value;
        }

        public string SkillLevel
        {
            get => SkillLevelBox.Text;
            set => SkillLevelBox.Text = value;
        }

        /// <summary>
        /// Want to disable some buttons while capturing the key?
        /// </summary>
        /// <param name="onstartListening"></param>
        /// <param name="onStopListening"></param>
        public void LinkListeningCallbacks(Action onstartListening, Action onStopListening)
        {
            _onStartListening = onstartListening;
            _onStopListening = onStopListening;
        }


        private void RemoveSkill_Click(object sender, RoutedEventArgs e)
        {
            _removeSkillCallback?.Invoke(SkillName);
        }

        private void CaptureHotKey_Click(object sender, RoutedEventArgs e)
        {
            if (_listeningButton != null)
            {
                if (_listeningButton != this)
                {
                    _listeningButton.Focus();
                }
                return;
            }

            _onStartListening?.Invoke();
            _listeningButton = this;
            KeyButton.Content = "Press hotkey";
            KeyButton.Background = Brushes.Yellow;

            // Set keyboard focus to the window to ensure key events are captured
            this.Focusable = true;
            this.Focus();

            // Hook into the PreviewKeyDown event temporarily
            this.PreviewKeyDown += Window_PreviewKeyDown;
            DeleteButton.IsEnabled = false; // Disable the delete button while capturing key
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_listeningButton == null)
                return;

            _listeningButton = null;
            this.PreviewKeyDown -= Window_PreviewKeyDown; // Remove the handler
            DeleteButton.IsEnabled = true; // Re-enable the delete button

            if (e.Key == Key.Escape)
            {
                // Cancel
                KeyButton.Content = "Key: --";
            }
            else
            {
                // Save the key (store however you like)
                string keyName = e.Key.ToString();
                KeyButton.Content = $"K: {keyName}";

                // Optionally: store this key in a variable or property
                _lastCapturedKey = e.Key;
                OnHotKeySelected?.Invoke(SkillName, e.Key);
            }

            // Reset button highlight
            KeyButton.Background = Brushes.LightGray;

            // Prevent further handling if needed
            e.Handled = true;

            _onStopListening?.Invoke();
        }


        #region validating input to be only numbers
        private static readonly Regex _intRegex = new Regex("^[0-9]+$"); // Only digits

        private void NumberOnlyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_intRegex.IsMatch(e.Text);
        }

        private void NumberOnlyTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = e.DataObject.GetData(DataFormats.Text) as string;
                if (!_intRegex.IsMatch(text))
                {
                    e.Handled = true;
                    e.CancelCommand();
                }
            }
            else
            {
                e.Handled = true;
                e.CancelCommand();
            }


        }

        private void NumberOnlyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Optional: block spacebar
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void NumberOnlyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string newText = textBox.Text;

            if (string.IsNullOrEmpty(newText))
            {
                textBox.Text = "1";
            }

            // If it's non-empty and valid, do something:
            if (int.TryParse(newText, out int skillLevel))
            {
                _characterHandler.CurrentChar?.SetTotalSkillPoints(SkillName, skillLevel);
                _characterHandler.SkillLevelUpdated();
                System.Diagnostics.Debug.WriteLine("Triggering update");
            }
            else
            {
                Debug.WriteLine("Invalid input (shouldn't happen if filtering works).");
            }
        }

        internal void SetHotKey(Key hotKey)
        {
            KeyButton.Content = hotKey == Key.None ? "Key: --" : $"K: {hotKey}";
            _lastCapturedKey = hotKey;
        }


        #endregion validating input to be only numbers
    }
}