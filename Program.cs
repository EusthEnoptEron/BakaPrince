using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;
using System.Text.RegularExpressions;

namespace BakaPrince
{
    class Program
    {
        static void Main(string[] args)
        {
            //args = new string[] { @" E:\Dev\prince\hakomari1.json", "-c" };
            bool showHelp = false;
            string configPath = null;
            string outputPath = null;
            string princePath = null;

            var p = new OptionSet() {
                { "p|prince=", "the {PATH} where PrinceXML is located. Leave away to find it automatically.",
                   v => princePath = v },
                { "o", "where to write the resulting PDF",
                   v => outputPath = v },
                { "h|help",  "show this message and exit", 
                   v => showHelp = v != null },
                { "c|cache", "enable caching", 
                   v => Helper.Caching = v != null}
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("bakaprince: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `bakaprince --help' for more information.");
                return;
            }

            // Show help if necessary
            if (extra.Count == 0 || showHelp) ShowHelp(p);

            // Validate arguments
            configPath = extra.First();

            if (princePath == null) {
                try {
                    princePath = GetPrincePath();
                } catch(PrinceNotFoundException e) {
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            if (outputPath == null)
            {
                outputPath = DetermineOutputPath(configPath);
            }

            // Parse config
            Config conf = new Config(configPath);

            // Generate document
            PrinceDocument doc = new PrinceDocument(conf, princePath);

            // Write PDF
            doc.Create(outputPath);
        }


        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: bakaprince [OPTIONS]+ config-path");
            Console.WriteLine("Create a PDF from a Baka-Tsuki project.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);

            Environment.Exit(0);
        }



        private static string GetPrincePath()
        {
            //Registry path which has information of all the softwares installed on machine
            string uninstallKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        if ( (string)sk.GetValue("DisplayName") == "Prince"
                            && (string)sk.GetValue("Publisher") == "Yes Logic Pty Ltd")
                        {
                            return ((string)sk.GetValue("InstallLocation")) + @"Engine\bin\prince.exe";
                        }
                    }

                }
            }

            throw new PrinceNotFoundException("Could not find an installation of PrinceXML on your system. Download it at http://www.princexml.com/");
        }

        private static string DetermineOutputPath(string configPath)
        {
            string outputPath = null;

            // Start by normalizing config path
            Uri inputPath = new Uri(configPath);
            if (inputPath.ToString().StartsWith("file://"))
            {
                // Local path -> try to make name
                outputPath = inputPath.LocalPath;
            }
            else
            {
                // Online -> write to CWD
                outputPath = inputPath.Segments.Last();
            }

            // Try to replace extension with .pdf
            if (Regex.IsMatch(outputPath, @"\w+\.\w+$"))
            {
                outputPath = Regex.Replace(outputPath, @"\.\w+$", ".pdf");
            }
            else
            {
                // Otherwise, just append it
                outputPath += ".pdf";
            }

            return outputPath;
        }
    }
}
