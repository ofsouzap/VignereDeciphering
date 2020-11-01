using System;
using System.Collections.Generic;
using System.Linq;

namespace VignereDeciphering
{
    static class ProgramMath
    {

        public static List<List<int>> GetListActivePossibilities(List<int> list,
            bool forceLengthTwo = false)
        {

            int length = list.Count;

            List<List<bool>> binaries = GetBinaryPossibilities(list.Count);

            List<List<int>> outputs = new List<List<int>>();

            foreach (List<bool> activePossibility in binaries)
            {

                List<int> appliedList = new List<int>();

                for (int i = 0; i < length; i++)
                {

                    if (activePossibility[i])
                    {

                        appliedList.Add(list[i]);

                    }

                }

                if (forceLengthTwo && appliedList.Count < 2) continue;

                outputs.Add(appliedList);

            }

            return outputs;

        }

        public static IEnumerable<List<T>> GetPermutations<T>(List<T> options)
        {

            if (options.Count == 1)
            {

                yield return new List<T> { options[0] };

            }
            else
            {

                foreach (T option in options)
                {

                    List<T> newOptions = new List<T>(options);
                    newOptions.Remove(option);

                    foreach (List<T> perm in GetPermutations(newOptions))
                    {

                        List<T> newOutput = new List<T>(perm);
                        newOutput.Insert(0, option);

                        yield return newOutput;

                    }

                }

            }

        }

        public static List<List<bool>> GetBinaryPossibilities(int length)
        {

            List<List<bool>> outputs = new List<List<bool>>();

            if (length == 1)
            {

                outputs.Add(new List<bool>() { true });
                outputs.Add(new List<bool>() { false });

                return outputs;

            }
            else
            {

                List<List<bool>> endings = GetBinaryPossibilities(length - 1);

                foreach (List<bool> ending in endings)
                {

                    List<bool> newOutputTrue = new List<bool>(ending);
                    newOutputTrue.Insert(0, true);
                    outputs.Add(newOutputTrue);

                    List<bool> newOutputFalse = new List<bool>(ending);
                    newOutputFalse.Insert(0, false);
                    outputs.Add(newOutputFalse);

                }

                return outputs;

            }

        }

        public static int[] GetFactors(int x)
        {

            List<int> found = new List<int>();

            for (int i = 2; i <= x; i++)
            {

                if (x % i == 0)
                {
                    found.Add(i);
                }

            }

            return found.ToArray();

        }

        public static Dictionary<T, int> GetTopDictionaryKeysByValue<T>(Dictionary<T, int> dictionary,
            int count)
        {

            List<KeyValuePair<T, int>> list = dictionary.OrderByDescending((pair) => pair.Value).ToList();

            Dictionary<T, int> output = new Dictionary<T, int>(count);

            for (int i = 0; i < count; i++)
            {

                KeyValuePair<T, int> currPair = list[i];

                output[currPair.Key] = currPair.Value;

            }

            return output;

        }

        public static double GetKeyFrequencyDifference<T>(Dictionary<T, double> a, Dictionary<T, double> b)
        {

            double difference = 0;

            foreach (T key in a.Keys)
            {

                difference += Math.Abs(a[key] - b[key]);

            }

            return difference;

        }

        public static Dictionary<char, char> GetCharacterMappingByCaesarCipherOffset(int offset)
        {

            Dictionary<char, char> mapping = new Dictionary<char, char>();

            for (int i = 0; i < 26; i++)
            {

                char inputChar = (char)(i + 65);
                char outputChar = (char)(((i + offset) % 26) + 65);

                mapping[inputChar] = outputChar;

            }

            return mapping;

        }

        public static string GetKeywordFromOffsets(int[] offsets)
        {

            string output = "";

            foreach (int offset in offsets)
            {

                /*validCharacters is used because the cipher could theoretically use another character or less but then should still have the encryption table based around the characters used*/

                char keychar = Program.validCharacters[offset];

                output = output + keychar;

            }

            return output;

        }

    }
}
