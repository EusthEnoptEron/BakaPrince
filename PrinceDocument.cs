using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CsQuery;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;


namespace BakaPrince
{
    /// <summary>
    /// Document that has to be converted to a PDF file.
    /// </summary>
    class PrinceDocument
    {
        private Prince prince;
        private Config conf;

        /// <summary>
        /// Initialize document.
        /// </summary>
        /// <param name="conf">Config object that defines which pages and images to use.</param>
        /// <param name="princePath">Path to the PrinceXML binary.</param>
        public PrinceDocument(Config conf, string princePath)
        {
            prince = new Prince(princePath);

            // We are dealing with Html
            prince.SetHTML(true);

            // Add default stylesheets
            prince.AddStyleSheet(AppDomain.CurrentDomain.BaseDirectory + "\\assets\\mediawiki.css");
            prince.AddStyleSheet(AppDomain.CurrentDomain.BaseDirectory + "\\assets\\book.css");

            // Add additional stylesheets
            foreach (Uri path in conf.StyleSheets)
            {
                prince.AddStyleSheet(path.AbsolutePath);
            }

            this.conf = conf;
        }


        /// <summary>
        /// Add a stylesheet to the PDF render process.
        /// </summary>
        /// <param name="cssPath">Path to the stylesheet.</param>
        public void AddStyleSheet(string cssPath)
        {
            prince.AddStyleSheet(cssPath);
        }


        /// <summary>
        /// Create the PDF file.
        /// </summary>
        /// <param name="path">Path where to save the file.</param>
        public void Create(string path) {
            // Set base url to the Wiki URL (for images that we didn't catch, etc.)
            prince.SetBaseURL(conf.BaseUrl);

            // Init builder
            StringBuilder htmlBuilder = new StringBuilder();
            InitBuilder(htmlBuilder);
         
            // Compile color images
            Console.WriteLine("Creating color pages...");
            foreach (Image image in conf.Images)
            {
                htmlBuilder.Append(image.Html);
            }


            Console.WriteLine("Compiling chapters...");
            // Compile Html
            foreach (Page page in conf.Pages)
            {
                htmlBuilder.Append(page.Html);
            }

            CloseBuilder(htmlBuilder);

            Console.WriteLine("Writing PDF to {0}...", new FileInfo(path).FullName);
            using (Stream outputStream = new FileStream(path, FileMode.Create))
            {
                prince.ConvertString(htmlBuilder.ToString(), outputStream);
            }

            Console.WriteLine("Cleaning...");
            MoveDisclaimer(path);

            Console.WriteLine("Et voilà -- your PDF is ready.");
        }

        private void MoveDisclaimer(string path)
        {
            using (PdfDocument importDoc = PdfReader.Open(path, PdfDocumentOpenMode.Import))
            using (PdfDocument modifyDoc = PdfReader.Open(path, PdfDocumentOpenMode.Modify))
            {
                PdfPage page = importDoc.Pages[0];
                modifyDoc.Pages.RemoveAt(0);
                modifyDoc.Pages.Insert(conf.Images.Length, page);

                modifyDoc.Save(path);
            }
        }


        private void InitBuilder(StringBuilder builder) {
            builder.Append(String.Format(@"
                <html>
                    <head>
                        <title>{0}</title>
                        <meta name=""author"" content=""{1}""/>
                        <meta name=""generator"" content=""BakaPrince""/>
                    </head>
                <body>", conf.Title.Replace(@"""", @"\"""),
                         String.Join(", ", conf.Contributors["authors"]).Replace(@"""", @"\""")));
            
            // Add disclaimer
            AppendDisclaimer(builder);
        }


        private void CloseBuilder(StringBuilder builder) {
            builder.Append("</body></html>");
        }

        private void AppendDisclaimer(StringBuilder builder)
        {
            PluralizationService lang = PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"));

            CQ disclaimer = File.ReadAllText(Helper.GetAssetsPath() + "disclaimer.html");
            CQ table = disclaimer.Find("table#contributors");

            // Append header row
            if(!conf.Title.Equals(""))
                table.Append("<tr><th colspan='2' class='header'></th></tr>").Find(".header").Text(conf.Title);

            IEnumerator<KeyValuePair<string, List<string>>> it = conf.Contributors.GetEnumerator();
            while (it.MoveNext())
            {
                string key = it.Current.Key;
                key = key[0].ToString().ToUpper() + key.Substring(1);
                if (it.Current.Value.Count == 1)
                {
                    key = lang.Singularize(key);
                }
                else if (it.Current.Value.Count == 0)
                {
                    continue;
                }

                CQ tr = "<tr>";
                ((CQ)("<th>")).Text(key).AppendTo(tr);
                ((CQ)("<td>")).Text(String.Join(", ", it.Current.Value)).AppendTo(tr);
                
                table.Append(tr);
            }

           
            if(!conf.Project.Equals(""))
                table.Append("<tr><th>Project page</th><td></td></tr>").Find("tr:last td").Append(String.Format("<a href='{0}'>{0}</a>", conf.Project));

            table.Append(String.Format("<tr><th>PDF creation date</th><td>{0}</td>", DateTime.Today.ToString("yyyy-MM-dd")));
            builder.Append(disclaimer.Render());
        }
    }

}
