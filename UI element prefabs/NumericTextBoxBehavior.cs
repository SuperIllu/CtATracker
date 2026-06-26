using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace CtATracker.UI_element_prefabs
{
    internal static class NumericTextBoxBehavior
    {
        private static readonly Regex _intRegex = new("^[0-9]+$");

        public static bool IsTextValid(string text)
        {
            return text != null && _intRegex.IsMatch(text);
        }

        public static void HandlePasting(DataObjectPastingEventArgs e)
        {
            bool valid = e.DataObject.GetDataPresent(DataFormats.Text)
                && e.DataObject.GetData(DataFormats.Text) is string text
                && _intRegex.IsMatch(text);

            if (!valid)
            {
                e.Handled = true;
                e.CancelCommand();
            }
        }

        public static bool ShouldBlockKey(Key key)
        {
            return key == Key.Space;
        }
    }
}
