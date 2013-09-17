﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsQuery;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;


namespace BakaPrince
{
    class PrinceDocument
    {
        private Prince prince;
        private Config conf;

        public PrinceDocument(Config conf, string princePath)
        {
            prince = new Prince(princePath);
            prince.SetHTML(true);
            prince.AddStyleSheet(AppDomain.CurrentDomain.BaseDirectory + "\\assets\\mediawiki.css");
            prince.AddStyleSheet(AppDomain.CurrentDomain.BaseDirectory + "\\assets\\book.css");

            this.conf = conf;
        }

        public void AddStyleSheet(string cssPath)
        {
            prince.AddStyleSheet(cssPath);
        }

        public void Create(string path) {
            prince.SetBaseURL(conf.BaseURL);

            StringBuilder cssBuilder = new StringBuilder();
            StringBuilder htmlBuilder = new StringBuilder();
            string tempFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".css";

            InitBuilder(htmlBuilder);
            InitCss(cssBuilder);

            // Compile front images
            Console.WriteLine("Creating color pages...");
            CompileCss(conf.Images, cssBuilder);
            foreach (Image image in conf.Images)
            {
                htmlBuilder.Append(image.HTML);
            }


            Console.WriteLine("Compiling chapters...");
            // Compile HTML
            foreach (Page page in conf.Pages)
            {
                htmlBuilder.Append(page.HTML);
                CompileCss(page.Images, cssBuilder);
            }

            CloseBuilder(htmlBuilder);

            // Create CSS
            File.WriteAllText(tempFile, cssBuilder.ToString());

            prince.AddStyleSheet(tempFile);

            Console.WriteLine("Writing PDF to {0}...", new FileInfo(path).FullName);
            using (Stream outputStream = new FileStream(path, FileMode.Create))
            {
                prince.ConvertString(htmlBuilder.ToString(), outputStream);
            }

            Console.WriteLine("Cleaning...");
            // Delete CSS
            File.Delete(tempFile);


            MoveDisclaimer(path);

            Console.WriteLine("Et voilà -- your PDF is ready.");
        }

        private void MoveDisclaimer(string path)
        {
            
            PdfDocument outputDocument = new PdfDocument();

            using (PdfDocument importDoc = PdfReader.Open(path, PdfDocumentOpenMode.Import))
            using (PdfDocument modifyDoc = PdfReader.Open(path, PdfDocumentOpenMode.Modify))
            {
                PdfPage page = importDoc.Pages[0];
                modifyDoc.Pages.RemoveAt(0);
                modifyDoc.Pages.Insert(conf.Images.Length, page);

                modifyDoc.Save(path);
            }
        }

        private void CompileCss(Image[] images, StringBuilder builder)
        {
            foreach (Image image in images)
            {
                builder.Append(image.Rules);
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

        private void InitCss(StringBuilder builder)
        {
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

            string rendered = disclaimer.Render();

        }
    }

}
