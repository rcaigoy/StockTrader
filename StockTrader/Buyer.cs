using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrader
{
    public class Buyer
    {
        //checks last 7 days for possible increase next day
        public static List<PriceCheck> LList = new List<PriceCheck>();
        public static List<IntervalCheck> DayLList = new List<IntervalCheck>();

        //checks last 7 weeks for possible increase next week
        public static List<IntervalCheck> WeekLList = new List<IntervalCheck>();

        public static List<IntervalCheck> MonthLList = new List<IntervalCheck>();

        public static void Run()
        {
            //Calculate Day Lists from All Stocks csv where Is Active

            //Calculate Week List from csv from All Stocks csv where Is Active

            //Calculate Month List from csv from All Stocks csv where Is Active

            //Buy top 3 10% from day list

            //Buy top 3 10% from week list that aren't in day list

            //Buy top 3 10% from Month list not in day or week list

            //Use all extra funds for next month list
        }
    }
}
