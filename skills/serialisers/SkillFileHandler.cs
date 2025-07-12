using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CtATracker.skills.serialisers
{
    internal class SkillFileHandler : ISkillFileHandler
    {
        public string FileName { get; }
        public string FilePath { get; }


        public SkillFileHandler(string fileName)
        {
            FileName = fileName;
            string basePath = AppContext.BaseDirectory;
            FilePath = Path.Combine(basePath, FileName);
        }

        public Dictionary<string, SkillHandler.Skill> LoadSkills()
        {
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException($"Skill configuration file '{FilePath}' not found.");
            }

            string fileContent = File.ReadAllText(FilePath);
            return SkillSerialiser.ParseSkills(fileContent);
        }
    }
}
