using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Collections;


namespace BakaPrince
{
    class Config
    {
        private Uri location;
        private JObject config;


        public List<string> Translators = new List<string>();
        public List<string> Editors = new List<string>();
        public readonly string Title;

        private List<Image> images = new List<Image>();
        private List<Page> pages = new List<Page>();


        public Config(string jsonLocation)
        {
            location = new Uri(jsonLocation);

            WebRequest req = HttpWebRequest.Create(location);
            using (Stream stream = req.GetResponse().GetResponseStream())
            {
                config = JObject.Load(new JsonTextReader(new StreamReader(stream)));

                ParseImages();
                ParsePages();

                if (config["title"] != null)
                {
                    Title = (string)config["title"];
                }
            }
        }

        public string BaseURL
        {
            get
            {
                if (pages.Count > 0)
                {
                    return pages[0].Wiki;
                }
                else
                {
                    return (new Uri(AppDomain.CurrentDomain.BaseDirectory)).ToString();
                }
            }
        }

        public Page[] Pages
        {
            get
            {
                return pages.ToArray();
            }
        }

        public Image[] Images
        {
            get
            {
                return images.ToArray();
            }
        }

        private void Parse()
        {
            
        }


        private void ParsePages() {
            if (config["pages"] != null)
            {
                foreach (object pageOptions in config["pages"])
                {
                    Page page = new Page();
                    if (config["defaults"] != null)
                    {   
                        page.ApplyConfig((JObject)config["defaults"]);
                    }

                    if (pageOptions is JValue)
                    {
                        page.Name = pageOptions.ToString();   
                    }
                    else if (pageOptions is JObject)
                    {
                        page.ApplyConfig((JObject)pageOptions);
                    }

                    pages.Add(page);
                }
            }
        }

        private void ParseImages()
        {
            if (config["images"] != null)
            {
                foreach(string image in config["images"]) {
                    images.Add(new Image(image, location));
                }
            }
        }
    }
}
