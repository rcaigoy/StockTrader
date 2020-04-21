using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrader
{
    interface IBuyAlgorithm
    {
        string BuyAlgorithmName { get; set; }
        AccuracyRating AccuracyRating { get; set; }
        //gets each stock's potential growth
        List<StockPotential> GetStockPotentials(DateTime Date);

        //checks 1 day, 1 week, 1 month, and full year potential based on day selected
        AccuracyRating CheckAccuracy(string Symbol, DateTime Date, int NumberOfTries);
    }

    public class StockPotential
    {
        public string Symbol { get; set; }
        public decimal PercentPotential { get; set; }
        
        public DateTime Date { get; set; }
    }

    public class AccuracyRating
    { 
        public string BuyAlgorithmName { get; set; }
        public int OneDayTries { get; set; }
        public decimal OneDaySuccessRate { get; set; }
        public int OneWeekTries { get; set; }
        public decimal OneWeekSuccessRate { get; set; }
        public int OneMonthTries { get; set; }
        public decimal OneMonthSuccessRate { get; set; }
        public int OneYearTries { get; set; }
        public decimal OneYearSuccessRate { get; set; }
    }

}
