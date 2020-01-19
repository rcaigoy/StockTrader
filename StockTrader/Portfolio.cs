using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Alpaca.Markets;

namespace StockTrader
{
    public partial class Portfolio
    {
        public void Update()
        {

        }


        public static async Task UpdatePortfolio()
        {
            var restClient = new RestClient(Switches.AlpacaAPIKey(), Switches.AlpacaSecretAPIKey(), Switches.AlpacaEndPoint());
            var positions = await restClient.ListPositionsAsync();

            //start existing
            List<Portfolio> existing = new List<Portfolio>();
            var allLines = File.ReadAllLines(Switches.stockDirectory + "Portfolio.csv");

            List<Portfolio> newList = new List<Portfolio>();
            Console.WriteLine("update portfolio 1");
            if (!File.Exists(Switches.stockDirectory + "Portfolio.csv"))
            {
                foreach (var position in positions)
                {
                    newList.Add(new Portfolio
                    {
                        symbol = position.Symbol,
                        datePurchased = DateTime.Today,
                        numberOfShares = position.Quantity,
                        averagePrice = (double)position.AverageEntryPrice
                    });
                }
            }//end if (!File.Exists(Switches.stockDirectory + "Portfolio.csv"))
            else
            {
                foreach (var position in positions)
                {
                    bool inposition = false;
                    int OldPosition = 0;
                    for (int i = 1; i < allLines.Count(); i++)
                    {
                        string[] temp = allLines[i].Split(',');
                        if (position.Symbol.ToLower() == temp[0].ToLower())
                        {
                            inposition = true;
                            OldPosition = i;
                        }
                    }
                    //if portfolio is still not sold
                    if (inposition)
                    {
                        newList.Add(new Portfolio
                        {
                            symbol = position.Symbol,
                            datePurchased = Convert.ToDateTime(allLines[OldPosition].Split(',')[1]),
                            numberOfShares = position.Quantity,
                            averagePrice = (double)position.AverageEntryPrice
                        });
                    }
                    else
                    {
                        newList.Add(new Portfolio
                        {
                            symbol = position.Symbol,
                            datePurchased = DateTime.Today,
                            numberOfShares = position.Quantity,
                            averagePrice = (double)position.AverageEntryPrice
                        });
                    }
                }//end foreach (var position in positions)
            }//end else (if (!File.Exists(Switches.stockDirectory + "Portfolio.csv")))

            File.Delete(Switches.stockDirectory + "Portfolio.csv");
            //File.Create(Switches.stockDirectory + "Portfolio.csv");

            //UpdatePortFolio(portfolio.symbol, portfolio.numberOfShares, portfolio.numberOfShares);
            using (StreamWriter file = File.AppendText(Switches.stockDirectory + "Portfolio.csv"))
            {
                file.Write("symbol");
                file.Write(",");

                file.Write("Date Purchased");
                file.Write(",");

                file.Write("Number of Shares");
                file.Write(",");

                file.Write("averagePrice");

                file.Write("\n");

                foreach (var portfolio in newList)
                {
                    file.Write(portfolio.symbol);
                    file.Write(",");

                    file.Write(portfolio.datePurchased);
                    file.Write(",");

                    file.Write(portfolio.numberOfShares);
                    file.Write(",");

                    file.Write(portfolio.averagePrice);

                    file.Write("\n");

                }//end using (StreamWriter file = File.AppendText(Switches.stockDirectory + "Portfolio.csv"))

            }//end using (StreamWriter file = File.AppendText(Switches.stockDirectory + "Portfolio.csv"))

            // Print the quantity of shares for each position.
            Console.WriteLine("Portfolio Update Finished");
            foreach (var position in positions)
            {
                Console.WriteLine($"{position.Quantity} shares of {position.Symbol}.");
            }
        }
    }
}
