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
            foreach(IDomElement el in ul.Find("li").Skip(1)) {
                var link = new CQ(el).Find("a:first");

                _chapters.Add(new Chapter(Regex.Replace(link.Attr("href"), "^.+title=", ""),link.Text()));
            }
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
