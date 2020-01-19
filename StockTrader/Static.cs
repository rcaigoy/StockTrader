using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrader
{

    public class Symbol
    { 
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class PriceCheck
    {
        public string symbol { get; set; }
        public List<HistoricalDataResponse> prices = new List<HistoricalDataResponse>();
        public double latestPrice { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

        public double GetTradePercent()
        {
            double d;

            double high = prices[6].high;
            if (high < prices[5].high)
                high = prices[5].high;
            if (high < prices[4].high)
                high = prices[4].high;

            d = (high - prices[0].close) / prices[0].close;

            return d / 2 * 100;
        }
    }

    public class Interval
    {
        public string symbol { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double open { get; set; }
        public double close { get; set; }
        public double volume { get; set; }
        public double change { get; set; }
        public int interval { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

    }

    public class IntervalCheck
    {
        public string symbol { get; set; }
        public int interval { get; set; }
        public List<Interval> intervals = new List<Interval>();
        public double latestPrice { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

        public double GetTradePercent()
        {
            double d;

            double high = intervals[6].high;
            if (high < intervals[5].high)
                high = intervals[5].high;
            if (high < intervals[4].high)
                high = intervals[4].high;

            d = (high - intervals[0].close) / intervals[0].close;

            return d / 2 * 100;
        }

    }
}
