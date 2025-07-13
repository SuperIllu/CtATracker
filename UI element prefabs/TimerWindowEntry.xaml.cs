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

namespace CtATracker.UI_element_prefabs
{
    /// <summary>
    /// Interaction logic for TimerWindowEntry.xaml
    /// </summary>
    public partial class TimerWindowEntry : UserControl
    {
        private string _skillName;

        public TimerWindowEntry()
        {
            InitializeComponent();
        }

        public void SetSkillName(string skillName)
        {
            _skillName = skillName;
        }

        public void SetTimeText(string timeText, int percentage)
        {
            SkillTime_text.Text = $"{_skillName}: {timeText}";
            SkillTime_bar.Value = percentage;
        }

        internal void SetColour(Color color)
        {
            SkillTime_bar.Foreground = new SolidColorBrush(color);
        }
    }
}
