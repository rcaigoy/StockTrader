using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using Alpaca.Markets;

namespace StockTrader
{
    class Seller
    {

        public static void Run()
        {
            try
            {
                //Choose between selling algorithms

                //1) Sell on one percent
                Task.Run(async () => await SellOnOnePercentGain());

                //2) Sell on Percent over time
                //Task.Run(async () => await SellOnPercentOverTime());
            }
            catch (Exception ex)
            {
                throw Utility.ThrowException(ex);
            }
        }
        public static async Task SellOnOnePercentGain()
        {
            var restClient = new RestClient(Switches.AlpacaAPIKey(), Switches.AlpacaSecretAPIKey(), Switches.AlpacaEndPoint());

            if (File.Exists(Switches.stockDirectory + "Portfolio.csv"))
            {
                var allLines = File.ReadAllLines(Switches.stockDirectory + "Portfolio.csv");
                for (int i = 1; i < allLines.Count(); i++)
                {
                    string[] temp = allLines[i].Split(',');
                    var p2 = await restClient.GetAssetAsync((temp[0]));

                    var position = await restClient.GetPositionAsync(temp[0]);
                    //double required = temp[]
                    int numberOfShares = position.Quantity;
                    int daysAlloted = DateTime.Today.Day - Convert.ToDateTime(temp[1]).Day;
                    double required = Convert.ToDouble(temp[3]);

                    //set minimal required selling point
                    required *= 1.01;

                    try
                    {
                        if ((double)position.AssetCurrentPrice > required)
                        {
                            var order = await restClient.PostOrderAsync(temp[0], numberOfShares, OrderSide.Sell, OrderType.Market, TimeInForce.Day);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("could not sell because " + ex.ToString());
                    }
                }
            }
        }

        public static async Task SellOnPercentOverTime()
        {
            var restClient = new RestClient(Switches.AlpacaAPIKey(), Switches.AlpacaSecretAPIKey(), Switches.AlpacaEndPoint());
            if (File.Exists(Switches.stockDirectory + "Portfolio.csv"))
            {
                var allLines = File.ReadAllLines(Switches.stockDirectory + "Portfolio.csv");
                for (int i = 1; i < allLines.Count(); i++)
                {
                    string[] temp = allLines[i].Split(',');
                    var p2 = await restClient.GetAssetAsync((temp[0]));

                    var position = await restClient.GetPositionAsync(temp[0]);

                    int numberOfShares = position.Quantity;
                    int daysAlloted = DateTime.Today.Day - Convert.ToDateTime(temp[1]).Day;

                    //set minimal required selling point
                    double required = Convert.ToDouble(temp[3]);
                    
                    if (daysAlloted < 7 && daysAlloted > 0)
                    {
                        required *= (1 + 0.005 * daysAlloted);
                    }
                    else if (daysAlloted >= 7 && daysAlloted < 30)
                    {
                        required *= (1.03);
                    }
                    else if (daysAlloted > 31 && daysAlloted <= 60)
                    {
                        required *= (1.1);
                    }
                    else
                    {
                        required *= 1.18;
                    }
                    

                    try
                    {
                        if ((double)position.AssetCurrentPrice > required)
                        {
                            var order = await restClient.PostOrderAsync(temp[0], numberOfShares, OrderSide.Sell, OrderType.Market, TimeInForce.Day);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("could not sell because " + ex.ToString());
                    }
                }
            }
        }
    }
}
