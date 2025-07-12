using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CtATracker.characters.serialisers
{
    internal class CharacterFileHandler : ICharacterFileHandler
    {
        public string FileName { get; }
        public string FilePath { get; }

        public CharacterFileHandler(string fileName)
        {
            FileName = fileName;
            string basePath = AppContext.BaseDirectory;
            FilePath = Path.Combine(basePath, fileName);
        }

        public Dictionary<string, CharacterEntry> LoadCharacters()
        {
            if (!File.Exists(FilePath))
            {
                return new Dictionary<string, CharacterEntry>();
            }
            else
            {
                string fileContent = File.ReadAllText(FilePath);
                return CharacterSerialiser.LoadCharacters(fileContent);
            }
        }


        public void SaveCharactersToFile(Dictionary<string, CharacterEntry> characters)
        {
            string serializedContent = CharacterSerialiser.SerialiseCharacters(characters);
            File.WriteAllText(FilePath, serializedContent);
        }

    }
}
