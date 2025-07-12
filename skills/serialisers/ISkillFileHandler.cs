using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CtATracker.skills.SkillHandler;

namespace CtATracker.skills.serialisers
{
    internal interface ISkillFileHandler
    {
        public Dictionary<string, Skill> LoadSkills();
    }
}
