using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//added
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Net;
using System.Net.Http;

using Alpaca.Markets;

namespace StockTrader
{
    class Downloader
    {
        public static string StockDirectory = Switches.stockDirectory;
        public static List<Symbol> Symbols = new List<Symbol>();
        public void Run()
        {
            //place all symbols from All Stocks into string list.
            Symbols = SetActiveSymbols();

            //Download Stocks and place into folder
            DownloadStocks();

            //edit csv file to update if exist or does not exist
            RewriteCSV();
        }

        public static List<Symbol> SetActiveSymbols()
        {
            try
            {
                List<Symbol> to_return = new List<Symbol>();
                using (TextFieldParser parser = new TextFieldParser(@"C:\Users\Ryan\Desktop\StocksFolder\AllStocks.csv"))
                {
                    List<string> symbols = new List<string>();

                    parser.Delimiters = new string[] { "," };
                    while (true)
                    {
                        string[] parts = parser.ReadFields();
                        if (parts == null)
                        {
                            break;
                        }

                        symbols.Add(parts[0]);                        
                    }//end while (true)

                    //for each symbol in list
                    for (int i = 1; i < symbols.Count(); i++)
                    {
                        //if directory doesn't exist, add to symbols with IsActive = false
                        if (!Directory.Exists(StockDirectory + symbols[i]))
                        {
                            //default active to false since directory doesn't exist
                            to_return.Add(new Symbol { Name = symbols[i], IsActive = false });
                            Console.WriteLine(StockDirectory + symbols[i] + " Doesn't Exist");
                        }
                        else
                        {
                            //add each symbol to list
                            to_return.Add(new Symbol { Name = symbols[i], IsActive = AppendLatestValues(symbols[i]) });
                            //AppendLatestValues(symbols[i]);
                        }
                    }//end for (int i = 0; i < symbols.Count(); i++)

                }//end using (TextFieldParser parser = new TextFieldParser(@"C:\Users\Ryan\Desktop\StocksFolder\AllStocks.csv"))

                return to_return;
            }
            catch (Exception ex)
            {
                throw Utility.ThrowException(ex);
            }
        }//end SetActiveSymbols(0


        public static List<Symbol> GetActiveSymbols()
        {
            try
            {
                List<Symbol> to_return = new List<Symbol>();
                using (TextFieldParser parser = new TextFieldParser(@"C:\Users\Ryan\Desktop\StocksFolder\AllStocks.csv"))
                {
                    List<string> symbols = new List<string>();

                    parser.Delimiters = new string[] { "," };
                    while (true)
                    {
                        string[] parts = parser.ReadFields();
                        if (parts == null)
                        {
                            break;
                        }

                        to_return.Add(new Symbol { Name = parts[0], IsActive = (parts[1] == "True") });
                    }//end while (true)

                }//end using (TextFieldParser parser = new TextFieldParser(@"C:\Users\Ryan\Desktop\StocksFolder\AllStocks.csv"))

                return to_return;
            }
            catch (Exception ex)
            {
                throw Utility.ThrowException(ex);
            }
        }

        public static void DownloadStocks()
        {
            try
            {
                //for each active symbol
                foreach(var p in Symbols.Where(x => x.IsActive))
                {
                    p.IsActive = AppendLatestValues(p.Name);
                }//end foreach(var p in Symbols.Where(x => x.IsActive))
            }
            catch (Exception ex)
            {
                throw Utility.ThrowException(ex);
            }
        }//end public static void DownloadStocks()

        private static bool AppendLatestValues(string symbol)
        {
            try
            {
                //get latest value's year
                int year = DateTime.Today.Year;
                for (; !Directory.Exists(StockDirectory + symbol + @"\" + year) && year > 2013; year--) ;
                if (year == 2013)
                    return false;

                //get latest value's month
                int month = 12;

                //if latest value is in this year, then default to this year's month
                if (year == DateTime.Today.Year)
                    month = DateTime.Today.Month;

                //get latest month
                for (; !File.Exists(StockDirectory + symbol + @"\" + year + @"\" + month + @"\" + symbol + ".csv") && month > 0; month--) ;
                if (month == 0)
                    return false;

                //place all values of current csv file lines into string array
                var allLines = File.ReadAllLines(StockDirectory + symbol + @"\" + year + @"\" + month + @"\" + symbol + ".csv");
                string[] temp = allLines[allLines.Count() - 1].Split(',');

                //latest day is on last line
                DateTime latestDay = Convert.ToDateTime(temp[0]);

                //start at first day after latest day
                latestDay = latestDay.AddDays(1);

                //example
                //https://cloud.iexapis.com/stable/stock/mmm/chart/date/20190618?chartByDay=true&token=pk_d77c6f838027448cae27d946fa677249

                //API has 10 tries to 
                int AttemptCounter = 0;

                while (latestDay < DateTime.Today)
                {
                    var IEXTrading_API_PATH = "https://cloud.iexapis.com/stable/stock/" + symbol + "/chart/date/" + latestDay.ToString("yyyyMMdd") + "?chartByDay=true&token=pk_d77c6f838027448cae27d946fa677249";

                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                        //For IP-API
                        client.BaseAddress = new Uri(IEXTrading_API_PATH);
                        HttpResponseMessage response = client.GetAsync(IEXTrading_API_PATH).GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Completing stock " + symbol);
                            var historicalDataList = response.Content.ReadAsAsync<List<HistoricalDataResponse>>().GetAwaiter().GetResult();

                            if (historicalDataList.Count() > 0)
                            {

                                Directory.CreateDirectory(StockDirectory + symbol);

                                for (int i = 0; i < historicalDataList.Count(); i++)
                                {
                                    historicalDataList[i].dateExact = Convert.ToDateTime(historicalDataList[i].date);
                                    //print year directory if not exist
                                    if (!Directory.Exists(StockDirectory + symbol + @"\" + historicalDataList[i].dateExact.Year))
                                    {
                                        Directory.CreateDirectory((StockDirectory + symbol + @"\" + historicalDataList[i].dateExact.Year));
                                    }

                                    //print month directory if not exist
                                    if (!Directory.Exists(StockDirectory + symbol + @"\" + historicalDataList[i].dateExact.Year + @"\" + historicalDataList[i].dateExact.Month))
                                    {
                                        Directory.CreateDirectory((StockDirectory + symbol + @"\" + historicalDataList[i].dateExact.Year + @"\" + historicalDataList[i].dateExact.Month));
                                    }

                                    //start csv file if not exist
                                    if (!File.Exists(StockDirectory + symbol + @"\" + historicalDataList[i].dateExact.Year + @"\" + historicalDataList[i].dateExact.Month + @"\" + symbol + ".csv"))
                                    {
                                        //File.Create(stockDirectory + symbol + @"\" + historicalDataList[i].dateExact.Year + @"\" + historicalDataList[i].dateExact.Month + @"\" + symbol + ".csv");
                                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(StockDirectory + symbol + @"\" + historicalDataList[i].dateExact.Year + @"\" + historicalDataList[i].dateExact.Month + @"\" + symbol + ".csv"))
                                        {
                                            file.WriteLine("Date,Open,Close,Low,High,Change,Volume");
                                        }
                                    }

                                    //using (System.IO.StreamWriter file = new System.IO.StreamWriter(stockDirectory + symbol + @"\" + historicalDataList[i].dateExact.Year + @"\" + historicalDataList[i].dateExact.Month + @"\" + symbol + ".csv"))
                                    using (StreamWriter file = File.AppendText(StockDirectory + symbol + @"\" + historicalDataList[i].dateExact.Year + @"\" + historicalDataList[i].dateExact.Month + @"\" + symbol + ".csv"))
                                    {
                                        Console.WriteLine(i.ToString());
                                        Console.WriteLine("\tDate:  " + historicalDataList[i].date);
                                        string correctedDate = Convert.ToDateTime(historicalDataList[i].date).ToString("M/d/yyyy");
                                        file.Write(correctedDate);

                                        file.Write(",");

                                        Console.WriteLine("\tOpen: " + historicalDataList[i].open);
                                        file.Write(historicalDataList[i].open);

                                        file.Write(",");

                                        Console.WriteLine("\tClose: " + historicalDataList[i].close);
                                        file.Write(historicalDataList[i].close);

                                        file.Write(",");

                                        Console.WriteLine("\tLow: " + historicalDataList[i].low);
                                        file.Write(historicalDataList[i].low);

                                        file.Write(",");

                                        Console.WriteLine("\tHigh: " + historicalDataList[i].high);
                                        file.Write(historicalDataList[i].high);

                                        file.Write(",");

                                        Console.WriteLine("\tChange: " + historicalDataList[i].change.ToString());
                                        file.Write(historicalDataList[i].change);

                                        file.Write(",");

                                        //Console.WriteLine("\tChange Percent:  " + historicalDataList[i].changePercent);
                                        //file.WriteLine(historicalDataList[i].changePercent);
                                        //file.Write(",");

                                        Console.WriteLine("\tChange Percentage: " + historicalDataList[i].volume);
                                        file.Write(historicalDataList[i].volume);

                                        file.Write("\n");
                                    }//end  using (StreamWriter file = File.AppendText(stockDirectory + symbol + @"\" + historicalDataList[i].dateExact.Year + @"\" + historicalDataList[i].dateExact.Month + @"\" + symbol + ".csv"))

                                }//end for (int i = 0; i < historicalDataList.Count(); i++)

                                //reset attempt counter if successful data read
                                AttemptCounter = 0;

                            }//end if (historicaldata.Count() > 0)
                            else
                            {
                                //if historical data count = 0
                                //increment attempt counter
                                AttemptCounter++;
                                if (AttemptCounter > 10)
                                {
                                    return false;
                                }
                            }

                        }//end if (response.IsSuccessStatusCode)
                        else
                        {
                            Console.WriteLine("Could not complete stock " + symbol);
                            return false;
                        }

                        //get next day
                        latestDay = latestDay.AddDays(1);

                    }//end using (HttpClient client = new HttpClient())

                }//end while (latestDay < DateTime.Today)

                return true;
            }
            catch (Exception ex)
            {
                throw Utility.ThrowException(ex);
            }

        }//end public static bool AppendLatestValues(string symbol)


        public static void  RewriteCSV()
        {
            try
            {
                using (var file = new System.IO.StreamWriter(StockDirectory + "AllStocks.csv"))
                {
                    file.WriteLine("Name,IsActive");
                    foreach (var p in Symbols)
                    {
                        file.WriteLine(p.Name + "," + p.IsActive);
                    }//end foreach (var p in Symbols)
                }//end using (var file = new System.IO.StreamWriter(StockDirectory + "AllStocks.csv"))
            }
            catch (Exception ex)
            {
                throw Utility.ThrowException(ex);
            }
        }



    }//end class Downloader
}
