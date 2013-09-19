using BakaPrince.BakaTsuki;
using CsQuery;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BakaPrince.PDF;
using System.IO;

namespace BakaPrince.Commands
{
    class ParseCommand : Command
    {
        private string _outputPath = null;
        private int _volumeNum = 0;
        public override void Execute(string[] args)
        {
            string projectName = args[0];

            if (_outputPath == null)
            {
                _outputPath = DetermineOutputPath(new Uri(Helper.Cwd, projectName + ".html"), ".json");
            }

            if(_volumeNum == 0)
            {
                Console.WriteLine("No volume number given, assuming '1'...");
                _volumeNum = 1;
            }

            Project project = new Project(projectName);
            if (project.Volumes.Length >= _volumeNum)
            {
                Volume volume = project.Volumes[_volumeNum - 1];
                PDF.Config conf = PDF.Config.Empty;

                conf.Title = project.Title;
                conf.Project = project.Url;
                //conf.Images.AddRange(volume.Illustrations);
                conf.Pages.AddRange(
                    volume.Chapters.Select(v => { return new Page() { Name = v.Name, Title = v.Title }; })
                );

                File.WriteAllText(_outputPath, conf.ToJSON());
            }
            else
            {
                Console.WriteLine("Volume {0} not found", _volumeNum);
            }
        }

        public override NDesk.Options.OptionSet Options
        {
            get {
                return new NDesk.Options.OptionSet
                {
                    {"o", "output path",
                        v => _outputPath = v },
                    {"v|volume=", "volume number",
                        (int v) => _volumeNum = v }
                };
            }
        }
    }
}
