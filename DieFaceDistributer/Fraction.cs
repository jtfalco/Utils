using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DieFaceDistributer
{
    class Fraction : IComparable
    {
        public long Numerator { get; set; }
        public long Denominator {
            get {
                return denominator;
            }
            set {
                if (value == 0) throw new ArgumentException("Cannot make a fraction with a denominator of 0."); denominator = value;
            }
        }
        private long denominator;

        public Fraction()
        {
            Numerator = 0;
            Denominator = 1;
        }

        public Fraction(long Numerator, long Denominator)
        {
            this.Numerator = Numerator;
            this.Denominator = Denominator;
        }

        public Fraction Inverted()
        {
            return new Fraction { Numerator = this.Denominator, Denominator = this.Numerator };
        }
        public void Reduce()
        {            
            for(int i = 2; i < Numerator && i < Denominator; i++)
            {
                while(Numerator % i == 0 && Denominator % i == 0)
                {
                    Numerator /= i;
                    Denominator /= i;
                }
            }
        }

        public override string ToString()
        {
            return Numerator.ToString() + "/" + Denominator.ToString();
        }

        public static void MakeToCommonDenominator(params Fraction[] fracs)
        {
            long lcd = FindCommonDenominator(fracs);            
            for(int i = 0; i < fracs.Length; i++)
            {
                long division = lcd / fracs[i].Denominator;
                fracs[i].Numerator *= division;
                fracs[i].Denominator *= division;
            }
        }
        public static long FindCommonDenominator(params Fraction[] fracs)
        {
            long lcdenom = LCM(fracs.Select(a => a.Denominator));
            return lcdenom;
        }

        private static long LCM(params long[] nums)
        {
            return LCM(nums.AsEnumerable<long>());
        }

        private static long LCM(IEnumerable<long> numbers)
        {
            return numbers.Aggregate(LowestCommonMultiple);
        }
        private static long LowestCommonMultiple(long a, long b)
        {
            return Math.Abs(a * b) / GCD(a, b);
        }
        private static long GCD(long a, long b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }

        public static Fraction operator +(Fraction temp1, Fraction temp2)
        {
            Fraction x = new Fraction(temp1.Numerator, temp1.Denominator), y = new Fraction(temp2.Numerator, temp2.Denominator);
            MakeToCommonDenominator(x, y);
            Fraction result = new Fraction { Numerator = x.Numerator + y.Numerator, Denominator = x.Denominator };
            result.Reduce();            
            return result;
        }

        public static Fraction operator -(Fraction temp1, Fraction temp2)
        {
            Fraction x = new Fraction(temp1.Numerator, temp1.Denominator), y = new Fraction(temp2.Numerator, temp2.Denominator);
            MakeToCommonDenominator(x, y);
            Fraction result =  new Fraction { Numerator = x.Numerator - y.Numerator, Denominator = x.Denominator };
            result.Reduce();
            return result;
        }

        public static Fraction operator *(Fraction x, Fraction y)
        {
            return new Fraction { Numerator = x.Numerator * y.Numerator, Denominator = x.Denominator * y.Denominator };
        }

        public static Fraction operator /(Fraction x, Fraction y)
        {
            return x * y.Inverted();
        }

        public int CompareTo(Object f)
        {
            if (f == null) return 1;
            Fraction otherFraction = f as Fraction;
            Fraction temp1 = new Fraction(Numerator, Denominator), temp2 = new Fraction(otherFraction.Numerator, otherFraction.Denominator);
            MakeToCommonDenominator(temp1, temp2);
            return temp1.Numerator.CompareTo(temp2.Numerator);
        }

        public static bool operator <(Fraction temp1, Fraction temp2)
        {
            Fraction x = new Fraction(temp1.Numerator, temp1.Denominator ), y = new Fraction(temp2.Numerator, temp2.Denominator);
            MakeToCommonDenominator(x, y);
            return x.Numerator < y.Numerator;
        }

        public static bool operator >(Fraction temp1, Fraction temp2)
        {
            Fraction x = new Fraction(temp1.Numerator, temp1.Denominator), y = new Fraction(temp2.Numerator, temp2.Denominator);
            MakeToCommonDenominator(x, y);
            return x.Numerator > y.Numerator;
        }

        public static bool operator ==(Fraction temp1, Fraction temp2)
        {
            Fraction x = new Fraction(temp1.Numerator, temp1.Denominator), y = new Fraction(temp2.Numerator, temp2.Denominator);
            MakeToCommonDenominator(x, y);
            return x.Numerator == y.Numerator;
        }

        public static bool operator !=(Fraction temp1, Fraction temp2)
        {
            Fraction x = new Fraction(temp1.Numerator, temp1.Denominator), y = new Fraction(temp2.Numerator, temp2.Denominator);
            MakeToCommonDenominator(x, y);
            return x.Numerator != y.Numerator;
        }

        public override bool Equals(object f)
        {
            if (f.GetType() == this.GetType())
            {
                Fraction temp1 = new Fraction { Numerator = this.Numerator, Denominator = this.Denominator };
                Fraction temp2 = new Fraction { Numerator = ((Fraction)f).Numerator, Denominator = ((Fraction)f).Denominator };
                MakeToCommonDenominator(temp1, temp2);                
                return temp1.Numerator == temp2.Numerator;
            }
            return base.Equals(f);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    
}
