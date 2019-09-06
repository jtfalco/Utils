using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DieFaceDistributer
{
    class Program
    {
        static void Main(string[] args)
        {            
            if (args.Length > 0 && int.TryParse(args[0], out int numberOfSymbol))
            {
                if(args.Length > 1 && int.TryParse(args[1], out int minNumberOfDice))
                {
                    if (args.Length > 2 && int.TryParse(args[2], out int maxNumberOfDice))
                    {
                        
                        if(args.Length > 3 && int.TryParse(args[3], out int targetNumber))
                        {
                            if (args.Length > 4 && int.TryParse(args[4], out int numSidesPerDie))
                            {
                                FigureWithOneSymbol(numberOfSymbol, targetNumber, numSidesPerDie, maxNumberOfDice, minNumberOfDice);
                            }
                            else
                            {
                                FigureWithOneSymbol(numberOfSymbol, targetNumber, 6, maxNumberOfDice, minNumberOfDice);
                            }
                        } else
                        {
                            FigureWithOneSymbol(numberOfSymbol, 1, 6, maxNumberOfDice, minNumberOfDice);
                        }
                    } else
                    {
                        FigureWithOneSymbol(numberOfSymbol, 1, 6, int.MaxValue, minNumberOfDice);
                    }
                } else
                {
                    FigureWithOneSymbol(numberOfSymbol, 1, 6, int.MaxValue, 1);
                }
            } else
            {
                FigureWithOneSymbol(1, 1, 6, int.MaxValue, 1);
            }
            
            
            //TestAll();
        }

        static void FigureWithOneSymbol(int numberOfSymbol, int targetNumber, int sidesPerDie = 6, int maxNumberOfDice = int.MaxValue, int minNumberOfDice = 1)
        {
            TreeNode<DiceConstruct> initialTree = ConstructTreeOfDice(sidesPerDie, sidesPerDie, numberOfSymbol);
            TreeNode<Tuple<DiceConstruct, List<decimal>>> scoreOddsTree = BuildScoreOddsTree(initialTree, new List<decimal>());
            List<TreeNode<Tuple<DiceConstruct, List<decimal>>>> scoreOddsTreeChildren = scoreOddsTree.DeepestChildren().ToList();
            foreach(TreeNode<Tuple<DiceConstruct, List<decimal>>> scoreOddsTreeChild in scoreOddsTreeChildren)
            {
                int[] winners = new int[] { 1 };
                TreeNode<Tuple<DiceConstruct, List<decimal>>> aParent = scoreOddsTreeChild.Parent;
                string target = scoreOddsTreeChild.Value.Item1.NumberOfWinningSides(winners).ToString();
                while(aParent != null)
                {
                    target += "," + aParent.Value.Item1.NumberOfWinningSides(winners).ToString();
                    aParent = aParent.Parent;
                }
                target += ":" + scoreOddsTreeChild.Value.Item2.Skip(targetNumber).Aggregate(0.0m, (a, b) => a + b).ToString("P");
                System.Console.WriteLine(target);
            }
        }

        static TreeNode<Tuple<DiceConstruct, List<decimal>>> BuildScoreOddsTree(TreeNode<DiceConstruct> diceTree, List<decimal> currentOdds)
        {
            Tuple<DiceConstruct, List<decimal>> scoreOddsUpToThisDie = new Tuple<DiceConstruct, List<decimal>>(diceTree.Value, FactorFromCurrentOdds(currentOdds, diceTree.Value.OddsOnSymbol(1)));
            TreeNode<Tuple<DiceConstruct, List<decimal>>> answer = new TreeNode<Tuple<DiceConstruct, List<decimal>>>(scoreOddsUpToThisDie);
            //if (scoreOdds != null) answer.Parent = scoreOdds;
            if (diceTree.Children.Count == 0)
            {
                return answer;
            }
            foreach(TreeNode<DiceConstruct> dieNode in diceTree.Children)
            {
                answer.AddChild(BuildScoreOddsTree(dieNode, scoreOddsUpToThisDie.Item2));
            }
            return answer;
        }    

        /// <summary>
        /// Takes a list containing numbers indicating the odds of having hit {index} number of successes and the odds of getting a new success, and returns similar, but with the new odds factored in.
        /// </summary>
        /// <param name="currentOdds">Imagine this is [Odds of 0 successes, odds of 1 success, odds of 2 successes,...]</param>
        /// <param name="oddsOfNewSuccess">The odds of the current die you're (about to roll/rolling) being a success.  Should be in the range [0..1]</param>
        /// <returns></returns>
        static List<decimal> FactorFromCurrentOdds(List<decimal> currentOdds, decimal oddsOfNewSuccess)
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


            


        static List<decimal> ListOfOdds(TreeNode<DiceConstruct> diceTree, int numSuccessesNeeded, int numSuccessesReachable)
        {
            if (diceTree == null) return new List<decimal>();
            if (numSuccessesReachable < numSuccessesNeeded)
            {
                decimal scorePiece = diceTree.Value.OddsOnSymbol(1);
                return new List<decimal>().Append(scorePiece).Concat(new List<decimal>()).ToList();
            }
            //diceTree.Select(a => a.OddsOnSymbol(1)).Aggregate
            System.Linq.Enumerable.Aggregate<decimal>(System.Linq.Enumerable.Select<DiceConstruct,decimal>(diceTree, a => a.OddsOnSymbol(1)),(a,b) => 1);
            return new List<decimal>();
            //diceTree.Aggregate()
            //return new List<decimal>();*/
        }

        //                                                      3                       1                           3
        static TreeNode<DiceConstruct> ConstructTreeOfDice(int numberSidesPerDie, int maxNumberSymbolsPerDie, int totalNumberSymbolsToConsume)
        {
            int maxOnThisDie = Math.Min(numberSidesPerDie, maxNumberSymbolsPerDie),
                newMaxNumberSymbolsPerDie = maxNumberSymbolsPerDie,
                currentNumberSymbolsToConsume = Math.Min(totalNumberSymbolsToConsume, maxOnThisDie);
            if (maxOnThisDie < 1 || totalNumberSymbolsToConsume < 1)
            {
                return null;
            }
            DiceConstruct newDie = DiceConstruct.ConstructWithNSidesOfXSymbol(1, currentNumberSymbolsToConsume, numberSidesPerDie);
            //if (totalNumberSymbolsToConsume > maxOnThisDie)
            //{
            int childNumberSymbolsToConsume = totalNumberSymbolsToConsume - currentNumberSymbolsToConsume;
            TreeNode<DiceConstruct> node = new TreeNode<DiceConstruct>(newDie);
            List<TreeNode<DiceConstruct>> children = new List<TreeNode<DiceConstruct>>();
            while (newMaxNumberSymbolsPerDie > 0)
            {
                TreeNode<DiceConstruct> child = ConstructTreeOfDice(numberSidesPerDie, newMaxNumberSymbolsPerDie, childNumberSymbolsToConsume);
                if (child != null)
                {
                    children.Add(child);
                }
                newMaxNumberSymbolsPerDie--;
            }
            node.AddChildren(children);
            return node;
            //}            
            //return new TreeNode<DiceConstruct>(newDie);
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
