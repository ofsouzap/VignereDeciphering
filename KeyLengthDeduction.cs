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

    }

}
