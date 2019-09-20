using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace DieFaceDistributerWebApp.Models
{
    public class SingleSymbolRequest
    {
        [Display(Name = "Number of Winning Symbols (Symbol Copies) Available")]
        [DefaultValue(3)]
        public int NumberOfSymbol { get; set; }
        [Display(Name = "Target Number of Minimum Needed Number of Winning Symbols to Roll for Success")]
        [DefaultValue(1)]
        public int TargetNumber { get; set; }
        [Display(Name = "Number of Sides on each Die")]
        [DefaultValue(6)]
        public int NumSidesPerDie { get; set; }
        [Display(Name = "Minimum Number of Dice Allowed in a Result Grouping of Dice")]   
        [DefaultValue(1)]
        public int MinNumberOfDice { get; set; }
        [Display(Name = "Maximum Number of Dice Allowed in a Result Grouping of Dice")]
        [DefaultValue(200000)]
        public int MaxNumberOfDice { get; set; }
    }
}
