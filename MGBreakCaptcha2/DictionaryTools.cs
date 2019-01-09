using System;
using System.Collections.Generic;
using System.Linq;

namespace MGBreakCaptcha2
{
    public static class DictionaryTools
    {
        public static Dictionary CreateDictionaries(char[] labels)
        {
            Dictionary<int, char> dictionaryIntToChar = new Dictionary<int, char>();

            char[] orderedLabels = labels.Distinct().OrderBy(c => c).ToArray();

            for (int i = 0; i < orderedLabels.Length; i++)
            {
                dictionaryIntToChar.Add(i, orderedLabels[i]);
            }

            Dictionary<char, int> dictionaryCharToInt = dictionaryIntToChar
                .Select(d => new KeyValuePair<char, int>(d.Value, d.Key))
                .ToDictionary(key => key.Key, value => value.Value);

            return new Dictionary()
            {
                DictionaryCharToInt = dictionaryCharToInt,
                DictionaryIntToChar = dictionaryIntToChar
            };
        }

        public static char IntToChar(Dictionary dictionary, int value)
        {
            return dictionary.DictionaryIntToChar.ContainsKey(value)
                ? dictionary.DictionaryIntToChar[value]
                : new char();
        }

        public static char[] IntToChar(Dictionary dictionary, int[] values)
        {
            return values.Select(value => IntToChar(dictionary, value)).ToArray();
        }

        public static int CharToInt(Dictionary dictionary, char value)
        {
            return dictionary.DictionaryCharToInt.ContainsKey(value)
                ? dictionary.DictionaryCharToInt[value]
                : new int();
        }

        public static int[] CharToInt(Dictionary dictionary, char[] values)
        {
            return values.Select(value => CharToInt(dictionary, value)).ToArray();
        }

        public static double[] IntToDoubles(int arraySize, int value)
        {
            double[] doubles = new double[arraySize];
            doubles[value] = 1;
            return doubles;
        }

        public static double[][] IntToDoubles(int arraySize, int[] values)
        {
            return values.Select(value => IntToDoubles(arraySize, value)).ToArray();
        }

        public static int DoublesToInt(double[] value)
        {
            return Array.IndexOf(value, value.Max());
        }

        public static int[] DoublesToInt(double[][] values)
        {
            return values.Select(DoublesToInt).ToArray();
        }
    }
}