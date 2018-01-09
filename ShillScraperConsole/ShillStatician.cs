using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShillScraperConsole
{
    class ShillStatician
    {
        private Dictionary<string, List<Coin>> OrderedIdentifiers = new Dictionary<string, List<Coin>>();
        private Dictionary<string, int> Frequencies = new Dictionary<string, int>();
        private List<Coin> AllCoins => GetAllCoins();
        public ShillStatician(List<string> words)
        {
            GetOrderedIdentifiers();
            GetFrequencies(words);
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

        public void GetOrderedIdentifiers()
        {
            foreach (Coin coin in AllCoins)
            {
                if (OrderedIdentifiers.ContainsKey(coin.symbol.ToLower()))
                {
                    OrderedIdentifiers[coin.symbol.ToLower()].Add(coin);
                }
                else
                {
                    OrderedIdentifiers.Add(coin.symbol.ToLower(), new List<Coin>() { coin });
                }
                if (!OrderedIdentifiers.ContainsKey(coin.name.ToLower()))
                {
                    OrderedIdentifiers.Add(coin.name.ToLower(), new List<Coin>() { coin });
                }
            }
            OrderedIdentifiers = OrderedIdentifiers.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        }

        public List<Coin> FindCoin(string value)
        {
            // Add support for coins being named like DeepBrainChain instead of Deep Brain Chain.
            value = value.ToLower().Trim();
            List<Coin> foundCoins = new List<Coin>();

            foreach(string key in OrderedIdentifiers.Keys)
            {
                if(value.Contains(key) && isCoinInComment(value, key))
                {
                    // find workaround for coins being given the same identifier (i.e. ICON && ICX)
                    foundCoins.Add(OrderedIdentifiers[key].First());
                }
            }

            return foundCoins;
        }
        // checks to see if a coin followed or preceded by a delimiter is within the sentence.
        private bool isCoinInComment(string sentence, string word)
        {
            char[] delims = { ' ', '?', '!', '.', ',' };
            int beginningOfWord = sentence.IndexOf(word);
            int endOfWord = beginningOfWord + word.Length - 1;
            if ((beginningOfWord == 0 || delims.Contains(sentence[beginningOfWord - 1])) && (endOfWord == sentence.Length - 1 || delims.Contains(sentence[endOfWord + 1])))
            {
                return true;
            }
            return false;
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
                        if (!Frequencies.ContainsKey(coin.name))
                        {
                            Frequencies.Add(coin.name, 1);
                        }
                        else
                        {
                            Frequencies[coin.name]++;
                        }
                    }
                }
            }
        }

        public void PrintFrequencies()
        {
            Frequencies = Frequencies.OrderBy(p => p.Value).ToDictionary(p => p.Key, p => p.Value);
            //AddExclusions(new List<string> { "for", "hold", "hodl", "all", "way", "lot", "time", "money", "$$$" });
            foreach (string key in Frequencies.Keys)
            {
                Console.WriteLine($"{key} : {Frequencies[key]}");
            }
            Console.ReadLine();
        }

        public void AddExclusions(List<string> exclusions)
        {
            foreach (string word in exclusions)
            {
                var found = FindCoin(word);
                if (found != null)
                {
                    // Place workaround for when multiple coins have the same symbol.
                    if (Frequencies.ContainsKey(found[0].name))
                    {
                        Frequencies.Remove(found[0].name);
                    }
                }


            }
        }
    }
}
