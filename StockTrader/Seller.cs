using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

using Alpaca.Markets;

namespace StockTrader
{
    class Seller
    {

        public async static Task Run()
        {
            try
            {
                //Choose between selling algorithms

                //1) Sell on one percent
                ///Task.Run(async () => await SellOnOnePercentGain());
                await SellOnePercentGain();

                //2) Sell on Percent over time
                //Task.Run(async () => await SellOnPercentOverTime());
            }
            catch (Exception ex)
            {
                throw Utility.ThrowException(ex);
            }
        }


        public async static Task SellOnePercentGain()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers.Add("APCA-API-KEY-ID", Switches.AlpacaAPIKey());
                    client.Headers.Add("APCA-API-SECRET-KEY", Switches.AlpacaSecretAPIKey());

                    var stream = client.DownloadString(Switches.AlpacaEndPoint() + "/v2/positions");
                    var Positions = JsonConvert.DeserializeObject<List<Position>>(stream);
                    foreach (var position in Positions)
                    {
                        //if current price is greater than 1 %
                        if (position.current_price > position.avg_entry_price * 1.01M)
                        {
                            //sell
                            SellStock(position.symbol, position.qty).Wait();

                            /*
                            AlpacaTradingClientConfiguration config = new AlpacaTradingClientConfiguration();
                            config.ApiEndpoint = new Uri(Switches.AlpacaEndPoint());
                            config.KeyId = Switches.AlpacaAPIKey();
                            config.SecurityId = new SecretKey(Switches.AlpacaSecretAPIKey());
                            var restClient = new AlpacaTradingClient(config);

                            //var p = await restClient.PostOrderAsync(position.symbol, position.qty, OrderSide.Sell, OrderType.Market, TimeInForce.Day);

                            //sell 2?
                            client.UploadData("", );
                            */
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw Utility.ThrowException(ex);
            }
        }

        public static async Task SellStock(string Symbol, int Quantity)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(Switches.AlpacaEndPoint() + "/v2/orders");

                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add("APCA-API-KEY-ID", Switches.AlpacaAPIKey());
                httpWebRequest.Headers.Add("APCA-API-SECRET-KEY", Switches.AlpacaSecretAPIKey());
                

                httpWebRequest.ContentType = "application/x-www-form-urlencoded";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {

                    //string json = "{" + "\"tracking\": {" + "\"slug\":\"" + Courier + "\"," + "\"tracking_number\":\"" + trackNumber + "\"}}";
                    SellOrderObject s = new SellOrderObject();
                    s.symbol = Symbol;
                    s.qty = Quantity;

                    var json = JsonConvert.SerializeObject(s);

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                    }
                }

                /*
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Post, Switches.AlpacaEndPoint() + "/v2/orders"))
                {
                    SellOrderObject content = new SellOrderObject();
                    content.symbol = Symbol;
                    content.qty = Quantity;
                    var json = JsonConvert.SerializeObject(content);
                    using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        request.Content = stringContent;

                        request.Headers.Add("APCA-API-KEY-ID", Switches.AlpacaAPIKey());
                        request.Headers.Add("APCA-API-SECRET-KEY", Switches.AlpacaSecretAPIKey());
                        client.DefaultRequestHeaders.Add("APCA-API-KEY-ID", Switches.AlpacaAPIKey());
                        client.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", Switches.AlpacaSecretAPIKey());
                        using (var response = await client
                            .PostAsJsonAsync(Switches.AlpacaEndPoint() + "/v2/orders", content))
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }
                */
            }
            catch (Exception ex)
            {
                //throw Utility.ThrowException(ex);
            }
        }

        public static async Task SellOnOnePercentGainAsync()
        {
            //setup rest client
            //var restClient = new RestClient(Switches.AlpacaAPIKey(), Switches.AlpacaSecretAPIKey(), Switches.AlpacaEndPoint());

            try
            {
                AlpacaTradingClientConfiguration config = new AlpacaTradingClientConfiguration();
                config.ApiEndpoint = new Uri(Switches.AlpacaEndPoint());
                config.KeyId = Switches.AlpacaAPIKey();
                config.SecurityId = new SecretKey(Switches.AlpacaSecretAPIKey());
                var restClient = new AlpacaTradingClient(config);

                //var positions1 = await restClient.ListAssetsAsync(AssetStatus.Active);
                //var positions2 = await restClient.ListPositionsAsync();

                /*
                foreach (var position in positions)
                {
                    Console.Write(position);
                }
                */
            }
            catch (Exception ex)
            {
                throw ex;
            }
            /*
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
            */
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


    public class Position
    {
        //"asset_id": "fc9c62fa-c635-41c8-8b58-9695c01d146d",
        public string asset_id { get; set; }
        //"symbol": "VMC",
        public string symbol { get; set; }
        //"exchange": "NYSE",
        public string exchange { get; set; }
        //"asset_class": "us_equity",
        public string asset_class { get; set; }
        //"qty": "56",
        public int qty { get; set; }
        //"avg_entry_price": "140.26",
        public decimal avg_entry_price { get; set; }
        //"side": "long",
        public string side { get; set; }
        //"market_value": "8230.32",
        public decimal market_value { get; set; }
        //"cost_basis": "7854.56",
        public decimal cost_basis { get; set; }
        //"unrealized_pl": "375.76",
        public decimal unrealized_pl { get; set; }
        //"unrealized_plpc": "0.0478397262227292",
        public decimal unrealized_plpc { get; set; }
        //"unrealized_intraday_pl": "105.84",
        public decimal unrealized_intraday_pl { get; set; }
        //"unrealized_intraday_plpc": "0.0130272952853598",
        public decimal unrealized_intraday_plpc { get; set; }
        //"current_price": "146.97",
        public decimal current_price { get; set; }
        //"lastday_price": "145.08",
        public decimal lastday_price { get; set; }
        //"change_today": "0.0130272952853598"
        public decimal change_today { get; set; }
    }

    public class SellOrderObject
    {
        public string symbol { get; set; }
        public int qty { get; set; }
        public string side { get; set; }
        public string type { get; set; }
        public string time_in_force { get; set; }
        public SellOrderObject()
        {
            this.side = "sell";
            this.type = "market";
            this.time_in_force = "day";
        }
    }

}
