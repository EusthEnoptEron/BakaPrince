using CsQuery;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BakaPrince.BakaTsuki
{
    class BakaPage
    {
        private string Wiki = "http://www.baka-tsuki.org/project/";
        public readonly string Name;
        private Dictionary<string, JToken> _cache = new Dictionary<string, JToken>();


        public BakaPage(string name, string Wiki) : this(name)
        {
            this.Wiki = Wiki;
        }

        public BakaPage(string name)
        {
            this.Name = name;
        }

        protected string API
        {
            get
            {
                return Wiki + "api.php?";
            }
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public string Content
        {
            get
            {
                return (string)FetchPage(new string[] { "action=parse", "page=" + Name }).SelectToken("parse.text.*");
            }
        }

        protected CQ CQNode
        {
            get
            {
                return Content;
            }
        }

        private JToken FetchPage(string[] args)
        {
            List<string> config = new List<string> { "format=json" };
            config.AddRange(args);

            // Build URL
            string url = API + String.Join("&", config);

            if (_cache.ContainsKey(url))
                return _cache[url];
            else 
                return JObject.Parse(Helper.GetString(new Uri(url)));
        }

        
        public string Url { get {
            return Wiki + "index.php?title=" + Name;
            }
        }
    }
}
