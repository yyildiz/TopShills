using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using IronWebScraper;

namespace ShillScraperConsole
{
    class RedditPage : WebScraper
    {
        private string Url { get; set; }
        private string NextPage { get; set; }
        private List<string> Urls { get; set; } = new List<string>();
        public Dictionary<string, int> Frequencies { get; set; } = new Dictionary<string, int>();

        private int CurrentPages;

        public RedditPage(string Url, int TotalPages)
        {
            this.Url = Url;
            this.CurrentPages = TotalPages;
            this.Start();
        }

        public override void Init()
        {
            this.LoggingLevel = LogLevel.All;
            this.Request(Url, Parse);
        }

        public override void Parse(Response response)
        {
            var entries = response.Css(".entry").ToList();
            foreach (HtmlNode entry in entries)
            {
                Urls.Add(entry.Css("a.comments").First().GetAttribute("href"));
            }
            NextPage = response.Css(".next-button a").First().GetAttribute("href");
            if (CurrentPages > 0)
            {
                RedditPage page = new RedditPage(NextPage, --CurrentPages);
            }
        }
    }
}
