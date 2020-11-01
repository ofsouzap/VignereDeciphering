using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace VignereDeciphering
{
    class ManualMappingDeciphering
    {

        public static ConsoleColor defaultConsoleColor = Console.ForegroundColor;

        public static readonly ConsoleColor[] seperateKeyColors = new ConsoleColor[]
        {
            ConsoleColor.Red,
            ConsoleColor.Green,
            ConsoleColor.Yellow,
            ConsoleColor.Blue,
            ConsoleColor.Magenta,
            ConsoleColor.Cyan,
            ConsoleColor.Gray,
            ConsoleColor.White,
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkGray,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkYellow
        };

        public static string RunDeciphering(string originalText,
            int keyLength,
            out string keyword)
        {

            //TODO: allow for keyLength bigger than seperateKeyColors.Length
            Debug.Assert(keyLength < seperateKeyColors.Length);

            string[] originalSelections = FrequencyAnalysis.SplitTextByOffset(originalText, keyLength);

            int[] selectionShifts = new int[originalSelections.Length];
            for (int i = 0; i < originalSelections.Length; i++) selectionShifts[i] = 0;

            #region Instructions

            PrintSeperator();

            Console.WriteLine("To shift a selection with id i by n (n can be negative), use \"{i}:{n}\"");
            Console.WriteLine("To set the current mapping to a calculated optimal mapping, use SET OPTIMAL");
            Console.WriteLine("To print the text in one color, use PRINT PLAIN");

            PrintSeperator();

            for (int i = 0; i < keyLength; i++)
            {

                Console.ForegroundColor = seperateKeyColors[i];
                Console.Write(i + " ");

            }
            Console.ForegroundColor = defaultConsoleColor;
            Console.WriteLine();

            PrintSeperator();

            #endregion

            bool running = true;
            while (running)
            {

                Console.WriteLine("Current Keyword: " + ProgramMath.GetKeywordFromOffsets(selectionShifts));

                Console.WriteLine("Current Text:");
                PrintColoredTextByKey(ConstructCurrentDeciphering(originalSelections, selectionShifts),
                    keyLength);

                Console.Write("> ");
                string inputRequest = Console.ReadLine().ToUpper();

                if (inputRequest == "" || inputRequest == "END")
                {
                    running = false;
                }
                else if (Regex.IsMatch(inputRequest, @"^\d+:-?\d+$", RegexOptions.IgnoreCase))
                {

                    string[] parts = inputRequest.Split(':');
                    Debug.Assert(parts.Length == 2);

                    int selectionId = int.Parse(parts[0]);
                    int shiftAmount = int.Parse(parts[1]);

                    if (selectionId < 0
                        || selectionId >= originalSelections.Length)
                    {

                        Console.WriteLine("Invalid selection id");

                    }
                    else
                    {

                        selectionShifts[selectionId] = (selectionShifts[selectionId] + shiftAmount + Program.validCharacters.Length) % Program.validCharacters.Length;

                    }

                }
                else if (inputRequest == "SET OPTIMAL" || inputRequest == "SETOPTIMAL")
                {

                    for (int selectionIndex = 0; selectionIndex < originalSelections.Length; selectionIndex++)
                    {

                        string selection = originalSelections[selectionIndex];

                        Dictionary<char, double> selectionProportions = FrequencyAnalysis.CharFrequencyToCharProportion(FrequencyAnalysis.GetTextCharFrequency(selection));

                        double _;
                        int optimalShiftAmount;
                        FrequencyAnalysis.GetOptimalCharacterMapping(selectionProportions,
                            EnglishLetterFrequency.GetLetterProportions(),
                            out _,
                            out optimalShiftAmount);

                        selectionShifts[selectionIndex] = optimalShiftAmount;

                    }

                }
                else if (inputRequest == "PRINT PLAIN" || inputRequest == "PRINTPLAIN")
                {

                    Console.WriteLine(ConstructCurrentDeciphering(originalSelections, selectionShifts));

                }
                else
                {
                    Console.WriteLine("Unknown Request");
                }

            }

            keyword = ProgramMath.GetKeywordFromOffsets(selectionShifts);
            return ConstructCurrentDeciphering(originalSelections, selectionShifts);

        }

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

        static void PrintColoredTextByKey(string text,
            int keyLength)
        {

            int colorIndex = 0;

            foreach (char c in text)
            {

                ConsoleColor textColor = seperateKeyColors[colorIndex];

                Console.ForegroundColor = textColor;
                Console.Write(c);

                colorIndex = (colorIndex + 1) % keyLength;

            }

            Console.ForegroundColor = defaultConsoleColor;
            Console.WriteLine();

        }

        private static string ConstructCurrentDeciphering(string[] selections,
            int[] selectionShifts)
        {

            string[] currentSelections = new string[selections.Length];
            for (int i = 0; i < currentSelections.Length; i++)
            {

                currentSelections[i] = FrequencyAnalysis.DecipherTextByMapping(selections[i],
                    ProgramMath.GetCharacterMappingByCaesarCipherOffset(selectionShifts[i]));

            }

            return FrequencyAnalysis.ReconstructTextFromOffsetSelections(currentSelections);

        }

    }
}
