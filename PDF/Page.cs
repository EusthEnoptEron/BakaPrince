using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using CsQuery;
using BakaPrince.BakaTsuki;
using Newtonsoft.Json;
using System.ComponentModel;

namespace BakaPrince.PDF
{
    class Page
    {
        // public

        [DefaultValue("")]
        public string Prefix = "";
        public string Name;
        public string Title = null;

        [DefaultValue(true)]
        public bool Pagebreak = true;
        
        public bool Notitle = false;
        public bool Noheader = false;

        [DefaultValue("http://www.baka-tsuki.org/project/")]
        public string Wiki = "http://www.baka-tsuki.org/project/";
        public bool EntryPicture = false;


        private bool _fetched;
        private string _html = "";
        private readonly List<Image> _images = new List<Image>();

        public void ApplyConfig(JObject values) {
            IEnumerator<KeyValuePair<string, JToken>> it = values.GetEnumerator();
            while (it.MoveNext())
            {
                switch (it.Current.Key)
                {
                    case "prefix": Prefix = it.Current.Value.ToString(); break;
                    case "name": Name = it.Current.Value.ToString(); break;
                    case "title": Title = it.Current.Value.ToString(); break;
                    case "pagebreak": Pagebreak = (bool)it.Current.Value; break;
                    case "notitle": Notitle = (bool)it.Current.Value; break;
                    case "noheader": Noheader = (bool)it.Current.Value; break;
                    case "wiki": Wiki = it.Current.Value.ToString(); break;
                    case "entrypicture": EntryPicture = (bool)it.Current.Value; break;
                }
            }
        }

        [JsonIgnore]
        public string Html
        {
            get
            {
                Fetch();

                return _html;
            }
        }

        /// <summary>
        /// Returns the images contained in this page.
        /// </summary>
        [JsonIgnore]
        public Image[] Images
        {
            get
            {
                Fetch();
                return _images.ToArray();
            }
        }

        private void Fetch() {
            if (!_fetched)
            {
                if (Title == null)
                    Title = Name;

                BakaPage page = new BakaPage(Prefix + Name, Wiki);

                Console.WriteLine("Fetching page {0}", Name);
                
                _html = PrepareHtml(page.Content);
              
                _fetched = true;
            }
        }

        private string PrepareHtml(string html)
        {

            // Make title
            if (!Notitle)
            {
                html = "<h2>" + Title + "</h2>" + html;
            }
             html = "<span class=\"invisible chapterstart\">" + (Noheader ? "" : Title) + "</span>" + html;
           
            // Make sure page break is set
            if (Pagebreak)
            {
                html = "<span class=\"invisible pagebreak\"></span>" + html;
            }       

            CQ dom = CQ.CreateFragment("<div class=\"content\">" + html + "</div>");

            // Remove next/prev table
            dom.Find("table").Last().Remove();


            // Find images
            foreach(IDomElement aNode in dom.Find("a.image")) {
                var a = new CQ(aNode);
                var img = new CQ(a.Find("img"));

                var image = new Image(img.Attr("src").Replace("/thumb", "")
                   , new Uri(Wiki)) {Sashie = true};

                CQ node = a.Closest(".thumb").Add(a).First();

                if (_images.Count == 0 && EntryPicture)
                {
                    // We can view it as a full-fledged image since we don't need to worry about text-flow
                    image.Sashie = false;
                    dom.Before(image.Html);
                }
                else
                {
                    node.Before(image.Html);
                    //node.After("<span class=\"image-stopper\"></span>");
                }

                node.Remove();

                _images.Add(image);
            }

            // Catch references
            foreach (IDomElement supNode in dom.Find("sup.reference"))
            {
                var sup = new CQ(supNode);
                CQ footnote = "<span class=\"fn\"></span>";
                CQ oldFootnote = dom.Find("#" + sup.Attr("id").Replace("_ref-", "_note-"));

                footnote.Html(oldFootnote.Find(".reference-text").Html());

                oldFootnote.Remove();
                sup.Before(footnote).Remove();
            }
            // Remove possible reference title
            dom.Find("#references").Prev(":header").Remove();

            // Remove edit links
            dom.Find(".editsection, #toc").Remove();
            

            // Hakomari specific
            foreach (IDomElement star in dom.Find("p:contains(✵)"))
            {
                star.InnerHTML = "<img src=\"" + (new Uri( Helper.GetAssetsPath() + "blackstar.jpg" )) + "\">";
            }

            return dom.Render();
        }
    }
}
