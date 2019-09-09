using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DieFaceDistributer
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
                        textToDisplay = ShowNumbersOfWinningSymbolsAndOdds(numberOfSymbol, targetNumber, numSidesPerDie);
                    }
                    else
                    {
                        textToDisplay = ShowNumbersOfWinningSymbolsAndOdds(numberOfSymbol, 1, 6);
                    }
                }
                else
                {
                    textToDisplay = ShowNumbersOfWinningSymbolsAndOdds(numberOfSymbol, 1, 6);
                }
            }
            else
            {
                textToDisplay = ShowNumbersOfWinningSymbolsAndOdds(1, 1, 6);
            }
            textToDisplay.ForEach(a => Console.WriteLine(a));
            Console.Write("Please press Enter...");
            System.IO.TextReader reader = Console.In;            
            int characterRead = reader.Read();
            while (characterRead != 13) {
                characterRead = reader.Read();
            }
            //TestAll();
        }
        
        static List<string> ShowNumbersOfWinningSymbolsAndOdds(int numberOfSymbol, int targetNumber, int sidesPerDie = 6)
        {
            List<string> answer = new List<string>();
            List<TreeNode<Dice>> diceForest = new List<TreeNode<Dice>>();
            IEnumerable<TreeNode<Tuple<Dice, string, List<Fraction>>>> scoreOddsForest = new List<TreeNode<Tuple<Dice, string, List<Fraction>>>>();
            int newMaxNumberSymbolsPerDie = Math.Min(sidesPerDie, numberOfSymbol);
            while (newMaxNumberSymbolsPerDie > 0)
            {
                TreeNode<Dice> tree = ConstructTreeOfDice(sidesPerDie, newMaxNumberSymbolsPerDie, numberOfSymbol);
                if (tree != null)
                {
                    diceForest.Add(tree);
                    TreeNode<Tuple<Dice, string, List<Fraction>>> scoreOddsTree = BuildOddsListingAndDiceDescription(tree, string.Empty, new List<Fraction>());
                    IEnumerable<TreeNode<Tuple<Dice, string, List<Fraction>>>> scoreOddsTreeChildren = scoreOddsTree.DeepestChildren();
                    scoreOddsForest = scoreOddsForest.Concat(scoreOddsTreeChildren);
                }
                newMaxNumberSymbolsPerDie--;
            }

            scoreOddsForest = scoreOddsForest.OrderByDescending(a => a.Value.Item3.Skip(targetNumber).Aggregate(new Fraction { Numerator = 0, Denominator = 1 }, (b, c) => b + c));
            foreach (TreeNode<Tuple<Dice, string, List<Fraction>>> scoreOddsTreeChild in scoreOddsForest)
            {
                int[] winners = new int[] { 1 };
                string target = scoreOddsTreeChild.Value.Item2 + ":" + scoreOddsTreeChild.Value.Item3.Skip(targetNumber).Aggregate(new Fraction { Numerator = 0, Denominator = 1 }, (a, b) => a + b).ToString();                
                answer.Add(target);
            }
            return answer;
        }

        static void ShowNumbersOfWinningSymbolsAndOddsTheOldWay(int numberOfSymbol, int targetNumber, int sidesPerDie = 6)
        {
            TreeNode<Dice> initialTree = StartConstructingTreeOfDice(sidesPerDie, Math.Min(sidesPerDie, numberOfSymbol), numberOfSymbol);
            TreeNode<Tuple<Dice, List<decimal>>> scoreOddsTree = BuildOddsListingAndDiceDescriptionTheOldWay(initialTree, new List<decimal>());
            List<TreeNode<Tuple<Dice, List<decimal>>>> scoreOddsTreeChildren = scoreOddsTree.DeepestChildren().ToList();
            foreach (TreeNode<Tuple<Dice, List<decimal>>> scoreOddsTreeChild in scoreOddsTreeChildren)
            {
                int[] winners = new int[] { 1 };
                TreeNode<Tuple<Dice, List<decimal>>> aParent = scoreOddsTreeChild.Parent;
                string target = scoreOddsTreeChild.Value.Item1.NumberOfWinningSides(winners).ToString();
                while (aParent != null)
                {
                    target += "," + aParent.Value.Item1.NumberOfWinningSides(winners).ToString();
                    aParent = aParent.Parent;
                }
                target += ":" + scoreOddsTreeChild.Value.Item2.Skip(targetNumber).Aggregate(0.0m, (a, b) => a + b).ToString("P");
                System.Console.WriteLine(target);
            }
        }

        private static TreeNode<Tuple<Dice, string, List<Fraction>>> BuildOddsListingAndDiceDescription(TreeNode<Dice> diceTree, string parentString, List<Fraction> currentOdds)
        {
            int[] winners = new int[]{ 1 };
            Tuple<Dice, string, List<Fraction>> scoreOddsUpToThisDie = new Tuple<Dice, string, List<Fraction>>(diceTree.Value, (parentString == string.Empty ? parentString : parentString + ",") + diceTree.Value.NumberOfWinningSides(winners), FractionallyMarkovMyChainButOnlyForSuccesses(currentOdds, diceTree.Value.FractionOddsOnSymbol(1)));
            TreeNode<Tuple<Dice, string, List<Fraction>>> answer = new TreeNode<Tuple<Dice, string, List<Fraction>>>(scoreOddsUpToThisDie);
            if (diceTree.Children.Count == 0)
            {
                return answer;
            }
            foreach (TreeNode<Dice> dieNode in diceTree.Children)
            {
                answer.AddChild(BuildOddsListingAndDiceDescription(dieNode, scoreOddsUpToThisDie.Item2, scoreOddsUpToThisDie.Item3));
            }
            return answer;
        }

        static TreeNode<Tuple<Dice, List<decimal>>> BuildOddsListingAndDiceDescriptionTheOldWay(TreeNode<Dice> diceTree, List<decimal> currentOdds)
        {
            Tuple<Dice, List<decimal>> scoreOddsUpToThisDie = new Tuple<Dice, List<decimal>>(diceTree.Value, MarkovMyChainButOnlyForSuccesses(currentOdds, diceTree.Value.OddsOnSymbol(1)));
            TreeNode<Tuple<Dice, List<decimal>>> answer = new TreeNode<Tuple<Dice, List<decimal>>>(scoreOddsUpToThisDie);
            if (diceTree.Children.Count == 0)
            {
                return answer;
            }
            foreach (TreeNode<Dice> dieNode in diceTree.Children)
            {
                answer.AddChild(BuildOddsListingAndDiceDescriptionTheOldWay(dieNode, scoreOddsUpToThisDie.Item2));
            }
            return answer;
        }

        /// <summary>
        /// Takes a list containing numbers indicating the odds of having hit {index} number of successes and the odds of getting a new success, and returns similar, but with the new odds factored in.
        /// </summary>
        /// <param name="currentOdds">Imagine this is [Odds of 0 successes, odds of 1 success, odds of 2 successes,...]</param>
        /// <param name="oddsOfNewSuccess">The odds of the current die you're (about to roll/rolling) being a success.  Should be in the range [0..1]</param>
        /// <returns></returns>
        static List<decimal> MarkovMyChainButOnlyForSuccesses(List<decimal> currentOdds, decimal oddsOfNewSuccess)
        {
            List<decimal> answer = new List<decimal>();
            if (currentOdds.Count == 0)
            {
                answer.Add(1.0m - oddsOfNewSuccess);
                answer.Add(oddsOfNewSuccess);
                return answer;
            }
            for (int i = 0; i < currentOdds.Count; i++)
            {
                if(i == 0) {
                    answer.Add(currentOdds[0] * (1.0m - oddsOfNewSuccess)); //Odds of having gotten 0 successes, total
                } else
                {
                    answer.Add(currentOdds[i - 1] * (oddsOfNewSuccess) + currentOdds[i] * (1.0m - oddsOfNewSuccess)); 
                    //Previous line: Odds of having gotten i-1 successes AND getting one on this roll, or, having gotten i successes AND not getting one on this roll
                }
            }            
            answer.Add(currentOdds[currentOdds.Count - 1] * oddsOfNewSuccess); //Odds of getting a number of success on each rolled die.  Impressive!
            return answer;
        }

        static List<Fraction> FractionallyMarkovMyChainButOnlyForSuccesses(List<Fraction> currentFractions, Fraction oddsOfNewSuccess)
        {
            List<Fraction> answer = new List<Fraction>();
            Fraction One = new Fraction { Numerator = 1, Denominator = 1 };
            if (currentFractions.Count == 0)
            {
                answer.Add(One - oddsOfNewSuccess);
                answer.Add(oddsOfNewSuccess);
                return answer;
            }
            for (int i = 0; i < currentFractions.Count; i++)
            {
                if (i == 0)
                {
                    answer.Add(currentFractions[0] * (One - oddsOfNewSuccess)); //Odds of having gotten 0 successes, total
                }
                else
                {
                    answer.Add(currentFractions[i - 1] * (oddsOfNewSuccess) + currentFractions[i] * (One - oddsOfNewSuccess));
                    //Previous line: Odds of having gotten i-1 successes AND getting one on this roll, or, having gotten i successes AND not getting one on this roll
                }
            }
            answer.Add(currentFractions[currentFractions.Count - 1] * oddsOfNewSuccess); //Odds of getting a number of success on each rolled die.  Impressive!
            return answer;
        }

        static TreeNode<Dice> StartConstructingTreeOfDice(int numberSidesPerDie, int maxNumberSymbolsPerDie, int totalNumberSymbolsToConsume)
        {
            TreeNode<Dice> node = new TreeNode<Dice>(new Dice { NumberOfSides = numberSidesPerDie });
            int newMaxNumberSymbolsPerDie = Math.Min(numberSidesPerDie, maxNumberSymbolsPerDie);
            while(newMaxNumberSymbolsPerDie > 0)
            {
                TreeNode<Dice> child = ConstructTreeOfDice(numberSidesPerDie, newMaxNumberSymbolsPerDie, totalNumberSymbolsToConsume);
                if (child != null)
                {
                    node.AddChild(child);
                }
                newMaxNumberSymbolsPerDie--;
            }
            return node;
        }

        static TreeNode<Dice> ConstructTreeOfDice(int numberSidesPerDie, int maxNumberSymbolsPerDie, int totalNumberSymbolsToConsume)
        {
            int maxOnThisDie = Math.Min(numberSidesPerDie, maxNumberSymbolsPerDie),
                currentNumberSymbolsToConsume = Math.Min(totalNumberSymbolsToConsume, maxOnThisDie),
            newMaxNumberSymbolsPerDie = Math.Min(maxNumberSymbolsPerDie,currentNumberSymbolsToConsume);                
            if (maxOnThisDie < 1 || totalNumberSymbolsToConsume < 1)
            {
                return null;
            }
            Dice newDie = Dice.ConstructWithNSidesOfXSymbol(1, currentNumberSymbolsToConsume, numberSidesPerDie);
            int childNumberSymbolsToConsume = totalNumberSymbolsToConsume - currentNumberSymbolsToConsume;
            newMaxNumberSymbolsPerDie = Math.Min(newMaxNumberSymbolsPerDie, childNumberSymbolsToConsume);
            TreeNode<Dice> node = new TreeNode<Dice>(newDie);            
            while (newMaxNumberSymbolsPerDie > 0)
            {
                TreeNode<Dice> child = ConstructTreeOfDice(numberSidesPerDie, newMaxNumberSymbolsPerDie, childNumberSymbolsToConsume);
                if (child != null)
                {                    
                    node.AddChild(child);
                }
                newMaxNumberSymbolsPerDie--;
            }            
            return node;
        }

        static void TestAll()
        {
            List<string> errors = Dice.TestDeserializationSamples();
            errors.AddRange(Dice.TestSerializationSamples());
            if(errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder("There were errors in running the tests: " + Environment.NewLine);
                foreach(string error in errors)
                {
                    sb.AppendLine(error);
                }
                Console.Error.Write(sb.ToString());
            }
        }
    }
}
