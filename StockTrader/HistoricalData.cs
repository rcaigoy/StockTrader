using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace StockTrader
{
    public class HistoricalDataResponse
    {

        public static string stockDirectory = Switches.stockDirectory;
        public string symbol { get; set; }
        public string date { get; set; }
        public DateTime dateExact { get; set; }
        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public int volume { get; set; }
        public int unadjustedVolume { get; set; }
        public double change { get; set; }
        public double changePercent { get; set; }
        public double vwap { get; set; }
        public string label { get; set; }
        public double changeOverTime { get; set; }

        public static HistoricalDataResponse GetHistoricalData(string symbol, DateTime d)
        {
            HistoricalDataResponse hr = new HistoricalDataResponse();
            if (File.Exists(stockDirectory + symbol + @"\" + d.Year + @"\" + d.Month + @"\" + symbol + ".csv"))
            {
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
                //Console.WriteLine(line);
                var values = allLines[i].Split(',');
                hr.symbol = symbol;
                hr.date = d.ToString();
                hr.dateExact = d;
                hr.open = Convert.ToDouble(values[1]);
                hr.close = Convert.ToDouble(values[2]);
                hr.low = Convert.ToDouble(values[3]);
                hr.high = Convert.ToDouble(values[4]);
                hr.change = Convert.ToDouble(values[5]);
                hr.volume = Convert.ToInt32(values[6]);
            }

            return hr;
        }

    }
}
