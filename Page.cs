using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsQuery;

namespace BakaPrince
{
    class Page
    {
        // public
        public string Prefix;
        public string Name;
        public string Title = null;
        public bool Pagebreak = true;
        public bool Notitle = false;
        public bool Noheader = false;

        public string Wiki = "http://www.baka-tsuki.org/project/";
        public bool EntryPicture = false;


        private bool fetched = false;
        private string html = "";
        private List<Image> images = new List<Image>();

        public Page()
        {
        }

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
                    default: break;
                }
            }
        }

        public string HTML
        {
            get
            {
                Fetch();

                return html;
            }
        }

        /// <summary>
        /// Returns the images contained in this page.
        /// </summary>
        public Image[] Images
        {
            get
            {
                Fetch();
                return images.ToArray();
            }
        }



        private void Fetch() {
            if (!fetched)
            {
                if (Title == null)
                    Title = Name;

                string url = Wiki + "api.php?action=parse&format=json&page=" + Prefix + Name;

                Console.WriteLine("Fetching page {0}", Name);
                string myString = Helper.GetString(new Uri(url));
                html = PrepareHTML((string)JObject.Parse(Helper.GetString(new Uri(url))).SelectToken("parse.text.*"));
              
                fetched = true;
            }
        }

        private string PrepareHTML(string html)
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
            foreach(IDomElement _a in dom.Find("a.image")) {
                CQ a = new CQ(_a);
                CQ img = new CQ(a.Find("img"));

                Image image = new Image(img.Attr("src").Replace("/thumb", "")
                   , new Uri(Wiki));

                image.Sashie = true;

                CQ node = a.Closest(".thumb").Add(a).First();

                if (images.Count == 0 && EntryPicture)
                {
                    // We can view it as a full-fledged image since we don't need to worry about text-flow
                    image.Sashie = false;
                    dom.Before(image.HTML);
                }
                else
                {
                    node.Before(image.HTML);
                    //node.After("<span class=\"image-stopper\"></span>");
                }

                node.Remove();

                images.Add(image);
            }

            // Catch references
            foreach (IDomElement _sup in dom.Find("sup.reference"))
            {
                CQ sup = new CQ(_sup);
                CQ footnote = "<span class=\"fn\"></span>";
                CQ oldFootnote = dom.Find("#" + sup.Attr("id").Replace("_ref-", "_note-"));

                footnote.Html(oldFootnote.Find(".reference-text").Html());

                oldFootnote.Remove();
                sup.Before(footnote).Remove();
            }

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
