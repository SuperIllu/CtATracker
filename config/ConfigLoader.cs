using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CtATracker.config
{
    public class ConfigLoader
    {
        private static ConfigLoader? _instance;
        public static ConfigLoader Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ConfigLoader();
                return _instance;
            }
        }

        public GamepadSection Gamepad { get; set; } = new();
        public KeyboardSection Keyboard { get; set; } = new();
        public OverlaySection Overlay { get; set; } = new();
        public CharacterDefaultsSection CharacterDefaults { get; set; } = new();
        public BattleCommandSection BattleCommand { get; set; } = new();
        public SkillShrineSection SkillShrine { get; set; } = new();
        public List<string> TimerColors { get; set; } = new();

        public static ConfigLoader Load(string filePath)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = File.ReadAllText(filePath);
            _instance = deserializer.Deserialize<ConfigLoader>(yaml);
            return _instance;
        }

        public class GamepadSection
        {
            public int PollIntervalMs { get; set; } = 16;
            public int CaptureTimeoutSec { get; set; } = 3;
            public byte TriggerThreshold { get; set; } = 30;
        }

        public class KeyboardSection
        {
            public int DuplicateFlashMs { get; set; } = 500;
        }

        public class OverlaySection
        {
            public float TimerResolutionSec { get; set; } = 0.2f;
        }

        public class CharacterDefaultsSection
        {
            public string Name { get; set; } = "NewCharacter";
            public int InitialSkillLevel { get; set; } = 1;
        }

        public class BattleCommandSection
        {
            public string SkillName { get; set; } = "BattleCommand";
            public int BonusPoints { get; set; } = 1;
        }

        public class SkillShrineSection
        {
            public string IconPath { get; set; } = "/img/SkillShrine.png";
            public float DurationSec { get; set; } = 10f;
            public int BonusPoints { get; set; } = 2;
        }
    }
}
