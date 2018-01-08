using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShillScraperConsole
{
    class ShillStatician
    {
        private List<Coin> AllCoins { get; set; }
        public static List<Coin> GetAllCoins()
        {
            string market = Uri.EscapeUriString("https://api.coinmarketcap.com/v1/ticker/?limit=0");
            string doc = "";
            using (System.Net.WebClient client = new System.Net.WebClient()) // WebClient class inherits IDisposable
            {
                doc = client.DownloadString(market);
            }
            return JsonConvert.DeserializeObject<List<Coin>>(doc);
        }
    }
}
