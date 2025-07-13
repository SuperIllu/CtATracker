using CtATracker.characters.serialisers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static CtATracker.skills.SkillHandler;

namespace CtATracker.characters
{
    public class CharacterHandler
    {

        private bool _autoSave = true;
        private ICharacterFileHandler _fileHandler;
        private Dictionary<string, CharacterEntry> _characters;
        public event Action<CharacterEntry>? OnCharacterSelected;

        public CharacterEntry? CurrentChar { get; private set; } = null;
        public event Action? OnSkillAddedOrRemoved;

        internal CharacterHandler(ICharacterFileHandler fileHandler)
        {
            _fileHandler = fileHandler;

            _characters = _fileHandler.LoadCharacters();
            if (_characters.Count > 0)
            {
                // load the first character as current
                CurrentChar = _characters.First().Value;
            }

        }

        public void AddNewCharacter(string charName)
        {
            if (_characters.ContainsKey(charName))
            {
                MessageBox.Show($"Character '{charName}' already exists. Please choose a different name.");
                return;
            }

            CharacterEntry newChar = new CharacterEntry(charName);
            _characters.Add(charName, newChar);
        }

        internal void SetCurrentCharacter(string newCharName)
        {
            if (_characters.TryGetValue(newCharName, out var character))
            {
                CurrentChar = character;
                OnCharacterSelected?.Invoke(CurrentChar);
            }
            else
            {
                throw new KeyNotFoundException($"Character '{newCharName}' not found.");
            }
        }

        public List<string> GetCharacterList()
        {
            return _characters.Keys.ToList();
        }


        public void DeleteCurrentCharacter()
        {
            if (CurrentChar == null || !_characters.ContainsKey(CurrentChar.Name))
            {
                throw new InvalidOperationException("No current character to delete.");
            }
            _characters.Remove(CurrentChar.Name);
            CurrentChar = null;
            if (_autoSave)
            {
                SaveCharactersToFile();
            }
        }



        private void SaveCharactersToFile()
        {
            _fileHandler.SaveCharactersToFile(_characters);
        }



        /// <summary>
        /// A function to signal if a skill has been added or removed.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        internal void SkillsUpdated()
        {
            OnSkillAddedOrRemoved?.Invoke();
            if (_autoSave)
            {
                SaveCharactersToFile();
            }
        }

        internal void SkillLevelUpdated()
        {
            // doesn't need to update the UI, but we need to save the characters
            if (_autoSave)
                SaveCharactersToFile();
        }
    }
}
