using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CtATracker
{
    /// <summary>
    /// Prefab for a skill entry in the main window.
    /// </summary>
    public partial class SynergySkill : UserControl
    {
        public SynergySkill()
        {
            InitializeComponent();
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
                    e.CancelCommand();
                }
            }
            else
            {
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

        #endregion validating input to be only numbers
    }
}