using CtATracker.characters;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CtATracker.UI_element_prefabs
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

        public void LinkHandler(CharacterHandler characterHandler)
        {
            _characterHandler = characterHandler;
        }


        #region validating input to be only numbers
        private static readonly Regex _intRegex = new Regex("^[0-9]+$"); // Only digits
        private CharacterHandler _characterHandler;

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
                textBox.Text = "0";
            }

            // If it's non-empty and valid, do something:
            if (int.TryParse(newText, out int skillLevel))
            {
                _characterHandler.CurrentChar?.SetHardSkillPoints(SkillName, skillLevel);
                _characterHandler.SkillLevelUpdated();
                System.Diagnostics.Debug.WriteLine("Triggering update");
            }
            else
            {
                Debug.WriteLine("Invalid input (shouldn't happen if filtering works).");
            }
        }


        #endregion validating input to be only numbers
    }
}