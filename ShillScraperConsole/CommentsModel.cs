using System;
using System.Collections.Generic;
using System.Text;

namespace ShillScraperConsole
{
    public class Data2
    {
        public string link_id { get; set; }
        public object likes { get; set; }
        public string replies { get; set; }
        public string id { get; set; }
        public int score { get; set; }
        public string body { get; set; }
        public string body_html { get; set; }
        public string subreddit { get; set; }
        public string name { get; set; }
        public double created { get; set; }
        public string link_url { get; set; }
        public int ups { get; set; }
    }

    public class Child
    {
        public string kind { get; set; }
        public Data2 data { get; set; }
    }

    public class Data
    {
        public List<Child> children { get; set; }
        public string after { get; set; }
        public string before { get; set; }
    }

    public class RootObject
    {
        public string kind { get; set; }
        public Data data { get; set; }
    }
}
