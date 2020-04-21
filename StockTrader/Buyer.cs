using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Net;
using System.Net.Http;

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

        public static List<IntervalCheck> SymbolsToCheck2 = new List<IntervalCheck>();

        public static List<IntervalCheck> SymbolsToCheck3 = new List<IntervalCheck>();

        public Buyer(List<Symbol> _Symbols)
        {
            Symbols = _Symbols;
        }

        public async Task Run()
        {
            //Calculate Lists from All Stocks csv where Is Active
            foreach (var p in Symbols.Where(x => x.IsActive))
            {
                CheckToAddSymbolDynamic(p.Name, DateTime.Today.AddDays(-1), 1, DayLList);
                CheckToAddSymbolDynamic(p.Name, DateTime.Today.AddDays(-1), 5, WeekLList);
                CheckToAddSymbolDynamic(p.Name, DateTime.Today.AddDays(-1), 20, MonthLList);
            }//end foreach (var p in Symbols.Where(x => x.IsActive))

            //order lists
            DayLList = DayLList.OrderBy(x => x.GetTradePercent()).ToList();
            WeekLList = WeekLList.OrderBy(x => x.GetTradePercent()).ToList();
            MonthLList = MonthLList.OrderBy(x => x.GetTradePercent()).ToList();

            //Create Purchase Lists
            CreatePurchaseLists();

            //Buy
            Buy(DayLList, SymbolsToCheck2, SymbolsToCheck3).Wait();
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


        public void CreatePurchaseLists()
        {
            try
            {
                //create SymbolsToCheck2
                foreach (var p in DayLList)
                {
                    foreach (var p2 in WeekLList)
                    {
                        foreach (var p3 in MonthLList)
                        {
                            if (p.symbol == p2.symbol && p.symbol == p3.symbol)
                            {
                                SymbolsToCheck3.Add(p);
                            }
                        }
                    }
                }//end foreach (var p in DayLList)

                foreach(var p in DayLList)
                {
                    foreach(var p2 in WeekLList)
                    {
                        if (p.symbol == p2.symbol)
                        {
                            SymbolsToCheck2.Add(p);
                        }
                    }
                }//end foreach(var p in DayLList)
            }
            catch (Exception ex) 
            {
                throw Utility.ThrowException(ex);
            }
        }


        public async Task Buy(List<IntervalCheck> singles, List<IntervalCheck> doubles, List<IntervalCheck> triples)
        {
            try
            {
                 AlpacaTradingClientConfiguration config = new AlpacaTradingClientConfiguration();
                config.ApiEndpoint = new Uri(Switches.AlpacaEndPoint());
                //config.KeyId = Switches.AlpacaAPIKey();
                config.SecurityId = new SecretKey(Switches.AlpacaAPIKey(), Switches.AlpacaSecretAPIKey());
                var restClient = new AlpacaTradingClient(config);

                var account = await restClient.GetAccountAsync();

                List<string> purchaseList = new List<string>();

                double BuyingPower = (double)account.BuyingPower;
                int lastCounter = 0;
                int triplesBought = 0;
                for (int i = 0; triplesBought < 3 && i < triples.Count(); lastCounter = i, i++)
                {

                    for (; InPortfolio(triples[i].symbol) || contains(triples[i].symbol, purchaseList); i++) ;

                    double price;
                    try
                    {
                        price = GetPrice(triples[i].symbol);
                    }
                    catch (Exception ex)
                    {
                        price = triples[i].intervals[0].close;
                    }
                    int j = 0;
                    for (; price * j < BuyingPower / 10; j++) ;
                    if (j > 1)
                    {
                        try
                        {
                            j--;
                            var order = await restClient.PostOrderAsync(triples[i].symbol, j, OrderSide.Buy, OrderType.Market, TimeInForce.Day);
                            triplesBought++;
                            purchaseList.Add(triples[i].symbol);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                int doublesBought = 0;
                for (int i = 0; doublesBought < 3 && i < doubles.Count(); i++)
                {
                    for (; InPortfolio(doubles[i].symbol) || contains(doubles[i].symbol, purchaseList); i++) ;

                    double price;
                    try
                    {
                        price = GetPrice(doubles[i].symbol);
                    }
                    catch (Exception ex)
                    {
                        price = doubles[i].intervals[0].close;
                    }
                    int j = 0;
                    for (; price * j < BuyingPower / 10; j++) ;
                    if (j > 1)
                    {
                        try
                        {
                            j--;
                            var order = await restClient.PostOrderAsync(doubles[i].symbol, j, OrderSide.Buy, OrderType.Market, TimeInForce.Day);
                            doublesBought++;
                            purchaseList.Add(doubles[i].symbol);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                    }
                }

                int singlesBought = 0;
                for (int i = 0; singlesBought < 3 && i < doubles.Count(); i++)
                {
                    for (; InPortfolio(singles[i].symbol) || contains(singles[i].symbol, purchaseList); i++) ;

                    double price;
                    try
                    {
                        price = GetPrice(singles[i].symbol);
                    }
                    catch (Exception ex)
                    {
                        price = singles[i].intervals[0].close;
                    }

                    int j = 0;
                    for (; price * j < BuyingPower / 10; j++) ;
                    if (j > 1)
                    {
                        try
                        {
                            j--;
                            var order = await restClient.PostOrderAsync(singles[i].symbol, j, OrderSide.Buy, OrderType.Market, TimeInForce.Day);
                            singlesBought++;
                            purchaseList.Add(singles[i].symbol);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }

                try
                {
                    BuyLeftover(triples, purchaseList, lastCounter).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("cannot buy because " + ex.ToString());
                }
            }
            catch(Exception ex)
            {
                throw Utility.ThrowException(ex);
            }
        }//end public static async void Buy(List<IntervalCheck> singles, List<IntervalCheck> doubles, List<IntervalCheck> triples)


        public static bool InPortfolio(string symbol)
        {
            if (File.Exists(Switches.stockDirectory + "Portfolio.csv"))
            {
                var allLines2 = File.ReadAllLines(Switches.stockDirectory + "Portfolio.csv");
                foreach (var line in allLines2)
                    if (line.ToLower().Contains(symbol.ToLower()))
                        return true;
            }

            return false;
        }


        public static bool contains(string symbol, List<string> purchaseList)
        {
            foreach (var p in purchaseList)
            {
                if (p.ToLower() == symbol.ToLower())
                    return true;
            }
            return false;
        }


        public static double GetPrice(string symbol)
        {
            double d = 0;
            var IEXTrading_API_PATH = "https://cloud.iexapis.com/stable/stock/" + symbol + "/price?token=" + Switches.AlpacaAPIKey();// pk_d77c6f838027448cae27d946fa677249";

            WebRequest request = WebRequest.Create(IEXTrading_API_PATH);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            d = Convert.ToDouble(responseFromServer);

            return d;
        }


        public static async Task BuyLeftover(List<IntervalCheck> triples, List<string> purchaseList, int lastCounter)
        {
            var restClient = new RestClient(Switches.AlpacaAPIKey(), Switches.AlpacaSecretAPIKey(), Switches.AlpacaEndPoint());
            var account = await restClient.GetAccountAsync();
            double BuyingPower = (double)account.BuyingPower;
            bool lastBought = false;
            double expectation = 0.9;
            while (!lastBought && BuyingPower > 100 && expectation > 0)
            {

                for (int i = lastCounter; i < triples.Count() && !lastBought; i++)
                {
                    //for (; InPortfolio(triples[i].symbol); i++) ;
                    for (; InPortfolio(triples[i].symbol) || contains(triples[i].symbol, purchaseList); i++) ;
                    double price;
                    try
                    {
                        price = GetPrice(triples[i].symbol);
                    }
                    catch (Exception ex)
                    {
                        price = triples[i].intervals[0].close;
                    }
                    int j = 0;
                    for (; price * j < BuyingPower; j++) ;
                    if (j > 1)
                    {
                        j--;
                        j--;
                        if (price * j > BuyingPower * expectation)
                        {
                            try
                            {
                                var order = await restClient.PostOrderAsync(triples[i].symbol, j, OrderSide.Buy, OrderType.Market, TimeInForce.Day);
                                lastBought = true;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }
                expectation = expectation - 0.05;
            } //while (!lastBought && BuyingPower > 100 && expectation > 0)
        }

    }//end public class Buyer
}
