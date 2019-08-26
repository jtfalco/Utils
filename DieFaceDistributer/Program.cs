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
            int numberOfSymbol = 0, maxNumberOfDice = int.MaxValue, minNumberOfDice = 1;
            if(args.Length > 0 && int.TryParse(args[0], out numberOfSymbol))
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

        static List<Tuple<TreeNode<DiceConstruct>,Decimal>> FigureWithOneSymbol(int numberOfSymbol, int targetNumber, int sidesPerDie = 6, int maxNumberOfDice = int.MaxValue, int minNumberOfDice = 1)
        {
            List<Tuple<TreeNode<DiceConstruct>, Decimal>> answer = new List<Tuple<TreeNode<DiceConstruct>, Decimal>>();
            
            int actualMax = Math.Min(numberOfSymbol, maxNumberOfDice);
            int actualMin = Math.Max((int)Math.Ceiling((decimal)numberOfSymbol / (decimal)sidesPerDie), minNumberOfDice);
            if (actualMax < actualMin) throw new ArgumentException("Cannot mathematically figure this one out.  Actual maximum possible dice: " 
                + actualMax.ToString() + ". Actual minumum possible dice: " + actualMin.ToString() + ".");
            TreeNode<DiceConstruct> possibleAnswer = new TreeNode<DiceConstruct>(DiceConstruct.ConstructWithNSidesOfXSymbol(1, 6, 6));
            for (int i = 0; i < numberOfSymbol; i++)
            {
                possibleAnswer.Add(DiceConstruct.ConstructWithNSidesOfXSymbol(1, 1, 6));                
            }
            Tuple<TreeNode<DiceConstruct>, Decimal> entry = new Tuple<TreeNode<DiceConstruct>, decimal>(possibleAnswer, FigureScore(possibleAnswer));
            answer.Add(entry);

            
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



            answer = answer.OrderBy(a => a.Item2).ToList();
            return answer;
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
