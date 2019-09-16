using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DieFaceDistributer;
using Rationals;
using System.Numerics;

namespace DieFaceDistributerConsoleApp
{
    class ConsoleApp
    {
        static void Main(string[] args)
        { /*Args: numberOfSymbol: Number of the Winning Symbol that we have to distribute among our dice.            
            targetNumber: Target Number of Successes to Roll amongst the dice we create
            numSidesPerDie: Number of Sides the dice we create will have.
            */

            string[] useableArgNames = new[] { "NumberOfSymbol", "TargetNumber", "NumSidesPerDie", "FormattingForDice", "FormattingForOdds", "FormattingBetween", "MinNumberOfDice", "MaxNumberOfDice" };
            int[] intArgs = new[] { 0, 1, 2, 6, 7 }; //These are just indices into the useableArgNames array
            int[] formatArgs = new[] { 3, 4 };
            int[] independentArgs = new[] { 5 };

            Dictionary<string, string> foundArgs = args
                .Select(s => s.Split(new[] { ':', '=' }, 2))
                .ToDictionary(s => s[0].Trim("-/".ToCharArray()), s => s[1]);
            Dictionary<string, string> matchedArgs = foundArgs.Where(a => useableArgNames.Contains(a.Key)).ToDictionary(b => b.Key, b => b.Value);
            if(matchedArgs.Count == 0)
            {
                WriteHelpMessageAndExit(0);
            }
            Dictionary<string, int> parsedArgs = new Dictionary<string, int>();
            Dictionary<string, string> formatFriendlyArgs = new Dictionary<string, string>();
            //Dictionary<string, string> openEndedArgs = new Dictionary<string, string>();
            // matchedArgs.Where(a => intArgs.Any(b => useableArgNames[b] == a.Key)).Select(c => new KeyValuePair<string, int>(c.Key, 0)).ToDictionary(d => d.Key, d.Value);
            foreach (int parseableIndex in intArgs)
            {
                if(matchedArgs.ContainsKey(useableArgNames[parseableIndex]))
                {
                    if (!int.TryParse(matchedArgs[useableArgNames[parseableIndex]], out int temp))
                    {
                        Console.Error.WriteLine("Couldn't read argument '" + useableArgNames[parseableIndex] + "' passed in as '" + matchedArgs[useableArgNames[parseableIndex]] + "'.  Need an integer value.");
                        Console.WriteLine();
                        WriteHelpMessageAndExit(3);
                        
                    }
                    parsedArgs[useableArgNames[parseableIndex]] = temp;
                } else
                {
                    parsedArgs[useableArgNames[parseableIndex]] = (int)(FindDefault(useableArgNames[parseableIndex]));
                }
            }

            foreach(int formatIndex in formatArgs)
            {
                if(matchedArgs.ContainsKey(useableArgNames[formatIndex]))
                {
                    if(!matchedArgs[useableArgNames[formatIndex]].Contains("{0}"))
                    {
                        Console.Error.WriteLine("Argument '" + useableArgNames[formatIndex] + "' doesn't indicate where the information should go with '{0}' (did not find '{0}' in the argument)");
                        Console.WriteLine();
                        WriteHelpMessageAndExit(4);
                    } else
                    {
                        formatFriendlyArgs[useableArgNames[formatIndex]] = matchedArgs[useableArgNames[formatIndex]];
                    }
                } else
                {
                    formatFriendlyArgs[useableArgNames[formatIndex]] = FindDefault(useableArgNames[formatIndex]).ToString();
                }
            }

            foreach(int indIndex in independentArgs)
            {
                if(matchedArgs.ContainsKey(useableArgNames[indIndex]))
                {
                    formatFriendlyArgs[useableArgNames[indIndex]] = matchedArgs[useableArgNames[indIndex]];
                } else
                {
                    formatFriendlyArgs[useableArgNames[indIndex]] = FindDefault(useableArgNames[indIndex]).ToString();
                }
            }
        
            List<string> textToDisplay = new List<string>();
            IEnumerable<Tuple<IEnumerable<Dice>, Rational>> pairingsOfDiceSetsToTheirOddsOfGettingTheTN;
            pairingsOfDiceSetsToTheirOddsOfGettingTheTN = SingleWinningSymbolHelper.ShowNumbersOfWinningSymbolsAndOdds(parsedArgs["NumberOfSymbol"], parsedArgs["TargetNumber"], parsedArgs["MinNumberOfDice"], parsedArgs["MaxNumberOfDice"], parsedArgs["NumSidesPerDie"]);
                        
            foreach(Tuple<IEnumerable<Dice>,Rational> pairing in pairingsOfDiceSetsToTheirOddsOfGettingTheTN)
            { //"FormattingForDice", "FormattingForOdds", "FormattingBetween"
                textToDisplay.Add(SerializeForConsole(pairing.Item1, pairing.Item2, formatFriendlyArgs["FormattingForDice"], formatFriendlyArgs["FormattingForOdds"], formatFriendlyArgs["FormattingBetween"]));
            }            
            textToDisplay.ForEach(a => Console.WriteLine(a));
            
        }

        private static object FindDefault(string argName)
        { 
            if (argName == "NumberOfSymbol") return ((int)6);
            if (argName == "TargetNumber") return ((int)2);
            if (argName == "NumSidesPerDie") return ((int)8);
            if (argName == "MinNumberOfDice") return ((int)1);
            if (argName == "MaxNumberOfDice") return ((int)6);
            if (argName == "FormattingForDice" || argName == "FormattingForOdds") return "{0}";
            if (argName == "FormattingBetween") return ":"; 

            return string.Empty;
        }

        public static string SerializeForConsole(IEnumerable<Dice> dieList, Rational oddsOfHittingTN, string formatForDice, string formatForRational, string formatBetween)
        {
            return formatForDice.Replace("{0}", dieList.Select(a => a.NumberOfWinningSides().ToString()).Aggregate((a, b) => a + "," + b)) 
                + formatBetween 
                + formatForRational.Replace("{0}",oddsOfHittingTN.ToString("C"));
        }

        public static void WriteHelpMessageAndExit(int exitCode = 0)
        {
            WriteHelpMessage();
            System.Environment.Exit(exitCode);
        }

        public static void WriteHelpMessage()
        {
            Console.WriteLine("HELP MESSAGE");
        }

    }
}
