using System;
using HtmlAgilityPack;
using ScrapySharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ShillScraperConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Shill shill = new Shill(4);
            shill.PrintFrequencies();
            Console.ReadLine();
        }
    }
}
