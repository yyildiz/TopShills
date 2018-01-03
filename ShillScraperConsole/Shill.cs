using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using IronWebScraper;

namespace ShillScraperConsole
{
    class Shill : WebScraper
    {
        public Dictionary<string, List<Coin>> OrderedIdentifiers = new Dictionary<string, List<Coin>>();
        public List<string> Urls = new List<string>();
        public List<string> TitleWords = new List<string>();
        public List<string> CommentWords = new List<string>();
        public Dictionary<string, int> frequencies = new Dictionary<string, int>();
        public Shill() {

            var AllCoins = GetAllCoins();

            foreach(Coin coin in AllCoins)
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
            this.LoggingLevel = LogLevel.All;
            this.Request("https://www.reddit.com/r/CryptoCurrency/", Parse);
        }
        public override void Parse(Response response)
        {
            GetCommentLinks(response);
            GetFrequencies(TitleWords);
            GetPostFrequencies();
        }
        public void GetCommentLinks(Response response)
        {
            foreach (var entry in response.Css(".entry"))
            {
                // Adding in TitleWords
                TitleWords.AddRange(entry.Css("a.title").First().InnerText.Split(' '));
                var link = entry.Css(".comments");
                Urls.Add(link.First().GetAttribute("href"));
                
            }
        }

        public void GetPostFrequencies()
        {
            foreach(string link in Urls)
            {
                this.Request(link, QueryEachComment);
            }
        }

        public void QueryEachComment(Response response)
        {
            foreach(var comment in response.Css(".md p"))
            {
                CommentWords.AddRange(comment.InnerText.Split(' '));
            }
            GetFrequencies(CommentWords);
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
