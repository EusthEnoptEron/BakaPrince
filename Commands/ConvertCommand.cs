using Microsoft.Win32;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BakaPrince.Commands
{
    public class ConvertCommand : Command
    {
        public ConvertCommand()
        {
            // TODO: Complete member initialization
        }
        private string _princePath = null;
        private string _outputPath = null;
        private string _stylesheet = null;

        public override OptionSet Options
        {
            get
            {
                return new OptionSet
                {
                    { "p|prince=", "the {PATH} where PrinceXML is located. Leave away to find it automatically.",
                       v => _princePath = v },
                    { "o", "where to write the resulting PDF",
                       v =>  _outputPath = v },
                    { "s|stylesheet=", "specify an additional stylesheet to use",
                       v => _stylesheet = v },

                    // Handled by Program.cs
                    { "c|cache", "enable caching", 
                       v => {} },
                    { "h|help",  "show this message and exit", 
                       v => {} }
                };
            }
        }
        public override void Execute(string[] args)
        {
            Uri inputPath = new Uri(Helper.Cwd, args[0]);

            if (_princePath == null)
            {
                try
                {
                    _princePath = GetPrincePath();
                }
                catch (PrinceNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            if (_outputPath == null)
            {
                _outputPath = DetermineOutputPath(inputPath, ".pdf");
            }

            // Parse config
            Config conf = new Config(inputPath);

            // Generate document
            PrinceDocument doc = new PrinceDocument(conf, _princePath);

            if (_stylesheet != null)
            {
                doc.AddStyleSheet(new Uri(Helper.Cwd, _stylesheet).AbsolutePath);
            }

            // Write PDF
            doc.Create(_outputPath);

        }


        private string GetPrincePath()
        {
            //Registry path which has information of all the softwares installed on machine
            string uninstallKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                if (rk != null)
                    foreach (string skName in rk.GetSubKeyNames())
                    {
                        using (RegistryKey sk = rk.OpenSubKey(skName))
                        {
                            if (sk != null && ((string)sk.GetValue("DisplayName") == "Prince"
                                                && (string)sk.GetValue("Publisher") == "Yes Logic Pty Ltd"))
                            {
                                return ((string)sk.GetValue("InstallLocation")) + @"Engine\bin\prince.exe";
                            }
                        }

                    }
            }

            throw new PrinceNotFoundException("Could not find an installation of PrinceXML on your system. Download it at http://www.princexml.com/");
        }
    }
}
