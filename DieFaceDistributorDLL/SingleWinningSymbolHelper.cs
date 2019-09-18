using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Numerics;
using Rationals;
using DieFaceDistributer;

namespace DieFaceDistributer
{
    public class SingleWinningSymbolHelper
    {
        public static IEnumerable<Tuple<IEnumerable<Dice>, Rational>> ShowNumbersOfWinningSymbolsAndOdds(int numberOfSymbol, int targetNumber, int minNumberOfDice, int maxNumberOfDice, int sidesPerDie = 6)
        {            
            List<TreeNode<Dice>> diceForest = new List<TreeNode<Dice>>();
            IEnumerable<TreeNode<Tuple<Dice, IEnumerable<Dice>, List<Rational>>>> scoreOddsForest = GrowOddsForest(sidesPerDie, numberOfSymbol, maxNumberOfDice, minNumberOfDice);            

            scoreOddsForest = scoreOddsForest.OrderByDescending(a => a.Value.Item3.Skip(targetNumber).Aggregate(new Rational(new BigInteger(0), new BigInteger(1)), (b, c) => b + c));
            foreach (TreeNode<Tuple<Dice, IEnumerable<Dice>, List<Rational>>> scoreOddsTreeChild in scoreOddsForest)
            {                
                Tuple<IEnumerable<Dice>, Rational> yieldedAnswer = new Tuple<IEnumerable<Dice>, Rational>(scoreOddsTreeChild.Value.Item2, scoreOddsTreeChild.Value.Item3.Skip(targetNumber).Aggregate(new Rational(new BigInteger(0), new BigInteger(1)), (a, b) => a + b));                
                int count = yieldedAnswer.Item1.Count();
                if(count <= maxNumberOfDice && count >= minNumberOfDice) yield return yieldedAnswer; 
            }
            yield break;
        }

        private static IEnumerable<TreeNode<Tuple<Dice, IEnumerable<Dice>, List<Rational>>>> GrowOddsForest(int sidesPerDie, int numberOfSymbol, int maxNumberOfDice, int minNumberOfDice)
        {
            int newMaxNumberSymbolsPerDie = Math.Min(sidesPerDie, numberOfSymbol);
            newMaxNumberSymbolsPerDie = Math.Min(numberOfSymbol - (minNumberOfDice - 1), newMaxNumberSymbolsPerDie);
            while (newMaxNumberSymbolsPerDie > 0)
            {
                TreeNode<Dice> tree = ConstructTreeOfDice(sidesPerDie, newMaxNumberSymbolsPerDie, numberOfSymbol, maxNumberOfDice, minNumberOfDice);
                if (tree != null)
                {                    
                    TreeNode<Tuple<Dice, IEnumerable<Dice>, List<Rational>>> scoreOddsTree = BuildOddsListingAndDiceDescription(tree, null, new List<Rational>());
                    IEnumerable<TreeNode<Tuple<Dice, IEnumerable<Dice>, List<Rational>>>> scoreOddsTreeChildren = scoreOddsTree.DeepestChildren(); //Is this necessary?
                    foreach(TreeNode<Tuple<Dice, IEnumerable<Dice>, List<Rational>>> child in scoreOddsTreeChildren)
                    {
                        yield return child;
                    }                    
                }
                newMaxNumberSymbolsPerDie--;
            }
            yield break;
        }

        static void ShowNumbersOfWinningSymbolsAndOddsTheOldWay(int numberOfSymbol, int targetNumber, int sidesPerDie = 6)
        {
            TreeNode<Dice> initialTree = StartConstructingTreeOfDice(sidesPerDie, Math.Min(sidesPerDie, numberOfSymbol), numberOfSymbol, 0, 0);
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

        private static TreeNode<Tuple<Dice, IEnumerable<Dice>, List<Rational>>> BuildOddsListingAndDiceDescription(TreeNode<Dice> diceTree, IEnumerable<Dice> currentList, List<Rational> currentOdds)
        {
            int[] winners = new int[] { 1 };
            Tuple<Dice, IEnumerable<Dice>, List<Rational>> scoreOddsUpToThisDie = new Tuple<Dice, IEnumerable<Dice>, List<Rational>>(diceTree.Value, (currentList ?? diceTree.Parents()).Append(diceTree.Value), RationalallyMarkovMyChainButOnlyForSuccesses(currentOdds, diceTree.Value.RationalOddsOnSymbol(1)));
            TreeNode<Tuple<Dice, IEnumerable<Dice>, List<Rational>>> answer = new TreeNode<Tuple<Dice, IEnumerable<Dice>, List<Rational>>>(scoreOddsUpToThisDie);
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
                if (i == 0)
                {
                    answer.Add(currentOdds[0] * (1.0m - oddsOfNewSuccess)); //Odds of having gotten 0 successes, total
                }
                else
                {
                    answer.Add(currentOdds[i - 1] * (oddsOfNewSuccess) + currentOdds[i] * (1.0m - oddsOfNewSuccess));
                    //Previous line: Odds of having gotten i-1 successes AND getting one on this roll, or, having gotten i successes AND not getting one on this roll
                }
            }
            answer.Add(currentOdds[currentOdds.Count - 1] * oddsOfNewSuccess); //Odds of getting a number of success on each rolled die.  Impressive!
            return answer;
        }

        static List<Rational> RationalallyMarkovMyChainButOnlyForSuccesses(List<Rational> currentRationals, Rational oddsOfNewSuccess)
        {
            List<Rational> answer = new List<Rational>();
            Rational One = new Rational(new BigInteger(1), new BigInteger(1));
            if (currentRationals.Count == 0)
            {
                answer.Add(One - oddsOfNewSuccess);
                answer.Add(oddsOfNewSuccess);
                return answer;
            }
            for (int i = 0; i < currentRationals.Count; i++)
            {
                if (i == 0)
                {
                    answer.Add(currentRationals[0] * (One - oddsOfNewSuccess)); //Odds of having gotten 0 successes, total
                }
                else
                {
                    answer.Add(currentRationals[i - 1] * (oddsOfNewSuccess) + currentRationals[i] * (One - oddsOfNewSuccess));
                    //Previous line: Odds of having gotten i-1 successes AND getting one on this roll, or, having gotten i successes AND not getting one on this roll
                }
            }
            answer.Add(currentRationals[currentRationals.Count - 1] * oddsOfNewSuccess); //Odds of getting a number of success on each rolled die.  Impressive!
            return answer;
        }

        static TreeNode<Dice> StartConstructingTreeOfDice(int numberSidesPerDie, int maxNumberSymbolsPerDie, int totalNumberSymbolsToConsume, int maxNumberOfDice, int minNumberOfDice)
        {
            TreeNode<Dice> node = new TreeNode<Dice>(new Dice { NumberOfSides = numberSidesPerDie });
            int newMaxNumberSymbolsPerDie = Math.Min(numberSidesPerDie, maxNumberSymbolsPerDie);
            while (newMaxNumberSymbolsPerDie > 0)
            {
                TreeNode<Dice> child = ConstructTreeOfDice(numberSidesPerDie, newMaxNumberSymbolsPerDie, totalNumberSymbolsToConsume, maxNumberOfDice - 1, minNumberOfDice - 1);
                if (child != null)
                {
                    node.AddChild(child);
                }
                newMaxNumberSymbolsPerDie--;
            }
            return node;
        }

        static TreeNode<Dice> ConstructTreeOfDice(int numberSidesPerDie, int maxNumberSymbolsPerDie, int totalNumberSymbolsToConsume, int maxNumberOfDice, int minNumberOfDice)
        {
            int maxOnThisDie = Math.Min(numberSidesPerDie, maxNumberSymbolsPerDie),
                currentNumberSymbolsToConsume = Math.Min(totalNumberSymbolsToConsume, maxOnThisDie),
            newMaxNumberSymbolsPerDie = Math.Min(maxNumberSymbolsPerDie, currentNumberSymbolsToConsume);
            if (maxOnThisDie < 1 || totalNumberSymbolsToConsume < 1 || maxNumberOfDice < 1 || maxOnThisDie + (newMaxNumberSymbolsPerDie * (maxNumberOfDice - 1)) < totalNumberSymbolsToConsume)
            {
                return null;
            }
            Dice newDie = Dice.ConstructWithNSidesOfXSymbol(1, currentNumberSymbolsToConsume, numberSidesPerDie);
            int childNumberSymbolsToConsume = totalNumberSymbolsToConsume - currentNumberSymbolsToConsume;
            newMaxNumberSymbolsPerDie = Math.Min(newMaxNumberSymbolsPerDie, childNumberSymbolsToConsume);
            TreeNode<Dice> node = new TreeNode<Dice>(newDie);
            while (newMaxNumberSymbolsPerDie > 0)
            {
                TreeNode<Dice> child = ConstructTreeOfDice(numberSidesPerDie, newMaxNumberSymbolsPerDie, childNumberSymbolsToConsume, maxNumberOfDice - 1, minNumberOfDice - 1);                                
                if (child != null && child.DeepestDepth() >= minNumberOfDice - 1)
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
            if (errors.Count > 0)
            {
                StringBuilder sb = new StringBuilder("There were errors in running the tests: " + Environment.NewLine);
                foreach (string error in errors)
                {
                    sb.AppendLine(error);
                }
                Console.Error.Write(sb.ToString());
            }
        }
    }

}

