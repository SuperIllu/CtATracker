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
using System.Windows.Shapes;

namespace CtATracker
{
    /// <summary>
    /// Shows the selection of available skills and allows the user to input a value.
    /// </summary>
    public partial class NewSkillInput : Window
    {
        private Action<string, int> _callback;
        private Action _cancelCallback;

        public string SelectedItem => ComboBoxItems.SelectedItem?.ToString();
        public string InputValue => InputTextBox.Text;

        public NewSkillInput(List<string> enabledItems, List<string> disabledItems = null)
        {
            InitializeComponent();

            // Add enabled items
            foreach (var item in enabledItems)
            {
                ComboBoxItems.Items.Add(new ComboBoxItem
                {
                    Content = item,
                    IsEnabled = true
                });
            }

            // Add disabled (grayed out) items if provided
            if (disabledItems != null)
            {
                foreach (var item in disabledItems)
                {
                    ComboBoxItems.Items.Add(new ComboBoxItem
                    {
                        Content = item,
                        IsEnabled = false,
                        Foreground = SystemColors.GrayTextBrush
                    });
                }
            }

            ComboBoxItems.SelectedIndex = 0; // Optional default selection
        }

        public void LinkSelectedCallback(Action<string, int> callback, Action cancelCallback)
        {
            _callback = callback;
            _cancelCallback = cancelCallback;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(InputTextBox.Text, out int inputValue))
            {
                return; // Handle invalid input gracefully
            }
            if (ComboBoxItems.SelectedItem is not ComboBoxItem cbi)
            {
                return;
            }

            _callback?.Invoke(cbi.Content.ToString(), inputValue);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancelCallback?.Invoke();
            this.Close();
        }
    }
}

