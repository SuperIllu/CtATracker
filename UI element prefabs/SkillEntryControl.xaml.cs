using CtATracker.characters;
using CtATracker.skills;
using CtATracker.window_elements;
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
        private static SkillEntryControl? _listeningButton;
        private Key _lastCapturedKey;
        private GamepadButton _lastCapturedGamepadButton;
        private readonly Brush _defaultBg;

        public event Action<string, Key> OnKeyboardHotkeySelected;
        public event Action<string, GamepadButton> OnGamepadButtonSelected;
        private Action<string> _removeSkillCallback;
        private Action _onStartListening;
        private Action _onStopListening;
        private CharacterHandler _characterHandler;

        public SkillEntryControl()
        {
            InitializeComponent();
            _defaultBg = KeyButton.Background;
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
            // re-focus on listening button if another one is pressed
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

        private void CaptureGamepad_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_listeningButton == null)
                return;

            _listeningButton = null;
            this.PreviewKeyDown -= Window_PreviewKeyDown;
            DeleteButton.IsEnabled = true;

            Key capturedKey = e.Key;
            // handle OS level interceptions, e.g. F10
            if (e.Key == Key.System)
            {
                capturedKey = e.SystemKey;
            }

            if (capturedKey == Key.Escape)
            {
                KeyButton.Content = _lastCapturedKey == Key.None ? "Key: --" : $"K: {_lastCapturedKey}";
                KeyButton.Background = Brushes.LightGray;
            }
            else
            {
                bool isDuplicate = _characterHandler?.CurrentChar?.Skills
                    .Any(s => s.Name != SkillName && s.HotKey == capturedKey) ?? false;

                if (isDuplicate)
                {
                    KeyButton.Content = _lastCapturedKey == Key.None ? "Key: --" : $"K: {_lastCapturedKey}";
                    KeyButton.Background = Brushes.Red;
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(500);
                        Dispatcher.Invoke(() => KeyButton.Background = _defaultBg);
                    });
                }
                else
                {
                    string keyName = capturedKey.ToString();
                    KeyButton.Content = $"K: {keyName}";
                    _lastCapturedKey = capturedKey;
                    OnKeyboardHotkeySelected?.Invoke(SkillName, capturedKey);
                    KeyButton.Background = Brushes.LightGray;
                    Debug.WriteLine($"Key captured: {capturedKey}");
                }
            }

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

        internal void SetKeyboardHotKey(Key hotKey)
        {
            KeyButton.Content = hotKey == Key.None ? "Key: --" : $"K: {hotKey}";
            _lastCapturedKey = hotKey;
        }

        internal void SetGamepadButton(GamepadButton button)
        {
            PadButton.Content = button == GamepadButton.None ? "Pad: --" : $"P: {button}";
            _lastCapturedGamepadButton = button;
        }

        public void SetControlScheme(ControlScheme scheme)
        {
            KeyButton.Visibility = scheme == ControlScheme.Keyboard ? Visibility.Visible : Visibility.Collapsed;
            PadButton.Visibility = scheme == ControlScheme.Controller ? Visibility.Visible : Visibility.Collapsed;
        }


        #endregion validating input to be only numbers
    }
}