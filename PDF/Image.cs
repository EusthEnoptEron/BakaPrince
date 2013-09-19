using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace BakaPrince.PDF
{
    [JsonConverter(typeof(ImageToStringConverter))]
    class Image
    {
        public readonly string Url;
        public readonly string Id;
        public readonly float Width;
        public readonly float Height;
        public readonly string Path;
        public bool Sashie = false;

        private const float _sheetWidth = 5.8f;
        private const float _sheetHeight = 8.3f;


        protected Image(string url)
        {
            Url = url;
        }

        public static Image MakeUnloadedImage(string url)
        {
            return new Image(url);
        }

        public Image(string url, Uri basePath) {
            Id = "i" + Helper.CalculateMD5Hash(url);
            var location = new Uri(basePath, url);
            Url = location.AbsolutePath;

            // Load image
            Console.WriteLine("Fetching image {0}", url);
            try
            {
                using (Stream stream = Helper.GetFile(location, Id))
                {
                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                    if (image.Width > image.Height)
                    {
                        // Landscape
                        Width = _sheetHeight;
                        Height = _sheetHeight / image.Width * image.Height;
                    }
                    else
                    {
                        // Portrait
                        Width = _sheetWidth;
                        Height = _sheetWidth / image.Width * image.Height;
                    }

                }
            }
            catch (WebException e)
            {
                Width = 0;
                Height = 0;
                Console.WriteLine("WARNING: couldn't fetch {0} ({1})", location, e.Message);
            }
            Path = Helper.GetTemp(Id);
        }

    
        public string Html
        {
            get
            {
                string classes = "";
                string html = "";

                if (Sashie)
                {
                    classes += "sashie";
                }
                if (Sashie && Width > Height)
                {
                    classes += " landscape";
                }

                if (Sashie && Width < Height)
                {
                    html += "<div class=\"sashie-wrapper\">";
                }
                
                html += String.Format("<div class=\"image {0} {1}\"></div></div>", Id, classes);

                if (Sashie && Width < Height)
                {
                    html += "</div>";
                }

                html += Style;
                //if (Sashie && Height >= Width)
                //{
                //    html += "<span class=\"image-stopper\"></span>";
                //}

                return html;
            }
        }

        public string Style
        {
            get
            {
                return String.Format("<style type='text/css'>{0}</style>", Rules);
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

                    .sashie-wrapper .{0} {{
                        top: {5}in;
                    }}
                ", Id, Width, Height, new Uri(Helper.Cwd, Path), (Sashie && Height > Width) ? "auto" : "p" + Id, (_sheetHeight - Height) / 2 );
                
            }
        }


        class ImageToStringConverter : JsonConverter
        {

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(((Image)value).Url);
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType) {
                return objectType == typeof(Image);
            }
        }

    }
}
