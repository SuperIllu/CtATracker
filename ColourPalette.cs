using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Windows.Media;

namespace CtATracker.Utilities
{
    public class ColourPalette
    {
        private readonly List<Color> _colors = new()
        {
            (Color)ColorConverter.ConvertFromString("#FF6C8EBF"), // Muted Blue
            (Color)ColorConverter.ConvertFromString("#FFB19C63"), // Muted Orange
            (Color)ColorConverter.ConvertFromString("#FF7FBF7F"), // Muted Green
            (Color)ColorConverter.ConvertFromString("#FFBF7F7F"), // Muted Red
            (Color)ColorConverter.ConvertFromString("#FFA89CBF"), // Muted Purple
            (Color)ColorConverter.ConvertFromString("#FF7FBFBF"), // Muted Teal
        };

        private int _currentIndex = 0;

        /// <summary>
        /// Returns the next color in the palette and advances the index. Wraps around when reaching the end.
        /// </summary>
        public Color GetColour()
        {
            var color = _colors[_currentIndex];
            _currentIndex = (_currentIndex + 1) % _colors.Count;
            return color;
        }

        /// <summary>
        /// Resets the index to start from the beginning again.
        /// </summary>
        public void Reset()
        {
            _currentIndex = 0;
        }

        /// <summary>
        /// Get all colors in the palette.
        /// </summary>
        public IReadOnlyList<Color> AllColors => _colors.AsReadOnly();
    }
}

