using System.Windows;
using System.Windows.Controls;

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
    }
}