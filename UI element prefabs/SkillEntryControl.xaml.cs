using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CtATracker
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

        public SkillEntryControl()
        {
            InitializeComponent();
        }

        public void LinkSkillRemovalCallback(Action<string> callback)
        {
            _removeSkillCallback = callback;
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

            _listeningButton = this;
            KeyButton.Content = "Press any key...";
            KeyButton.Background = Brushes.Yellow;

            // Set keyboard focus to the window to ensure key events are captured
            this.Focusable = true;
            this.Focus();

            // Hook into the PreviewKeyDown event temporarily
            this.PreviewKeyDown += Window_PreviewKeyDown;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_listeningButton == null)
                return;

            _listeningButton = null;
            this.PreviewKeyDown -= Window_PreviewKeyDown; // Remove the handler

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
        }

    }
}