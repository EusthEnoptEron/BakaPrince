using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using Newtonsoft.Json.Serialization;


namespace BakaPrince.PDF
{
    [JsonObject(MemberSerialization.OptIn)]
    class Config
    {
        private readonly Uri _location;
        private readonly JObject _config;

        public Dictionary<string, List<string>> Contributors = new Dictionary<string, List<string>>();

        [JsonProperty]
        public string Title;
        [JsonProperty]
        public string Project;

        [JsonProperty("stylesheets")]
        public List<Uri> StyleSheets = new List<Uri>(); 

        private readonly List<Image> _images = new List<Image>();
        private readonly List<Page> _pages = new List<Page>();

        protected Config()
        {
        }

        public static Config Empty
        {
            get
            {
                return new Config();
            }
        }

        public Config(string jsonLocation) : this(new Uri(Helper.Cwd, jsonLocation)) { }
        public Config(Uri jsonLocation)
        {
            _location = jsonLocation;

            WebRequest req = WebRequest.Create(_location);
            using (Stream stream = req.GetResponse().GetResponseStream())
            {
                if (stream != null) _config = JObject.Load(new JsonTextReader(new StreamReader(stream)));

                ParseImages();
                ParsePages();
                ParseStyleSheets();

                ParseContributorList("authors");
                ParseContributorList("artists");
                ParseContributorList("translators");
                ParseContributorList("editors");

                
                if (_config["title"] != null)
                {
                    Title = (string)_config["title"];
                }
                if (_config["project"] != null)
                {
                    Project = (string)_config["project"];
                }
            }
        }

        private void ParseStyleSheets()
        {
            if (_config["stylesheets"] != null)
            {
                StyleSheets.AddRange(_config["stylesheets"].OfType<JValue>().Select(token => new Uri(_location, token.ToString())));
            }
        }



        public string BaseUrl
        {
            get
            {
                if (_pages.Count > 0)
                {
                    return _pages[0].Wiki;
                }
                return (new Uri(AppDomain.CurrentDomain.BaseDirectory)).ToString();
            }
        }

        [JsonProperty]
        public List<Page> Pages
        {
            get
            {
                return _pages;
            }
        }

        [JsonProperty]
        public List<Image> Images
        {
            get
            {
                return _images;
            }
        }

        private void ParseContributorList(string key)
        {
            // Init
            if (!Contributors.ContainsKey(key))
            {
                Contributors.Add(key, new List<string>());
            }
            
            List<string> list = Contributors[key];

            if (_config != null && (_config[key] is JArray))
            {
                list.AddRange(_config[key].OfType<JValue>().Select(token => token.ToString()));
            }
        }

        private void ParsePages() {
            if (_config["pages"] != null)
            {
                foreach (JToken pageOptions in _config["pages"])
                {
                    var page = new Page();
                    if (_config["defaults"] != null)
                    {   
                        page.ApplyConfig((JObject)_config["defaults"]);
                    }

                    if (pageOptions is JValue)
                    {
                        page.Name = pageOptions.ToString();   
                    }
                    else
                    {
                        var o = pageOptions as JObject;
                        if (o != null)
                        {
                            page.ApplyConfig(o);
                        }
                    }

                    _pages.Add(page);
                }
            }
        }

        private void ParseImages()
        {
            if (_config["images"] != null)
            {
                foreach(string image in _config["images"]) {
                    _images.Add(new Image(image, _location));
                }
            }
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(
                this,
                Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                             DefaultValueHandling = DefaultValueHandling.Ignore,
                                             
                }
            );
        }
    }
}
