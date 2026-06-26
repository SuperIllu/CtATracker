using CtATracker.characters;
using CtATracker.config;
using CtATracker.Utilities;
using CtATracker.skills;
using CtATracker.UI_element_prefabs;
using CtATracker.window_elements;
using Gma.System.MouseKeyHook;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Diagnostics;


namespace CtATracker.secondary_windows
{
    /// <summary>
    /// Interaction logic for SummaryWindow.xaml
    /// </summary>
    public partial class SummaryWindow : Window
    {
        private readonly IKeyboardMouseEvents? _keyboardEvents;

        private DispatcherTimer _timer;
        private SkillTimerManager _timerManager;

        private Dictionary<Key, SkillHandler.SkillConfig> _skillKeyBindings;
        private Dictionary<GamepadButton, SkillHandler.SkillConfig> _skillGamepadBindings;
        private SkillHandler _skillHandler;
        private CharacterEntry _character;
        private ControlScheme _controlScheme;
        private XINPUT_STATE _previousGamepadState;


        private ColourPalette colourPalette = new ColourPalette();
        private Dictionary<SkillHandler.SkillConfig, TimerWindowEntry> _skillUIElements;




        public SummaryWindow(CharacterEntry character, SkillHandler skillHandler, ControlScheme controlScheme)
        {
            InitializeComponent();
            _controlScheme = controlScheme;
            if (_controlScheme == ControlScheme.Keyboard)
            {
                _keyboardEvents = Hook.GlobalEvents();
                _keyboardEvents.KeyDown += OnKeyDown;
            }

            _skillHandler = skillHandler;
            _character = character;
            SetupKeys(character);
            _timerManager = new SkillTimerManager(_skillHandler, _character, GetBoundSkills());
            GenerateUIElements();

            SetListenging(true);
            CreateTimer();
            this.Closed += OnClosed;
        }

        private IEnumerable<SkillHandler.SkillConfig> GetBoundSkills()
        {
            return _controlScheme == ControlScheme.Keyboard
                ? _skillKeyBindings.Values
                : _skillGamepadBindings.Values;
        }

        private void GenerateUIElements()
        {
            _skillUIElements = new Dictionary<SkillHandler.SkillConfig, TimerWindowEntry>();
            TimerEntriesPanel.Children.Clear(); // Clear existing entries
            foreach (var skill in GetBoundSkills())
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
            _skillGamepadBindings = new Dictionary<GamepadButton, SkillHandler.SkillConfig>();
            foreach (var skill in character.Skills)
            {
                if (skill.HotKey != Key.None && skill.TotalPoints > 0)
                {
                    _skillKeyBindings[skill.HotKey] = skill;
                }
                if (skill.GamepadButton != GamepadButton.None && skill.TotalPoints > 0)
                {
                    _skillGamepadBindings[skill.GamepadButton] = skill;
                }
            }

            var skillsWithKeys = character.Skills
                .Where(s => s.HotKey != Key.None && s.TotalPoints > 0)
                .ToList();
            if (skillsWithKeys.Count != _skillKeyBindings.Count)
            {
                var dupes = skillsWithKeys.GroupBy(s => s.HotKey).Where(g => g.Count() > 1);
                var msg = string.Join("\n", dupes.Select(g => $"  {g.Key}: {string.Join(", ", g.Select(s => s.Name))}"));
                MessageBox.Show($"Duplicate hotkeys detected:\n{msg}\n\nThe overlay will now close.",
                    "Hotkey Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
            }

            var skillsWithGamepad = character.Skills
                .Where(s => s.GamepadButton != GamepadButton.None && s.TotalPoints > 0)
                .ToList();
            if (skillsWithGamepad.Count != _skillGamepadBindings.Count)
            {
                var dupes = skillsWithGamepad.GroupBy(s => s.GamepadButton).Where(g => g.Count() > 1);
                var msg = string.Join("\n", dupes.Select(g => $"  {g.Key}: {string.Join(", ", g.Select(s => s.Name))}"));
                MessageBox.Show($"Duplicate gamepad buttons detected:\n{msg}\n\nThe overlay will now close.",
                    "Gamepad Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.Close();
            }
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            if (_keyboardEvents != null)
            {
                _keyboardEvents.KeyDown -= OnKeyDown;
                _keyboardEvents.Dispose();
            }
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

            Debug.WriteLine($"hotkey {e}->{wpfKey} pressed");

            if (_skillKeyBindings.TryGetValue(wpfKey, out var skillConfig))
                TriggerSkill(skillConfig);
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
                _timer.Interval = TimeSpan.FromSeconds(ConfigLoader.Instance.Overlay.TimerResolutionSec);
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
        }

        private async void ActivateSkillShrineTimer()
        {
            _timerManager.ActivateSkillShrine();
            SkillShrineButton.BorderBrush = Brushes.LimeGreen;
            await Task.Delay(TimeSpan.FromSeconds(ConfigLoader.Instance.SkillShrine.DurationSec)); 
            _timerManager.DeactivateSkillShrine();
            SkillShrineButton.BorderBrush = Brushes.Transparent;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_controlScheme == ControlScheme.Controller)
            {
                PollGamepad();
            }

            DecrementTimers();
            UpdateUI();
        }

        private void DecrementTimers()
        {
            _timerManager.DecrementTimers(ConfigLoader.Instance.Overlay.TimerResolutionSec);
        }

        private void PollGamepad()
        {
            XINPUT_STATE state = new XINPUT_STATE();
            int result = XInput.XInputGetState(0, ref state);
            if (result != XInput.ERROR_SUCCESS) return;

            foreach (var kvp in _skillGamepadBindings)
            {
                if (XInput.IsButtonHeld(state, kvp.Key) && !XInput.IsButtonHeld(_previousGamepadState, kvp.Key))
                    TriggerSkill(kvp.Value);
            }

            _previousGamepadState = state;
        }

        private void TriggerSkill(SkillHandler.SkillConfig skillConfig)
        {
            _timerManager.TriggerSkill(skillConfig);
        }

        private void UpdateUI()
        {
            foreach (var uiElement in _skillUIElements)
            {
                SkillHandler.SkillConfig skill = uiElement.Key;
                SkillTimerManager.Times time = _timerManager.SkillTimes[skill];
                string timeText = time.CurrentTime <= 0 ? " --:--" : FormatSeconds(time.CurrentTime);

                int percentage = time.MaxTime > 0
                    ? (int)(time.CurrentTime * 100 / time.MaxTime)
                    : 0;
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
