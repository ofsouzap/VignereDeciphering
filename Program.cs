//#define debug

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

namespace VignereDeciphering
{
    class Program
    {

        //TODO: allow for spaces in text to be shown with output

        /*Character mapping Dictionary format: { inputTextChar : charThatItIsMappedTo (a.k.a. outputChar/decipheredChar) }*/

        public static readonly char[] validCharacters = new char[]
        {
            'A',
            'B',
            'C',
            'D',
            'E',
            'F',
            'G',
            'H',
            'I',
            'J',
            'K',
            'L',
            'M',
            'N',
            'O',
            'P',
            'Q',
            'R',
            'S',
            'T',
            'U',
            'V',
            'W',
            'X',
            'Y',
            'Z'
        };

        static void Main(string[] args)
        {

            Debug.Assert(validCharacters.Length == 26);

#if !debug
            ReleaseMain(args);
#endif

#if debug

            DebugStringByOffset();

#endif

            Console.WriteLine("Program Finished");
            Console.ReadKey();

        }

        #region MainFunctions

        static void PrintSeperator(char seperatorCharacter = '-',
            int length = 50,
            int lengthMultiplier = 1)
        {
            for (int i = 0; i < (length * lengthMultiplier); i++)
            {
                Console.Write(seperatorCharacter);
            }
            Console.WriteLine();
        }

        static void ReleaseMain(string[] args)
        {

            Console.Write("Text> ");
            string text;

            string input = Console.ReadLine().ToUpper().Replace(" ", "").ToUpper();

            if (input[0] == '\\')
            {
                text = GetTextFromFile(input.Substring(1)).ToUpper();
            }
            else
            {
                text = input;
            }

            foreach (char c in text)
            {

                if (!validCharacters.Contains(c))
                {
                    Console.WriteLine($"Error: Invalid character in text ({c})");
                    return;
                }

            }

            int keyLengthSelection = KeyLengthSelection(text);

            Console.Write("Automatically decipher text (0/1)?> ");
            bool autoDecipher = int.Parse(Console.ReadLine()) != 0;

            if (autoDecipher)
            {

                string[] offsetTextSelections = FrequencyAnalysis.SplitTextByOffset(text, keyLengthSelection);

                string[] decipheredStrings = new string[offsetTextSelections.Length];
                int[] shiftAmounts = new int[offsetTextSelections.Length];

                for (int i = 0; i < decipheredStrings.Length; i++)
                {

                    string selection = offsetTextSelections[i];

                    double _;
                    Dictionary<char, double> selectionProportions = FrequencyAnalysis.CharFrequencyToCharProportion(FrequencyAnalysis.GetTextCharFrequency(selection));
                    Dictionary<char, char> optimalMapping = FrequencyAnalysis.GetOptimalCharacterMapping(selectionProportions,
                        EnglishLetterFrequency.GetLetterProportions(),
                        out _,
                        out shiftAmounts[i]);

                    decipheredStrings[i] = FrequencyAnalysis.DecipherTextByMapping(selection, optimalMapping);

                }

                string fullDeciphering = FrequencyAnalysis.ReconstructTextFromOffsetSelections(decipheredStrings);

                Console.WriteLine("Keyword: " + ProgramMath.GetKeywordFromOffsets(shiftAmounts));
                Console.WriteLine("Deciphered Text:");
                Console.WriteLine(fullDeciphering);

            }
            else
            {

                string keyword;
                string fullDeciphering = ManualMappingDeciphering.RunDeciphering(text,
                    keyLengthSelection,
                    out keyword);

                Console.WriteLine("Keyword: " + keyword);
                Console.WriteLine("Deciphered Text:");
                Console.WriteLine(fullDeciphering);

            }

        }

        static string GetTextFromFile(string filename)
        {
            Debug.Assert(File.Exists(filename));
            return File.ReadAllText(filename);
        }

        static int KeyLengthSelection(string text)
        {

            Console.Write("Discard single-occurence key lengths (1/0)?> ");
            bool discardSingle = int.Parse(Console.ReadLine()) != 0;

            Repetition[] repetitions = Repetition.GetTextRepetitions(text);
            Dictionary<int, int> keyLengthWeightings = KeyLengthDeduction.GetKeyLengthWeights(repetitions, discardSingle);
            Dictionary<int, int> factoredWeightings = KeyLengthDeduction.GetFactoredKeyLengthWeights(keyLengthWeightings);

            Console.Write("View how many top lengths?> ");
            int topLengthsCount = int.Parse(Console.ReadLine());

            int requestCount = topLengthsCount < factoredWeightings.Count ? topLengthsCount : factoredWeightings.Count;
            Dictionary<int, int> topLengths = ProgramMath.GetTopDictionaryKeysByValue(factoredWeightings, requestCount);

            Console.WriteLine();
            Console.WriteLine("(English average IOC is about 0.0667)");
            Console.WriteLine("Top Key Lengths:");

            foreach (int length in topLengths.Keys)
            {

                decimal ioc = KeyLengthDeduction.GetAverageIOCOfTextByOffset(text, length);

                Console.WriteLine($"{length} (weighting = {topLengths[length]}) - Average IOC = {ioc}");

            }

            Console.Write("Select chosen key length> ");

            int keyLengthSelection = int.Parse(Console.ReadLine());

            return keyLengthSelection;

        }

        static string IndividualOffsetTextSelection(string text,
            int keyLengthSelection)
        {

            string[] selections = FrequencyAnalysis.SplitTextByOffset(text, keyLengthSelection);

            Console.WriteLine();
            Console.WriteLine("Text Offset Selections:");

            for (int i = 0; i < selections.Length; i++)
            {

                Console.WriteLine(i + ") " + selections[i]);

            }

            Console.Write("Select text offset selection> ");
            int offsetSelectionIndex = int.Parse(Console.ReadLine());

            Debug.Assert(0 <= offsetSelectionIndex &&
                offsetSelectionIndex < selections.Length);

            string selectedTextOffsetSelection = selections[offsetSelectionIndex];

            return selectedTextOffsetSelection;

        }

        static bool InputIsCharacterRemapping(string inputText)
        {
            return inputText.Length == 3 &&
                validCharacters.Contains(inputText[0]) &&
                inputText[1] == ':' &&
                validCharacters.Contains(inputText[2]);
        }

        [Obsolete("Unecessary as Vignere selections are Caesar ciphered as opposed to being regular monoalphabetic ciphers with 26! possibilities")]
        static Dictionary<char, char> GetMonoalphabeticMapping(string selectedTextOffsetSelection)
        {

            int graphHeight = 20;

            Dictionary<char, char> currentMapping = new Dictionary<char, char>();

            foreach (char c in validCharacters)
            {

                currentMapping.Add(c, c);

            }

            bool editing = true;

            while (editing)
            {

                Console.WriteLine("\n\n");

                string currentDeciphering = FrequencyAnalysis.DecipherTextByMapping(selectedTextOffsetSelection, currentMapping);

                Dictionary<char, double> englishCharacterProportions = EnglishLetterFrequency.GetLetterProportions();

                Dictionary<char, double> selectionCharacterProportion = FrequencyAnalysis.CharFrequencyToCharProportion(FrequencyAnalysis.GetTextCharFrequency(currentDeciphering));

                PrintSeperator();
                Console.WriteLine("English Language Average Character Proportions (Target Proportions):");
                FrequencyAnalysis.DrawConsoleCharFrequencyGraph(englishCharacterProportions,
                    graphHeight,
                    forcedContainChars: validCharacters);
                PrintSeperator();

                Console.WriteLine();
                PrintSeperator();
                Console.WriteLine("Current Character Proportions:");
                FrequencyAnalysis.DrawConsoleCharFrequencyGraph(selectionCharacterProportion,
                    graphHeight,
                    forcedContainChars: validCharacters);
                PrintSeperator();

                Console.WriteLine("Request mapping swap ({character 1}:{character 2}) or leave blank to finish or +/- to increase/decrease graph magnification");
                Console.Write("> ");
                string inputRequest = Console.ReadLine().ToUpper();

                if (inputRequest.Length == 0)
                {
                    editing = false;
                }
                else if (InputIsCharacterRemapping(inputRequest))
                {

                    char requestCharA = inputRequest[0];
                    char requestCharB = inputRequest[2];

                    Debug.Assert(inputRequest[1] == ':');

                    char currentInputCharToRequestCharA = ' ';
                    char currentInputCharToRequestCharB = ' ';

                    foreach (char c in currentMapping.Keys)
                    {

                        if (currentMapping[c] == requestCharA) currentInputCharToRequestCharA = c;
                        if (currentMapping[c] == requestCharB) currentInputCharToRequestCharB = c;

                    }

                    if (currentInputCharToRequestCharA == ' ' ||
                       currentInputCharToRequestCharB == ' ')
                    {

                        Debug.Fail("Error: Failed to locate char mapping to request char");

                    }

                    char requestInputCharOldMapping = currentMapping[currentInputCharToRequestCharA];
                    currentMapping[currentInputCharToRequestCharA] = requestCharB;
                    currentMapping[currentInputCharToRequestCharB] = requestInputCharOldMapping;

                }
                else if (inputRequest == "+")
                {

                    graphHeight += 5;

                }
                else if (inputRequest == "-")
                {

                    if (graphHeight > 5) graphHeight -= 5;
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Unable to further reduce graph height");
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                }
                else
                {

                    Console.WriteLine("Unknown request");

                }

                List<char> outputCharsUnique = new List<char>();
                List<char> warnedCharsNonUnique = new List<char>();
                foreach (char c in currentMapping.Keys)
                {

                    char outputChar = currentMapping[c];

                    if (outputCharsUnique.Contains(outputChar) &&
                        !warnedCharsNonUnique.Contains(outputChar))
                    {

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"WARNING: duplicate mapping to character '{outputChar}'");
                        Console.ForegroundColor = ConsoleColor.White;
                        warnedCharsNonUnique.Add(outputChar);

                    }
                    else
                    {
                        outputCharsUnique.Add(outputChar);
                    }

                }

            }

            return currentMapping;

        }

        #endregion

        #region DebugFunctions
#if debug

        static void DebugRepetitionLocating()
        {

            Console.Write("Text> ");
            string text = Console.ReadLine().ToUpper();

            Repetition[] repetitions = Repetition.GetTextRepetitions(text);

            foreach (Repetition repetition in repetitions)
            {
                Console.WriteLine($"{repetition.value} found {repetition.count} times at:");
                foreach (int position in repetition.positions)
                {
                    Console.WriteLine("\t" + position);
                }
            }

        }

        static void DebugBinaryPossibilities()
        {

            Console.Write("Length> ");
            int length = int.Parse(Console.ReadLine());

            List<List<bool>> possibilities = ProgramMath.GetBinaryPossibilities(length);

            Console.WriteLine($"{possibilities.Count} found");

            foreach (List<bool> possibility in possibilities)
            {

                foreach (bool value in possibility)
                {

                    Console.Write(value ? "1" : "0");

                }

                Console.WriteLine();

            }

        }

        static void DebugListActivePossibilities()
        {

            Console.Write("Range> ");

            int range = int.Parse(Console.ReadLine());

            List<int> input = new List<int>();

            for (int i = 0; i < range; i++) input.Add(i);

            List<List<int>> possibilities = ProgramMath.GetListActivePossibilities(input);

            foreach (List<int> possibility in possibilities)
            {

                foreach (int n in possibility)
                {
                    Console.Write(n.ToString() + ' ');
                }

                Console.WriteLine();

            }

        }

        static void DebugGetKeyLengthWeightings()
        {

            Console.Write("Text> ");
            string text = Console.ReadLine().ToUpper();
            Console.Write("Discard single occurences (1/0)?> ");
            bool discardSingle = int.Parse(Console.ReadLine()) != 0;

            Repetition[] repetitions = Repetition.GetTextRepetitions(text);
            Dictionary<int, int> keyLengthWeightings = KeyLengthDeduction.GetKeyLengthWeights(repetitions, discardSingle);

            foreach (int keyLength in keyLengthWeightings.Keys)
            {

                Console.WriteLine($"{keyLength} : {keyLengthWeightings[keyLength]}");

            }

        }

        static void DebugGetFactoredKeyLengthWeightings()
        {

            Console.Write("Text> ");
            string text = Console.ReadLine().ToUpper();
            Console.Write("Discard single occurences (1/0)?> ");
            bool discardSingle = int.Parse(Console.ReadLine()) != 0;

            Repetition[] repetitions = Repetition.GetTextRepetitions(text);
            Dictionary<int, int> keyLengthWeightings = KeyLengthDeduction.GetKeyLengthWeights(repetitions, discardSingle);
            Dictionary<int, int> factoredWeightings = KeyLengthDeduction.GetFactoredKeyLengthWeights(keyLengthWeightings);

            foreach (int factoredkeyLength in factoredWeightings.Keys)
            {

                Console.WriteLine($"{factoredkeyLength} : {factoredWeightings[factoredkeyLength]}");

            }

            Console.WriteLine();
            Console.Write("Get Top x Results> ");
            int count = int.Parse(Console.ReadLine());

            Dictionary<int, int> topResults = ProgramMath.GetTopDictionaryKeysByValue(factoredWeightings, count);

            foreach (int topResult in topResults.Keys)
            {

                Console.WriteLine($"{topResult} : {topResults[topResult]}");

            }

        }

        static void DebugTextSplitAndReconstruct()
        {

            Console.Write("Text> ");
            string text = Console.ReadLine().ToUpper();

            Console.Write("Offset> ");
            int offset = int.Parse(Console.ReadLine());

            string[] newTexts = FrequencyAnalysis.SplitTextByOffset(text, offset);

            foreach (string newText in newTexts)
            {

                Console.WriteLine(newText);

            }

            string reconstruction = FrequencyAnalysis.ReconstructTextFromOffsetSelections(newTexts);
            Console.WriteLine(reconstruction);

        }

        static void DebugGetCharProportion()
        {

            Console.Write("Text> ");
            string text = Console.ReadLine().ToUpper();

            Dictionary<char, int> charFrequency = FrequencyAnalysis.GetTextCharFrequency(text);

            Console.WriteLine("Frequencies:");
            foreach (char c in charFrequency.Keys)
            {

                Console.WriteLine($"{c} : {charFrequency[c]}");

            }

            Dictionary<char, double> charProportion = FrequencyAnalysis.CharFrequencyToCharProportion(charFrequency);

            Console.WriteLine("Proportions:");
            foreach (char c in charProportion.Keys)
            {

                Console.WriteLine($"{c} : {charProportion[c]}");

            }

        }

        static void DebugLetterProportionLoading()
        {

            Dictionary<char, double> charProportions = EnglishLetterFrequency.GetLetterProportions();
            
            foreach (char c in charProportions.Keys)
            {

                Console.WriteLine($"{c} : {charProportions[c]}");

            }

        }

        static void DebugGetPermutations()
        {

            Console.Write("Values> ");
            string[] values = Console.ReadLine().Split(',');

            foreach (List<string> perm in ProgramMath.GetPermutations(new List<string>(values)))
            {

                foreach (string part in perm)
                {

                    Console.Write(part);

                }

                Console.WriteLine();

            }

        }

        static void DebugOptimalCharacterMappingFull()
        {

            Console.Write("Text> ");
            string text = Console.ReadLine().ToUpper();

            const bool discardSingle = true;

            Repetition[] repetitions = Repetition.GetTextRepetitions(text);
            Dictionary<int, int> keyLengthWeightings = KeyLengthDeduction.GetKeyLengthWeights(repetitions, discardSingle);
            Dictionary<int, int> factoredWeightings = KeyLengthDeduction.GetFactoredKeyLengthWeights(keyLengthWeightings);

            const int topLengthsCount = 7;

            Dictionary<int, int> topLengths = ProgramMath.GetTopDictionaryKeysByValue(factoredWeightings, topLengthsCount);

            Console.WriteLine();
            Console.WriteLine("Top Key Lengths:");

            foreach (int length in topLengths.Keys)
            {

                Console.WriteLine($"{length} (weighting = {topLengths[length]})");

            }

            Console.Write("Select chosen key length> ");

            int keyLengthSelection = int.Parse(Console.ReadLine());

            string[] selections = FrequencyAnalysis.SplitTextByOffset(text, keyLengthSelection);

            Console.WriteLine();
            Console.WriteLine("Text Offset Selections:");

            for (int i = 0; i < selections.Length; i++)
            {

                Console.WriteLine(i + ") " + selections[i]);

            }

            Console.Write("Select text offset selection> ");
            int offsetSelectionIndex = int.Parse(Console.ReadLine());

            Debug.Assert(0 <= offsetSelectionIndex &&
                offsetSelectionIndex < selections.Length);

            string selectedTextOffsetSelection = selections[offsetSelectionIndex];

            Dictionary<char, double> selectionCharacterProportion = FrequencyAnalysis.CharFrequencyToCharProportion(FrequencyAnalysis.GetTextCharFrequency(selectedTextOffsetSelection));

            Dictionary<char, double> englishCharacterProportions = EnglishLetterFrequency.GetLetterProportions();

            double mappingDifference;
            int _;
            Dictionary<char, char> optimalMapping = FrequencyAnalysis.GetOptimalCharacterMapping(selectionCharacterProportion,
                englishCharacterProportions,
                out mappingDifference,
                out _);

            Console.WriteLine();
            Console.WriteLine($"Mapping Difference: {mappingDifference}");

            foreach (char textChar in optimalMapping.Keys)
            {

                Console.WriteLine($"{textChar} -> {optimalMapping[textChar]}");

            }

        }

        static void DebugConsoleCharGraphDrawing()
        {

            FrequencyAnalysis.DrawConsoleCharFrequencyGraph(EnglishLetterFrequency.GetLetterProportions(),
                15);

        }

        static void DebugRemappingPatternRegex()
        {

            string[] samples = new string[]
            {
                "aa",
                "AA",
                "test",
                "TEST",
                "A:B",
                "A:A",
                "TE:ST",
                "te:st",
                ":test",
                "test:",
                "a:b",
                "a:a",
                "A:BB",
                "AA:B"
            };

            foreach (string sample in samples)
            {

                Console.WriteLine($"{sample} -> {InputIsCharacterRemapping(sample)}");

            }

        }

        static void DebugManualMappingDeciphering()
        {

            Console.Write("Text> ");
            string text;

            string input = Console.ReadLine().ToUpper().Replace(" ", "").ToUpper();

            if (input[0] == '\\')
            {
                text = GetTextFromFile(input.Substring(1)).ToUpper();
            }
            else
            {
                text = input;
            }

            Console.Write("Key Length> ");
            int keyLength = int.Parse(Console.ReadLine());

            string _;
            ManualMappingDeciphering.RunDeciphering(text, keyLength, out _);

        }

        static void DebugStringByOffset()
        {

            Console.WriteLine("Text:");
            string text = Console.ReadLine();

            Console.Write("Offset> ");
            int offset = int.Parse(Console.ReadLine());

            string[] texts = KeyLengthDeduction.GetTextsFromStringByOffset(text, offset);

            foreach (string sampleText in texts)
            {

                Console.WriteLine(sampleText);

            }

        }

#endif
        #endregion

    }
}
