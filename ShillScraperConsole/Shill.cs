using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using IronWebScraper;

namespace ShillScraperConsole
{
    class Shill : WebScraper
    {
        private Dictionary<string, List<Coin>> OrderedIdentifiers = new Dictionary<string, List<Coin>>();
        private List<string> Urls = new List<string>();
        private string CurrentUrl = "https://www.reddit.com/r/CryptoCurrency/";
        private int CurrentPage = 0;
        private int TotalPages;
        private string NextPage;
        public Dictionary<string, int> frequencies = new Dictionary<string, int>();
        public Shill(int TotalPages) {
            var AllCoins = GetAllCoins();
            this.TotalPages = TotalPages;
            foreach (Coin coin in AllCoins)
            {
                if(OrderedIdentifiers.ContainsKey(coin.symbol.ToLower()))
                {
                    OrderedIdentifiers[coin.symbol.ToLower()].Add(coin);
                } else
                {
                    OrderedIdentifiers.Add(coin.symbol.ToLower(), new List<Coin>() { coin });
                }
                if(!OrderedIdentifiers.ContainsKey(coin.name.ToLower()))
                {
                    OrderedIdentifiers.Add(coin.name.ToLower(), new List<Coin>() { coin });
                }
            }
            OrderedIdentifiers = OrderedIdentifiers.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            Start();
        }
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
        public List<Coin> FindCoin(string value)
        {
            value = value.ToLower();
            if(OrderedIdentifiers.ContainsKey(value))
            {
                return OrderedIdentifiers[value];
            }
            return null;
        }
        public override void Init()
        {
            var current = 0;
            this.LoggingLevel = LogLevel.All;
            while(current++ < TotalPages)
            {
                this.Request(CurrentUrl, Parse);
            }
            
        }
        public override void Parse(Response response)
        {
            GetTitles(response);
            this.Request(Urls, ParseComments);
            CurrentUrl = NextPage;
        }

        private void GetTitles(Response response)
        {
            var Entries = response.Css(".entry");
            foreach(HtmlNode entry in Entries)
            {
                GetFrequencies(entry.Css(".title a.title").First().InnerText.Split(' ').ToList());
                Urls.Add(entry.Css("a.comments").First().GetAttribute("href"));
            }
            NextPage = response.Css(".next-button a").First().GetAttribute("href");
        }

        private void ParseComments(Response response)
        {
            var Comments = response.Css(".entry .usertext-body .md");
            foreach(HtmlNode comment in Comments)
            {
                GetFrequencies(comment.InnerText.Split(' ').ToList());
            }
        }

        private void ParseNonsense(Response response)
        {
            var Comments = response.Css(".entry .usertext-body .md");
            foreach (HtmlNode comment in Comments)
            {
                GetFrequencies(comment.InnerText.Split(' ').ToList());
            }
        }

        public void GetFrequencies(List<string> words)
        {
            foreach (string word in words)
            {
                var found = FindCoin(word);
                if (found != null)
                {
                    foreach (Coin coin in found)
                    {
                        if (!frequencies.ContainsKey(coin.name))
                        {
                            frequencies.Add(coin.name, 1);
                        }
                        else
                        {
                            frequencies[coin.name]++;
                        }
                    }
                }
            }
        }

        public void PrintFrequencies()
        {
            frequencies = frequencies.OrderBy(p => p.Value).ToDictionary(p => p.Key, p => p.Value);
            AddExclusions(new List<string> { "for", "hold", "hodl", "all", "way", "lot", "time", "money", "$$$" });
            foreach (string key in frequencies.Keys)
            {
                Console.WriteLine($"{key} : {frequencies[key]}");
            }
        }

        public void AddExclusions(List<string> exclusions)
        {
            foreach(string word in exclusions)
            {
                var found = FindCoin(word);
                if(found != null)
                {
                    // Place workaround for when multiple coins have the same symbol.
                    if(frequencies.ContainsKey(found[0].name))
                    {
                        frequencies.Remove(found[0].name);
                    }
                }


            }
        }
    }
}
