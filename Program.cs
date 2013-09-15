using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BakaPrince
{
    class Program
    {
        //static string jsonLocation = @"E:\Dev\prince\hakomari1.json";
        //static string jsonLocation = @"E:\Dev\prince\toradora.json";
        //static string jsonLocation = @"E:\Dev\prince\hantsuki.json";
        //static string jsonLocation = @"E:\Dev\prince\gekkou.json";
        //static string jsonLocation = @"E:\Dev\prince\tsukumodo1.json";
        static string jsonLocation = @"E:\Dev\prince\hakomari1.json";

        static void Main(string[] args)
        {
            Config conf = new Config(jsonLocation);

            PrinceDocument doc = new PrinceDocument(conf);


            doc.Create(@"E:\Dev\prince\output.pdf");
        }
    }
}
