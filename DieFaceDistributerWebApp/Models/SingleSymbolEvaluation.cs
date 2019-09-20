using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DieFaceDistributer;
using Rationals;

namespace DieFaceDistributerWebApp.Models
{
    public class SingleSymbolEvaluation
    {
        [Display(Name = "A Result Grouping of Dice")]
        public IEnumerable<Dice> Dice { get; set; }
        [Display(Name = "Odds of Success")]
        public Rational OddsOfGettingDesiredTN { get; set; }

        #region ReadOnlies
        //These were, presumably, the values provided when the user made the request that generated this Evaluation Result...
        [Display(Name = "Number of Winning Symbols (Symbol Copies) Available")]
        public int NumberOfSymbol { get; }
        [Display(Name = "Target Number of Minimum Needed Number of Winning Symbols to Roll for Success")]
        public int TargetNumber { get; }
        [Display(Name = "Number of Sides on each Die")]
        public int NumSidesPerDie { get; }
        [Display(Name = "Minimum Number of Dice Allowed in a Result Grouping of Dice")]
        public int MinNumberOfDice { get; }
        [Display(Name = "Maximum Number of Dice Allowed in a Result Grouping of Dice")]
        public int MaxNumberOfDice { get; }
        #endregion ReadOnlies
    }
}
