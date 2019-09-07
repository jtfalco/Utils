using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DieFaceDistributer
{
    class Program
    {
        static void Main(string[] args)
        { /*Args: numberOfSymbol: Number of the Winning Symbol that we have to distribute among our dice.            
            targetNumber: Target Number of Successes to Roll amongst the dice we create
            numSidesPerDie: Number of Sides the dice we create will have.
            */
            if (args.Length > 0 && int.TryParse(args[0], out int numberOfSymbol))
            {
                if (args.Length > 1 && int.TryParse(args[1], out int targetNumber))
                {
                    if (args.Length > 2 && int.TryParse(args[2], out int numSidesPerDie))
                    {
                        ShowNumbersOfWinningSymbolsAndOdds(numberOfSymbol, targetNumber, numSidesPerDie);
                    }
                    else
                    {
                        ShowNumbersOfWinningSymbolsAndOdds(numberOfSymbol, 1, 6);
                    }
                }
                else
                {
                    ShowNumbersOfWinningSymbolsAndOdds(numberOfSymbol, 1, 6);
                }
            }
            else
            {
                ShowNumbersOfWinningSymbolsAndOdds(1, 1, 6);
            }
            Console.Write("Please press Enter...");
            System.IO.TextReader reader = Console.In;            
            int characterRead = reader.Read();
            while (characterRead != 13) {
                characterRead = reader.Read();
            }
            //TestAll();
        }
        
        static void ShowNumbersOfWinningSymbolsAndOdds(int numberOfSymbol, int targetNumber, int sidesPerDie = 6)
        {
            List<TreeNode<DiceConstruct>> diceForest = new List<TreeNode<DiceConstruct>>();
            IEnumerable<TreeNode<Tuple<DiceConstruct, string, List<decimal>>>> scoreOddsForest = new List<TreeNode<Tuple<DiceConstruct, string, List<decimal>>>>();
            int newMaxNumberSymbolsPerDie = Math.Min(sidesPerDie, numberOfSymbol);
            while (newMaxNumberSymbolsPerDie > 0)
            {
                TreeNode<DiceConstruct> tree = ConstructTreeOfDice(sidesPerDie, newMaxNumberSymbolsPerDie, numberOfSymbol);
                if (tree != null)
                {
                    diceForest.Add(tree);
                    TreeNode<Tuple<DiceConstruct, string, List<decimal>>> scoreOddsTree = BuildOddsListingAndDiceDescription(tree, string.Empty, new List<decimal>());
                    IEnumerable<TreeNode<Tuple<DiceConstruct, string, List<decimal>>>> scoreOddsTreeChildren = scoreOddsTree.DeepestChildren();
                    scoreOddsForest = scoreOddsForest.Concat(scoreOddsTreeChildren);
                }
                newMaxNumberSymbolsPerDie--;
            }

            scoreOddsForest = scoreOddsForest.OrderByDescending(a => a.Value.Item3.Skip(targetNumber).Aggregate(0.0m, (b, c) => b + c));
            foreach (TreeNode<Tuple<DiceConstruct, string, List<decimal>>> scoreOddsTreeChild in scoreOddsForest)
            {
                int[] winners = new int[] { 1 };
                string target = scoreOddsTreeChild.Value.Item2 + ":" + scoreOddsTreeChild.Value.Item3.Skip(targetNumber).Aggregate(0.0m, (a, b) => a + b).ToString("P");                
                System.Console.WriteLine(target);
            }
        }

        static void ShowNumbersOfWinningSymbolsAndOddsTheOldWay(int numberOfSymbol, int targetNumber, int sidesPerDie = 6)
        {
            TreeNode<DiceConstruct> initialTree = StartConstructingTreeOfDice(sidesPerDie, Math.Min(sidesPerDie, numberOfSymbol), numberOfSymbol);
            TreeNode<Tuple<DiceConstruct, List<decimal>>> scoreOddsTree = BuildOddsListingAndDiceDescriptionTheOldWay(initialTree, new List<decimal>());
            List<TreeNode<Tuple<DiceConstruct, List<decimal>>>> scoreOddsTreeChildren = scoreOddsTree.DeepestChildren().ToList();
            foreach (TreeNode<Tuple<DiceConstruct, List<decimal>>> scoreOddsTreeChild in scoreOddsTreeChildren)
            {
                int[] winners = new int[] { 1 };
                TreeNode<Tuple<DiceConstruct, List<decimal>>> aParent = scoreOddsTreeChild.Parent;
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

        private static TreeNode<Tuple<DiceConstruct, string, List<decimal>>> BuildOddsListingAndDiceDescription(TreeNode<DiceConstruct> diceTree, string parentString, List<decimal> currentOdds)
        {
            int[] winners = new int[]{ 1 };
            Tuple<DiceConstruct, string, List<decimal>> scoreOddsUpToThisDie = new Tuple<DiceConstruct, string, List<decimal>>(diceTree.Value, (parentString == string.Empty ? parentString : parentString + ",") + diceTree.Value.NumberOfWinningSides(winners), MarkovMyChainButOnlyForSuccesses(currentOdds, diceTree.Value.OddsOnSymbol(1)));
            TreeNode<Tuple<DiceConstruct, string, List<decimal>>> answer = new TreeNode<Tuple<DiceConstruct, string, List<decimal>>>(scoreOddsUpToThisDie);
            if (diceTree.Children.Count == 0)
            {
                return answer;
            }
            foreach (TreeNode<DiceConstruct> dieNode in diceTree.Children)
            {
                answer.AddChild(BuildOddsListingAndDiceDescription(dieNode, scoreOddsUpToThisDie.Item2, scoreOddsUpToThisDie.Item3));
            }
            return answer;
        }

        static TreeNode<Tuple<DiceConstruct, List<decimal>>> BuildOddsListingAndDiceDescriptionTheOldWay(TreeNode<DiceConstruct> diceTree, List<decimal> currentOdds)
        {
            Tuple<DiceConstruct, List<decimal>> scoreOddsUpToThisDie = new Tuple<DiceConstruct, List<decimal>>(diceTree.Value, MarkovMyChainButOnlyForSuccesses(currentOdds, diceTree.Value.OddsOnSymbol(1)));
            TreeNode<Tuple<DiceConstruct, List<decimal>>> answer = new TreeNode<Tuple<DiceConstruct, List<decimal>>>(scoreOddsUpToThisDie);
            if (diceTree.Children.Count == 0)
            {
                return answer;
            }
            foreach (TreeNode<DiceConstruct> dieNode in diceTree.Children)
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

        static TreeNode<DiceConstruct> StartConstructingTreeOfDice(int numberSidesPerDie, int maxNumberSymbolsPerDie, int totalNumberSymbolsToConsume)
        {
            TreeNode<DiceConstruct> node = new TreeNode<DiceConstruct>(new DiceConstruct { NumberOfSides = numberSidesPerDie });
            int newMaxNumberSymbolsPerDie = Math.Min(numberSidesPerDie, maxNumberSymbolsPerDie);
            while(newMaxNumberSymbolsPerDie > 0)
            {
                TreeNode<DiceConstruct> child = ConstructTreeOfDice(numberSidesPerDie, newMaxNumberSymbolsPerDie, totalNumberSymbolsToConsume);
                if (child != null)
                {
                    node.AddChild(child);
                }
                newMaxNumberSymbolsPerDie--;
            }
            return node;
        }

        static TreeNode<DiceConstruct> ConstructTreeOfDice(int numberSidesPerDie, int maxNumberSymbolsPerDie, int totalNumberSymbolsToConsume)
        {
            int maxOnThisDie = Math.Min(numberSidesPerDie, maxNumberSymbolsPerDie),
                currentNumberSymbolsToConsume = Math.Min(totalNumberSymbolsToConsume, maxOnThisDie),
            newMaxNumberSymbolsPerDie = Math.Min(maxNumberSymbolsPerDie,currentNumberSymbolsToConsume);                
            if (maxOnThisDie < 1 || totalNumberSymbolsToConsume < 1)
            {
                return null;
            }
            DiceConstruct newDie = DiceConstruct.ConstructWithNSidesOfXSymbol(1, currentNumberSymbolsToConsume, numberSidesPerDie);
            int childNumberSymbolsToConsume = totalNumberSymbolsToConsume - currentNumberSymbolsToConsume;
            newMaxNumberSymbolsPerDie = Math.Min(newMaxNumberSymbolsPerDie, childNumberSymbolsToConsume);
            TreeNode<DiceConstruct> node = new TreeNode<DiceConstruct>(newDie);            
            while (newMaxNumberSymbolsPerDie > 0)
            {
                TreeNode<DiceConstruct> child = ConstructTreeOfDice(numberSidesPerDie, newMaxNumberSymbolsPerDie, childNumberSymbolsToConsume);
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
            List<string> errors = DiceConstruct.TestDeserializationSamples();
            errors.AddRange(DiceConstruct.TestSerializationSamples());
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
