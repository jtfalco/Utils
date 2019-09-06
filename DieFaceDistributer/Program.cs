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
            int maxNumberOfDice = int.MaxValue, minNumberOfDice = 1;
            if (args.Length > 0 && int.TryParse(args[0], out int numberOfSymbol))
            {
                if(args.Length > 1 && int.TryParse(args[1], out minNumberOfDice))
                {
                    if(args.Length > 2){
                        int.TryParse(args[2], out maxNumberOfDice);
                    }                    
                }
            }
            if(minNumberOfDice < 1)
            {
                Console.Error.WriteLine("FAIL!  Minimum number of dice must be at least 1.");
                return;
            }
            if (maxNumberOfDice < 1)
            {
                Console.Error.WriteLine("FAIL!  Maximum number of dice must be at least 1.");
                return;
            }
            //FigureWithOneSymbol(numberOfSymbol, maxNumberOfDice, minNumberOfDice);
            TestAll();
        }

        static void FigureWithOneSymbol(int numberOfSymbol, int targetNumber, int sidesPerDie = 6, int maxNumberOfDice = int.MaxValue, int minNumberOfDice = 1)
        {
            TreeNode<DiceConstruct> initialTree = ConstructTreeOfDice(sidesPerDie, sidesPerDie, numberOfSymbol);
            TreeNode<Tuple<DiceConstruct, List<decimal>>> scoreOddsTree = BuildScoreOddsTree(initialTree, new List<decimal>());
            List<Tuple<DiceConstruct, List<decimal>>> scoreOddsTreeChildren = scoreOddsTree.DeepestChildren().ToList();
        }

        static TreeNode<Tuple<DiceConstruct, List<decimal>>> BuildScoreOddsTree(TreeNode<DiceConstruct> diceTree, List<decimal> currentOdds)
        {
            Tuple<DiceConstruct, List<decimal>> scoreOddsUpToThisDie = new Tuple<DiceConstruct, List<decimal>>(diceTree.Value, FactorFromCurrentOdds(currentOdds, diceTree.Value.OddsOnSymbol(1)));
            TreeNode<Tuple<DiceConstruct, List<decimal>>> answer = new TreeNode<Tuple<DiceConstruct, List<decimal>>>(scoreOddsUpToThisDie);
            //if (scoreOdds != null) answer.Parent = scoreOdds;
            if (diceTree.Children.Count > 0)
            {
                return answer;
            }
            foreach(TreeNode<DiceConstruct> dieNode in diceTree.Children)
            {
                answer.AddChild(BuildScoreOddsTree(dieNode, scoreOddsUpToThisDie.Item2));
            }
            return answer;
        }    

        static List<decimal> FactorFromCurrentOdds(List<decimal> currentOdds, decimal oddsOfNewSuccess)
        {
            List<decimal> answer = new List<decimal>();
            for(int i = 0; i < currentOdds.Count; i++)
            {
                if(i == 0) {
                    answer.Add(currentOdds[0] * (1.0m - oddsOfNewSuccess));
                } else
                {
                    answer.Add(currentOdds[i - 1] * (oddsOfNewSuccess) + currentOdds[i] * (1.0m - oddsOfNewSuccess));
                }
            }
            answer.Add(currentOdds[currentOdds.Count - 1] * oddsOfNewSuccess);
            return answer;
        }


            


        static List<decimal> ListOfOdds(TreeNode<DiceConstruct> diceTree, int numSuccessesNeeded, int numSuccessesReachable)
        {
            if (diceTree == null) return new List<Decimal>();
            if (numSuccessesReachable < numSuccessesNeeded)
            {
                decimal scorePiece = diceTree.Value.OddsOnSymbol(1);
                return new List<decimal>().Append(scorePiece).Concat(new List<decimal>()).ToList();
            }
            //diceTree.Select(a => a.OddsOnSymbol(1)).Aggregate
            System.Linq.Enumerable.Aggregate<decimal>(System.Linq.Enumerable.Select<DiceConstruct,decimal>(diceTree, a => a.OddsOnSymbol(1)),(a,b) => 1);
            return new List<decimal>();
            //diceTree.Aggregate()
            return new List<decimal>();*/
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
            if (totalNumberSymbolsToConsume > maxOnThisDie)
            {
                int childNumberSymbolsToConsume = totalNumberSymbolsToConsume - currentNumberSymbolsToConsume;                
                TreeNode<DiceConstruct> node = new TreeNode<DiceConstruct>(newDie);
                List<TreeNode<DiceConstruct>> children = new List<TreeNode<DiceConstruct>>();
                while (newMaxNumberSymbolsPerDie > 0)
                {
                    TreeNode<DiceConstruct> child = ConstructTreeOfDice(numberSidesPerDie, newMaxNumberSymbolsPerDie, childNumberSymbolsToConsume);
                    if(child != null)
                    {
                        children.Add(child);
                    }                    
                    newMaxNumberSymbolsPerDie--;
                }                
                node.AddChildren(children);
                return node;
            }            
            return new TreeNode<DiceConstruct>(newDie);
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
