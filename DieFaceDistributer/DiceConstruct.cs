using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DieFaceDistributer
{
    class DiceConstruct
    {
        public const int Blank = 0;
        public int NumberOfSides {
            get { return SideSymbolIds.Length; }
            set {
                if(value > 0) {
                    SideSymbolIds = new int[value];
                    Array.Fill(SideSymbolIds, Blank);
                } else {
                    throw new ArgumentException("Dice cannot have a number of sides that is less than 1.");
                }
            }
        }
        public int[] SideSymbolIds { get; set; }
        public bool IsEmpty()
        {
            return SideSymbolIds.All(a => a == Blank);
        }
        public int EmptyCount()
        {
            int result = 0;
            SideSymbolIds.Aggregate((a, b) => 1);
            return result;
        }

        public int Roll()
        {
            Random r = new Random();
            return SideSymbolIds[r.Next(0, NumberOfSides-1)];
        }

        public int RollWithSeed(int seed)
        {
            Random r = new Random(seed);
            return SideSymbolIds[r.Next(0, NumberOfSides-1)];            
        }

        public Decimal OddsOnSymbol(int symbol)
        {
            return (Decimal)(SideSymbolIds.Aggregate((total, aggregand) => total + (aggregand == symbol ? 1 : 0))) / (Decimal)NumberOfSides;
        }

        public DiceConstruct Copy()
        {
            DiceConstruct res = new DiceConstruct { NumberOfSides = SideSymbolIds.Length };            
            Array.Copy(SideSymbolIds, res.SideSymbolIds, NumberOfSides);
            return res;
        }

        public DiceConstruct MergeNonBlanks(DiceConstruct merging)
        {
            DiceConstruct res = Copy();            
            res.SideSymbolIds.OrderBy(a => a);
            merging.SideSymbolIds.OrderByDescending(a => a);
            for(int i = 0; i < merging.NumberOfSides && i < res.NumberOfSides; i++)
            {
                if(res.SideSymbolIds[i] == Blank)
                {
                    res.SideSymbolIds[i] = merging.RemoveSide(i);
                }
            }
            return res;
        }

        public int RemoveSide(int index)
        {
            int result = 0;
            if(index < 0)
            {
                throw new ArgumentException("Must count a side from at least 0.  You asked to remove a side less than the 0th side from a die.");
            }
            if(index < NumberOfSides)
            {
                result = SideSymbolIds[index];
                SideSymbolIds[index] = Blank;
                return result;
            }
            throw new ArgumentException("Cannot reference side " + index.ToString() + ": only " + NumberOfSides.ToString() + " sides on this die.  (Sides counted from 0)");
        }

        public static DiceConstruct ConstructWithNSidesOfXSymbol(int x, int n, int totalNumberOfSides)
        {
            DiceConstruct construct = new DiceConstruct { NumberOfSides = totalNumberOfSides };
            for(int i = 0; i < totalNumberOfSides && i < n; i++)
            {
                construct.SideSymbolIds[i] = x;
            }
            return construct;
        }

        public static DiceConstruct ConstructWithEnumerableSidesOfEnumerableSymbols(IEnumerable<int> symbols, IEnumerable<int> numbersOfSides, int totalNumberOfSides)
        {
            DiceConstruct construct = new DiceConstruct { NumberOfSides = totalNumberOfSides };
            int i = 0, accumulator = 0;
            IEnumerator<int> symEnum = symbols.GetEnumerator(), sidEnum = numbersOfSides.GetEnumerator();
            while(symEnum.MoveNext() && sidEnum.MoveNext())
            {
                for(; i - accumulator < sidEnum.Current && i < totalNumberOfSides;i++)
                {
                    construct.SideSymbolIds[i] = symEnum.Current;
                }
                accumulator = i;
            }
            return construct;
        }

        private static readonly string[] endCaps = { "{", "}" };        //This should always have two elements, and neither, particularly not the second one, should start with a digit.
        private static string separator = ",";
        public string Serialize()
        {
            return SerializeToBuilder().ToString();                
        }

        public StringBuilder SerializeToBuilder()
        {
            StringBuilder builder = new StringBuilder(endCaps[0], NumberOfSides * 3);
            for (int i = 0; i < SideSymbolIds.Length; i++)
            {                
                builder.Append(SideSymbolIds.Select<int, string>(symbolId => symbolId.ToString()).Aggregate((a, b) => a + separator + b));                
            }
            return builder.Append(endCaps[1]);            
        }

        public static DiceConstruct Deserialize(string input)
        {
            if(!(input.IndexOf(endCaps[0]) == 0))
            {
                throw new ArgumentException("Couldn't read start sequence of '" + endCaps[0] + "' from beginning of input.");
            }
            int startOfEndIndex = input.LastIndexOf(endCaps[1]);
            if(!(startOfEndIndex + endCaps[1].Length == input.Length))
            {
                throw new ArgumentException("Couldn't read end sequence of '" + endCaps[1] + "' from end of input.");
            }
            List<int> listOfSymbolIds = new List<int>();
            char[] anies = { endCaps[1][0], separator[0] };            
            int currentIndex = endCaps[0].Length;
            int nextIndex;
            while(currentIndex < startOfEndIndex)
            {
                nextIndex = input.IndexOfAny(anies, currentIndex);
                string parseable = input.Substring(currentIndex, nextIndex - currentIndex);                
                if(!int.TryParse(parseable, out int nextSymbolId))
                {
                    throw new ArgumentException("Couldn't read integer symbol id.");
                }
                listOfSymbolIds.Add(nextSymbolId);
                currentIndex = nextIndex + 1;
            }
            return new DiceConstruct { SideSymbolIds = listOfSymbolIds.ToArray()};
        }

        public static List<string> TestSerializationSamples()
        {
            List<string> errors = new List<string>();
            int[] SymbolIds = { 1 };
            DiceConstruct dc = new DiceConstruct{ SideSymbolIds = SymbolIds};
            errors.AddRange(TestSerialization(dc, "{1}"));
            return errors;
        }

        public static List<string> TestSerialization(DiceConstruct serializeMe, string serializationResult)
        {
            List<string> errors = new List<string>();
            if (serializeMe.Serialize() != serializationResult)
            {
                StringBuilder b = new StringBuilder("[");
                for(int i = 0; i < serializeMe.NumberOfSides; i++)
                {
                    b.Append(serializeMe.SideSymbolIds[i].ToString());
                    if(i < serializeMe.NumberOfSides - 1)
                    {
                        b.Append(",");
                    }                    
                }
                b.Append("]");
                errors.Add("Didn't serialize a " + serializeMe.NumberOfSides + "-sided die with sides of " + b.ToString() + " correctly");
            }
            return errors;
        }

        public static List<string> TestDeserializationSamples()
        {
            List<string> errors = new List<string>();
            int[] SymbolIds = { 1 };
            errors.AddRange(TestDeserialization("{1}", new DiceConstruct{SideSymbolIds = SymbolIds }));
            SymbolIds = SymbolIds.Append(2).ToArray();
            errors.AddRange(TestDeserialization("{2,1}", new DiceConstruct{ SideSymbolIds = SymbolIds }));
            return errors;
        }

        public static List<string> TestDeserialization(string deserializeMe, DiceConstruct checkMe)
        {
            List<string> errors = new List<string>();
            DiceConstruct newConstruct = DiceConstruct.Deserialize(deserializeMe);
            IEnumerable<int> intersection = checkMe.SideSymbolIds.Intersect(newConstruct.SideSymbolIds);
            if(intersection.Count() != newConstruct.NumberOfSides || intersection.Count() != checkMe.NumberOfSides || intersection.Count() != newConstruct.SideSymbolIds.Length || intersection.Count() != checkMe.SideSymbolIds.Length)
            {
                StringBuilder b = new StringBuilder("[");
                for (int i = 0; i < checkMe.NumberOfSides; i++)
                {
                    b.Append(checkMe.SideSymbolIds[i].ToString());
                    if (i < checkMe.NumberOfSides - 1)
                    {
                        b.Append(",");
                    }                    
                }
                b.Append("]");
                errors.Add("Did not properly deserialize \"" + deserializeMe + "\" into a " + checkMe.NumberOfSides + "-sided die with sides of " + b.ToString());
            }            
            return errors;
        }
    }
}
