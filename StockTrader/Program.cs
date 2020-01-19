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
            //download stocks
            Downloader.Run();

            //sell stocks
            Seller.Run();

            //buy stocks
            Buyer.Run();

        }
    }
}
