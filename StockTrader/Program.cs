using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrader
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //download stocks
                Downloader Downloader = new Downloader();
                Downloader.Run();

                //Update Portfolio
                Portfolio.UpdatePortfolio();

                //sell stocks
                Seller.Run();

                //Update Portfolio
                Portfolio.UpdatePortfolio();

                //buy stocks
                Buyer.Run();

                //Update Portfolio
                Portfolio.UpdatePortfolio();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }
}
