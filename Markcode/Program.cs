﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Markcode.Core;
using Mono.Options;

namespace Markcode
{
    class Program
    {
        private const string SolutionFileNameExtension = "sln";

        static void Main(string[] args)
        {
            string solutionFileName = null;
            string searchPattern = "*";
            string transformDirectory = null;
            string currentDirectory = null;
            bool allDirectories = false;
            bool showHelp = false;

            Console.WriteLine("markcode v0.1");
            
            var p = new OptionSet()
            {
                {"s=","the {SOLUTION} that contains the source code. If not provided, will search the default solution file",(string v)=>solutionFileName=v},
                {"d=","the {DIRECTORY} where the wiki document files located. If not provided, will tranform the files in the solution.",v=>transformDirectory = v},
                {"p=","the search {PATTERN} that used to filter the wiki document files",v=>searchPattern=v},
                {"c=","the {CURRENT DIRECTORY} used to resolve the relative file path link in the wiki documents.", v=>currentDirectory = v},
                {"a","search all the directoires",v=>allDirectories=(v!=null)},
                {"h|help","show this message and exit", v=>showHelp=(v!=null)}
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("markcode: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `markcode -help for more information.");
                return;
            }

            if (showHelp)
            {
                ShowHelp(p);
                return;
            }
            
            Console.WriteLine("Initial Current Directory: " + Directory.GetCurrentDirectory());

            if (string.IsNullOrEmpty(solutionFileName))
            {
                solutionFileName = GetDefaultSolutionFile();
            }

            if (solutionFileName == null)
            {
                Console.Write("markcode: ");
                Console.WriteLine("solution filename is not specified.");
                Console.WriteLine("Try `markcode --help' for more information.");
                return;
            }
            else
            {
                Console.WriteLine("Use solution file " + solutionFileName);
            }

            using (ICodeReflection r = new RoslynReflection(solutionFileName))
            {
                if (currentDirectory != null)
                {
                    Console.WriteLine("Set Current Directory to: " + currentDirectory);
                    Directory.SetCurrentDirectory(currentDirectory);
                }
                else
                {
                    Console.WriteLine("Set Current Directory to: " + r.GetSolutionDirectory());
                    Directory.SetCurrentDirectory(r.GetSolutionDirectory());
                }
                Console.WriteLine("Start transform...");
                IMarkcodeTransform markcode = new MarkcodeTransform(r);
                if (string.IsNullOrEmpty(transformDirectory))
                {
                    markcode.TransformSolution(searchPattern);
                }
                else
                {
                    Console.WriteLine("Transform Directory :" + transformDirectory);
                    markcode.TransformDirectory(transformDirectory, searchPattern, allDirectories?SearchOption.AllDirectories: SearchOption.TopDirectoryOnly);
                }
                Console.WriteLine("Transform finished.");
            }
        }

        static string GetDefaultSolutionFile()
        {
            string[] DefaultSolutionFilePaths = new string[]{
                @".\",
                @"..\",
                @"..\..\"};

            string defaultSolutionFile = null;

            foreach (string s in DefaultSolutionFilePaths)
            {

                defaultSolutionFile = Directory.GetFiles(s, "*." + SolutionFileNameExtension, SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (!string.IsNullOrEmpty(defaultSolutionFile))
                {
                    break;
                }
            }
            
            return defaultSolutionFile;
        }

        static void ShowHelp(OptionSet p)
        {            
            Console.WriteLine("Transform all markcode links in the wiki document files.");           
            Console.WriteLine();
            Console.WriteLine("Usage: markcode [-s solution-filename] [-d wiki-file-directory] [-p search-pattern] [-a] [-h|help]");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
