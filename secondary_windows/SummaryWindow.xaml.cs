using CtATracker.characters;
using CtATracker.Utilities;
using CtATracker.skills;
using CtATracker.skills.serialisers;
using CtATracker.UI_element_prefabs;
using Gma.System.MouseKeyHook;
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
using System.Windows.Threading;


namespace CtATracker.secondary_windows
{
    /// <summary>
    /// Interaction logic for SummaryWindow.xaml
    /// </summary>
    public partial class SummaryWindow : Window
    {
        public const float SkillShrineDuration = 10f;

        private class Times
        {
            public float CurrentTime;
            public float MaxTime;

            public override string ToString()
            {
                return $"{CurrentTime}/{MaxTime}";
            }
        }

        public const float TimerResolution = 0.2f;
        private IKeyboardMouseEvents _keyboardEvents;

        private bool _isListening = false;
        private bool _keysLoaded = false;

        private DispatcherTimer _timer;
        private float _remainingBattleOrderTime;
        private float _remainingBattleCommandsTime;

        private Dictionary<Key, SkillHandler.SkillConfig> _skillKeyBindings;
        private SkillHandler _skillHandler;
        private CharacterEntry _character;


        private ColourPalette colourPalette = new ColourPalette();
        private Dictionary<SkillHandler.SkillConfig, Times> _skillTimes;
        private Dictionary<SkillHandler.SkillConfig, TimerWindowEntry> _skillUIElements;
        private bool _skillShrineActive;

        // gives you a bonus point
        private const string BattleCommandsSkillName = "BattleCommand";

        //private Skills.Levels _levels;


        public SummaryWindow(CharacterEntry character, SkillHandler skillHandler)
        {
            InitializeComponent();
            _keyboardEvents = Hook.GlobalEvents();
            _keyboardEvents.KeyDown += OnKeyDown;

            _skillHandler = skillHandler;
            _character = character;
            SetupKeys(character);
            InitialiseTimers();
            GenerateUIElements();

            _isListening = true;
            SetListenging(true);
            CreateTimer();

        }

        private void GenerateUIElements()
        {
            _skillUIElements = new Dictionary<SkillHandler.SkillConfig, TimerWindowEntry>();
            TimerEntriesPanel.Children.Clear(); // Clear existing entries
            foreach (var skill in _skillKeyBindings.Values)
            {
                TimerWindowEntry entry = new TimerWindowEntry();
                entry.SetSkillName(skill.Name);
                entry.SetColour(colourPalette.GetColour());
                _skillUIElements[skill] = entry;

                TimerEntriesPanel.Children.Add(entry);
            }
        }

        private void SetupKeys(CharacterEntry character)
        {
            _skillKeyBindings = new Dictionary<Key, SkillHandler.SkillConfig>();
            foreach (var skill in character.Skills)
            {
                if (skill.HotKey != Key.None && skill.TotalPoints > 0)
                {
                    _skillKeyBindings[skill.HotKey] = skill;
                }
            }
        }

        private void InitialiseTimers()
        {
            _skillTimes = new Dictionary<SkillHandler.SkillConfig, Times>();
            foreach (var skill in _skillKeyBindings.Values)
            {
                _skillTimes[skill] = new();
            }
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _keyboardEvents.KeyDown -= OnKeyDown;
            _keyboardEvents.Dispose();
        }

        public void LoadLevels(object levels)
        {
            //_levels = levels;
            StateDot.Fill = Brushes.Yellow;
            StateText.Text = "Ready";
        }


        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove(); // Enables window dragging
        }

        private void OnKeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            Key wpfKey = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);

            /*
             * TODO implement toggle listening?
            if (wpfKey == ToggleListening)
            {
                _isListening = !_isListening;
                SetListenging(_isListening);
            }
            */

            
            if (!_isListening)
            {
                return; // Ignore key presses if not listening
            }

            if (_skillKeyBindings.TryGetValue(wpfKey, out var skillConfig))
            {
                float skillTime = CalculateSkillTime(skillConfig);
                _skillTimes[skillConfig].CurrentTime = skillTime;
                _skillTimes[skillConfig].MaxTime = skillTime;
            }
        }

        private float CalculateSkillTime(SkillHandler.SkillConfig skillConfig)
        {
            int totalPoints = skillConfig.TotalPoints;
            int bonusPoints = CalculateBonusPoints();
            int level = totalPoints + bonusPoints;

            return _skillHandler.GetSkill(skillConfig.Name).DurationFunc(level, _character.MappedSkills);
        }

        private int CalculateBonusPoints()
        {
            int bonusPoints = 0;
            if (_character.MappedSkills.TryGetValue(BattleCommandsSkillName, out SkillHandler.SkillConfig? battleCommandsSkill))
            {
                if (_skillTimes.TryGetValue(battleCommandsSkill, out Times battleCommandsTime) && battleCommandsTime.CurrentTime > 0)
                {
                    bonusPoints += 1;
                }
            }
            if (_skillShrineActive)
            {
                bonusPoints += 2;
            }

            return bonusPoints;
        }

        public void SkillShrine_Click(object sender, RoutedEventArgs e)
        {
            ActivateSkillShrineTimer();
        }

        private void SetListenging(bool isListening)
        {
            StateDot.Fill = isListening ? Brushes.LimeGreen : Brushes.OrangeRed;
            StateText.Text = isListening ? "Active" : "Paused";
        }


        private void CreateTimer()
        {
            if (_timer is null)
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(0.2f);
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
        }

        private async void ActivateSkillShrineTimer()
        {
            _skillShrineActive = true;
            SkillShrineButton.BorderBrush = Brushes.LimeGreen;
            await Task.Delay(TimeSpan.FromSeconds(SkillShrineDuration)); 
            _skillShrineActive = false;
            SkillShrineButton.BorderBrush = Brushes.Transparent; // Reset border
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            foreach (var skill in _skillTimes.Keys)
            {
                if (_skillTimes[skill].CurrentTime > 0)
                {
                    _skillTimes[skill].CurrentTime -= TimerResolution; // Decrease the time by the timer interval
                    if (_skillTimes[skill].CurrentTime < 0)
                    {
                        _skillTimes[skill].CurrentTime = 0; // Ensure it doesn't go negative
                    }
                }
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            foreach (var uiElement in _skillUIElements)
            {
                SkillHandler.SkillConfig skill = uiElement.Key;
                Times time = _skillTimes[skill];
                string timeText = time.CurrentTime <= 0 ? " --:--" : FormatSeconds(time.CurrentTime);

                int percentage = (int)(time.CurrentTime * 100 / time.MaxTime);
                _skillUIElements[skill].SetTimeText(timeText, percentage);
            }
        }

        public static string FormatSeconds(float s)
        {
            TimeSpan time = TimeSpan.FromSeconds(s);
            return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
        }

    }
}
