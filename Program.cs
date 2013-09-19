﻿
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using NDesk.Options;
using System.Text.RegularExpressions;
using BakaPrince.Commands;

namespace BakaPrince
{
    class Program
    {
        static void Main(string[] args)
        {

            args = new string[] { @"parse", "Utsuro no Hako to Zero no Maria", "-v", "2", "-c" };
            //args = new string[] { @"convert", "http://www.zomg.ch/baka/config/hantsuki.json", "-c" };
            //args = new string[] { @" E:\Dev\prince\hakomari1.json", "-c", "-s", "stylesheet.css" };
            Command command = null;
            string commandName;
            bool showHelp = false;

            var p = new OptionSet
            {
                { "c|cache", "enable caching", 
                   v => Helper.Caching = v != null},
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
            if (extra.Count < 2 || showHelp) ShowHelp(p);

            // Validate arguments
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

            if (command != null)
            {
                List<string> newArgs = command.Options.Parse(args);
                command.Execute(newArgs.Skip(1).ToArray());
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
