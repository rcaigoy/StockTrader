using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Alpaca.Markets;

namespace StockTrader
{
    public class Buyer
    {
        private static string stockDirectory = Switches.stockDirectory;

        public static List<Symbol> Symbols = new List<Symbol>();

        //checks last 7 days for possible increase next day
        public static List<PriceCheck> LList = new List<PriceCheck>();
        public static List<IntervalCheck> DayLList = new List<IntervalCheck>();

        //checks last 7 weeks for possible increase next week
        public static List<IntervalCheck> WeekLList = new List<IntervalCheck>();

        public static List<IntervalCheck> MonthLList = new List<IntervalCheck>();

        public Buyer(List<Symbol> _Symbols)
        {
            Symbols = _Symbols;
        }

        public static void Run()
        {
            //Calculate Day Lists from All Stocks csv where Is Active
            foreach (var p in Symbols.Where(x => x.IsActive))
            {

            }//end foreach (var p in Symbols.Where(x => x.IsActive))

            //Calculate Week List from csv from All Stocks csv where Is Active

            //Calculate Month List from csv from All Stocks csv where Is Active

            //Buy top 3 10% from day list

            //Buy top 3 10% from week list that aren't in day list

            //Buy top 3 10% from Month list not in day or week list

            //Use all extra funds for next month list
        }


        public static void CheckToAddSymbolDynamic(string symbol, DateTime dt, int _interval, List<IntervalCheck> intervalChecks)
        {
            IntervalCheck ic = GetIntervalCheck(symbol, dt, _interval);
            //PriceCheck p = new PriceCheck();
            //p.symbol = symbol;
            //DateTime tempDay = GetPreviousTradeDay(symbol, dt);
            //checks past inteval * days 7 times

            //p.endDate = tempDay;

            double latestPrice = ic.intervals[0].close;
            //p.latestPrice = latestPrice;
            int lowpoints = 0;

            if (ic.intervals[1].low < latestPrice)
            {
                lowpoints++;
            }
            if (ic.intervals[2].low < latestPrice)
            {
                lowpoints++;
            }
            if (ic.intervals[3].low < latestPrice)
            {
                lowpoints++;
            }
            int highpoints = 0;
            if (ic.intervals[4].high > latestPrice)
            {
                highpoints++;
            }
            if (ic.intervals[5].high > latestPrice)
            {
                highpoints++;
            }
            if (ic.intervals[6].high > latestPrice)
            {
                highpoints++;
            }

            if (highpoints > 1 && lowpoints > 1)
            {
                intervalChecks.Add(ic);
            }
        }//end CheckToAddSymbolDynamic


        public static IntervalCheck GetIntervalCheck(string symbol, DateTime endDate, int _interval)
        {
            IntervalCheck ic = new IntervalCheck();
            ic.symbol = symbol;
            ic.endDate = endDate;
            ic.interval = _interval;
            DateTime tempEndDate = endDate;
            Interval tempInterval = GetInterval(symbol, endDate, _interval);
            for (int i = 0; i < 7; i++)
            {
                ic.intervals.Add(tempInterval);
                tempEndDate = GetPreviousTradeDay(symbol, tempInterval.startDate);
                tempInterval = GetInterval(symbol, tempEndDate, _interval);
            }
            ic.latestPrice = ic.intervals[0].close;
            ic.startDate = ic.intervals[6].startDate;

            return ic;
        }


        public static Interval GetInterval(string symbol, DateTime endDate, int _interval)
        {
            Interval i = new Interval();

            i.symbol = symbol;
            i.endDate = endDate;
            i.interval = _interval;

            //last day algorithms
            i.close = HistoricalDataResponse.GetHistoricalData(symbol, endDate).close;

            int counter = 0;
            DateTime tempDay = endDate;
            double low = 9999999;
            double high = 0;
            double volume = 0;
            HistoricalDataResponse tempHistoricalData = HistoricalDataResponse.GetHistoricalData(symbol, tempDay);
            for (; counter < _interval - 1; counter++)
            {
                //low
                double tempLow = tempHistoricalData.low;
                if (low > tempLow)
                    low = tempLow;

                //high
                double tempHigh = tempHistoricalData.high;
                if (high < tempHigh)
                    high = tempHigh;

                //volume
                volume += tempHistoricalData.volume;
                tempDay = GetPreviousTradeDay(symbol, tempDay);
                tempHistoricalData = HistoricalDataResponse.GetHistoricalData(symbol, tempDay);

            }
            double open = tempHistoricalData.open;
            DateTime startDate = tempDay;
            //low
            double tempLow2 = tempHistoricalData.low;
            if (low > tempLow2)
                low = tempLow2;

            //high
            double tempHigh2 = tempHistoricalData.high;
            if (high < tempHigh2)
                high = tempHigh2;

            //volume
            volume += tempHistoricalData.volume;


            //first day algorithms
            //i.open = GetOpen(symbol, endDate, _interval);
            //for (; counter < _interval - 1; tempDay = GetPreviousTradeDay(symbol, tempDay), counter++);
            i.open = open;
            //i.startDate = GetIntervalStartDate(symbol, endDate, _interval);
            //for (int i = 0; i < _interval - 1; i++, d = GetPreviousTradeDay(symbol, d)) ;
            i.startDate = startDate;

            //each day algorithms
            //i.high = GetHigh(symbol, endDate, _interval);
            //for (DateTime tempDay = endDate; counter < _interval; tempDay = GetPreviousTradeDay(symbol, tempDay), counter++)
            i.high = high;
            //i.low = GetLow(symbol, endDate, _interval);
            //for (DateTime tempDay = endDate; counter < _interval; tempDay = GetPreviousTradeDay(symbol, tempDay), counter++)
            i.low = low;
            //i.volume = GetVolume(symbol, endDate, _interval);
            //for (DateTime tempDay = endDate; counter < _interval; tempDay = GetPreviousTradeDay(symbol, tempDay), counter++)
            i.volume = volume;

            return i;
        }


        public static DateTime GetPreviousTradeDay(string symbol, DateTime d)
        {
            try
            {
                DateTime dt = d.AddDays(-1);
                var allLines = File.ReadAllLines(stockDirectory + symbol + @"\" + d.Year + @"\" + d.Month + @"\" + symbol + ".csv");
                int i = 1;
                string line = allLines[1].Split(',')[0];
                for (; i < allLines.Count() && d > Convert.ToDateTime(line); i++)
                {
                    //Console.WriteLine(i);
                    string[] temp = allLines[i].Split(',');
                    line = temp[0];
                }
                if (i > 1)
                    i--;
                var values = allLines[i].Split(',');

                if (i == 1)
                {
                    if (d.Month == 1)
                    {
                        var allLines2 = File.ReadAllLines(stockDirectory + symbol + @"\" + (d.Year - 1).ToString() + @"\" + 12.ToString() + @"\" + symbol + ".csv");
                        string lastDate = allLines2[allLines2.Count() - 1].Split(',')[0];
                        return Convert.ToDateTime(lastDate);
                    }
                    else
                    {
                        var allLines2 = File.ReadAllLines(stockDirectory + symbol + @"\" + d.Year.ToString() + @"\" + (d.Month - 1).ToString() + @"\" + symbol + ".csv");
                        string lastDate = allLines2[allLines2.Count() - 1].Split(',')[0];
                        return Convert.ToDateTime(lastDate);
                    }
                }
                else
                {
                    return Convert.ToDateTime(allLines[i - 1].Split(',')[0]);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw;
            }

        }//end public static DateTime GetPreviousTradeDay(string symbol, DateTime d)



    }//end public class Buyer
}
