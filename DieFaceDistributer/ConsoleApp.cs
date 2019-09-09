using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DieFaceDistributer;

namespace DieFaceDistributerConsoleApp
{
    class ConsoleApp
    {
        static void Main(string[] args)
        { /*Args: numberOfSymbol: Number of the Winning Symbol that we have to distribute among our dice.            
            targetNumber: Target Number of Successes to Roll amongst the dice we create
            numSidesPerDie: Number of Sides the dice we create will have.
            */
            List<string> textToDisplay;
            if (args.Length > 0 && int.TryParse(args[0], out int numberOfSymbol))
            {
                if (args.Length > 1 && int.TryParse(args[1], out int targetNumber))
                {
                    if (args.Length > 2 && int.TryParse(args[2], out int numSidesPerDie))
                    {
                        textToDisplay = SingleWinningSymbolHelper.ShowNumbersOfWinningSymbolsAndOdds(numberOfSymbol, targetNumber, numSidesPerDie);
                    }
                    else
                    {
                        textToDisplay = SingleWinningSymbolHelper.ShowNumbersOfWinningSymbolsAndOdds(numberOfSymbol, 1, 6);
                    }
                }
                else
                {
                    textToDisplay = SingleWinningSymbolHelper.ShowNumbersOfWinningSymbolsAndOdds(numberOfSymbol, 1, 6);
                }
            }
            else
            {
                textToDisplay = SingleWinningSymbolHelper.ShowNumbersOfWinningSymbolsAndOdds(1, 1, 6);
            }
            textToDisplay.ForEach(a => Console.WriteLine(a));
            Console.Write("Please press Enter...");
            System.IO.TextReader reader = Console.In;
            int characterRead = reader.Read();
            while (characterRead != 13)
            {
                characterRead = reader.Read();
            }
            //TestAll();
        }

    }
}
