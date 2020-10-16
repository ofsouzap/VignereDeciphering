using System;
using System.Collections.Generic;
using System.Linq;

namespace VignereDeciphering
{

    class Repetition
    {

        public string value;
        public List<int> positions;
        public int count;

        public Repetition(string value,
            List<int> positions,
            int count)
        {
            this.value = value;
            this.positions = positions;
            this.count = count;
        }

        public Repetition(string value,
            int position,
            int count)
        {
            this.value = value;
            this.positions = new List<int>() { position };
            this.count = count;
        }

        public override bool Equals(object obj)
        {
            return obj is Repetition repetition &&
                   value == repetition.value &&
                   EqualityComparer<List<int>>.Default.Equals(positions, repetition.positions) &&
                   count == repetition.count;
        }

        public int ValueLength
        {
            get
            {
                return value.Length;
            }
        }

        public static bool RepetitionListContainsValue(List<Repetition> list, string value)
        {

            foreach (Repetition repetition in list)
            {

                if (repetition.value == value)
                {
                    return true;
                }

            }

            return false;

        }

        public static Repetition GetRepetitionFromListByValue(List<Repetition> list, string value)
        {

            foreach (Repetition repetition in list)
            {

                if (repetition.value == value)
                {
                    return repetition;
                }

            }

            return null;

        }

        public static Repetition[] GetTextRepetitions(string text)
        {

            /*Repetitions are of minimum length 3*/

            /*Find 3-letter repetitions*/

            List<Repetition> repetitions = new List<Repetition>();

            Dictionary<string, int> triplets = new Dictionary<string, int>();

            for (int i = 0; i < text.Length - 2; i++)
            {

                string snippet = text.Substring(i, 3);

                if (RepetitionListContainsValue(repetitions, snippet))
                {
                    GetRepetitionFromListByValue(repetitions, snippet).count++;
                }
                else if (triplets.ContainsKey(snippet))
                {

                    List<int> positions = new List<int> { triplets[snippet], i };
                    repetitions.Add(new Repetition(snippet, positions, 2));

                }
                else
                {
                    triplets.Add(snippet, i);
                }

            }

            /*Use 3-letter repetitions to find extended repetitions*/

            bool changed = true;
            while (changed)
            {

                changed = false;

                foreach (Repetition repetition in repetitions)
                {

                    foreach (int samplePosition in repetition.positions)
                    {

                        if ((samplePosition + repetition.ValueLength) >= text.Length) continue;

                        char newChar = text[samplePosition + repetition.ValueLength];

                        List<int> matches = new List<int>();

                        foreach (int checkPosition in repetition.positions)
                        {

                            if (checkPosition == samplePosition) continue;

                            if ((checkPosition + repetition.ValueLength) >= text.Length) continue;

                            if (text[checkPosition + repetition.ValueLength] == newChar)
                            {
                                matches.Add(checkPosition);
                            }

                        }

                        if (matches.Count > 0)
                        {

                            /*Add to new Repetition*/

                            Repetition longerRepetition = new Repetition(repetition.value + newChar,
                                samplePosition,
                                1);

                            foreach (int match in matches)
                            {

                                longerRepetition.positions.Add(match);
                                longerRepetition.count++;

                            }

                            repetitions.Add(longerRepetition);

                            /*Remove from old instance*/

                            repetition.positions.Remove(samplePosition);

                            foreach (int match in matches)
                            {

                                repetition.positions.Remove(match);

                            }

                            /*Check old instance's validity*/

                            if (repetition.positions.Count < 2)
                            {

                                repetitions.Remove(repetition);

                            }

                            changed = true;
                            break;

                        }

                    }

                    if (changed) break;

                }

            }

            return repetitions.ToArray();

        }

        public override int GetHashCode() /*Generated by VS*/
        {
            int hashCode = -1278286361;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(value);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<int>>.Default.GetHashCode(positions);
            hashCode = hashCode * -1521134295 + count.GetHashCode();
            hashCode = hashCode * -1521134295 + ValueLength.GetHashCode();
            return hashCode;
        }

    }

}
