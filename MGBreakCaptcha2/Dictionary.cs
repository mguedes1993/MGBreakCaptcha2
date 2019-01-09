using System;
using System.Collections.Generic;

namespace MGBreakCaptcha2
{
    [Serializable]
    public class Dictionary
    {
        public Dictionary<int, char> DictionaryIntToChar { get; set; }
        public Dictionary<char, int> DictionaryCharToInt { get; set; }
    }
}