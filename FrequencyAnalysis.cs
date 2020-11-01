using System;
using System.Collections.Generic;
using System.Linq;

namespace VignereDeciphering
{
    static class FrequencyAnalysis
    {

        public static string[] SplitTextByOffset(string text,
            int offset)
        {

            string[] outputStrings = new string[offset];

            for (int i = 0; i < text.Length; i++)
            {

                outputStrings[i % offset] += text[i];

            }

            return outputStrings;

        }

        public static string ReconstructTextFromOffsetSelections(string[] selections)
        {

            string output = "";

            for (int i = 0; i < selections[0].Length; i++)
            {

                foreach (string selection in selections)
                {

                    if (selection.Length <= i) continue;

                    output += selection[i];

                }

            }

            return output;

        }


        public static Dictionary<char, char> GetOptimalCharacterMapping(Dictionary<char, double> textCharProportion,
            Dictionary<char, double> targetCharProportion)
        {

            double _;
            int __;
            return GetOptimalCharacterMapping(textCharProportion,
                targetCharProportion,
                out _,
                out __);

        }

        
        [Obsolete("Not needed as Vignere selections are done by Caesar ciphers not regular monoalphabetic ciphers with 26! possibilities")]
        public static Dictionary<char, char> GetOptimalCharacterMappingNonCaesar(Dictionary<char, double> textCharProportion,
            Dictionary<char, double> targetCharProportion,
            out double mappingDifference)
        {

            /*Mappings are { inputTextChar : actualChar }*/

            double minDifference = double.MaxValue;
            Dictionary<char, char> optimalMapping = null;

            foreach (List<char> charList in ProgramMath.GetPermutations(new List<char>(Program.validCharacters)))
            {

                /*Create Character Mapping*/

                Dictionary<char, char> mapping = new Dictionary<char, char>();

                for (int i = 0; i < Program.validCharacters.Length; i++)
                {

                    mapping[charList[i]] = Program.validCharacters[i];

                }

                /*Test character mapping*/

                Dictionary<char, double> mappedCharProportions = new Dictionary<char, double>();

                foreach (char key in textCharProportion.Keys)
                {

                    mappedCharProportions[mapping[key]] = textCharProportion[key];

                }

                double difference = ProgramMath.GetKeyFrequencyDifference(mappedCharProportions, targetCharProportion);

                /*Compare mapping to best*/

                if (difference < minDifference)
                {

                    minDifference = difference;
                    optimalMapping = new Dictionary<char, char>(mapping);

                }

            }


            mappingDifference = minDifference;
            return optimalMapping;

        }

        public static Dictionary<char, char> GetOptimalCharacterMapping(Dictionary<char, double> textCharProportion,
            Dictionary<char, double> targetCharProportion,
            out double mappingDifference,
            out int mappingShiftAmount)
        {

            /*Mappings are { inputTextChar : actualChar }*/

            double minDifference = double.MaxValue;
            Dictionary<char, char> optimalMapping = null;
            int optimalShiftAmount = 0;

            for (int i = 0; i < 26; i++)
            {

                /*Create Character Mapping*/

                Dictionary<char, char> mapping = ProgramMath.GetCharacterMappingByTableRow(i);

                /*Test character mapping*/

                Dictionary<char, double> mappedCharProportions = new Dictionary<char, double>();

                foreach (char key in textCharProportion.Keys)
                {

                    mappedCharProportions[mapping[key]] = textCharProportion[key];

                }

                double difference = ProgramMath.GetKeyFrequencyDifference(mappedCharProportions, targetCharProportion);

                /*Compare mapping to best*/

                if (difference < minDifference)
                {

                    minDifference = difference;
                    optimalMapping = new Dictionary<char, char>(mapping);
                    optimalShiftAmount = i;

                }

            }


            mappingDifference = minDifference;
            mappingShiftAmount = optimalShiftAmount;
            return optimalMapping;

        }

        public static Dictionary<char, int> GetTextCharFrequency(string text)
        {

            Dictionary<char, int> charFrequency = new Dictionary<char, int>();

            foreach (char c in text)
            {

                if (charFrequency.ContainsKey(c))
                {

                    charFrequency[c]++;

                }
                else
                {

                    charFrequency.Add(c, 1);

                }

            }

            return charFrequency;

        }

        public static Dictionary<char, double> CharFrequencyToCharProportion(Dictionary<char, int> charFrequency)
        {

            int totalChars = 0;

            foreach (char c in charFrequency.Keys) totalChars += charFrequency[c];

            Dictionary<char, double> charProportion = new Dictionary<char, double>();

            foreach (char c in charFrequency.Keys)
            {

                charProportion[c] = (double)charFrequency[c] / totalChars;

            }

            return charProportion;

        }

        public static void DrawConsoleCharFrequencyGraph(Dictionary<char, double> charProportions,
            int graphCharacterHeight,
            char graphCharacter = '|',
            char[] forcedContainChars = null)
        {

            if (forcedContainChars != null)
            {
                foreach (char c in forcedContainChars)
                {

                    if (!charProportions.ContainsKey(c))
                    {
                        charProportions.Add(c, 0);
                    }

                }
            }

            char[,] graph = new char[graphCharacterHeight, charProportions.Count];

            List<KeyValuePair<char, double>> charProportionList = charProportions.ToList();
            charProportionList.Sort((valueA, valueB) => valueA.Key.CompareTo(valueB.Key));

            double maxValue = charProportionList[0].Value;

            foreach (KeyValuePair<char, double> pair in charProportionList)
            {

                if (pair.Value > maxValue)
                {
                    maxValue = pair.Value;
                }

            }

            for (int i = 0; i < charProportionList.Count; i++)
            {

                double normalizedValue = charProportionList[i].Value / maxValue;

                if (normalizedValue == 0) continue;

                int barTopIndex = graphCharacterHeight - (int)Math.Round(normalizedValue * (graphCharacterHeight - 1) + 1);

                for (int barIndex = barTopIndex; barIndex < graphCharacterHeight; barIndex++)
                {

                    graph[barIndex, i] = graphCharacter;

                }

            }

            const int barSpacing = 2;

            for (int y = 0; y < graph.GetLength(0); y++)
            {

                for (int x = 0; x < graph.GetLength(1); x++)
                {

                    Console.Write(graph[y, x]);

                    for (int i = 0; i < barSpacing; i++) Console.Write(' ');

                }

                Console.WriteLine();

            }

            foreach (KeyValuePair<char, double> pair in charProportionList)
            {

                Console.Write(pair.Key);
                for (int i = 0; i < barSpacing; i++) Console.Write(' ');

            }

            Console.WriteLine();

        }

        public static string DecipherTextByMapping(string text,
            Dictionary<char, char> mapping)
        {

            string outputText = "";

            foreach (char c in text)
            {

                if (mapping.ContainsKey(c))
                {
                    outputText += mapping[c];
                }
                else
                {
                    outputText += c;
                }

            }

            return outputText;

        }

    }
}
