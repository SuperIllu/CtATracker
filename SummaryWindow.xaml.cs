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


namespace CtATracker
{
    /// <summary>
    /// Interaction logic for SummaryWindow.xaml
    /// </summary>
    public partial class SummaryWindow : Window
    {
        private static IKeyboardMouseEvents _keyboardEvents;

        private bool _isListening = false;
        private bool _keysLoaded = false;
        private System.Windows.Input.Key ToggleListening;
        private System.Windows.Input.Key BattleOrderKey;
        private System.Windows.Input.Key BattleCommandsKey;

        private DispatcherTimer _timer;
        private float _remainingBattleOrderTime;
        private float _remainingBattleCommandsTime;

        //private Skills.Levels _levels;


        public SummaryWindow()
        {
            InitializeComponent();
            _keyboardEvents = Hook.GlobalEvents();

            _keyboardEvents.KeyDown += OnKeyDown;
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

        public void SetListeningKeys(System.Windows.Input.Key toggleListening, System.Windows.Input.Key battleOrderKey, System.Windows.Input.Key battleCommandsKey)
        {
            ToggleListening = toggleListening;
            BattleOrderKey = battleOrderKey;
            BattleCommandsKey = battleCommandsKey;
            _keysLoaded = true;
        }


        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove(); // Enables window dragging
        }

        private void OnKeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
        {
            //if (_levels == null) return;
            if (!_keysLoaded) return;

            Key wpfKey = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);

            if (wpfKey == ToggleListening)
            {
                _isListening = !_isListening;
                StateDot.Fill = _isListening ? Brushes.LimeGreen : Brushes.OrangeRed;
                StateText.Text = _isListening ? "Listening" : "Not listening";
            }

            if (!_isListening)
            {
                return; // Ignore key presses if not listening
            }

            if (wpfKey == BattleOrderKey)
            {
                StartTimerBattleOrders();
            }
            else if (wpfKey == BattleCommandsKey)
            {
                StartTimerBattleCommands();
            }
        }

        private void StartTimerBattleCommands()
        {
            CreateTimer();
        }


        private void StartTimerBattleOrders()
        {
            CreateTimer();
        }

        private void CreateTimer()
        {
            if (_timer is null)
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(0.2f);
                _timer.Tick += Timer_Tick;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _remainingBattleCommandsTime -= 0.2f;
            _remainingBattleOrderTime -= 0.2f;

            UpdateUI();
        }

        private void UpdateUI()
        {
            _remainingBattleOrderTime = Math.Max(_remainingBattleOrderTime, 0);
            _remainingBattleCommandsTime = Math.Max(_remainingBattleCommandsTime, 0);

            Text_BattleCommand.Text = $"{FormatSeconds(_remainingBattleCommandsTime)} ({BattleCommandsKey})";
            Text_BattleOrders.Text = $"{FormatSeconds(_remainingBattleOrderTime)} ({BattleOrderKey})";
        }

        public static string FormatSeconds(float s)
        {
            TimeSpan time = TimeSpan.FromSeconds(s);
            return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
        }

    }
}
