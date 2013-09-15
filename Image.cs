using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BakaPrince
{
    class Image
    {
        public readonly string ID;
        public readonly float Width;
        public readonly float Height;
        public readonly string Path;
        public bool Sashie = false;

        private float a5width = 5.8f;
        private float a5height = 8.3f;

        public Image(string url, Uri basePath) {
            ID = "i" + Helper.CalculateMD5Hash(url);

            Uri location = new Uri(basePath, url);

            Path = System.IO.Path.GetTempPath() + ID;
            if (File.Exists(Path))
            {
                location = new Uri(Path);
            }

            
            // Load image
            WebRequest req = HttpWebRequest.Create(location);
            using (Stream stream = req.GetResponse().GetResponseStream())
            {
                System.Drawing.Image image = Bitmap.FromStream(stream);
                if (image.Width > image.Height)
                {
                    // Landscape
                    Width = a5height;
                    Height = a5height / image.Width * image.Height;
                }
                else
                {
                    // Portrait
                    Width = a5width;
                    Height = a5width / image.Width * image.Height;
                }

                // Save image
                if (!File.Exists(Path))
                {
                    image.Save(Path);
                }
            }
        }

    
        public string HTML
        {
            get
            {
                string classes = "";
                if (Sashie)
                {
                    classes += "sashie";
                }
                if (Sashie && Width > Height)
                {
                    classes += " landscape";
                }
                string html = String.Format("<div class=\"image {0} {1}\"></div>", ID, classes);

                //if (Sashie && Height >= Width)
                //{
                //    html += "<span class=\"image-stopper\"></span>";
                //}

                return html;
            }
        }

        public string Rules
        {
            get
            {
                return String.Format(@"
                    @page p{0} {{
			            size: {1}in {2}in;
			            margin: 0;

                        @top-center {{
                           content: normal;
                        }}

                        @bottom-center {{
                            content: normal;
                        }}
		            }}

                    .{0} {{
                        background-image: url({3});
                        page: {4};
	                    width: {1}in;
	                    height: {2}in;
                     }}
                ", ID, Width, Height, new Uri(Path), (Sashie && Height > Width) ? "auto" : "p" + ID);
            }
        }
    }
}
