﻿
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using NDesk.Options;
using System.Text.RegularExpressions;
using BakaPrince.Commands;
using System.IO;
using System.Net;

namespace BakaPrince
{
    class Program
    {
        static void Main(string[] args)
        {

            //args = new string[] { @"parse", "Kamisama_no_Memochou", "-v", "2", "-c" };
            //args = new string[] { @"convert", "http://www.zomg.ch/baka/config/hantsuki.json" };
            //args = new string[] { @"E:\Dev\prince\hakomari1.json", "-c", "-s", "stylesheet.css" };
           // args = new string[] { @"convert", @"E:\Dev\prince\tests\Accel World1Return of Princess Snow Black978-4-04-867517-8.json" };

            Command command = null;
            string commandName;
            bool showHelp = false;

            var p = new OptionSet
            {
                { "f", "force re-download of files",
                   v => {
                       Helper.Caching = v == null;
                   }},
                { "h|help",  "show this message and exit", 
                   v => showHelp = v != null }
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
            if (extra.Count == 0) ShowHelp(p);

            // Validate arguments
            foreach (string arg in extra)
            {
                commandName = extra.First();
                switch (commandName)
                {
                    case "parse":
                        command = new ParseCommand();

                        break;
                    case "convert":
                        command = new ConvertCommand();
                        break;
                }
                if (command == null) continue;
                else break;
            }

            if (showHelp || extra.Count < 2)
            {
                ShowHelp(command != null ? command.Options : p);
            }

            if (command != null)
            {
                List<string> newArgs = command.Options.Parse(extra);

                try
                {
                    command.Execute(newArgs.Skip(1).ToArray());
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (WebException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                ShowHelp(p);
            }

        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: bakaprince [OPTIONS]+ [parse|convert] config-path");
            Console.WriteLine("Create a PDF from a Baka-Tsuki project.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);

            Environment.Exit(0);
        }
    }
}
