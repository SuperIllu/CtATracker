using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CtATracker.characters.serialisers
{
    internal interface ICharacterFileHandler
    {
        public Dictionary<string, CharacterEntry> LoadCharacters();
        public void SaveCharactersToFile(Dictionary<string, CharacterEntry> characters);
    }
}
