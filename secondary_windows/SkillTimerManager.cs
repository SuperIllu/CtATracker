using CtATracker.characters;
using CtATracker.config;
using CtATracker.skills;

namespace CtATracker.secondary_windows
{
    internal class SkillTimerManager
    {
        public class Times
        {
            public float CurrentTime;
            public float MaxTime;

            public override string ToString()
            {
                return $"{CurrentTime}/{MaxTime}";
            }
        }

        private readonly Dictionary<SkillHandler.SkillConfig, Times> _skillTimes;
        private readonly SkillHandler _skillHandler;
        private readonly CharacterEntry _character;
        private bool _skillShrineActive;

        public IReadOnlyDictionary<SkillHandler.SkillConfig, Times> SkillTimes => _skillTimes;

        public SkillTimerManager(SkillHandler skillHandler, CharacterEntry character, IEnumerable<SkillHandler.SkillConfig> boundSkills)
        {
            _skillHandler = skillHandler;
            _character = character;
            _skillTimes = new Dictionary<SkillHandler.SkillConfig, Times>();
            foreach (var skill in boundSkills)
            {
                _skillTimes[skill] = new Times();
            }
        }

        public void TriggerSkill(SkillHandler.SkillConfig skillConfig)
        {
            float skillTime = CalculateSkillTime(skillConfig);
            _skillTimes[skillConfig].CurrentTime = skillTime;
            _skillTimes[skillConfig].MaxTime = skillTime;
        }

        public void DecrementTimers(float resolutionSec)
        {
            foreach (var skill in _skillTimes.Keys)
            {
                if (_skillTimes[skill].CurrentTime > 0)
                {
                    _skillTimes[skill].CurrentTime -= resolutionSec;
                    if (_skillTimes[skill].CurrentTime < 0)
                        _skillTimes[skill].CurrentTime = 0;
                }
            }
        }

        public void ActivateSkillShrine()
        {
            _skillShrineActive = true;
        }

        public void DeactivateSkillShrine()
        {
            _skillShrineActive = false;
        }

        private float CalculateSkillTime(SkillHandler.SkillConfig skillConfig)
        {
            int totalPoints = skillConfig.TotalPoints;
            int bonusPoints = CalculateBonusPoints();
            int level = totalPoints + bonusPoints;

            if (_skillHandler.TryGetSkill(skillConfig.Name, out var skill))
                return skill.DurationFunc(level, _character.MappedSkills);
            return 0;
        }

        private int CalculateBonusPoints()
        {
            int bonusPoints = 0;
            if (_character.MappedSkills.TryGetValue(ConfigLoader.Instance.BattleCommand.SkillName, out SkillHandler.SkillConfig? battleCommandsSkill))
            {
                if (_skillTimes.TryGetValue(battleCommandsSkill, out Times battleCommandsTime) && battleCommandsTime.CurrentTime > 0)
                {
                    bonusPoints += ConfigLoader.Instance.BattleCommand.BonusPoints;
                }
            }
            if (_skillShrineActive)
            {
                bonusPoints += ConfigLoader.Instance.SkillShrine.BonusPoints;
            }

            return bonusPoints;
        }
    }
}
