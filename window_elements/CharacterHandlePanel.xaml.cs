using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CtATracker.window_elements
{
    /// <summary>
    /// Interaction logic for CharacterHandlePanel.xaml
    /// </summary>
    public partial class CharacterHandlePanel : UserControl
    {
        private CharacterHandler _characterHandler;

        public CharacterHandlePanel()
        {
            InitializeComponent();
        }

        public void Initialize(CharacterHandler characterHandler)
        {
            _characterHandler = characterHandler;
            PresetComboBox.Items.Clear();
            foreach (var character in _characterHandler.GetCharacterList())
            {
                PresetComboBox.Items.Add(character);
            }
            if (_characterHandler.CurrentChar != null)
            {
                PresetComboBox.SelectedItem = _characterHandler.CurrentChar.Name;
            }
        }

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

        private void DeleteChar_Click(object sender, RoutedEventArgs e)
        {
            if (_characterHandler.CurrentChar == null)
            {
                return;
            }
            string charName = _characterHandler.CurrentChar.Name;
            if (MessageBox.Show($"Are you sure you want to delete the character '{charName}'?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _characterHandler.DeleteCurrentCharacter();
                PresetComboBox.Items.Remove(charName);
                PresetComboBox.SelectedIndex = -1;
            }
        }

        private void CharSelected(object sender, SelectionChangedEventArgs e)
        {
            if (PresetComboBox.SelectedItem is string selectedCharName)
            {
                _characterHandler.SetCurrentCharacter(selectedCharName);
                // TODO update sub panels with the current character's skills
            }
        }
    }
}
