using BakaPrince.PDF;
using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BakaPrince.BakaTsuki
{
    class Volume
    {
        private List<Chapter> _chapters = new List<Chapter>();
        private IllustrationsPage _illustrationsPage;

        /// <summary>
        /// Parses volume information based on its title element.
        /// </summary>
        /// <param name="hEl"></param>
        public Volume(CQ ul)
        {
            ul.Find("li").Each((el) =>
            {
                var link = new CQ(el).Find("a").First();
                if (link.Count() == 0) return;

                var name = Regex.Replace(link.Attr("href"), "^.+title=", "");

                if (name.Contains("Illustrations") && _illustrationsPage == null)
                {
                    _illustrationsPage = new IllustrationsPage(name);
                }
                else
                {
                    _chapters.Add(new Chapter(name, link.Text()));
                }
            });
        }

        public Chapter[] Chapters
        {
            get
            {
                return _chapters.ToArray();
            }
        }

        public IEnumerable<Image> Illustrations
        {
            get
            {
                if (_illustrationsPage == null)
                    return new Image[0];

                return _illustrationsPage.Images;
            }
        }

        public class Chapter : BakaPage {
            public readonly string Title;
            public Chapter(string name, string label)
                : base(name)
            {
                Title = label;
            }
        }
    }
}
