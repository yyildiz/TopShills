namespace ShillScraperConsole
{
    class Shill
    {
        public enum ShillTypes
        {
            Reddit,
            FourChan
        }

        public Shill() {
            ShillReddit();
        }

        public void ShillReddit()
        {
            var cryptoCurrencyComments = RedditComments.GetComments("cryptocurrency", 1000);
            var cryptoMarketsComments = RedditComments.GetComments("cryptomarkets", 1000);
            var altcoinComments = RedditComments.GetComments("altcoin", 1000);
            var cryptoCurrencyTradingComments = RedditComments.GetComments("CryptoCurrencyTrading", 1000);

            cryptoCurrencyComments.AddRange(cryptoMarketsComments);
            cryptoCurrencyComments.AddRange(altcoinComments);
            cryptoCurrencyComments.AddRange(cryptoCurrencyTradingComments);

            ShillStatician statician = new ShillStatician(cryptoCurrencyComments);
            statician.PrintFrequencies();
        }
    }
}
