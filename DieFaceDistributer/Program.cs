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
            TreeNode<Tuple<DiceConstruct, List<decimal>>> scoreOddsTree = BuildScoreOddsTree(initialTree, scoreOddsTree, new List<decimal>());
            List<Tuple<DiceConstruct, List<decimal>>> scoreOddsTreeChildren = scoreOddsTree.DeepestChildren().ToList();
        }

        static TreeNode<Tuple<DiceConstruct, List<decimal>>> BuildScoreOddsTree(TreeNode<DiceConstruct> diceTree, TreeNode<Tuple<DiceConstruct, List<decimal>>> scoreOdds, List<decimal> currentOdds)
        {
            Tuple<DiceConstruct, List<decimal>> scoreOddsUpToThisDie = new Tuple<DiceConstruct, List<decimal>>(diceTree.Value, FactorFromCurrentOdds(currentOdds, diceTree.Value.OddsOnSymbol(1)));
            return new TreeNode<Tuple<DiceConstruct, List<decimal>>>(scoreOddsUpToThisDie);
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


            /* First, pack them in
             * A: Then, until your left-most die is "fully expanded",
             * Take your right-most, not-fully-expanded die, and,
             * "peel" a victory symbol off the right of it. (
             * Expand it one to the right. Then, evaluate the score for that list.
             * Loop back to A
             * 
             * Start fully expanded
             * B: Then, until you are "fully packed in",
             * 
             * 111
             * 110 100
             * 100 110
             * 100 100 100
             * 
             * 1111
             * 1110 1000
             * 1100 1100
             * 1100 1000 1000
             * 1000 1100 1000
             * 1000 1000 1100
             * 1000 1000 1000 1000
             * 
             * 11111
             * 11110 10000
             * 11100 11000
             * 11100 10000 10000
             * 11000 11000 10000
             * 11000 10000 11000
             * 11000 10000 10000 10000
             * 10000 11000 10000 10000
             * 10000 10000 11000 10000
             * 11000 10000 10000 11000
             * 10000 10000 10000 10000 10000
             * 
             * 111111
             * 111110 100000
             * 111100 110000
             * 111100 100000 100000
             * 111000 111000
             * 111000 110000 100000
             * 111000 100000 100000 100000
             * 110000 110000 110000
             * 110000 110000 100000 100000
             * 110000 100000 100000 100000 100000
             * 100000 100000 100000 100000 100000 100000
             * 
             * 1111111
             * 1111110 1000000
             * 1111100 1100000
             * 1111100 1000000 1000000
             * 1111000 1110000 
             * 1111000 1100000 1000000
             * 1111000 1000000 1000000 1000000
             * 1110000 1110000 1000000
             * 1110000 1100000 1100000
             * 1110000 1100000 1000000 1000000
             * 1110000 1000000 1000000 1000000 1000000
             * 1100000 1100000 1100000 1000000
             * 1100000 1100000 1000000 1000000 1000000
             * 1100000 1000000 1000000 1000000 1000000 1000000
             * 1000000 1000000 1000000 1000000 1000000 1000000 100000
             * 
             * 11111111
             * 11111110 10000000
             * 11111100 11000000
             * 11111100 10000000 10000000
             * 11111000 11100000
             * 11111000 11000000 10000000
             * 11111000 10000000 10000000 10000000
             * 11110000 11110000
             * 11110000 11100000 10000000
             * 11110000 11000000 11000000
             * 11110000 11000000 10000000 10000000
             * 11110000 10000000 10000000 10000000 10000000
             * 11100000 11100000 11000000
             * 11100000 11100000 10000000 10000000
             * 11100000 11000000 11000000 1000000
             * 11100000 11000000 10000000 10000000 10000000
             * 11100000 10000000 10000000 10000000 10000000 10000000
             * 11000000 11000000 11000000 11000000
             * 11000000 11000000 11000000 10000000 10000000 
             * 11000000 11000000 10000000 10000000 10000000 10000000 
             * 11000000 10000000 10000000 10000000 10000000 10000000 10000000 
             * 10000000 10000000 10000000 10000000 10000000 10000000 10000000 10000000 
             * 
             * 111111111
             * 111111110 100000000
             * 111111100 110000000
             * 111111100 100000000 100000000
             * 111111000 111000000
             * 111111000 110000000 100000000
             * 111111000 100000000 100000000 100000000
             * 111110000 111100000
             * 111110000 111000000 100000000
             * 111110000 110000000 110000000
             * 111110000 110000000 100000000 100000000
             * 111110000 100000000 100000000 100000000 100000000
             * 111100000 111100000 100000000
             * 111100000 111000000 110000000
             * 111100000 111000000 100000000 100000000
             * 111100000 110000000 110000000 100000000
             * 111100000 110000000 100000000 100000000 100000000
             * 111100000 100000000 100000000 100000000 100000000 100000000 
             * 111000000 111000000 111000000
             * 111000000 111000000 110000000 100000000 
             * 111000000 111000000 100000000 100000000 100000000 
             * 111000000 110000000 110000000 110000000
             * 111000000 110000000 110000000 100000000 100000000 
             * 111000000 110000000 100000000 100000000 100000000 100000000 
             * 111000000 100000000 100000000 100000000 100000000 100000000 100000000 
             * 110000000 110000000 110000000 110000000 100000000 
             * 110000000 110000000 110000000 100000000 100000000 100000000 
             * 110000000 110000000 100000000 100000000 100000000 100000000 100000000 
             * 110000000 100000000 100000000 100000000 100000000 100000000 100000000 100000000 
             * 100000000 100000000 100000000 100000000 100000000 100000000 100000000 100000000 100000000 
             * 
             * 30 for 9, 22 for 8, 15 for 7, 11 for 6, 7 for 5, 5 for 4, 3 for 3, 2 for 2, 1 for 1
             */


        static List<Decimal> ListOfOdds(TreeNode<DiceConstruct> diceTree, int numSuccessesNeeded, int numSuccessesReachable)
        {
            if (diceTree == null) return new List<Decimal>();
            if (numSuccessesReachable < numSuccessesNeeded)
            {
                Decimal scorePiece = diceTree.Value.OddsOnSymbol(1);
                return new List<decimal>().Append(scorePiece).Concat(ListOfOdds());
            }
            //diceTree.Select(a => a.OddsOnSymbol(1)).Aggregate
            System.Linq.Enumerable.Aggregate<Decimal>(System.Linq.Enumerable.Select<DiceConstruct,Decimal>(diceTree, a => a.OddsOnSymbol(1)),(a,b) => 1);
            //diceTree.Aggregate()
        }

        static TreeNode<DiceConstruct> ConstructTreeOfDice(int numberSidesPerDie, int maxNumberSymbolsPerDie, int numberSymbolsToConsume)
        {
            int maxOnThisDie = Math.Min(numberSidesPerDie, maxNumberSymbolsPerDie);
            if (maxOnThisDie < 1 || numberSymbolsToConsume < 1)
            {
                return null;
            }
            DiceConstruct newDie;
            if (numberSymbolsToConsume > maxOnThisDie)
            {
                int newNumberSymbolsToConsume = numberSymbolsToConsume - maxOnThisDie;
                newDie = DiceConstruct.ConstructWithNSidesOfXSymbol(1, maxOnThisDie, numberSidesPerDie);
                TreeNode<DiceConstruct> node = new TreeNode<DiceConstruct>(newDie);
                List<TreeNode<DiceConstruct>> children = new List<TreeNode<DiceConstruct>>();
                while (newNumberSymbolsToConsume > 0)
                {
                    TreeNode<DiceConstruct> child = ConstructTreeOfDice(numberSidesPerDie, maxNumberSymbolsPerDie, newNumberSymbolsToConsume);
                    if(child != null)
                    {
                        children.Add(child);
                    }
                    newNumberSymbolsToConsume--;
                }                
                node.AddChildren(children);
                return node;
            }
            newDie = DiceConstruct.ConstructWithNSidesOfXSymbol(1, numberSymbolsToConsume, numberSidesPerDie);
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
