using System.Windows;
using System.Windows.Controls;

namespace CtATracker
{
    /// <summary>
    /// Prefab for a skill entry in the main window.
    /// </summary>
    public partial class SkillEntryControl : UserControl
    {
        public SkillEntryControl()
        {
            InitializeComponent();
            DeleteButton.Click += (s, e) => RaiseDeleteRequested();
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

        public event RoutedEventHandler DeleteRequested;

        private void RaiseDeleteRequested()
        {
            DeleteRequested?.Invoke(this, new RoutedEventArgs());
        }

        public Button KeyButtonControl => KeyButton;
    }
}