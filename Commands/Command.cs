using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BakaPrince.Commands
{
    public abstract class Command
    {
        public abstract void Execute(string[] args);

        protected string DetermineOutputPath(Uri inputPath, string extension)
        {
            string outputPath;
            
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
                outputPath = Regex.Replace(outputPath, @"\.\w+$", extension);
            }
            else
            {
                // Otherwise, just append it
                outputPath +=  extension;
            }

            return outputPath;
        }

        public abstract OptionSet Options
        {
            get;
        }
    }
}
