using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using IronWebScraper;

namespace ShillScraperConsole
{
    class RedditComments
    {
        public static List<string> GetComments(string subreddit, int count)
        {
            string before = null;
            string after = null;
            List<string> comments = new List<string>();
            for(int i = 0; i < count / 100; i++)
            {
                string Url = $"https://www.reddit.com/r/{subreddit}/comments/.json?limit=100&before={before}&after={after}";
                string page = Uri.EscapeUriString(Url);
                string doc = "";
                using (System.Net.WebClient client = new System.Net.WebClient()) // WebClient class inherits IDisposable
                {
                    doc = client.DownloadString(page);
                }
                var data = JsonConvert.DeserializeObject<RootObject>(doc).data;
                before = data.before;
                after = data.after;
                foreach(var child in data.children)
                {
                    comments.Add(child.data.body);
                }
            }
            return comments;
            
        }

        
    }
}
