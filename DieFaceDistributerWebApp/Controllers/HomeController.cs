using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DieFaceDistributerWebApp.Models;
using DieFaceDistributer;
using Rationals;

namespace DieFaceDistributerWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet, HttpPost]
        public IActionResult SingleDieMaximizeSingleSymbolOdds(SingleSymbolRequest request)
        {
            /*
            foreach (Tuple<IEnumerable<Dice>, Rational> pairing in pairingsOfDiceSetsToTheirOddsOfGettingTheTN)
            { //"FormattingForDice", "FormattingForOdds", "FormattingBetween"
                textToDisplay.Add(SerializeForConsole(pairing.Item1, pairing.Item2, formatFriendlyArgs["FormattingForDice"], formatFriendlyArgs["FormattingForOdds"], formatFriendlyArgs["FormattingBetween"]));
            }
            textToDisplay.ForEach(a => Console.WriteLine(a));
            */
            
            if (HttpContext.Request.Method == "GET") return View();
            IEnumerable<Tuple<IEnumerable<Dice>, Rational>> pairingsOfDiceSetsToTheirOddsOfGettingTheTN;
            pairingsOfDiceSetsToTheirOddsOfGettingTheTN = SingleWinningSymbolHelper.ShowNumbersOfWinningSymbolsAndOdds(request.NumberOfSymbol, request.TargetNumber, request.MinNumberOfDice, request.MaxNumberOfDice, request.NumSidesPerDie);
            return View(pairingsOfDiceSetsToTheirOddsOfGettingTheTN.Select(a => new SingleSymbolEvaluation { Dice = a.Item1, OddsOfGettingDesiredTN = a.Item2}));            
        }

        [HttpGet]
        public IActionResult Prepare()
        {
            return View();
        }

    }
}
