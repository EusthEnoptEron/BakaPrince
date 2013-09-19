using BakaPrince.PDF;
using CsQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace BakaPrince.BakaTsuki
{
    class IllustrationsPage : BakaPage
    {
        public IllustrationsPage(string name) : base(name) { }
        public Image[] Images
        {
            get
            {
                List<Image> images = new List<Image>();
                foreach(IDomElement el in CQNode.Find("img")) {
                    CQ img = new CQ(el);
                    var uri = new Uri(new Uri(Url),  img.Attr("src"));

                    if (IsColorful(uri))
                    {
                        string url = uri.AbsoluteUri;
                        url = url.Replace("images/thumb", "images");
                        url = url.Substring(0, url.LastIndexOf('/'));

                        images.Add(Image.MakeUnloadedImage(url));
                    }
                }

                return images.ToArray();
            }
        }

        private bool IsColorful(Uri url)
        {
            using (Stream stream = Helper.GetFile(url))
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(stream);
                float saturation = 0;
                int threshold = 100;

                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        saturation += bmp.GetPixel(x, y).GetSaturation();

                        if (saturation > threshold) {
                            return true;
                        };
                    }
                }
            }
            return false;
        }


    }
}
