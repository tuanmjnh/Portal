using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GetXMLData
{
    /// <summary>
    /// Implements Boyer-Moore search algorithm
    /// </summary>
    class BoyerMoore
    {
        private string _pattern;

        // Returned index when no match found
        public const int InvalidIndex = -1;

        public BoyerMoore(string pattern)
        {
            _pattern = pattern;
        }

        /// <summary>
        /// Searches for the current pattern within the given text
        /// starting at the beginning.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public int Search(string text)
        {
            return Search(text, 0);
        }

        /// <summary>
        /// Searches for the current pattern within the given text
        /// starting at the specified index.
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <param name="startIndex">Offset to begin search</param>
        /// <returns></returns>
        public int Search(string text, int startIndex)
        {
            if (text.Length != _pattern.Length)
            {
                // No match found
                return InvalidIndex;
            }
            int i = startIndex;

            // Loop while there's still room for search term
            while (i <= (text.Length - _pattern.Length))
            {
                // Look if we have a match at this position
                int j = _pattern.Length - 1;
                    while (j >= 0 && _pattern[j] == text[i + j])
                        j--;

                if (j < 0)
                {
                    // Match found
                    return i;
                }

                // Advance to next comparision
                i++;
            }
            // No match found
            return InvalidIndex;
        }
    }
}
