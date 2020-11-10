using System;
using System.Collections.Generic;
using System.Linq;

namespace VignereDeciphering
{

    static class KeyLengthDeduction
    {

        /// <summary>
        /// Get the likelihood of possible key lengths given repetitions that have been found in a text
        /// </summary>
        /// <param name="repetitions">The repetitions that have been found in a text</param>
        /// <returns>{keyLength : occurences}</returns>
        public static Dictionary<int, int> GetKeyLengthWeights(Repetition[] repetitions,
            bool discardSingleOccurence = false)
        {

            Dictionary<int, int> lengthWeightings = new Dictionary<int, int>();

            foreach (Repetition repetition in repetitions)
            {

                List<List<int>> positionsActivePossibilities = ProgramMath.GetListActivePossibilities(repetition.positions, true);

                foreach (List<int> positions in positionsActivePossibilities)
                {

                    //Try each combination of repetitions being checked and not being checked in case some where coincidence

                    bool valid = true; /*All spacings are equal*/

                    int spacing = positions[1] - positions[0];

                    if (positions.Count > 2)

                        for (int i = 2; i < positions.Count; i++)
                        {

                            if (positions[i] - positions[i - 1] != spacing) valid = false;

                        }

                    if (valid)
                    {
                        if (lengthWeightings.ContainsKey(spacing))
                        {
                            lengthWeightings[spacing]++;
                        }
                        else
                        {
                            lengthWeightings.Add(spacing, 1);
                        }
                    }

                }

            }

            if (discardSingleOccurence)
            {

                Dictionary<int, int> newOutput = new Dictionary<int, int>();

                foreach (int key in lengthWeightings.Keys)
                {

                    if (lengthWeightings[key] > 1)
                    {
                        newOutput.Add(key, lengthWeightings[key]);
                    }

                }

                return newOutput;

            }
            else
            {

                return lengthWeightings;

            }

        }

        public static Dictionary<int, int> GetFactoredKeyLengthWeights(Dictionary<int, int> originalKeyLengthWeightings)
        {

            Dictionary<int, int> outputWeightings = new Dictionary<int, int>();

            foreach (int keyLength in originalKeyLengthWeightings.Keys)
            {

                int[] lengthFactors = ProgramMath.GetFactors(keyLength);

                foreach (int factor in lengthFactors)
                {

                    if (outputWeightings.ContainsKey(factor))
                    {
                        outputWeightings[factor] += originalKeyLengthWeightings[keyLength];
                    }
                    else
                    {
                        outputWeightings.Add(factor, originalKeyLengthWeightings[keyLength]);
                    }

                }

            }

            return outputWeightings;

        }

        public static decimal GetAverageIOCOfTextByOffset(string text, int offset)
        {

            string[] texts = GetTextsFromStringByOffset(text, offset);

            decimal[] iocs = CalculateIOCFromTexts(texts);

            decimal total = 0;
            foreach (decimal ioc in iocs) total += ioc;

            return total / iocs.Length;

        }

        public static decimal[] CalculateIOCFromTexts(string[] texts)
        {

            decimal[] output = new decimal[texts.Length];

            for (int i = 0; i < texts.Length; i++)
            {

                output[i] = CalculateIOC(texts[i]);

            }

            return output;

        }

        private static decimal CalculateIOC(string text) /*From IOCCalculator Project*/
        {

            Dictionary<char, int> charFrequencies = new Dictionary<char, int>();
            int includedCharCount = 0;

            foreach (char c in text)
            {

                if (!Program.validCharacters.Contains(c)) continue;

                if (charFrequencies.ContainsKey(c)) charFrequencies[c]++;
                else charFrequencies.Add(c, 1);

                includedCharCount++;

            }

            decimal denominator = (includedCharCount - 1) * includedCharCount;

            decimal total = 0;
            foreach (char c in charFrequencies.Keys)
            {

                total += (decimal)((charFrequencies[c] - 1) * charFrequencies[c]) / denominator;

            }

            return total;

        }

        public static string[] GetTextsFromStringByOffset(string text, int offset)
        {

            string[] texts = new string[offset];

            for (int i = 0; i < offset; i++)
            {

                texts[i] = GetTextFromStringByOffset(text.Substring(i), offset);

            }

            return texts;

        }

        private static string GetTextFromStringByOffset(string oldText, int offset)
        {

            string output = "";

            for (int i = 0; oldText.Length > i; i += offset)
            {

                output = output + oldText[i];

            }

            return output;

        }

    }

}
