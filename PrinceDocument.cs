using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BakaPrince
{
    class PrinceDocument
    {
        private static string princePath = @"E:\Applications\Prince\Engine\bin\prince.exe";
        private Prince prince;
        private Config conf;

        public PrinceDocument(Config conf)
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

            Console.WriteLine("Making PDF...");
            using (Stream outputStream = new FileStream(path, FileMode.Create))
            {
                prince.ConvertString(htmlBuilder.ToString(), outputStream);
            }

            Console.WriteLine("Cleaning...");
            // Delete CSS
            File.Delete(tempFile);
        }

        private void CompileCss(Image[] images, StringBuilder builder) {
            foreach (Image image in images)
            {
                builder.Append(image.Rules);
            }
        }


        private void InitBuilder(StringBuilder builder) {
            builder.Append("<html><head></head><body>");
        }

        private void InitCss(StringBuilder builder)
        {
        }

        private void CloseBuilder(StringBuilder builder) {
            builder.Append("</body></html>");
        }
    }

}
