using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace DieFaceDistributerWebApp.Models
{
    public class SingleSymbolRequest
    {
        [Display(Name = "Number of Winning Symbols (Symbol Copies) Available")]
        public int NumberOfSymbol { get; set; }
        [Display(Name = "Target Number of Minimum Needed Number of Winning Symbols to Roll for Success")]
        public int TargetNumber { get; set; }
        [Display(Name = "Number of Sides on each Die")]
        public int NumSidesPerDie { get; set; }
        [Display(Name = "Minimum Number of Dice Allowed in a Result Grouping of Dice")]
        public int MinNumberOfDice { get; set; }
        [Display(Name = "Maximum Number of Dice Allowed in a Result Grouping of Dice")]
        public int MaxNumberOfDice { get; set; }
    }
}
