using System;
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
        static void Main(string[] args)
        {
            string solutionFileName = null;
            string searchPattern = "*";
            string transformDirectory = null;
            bool allDirectories = false;
            bool showHelp = false;

            Console.WriteLine("markcode v0.1");

            var p = new OptionSet()
            {
                {"s=","the {SOLUTION} that has the source code.",(string v)=>solutionFileName=v},
                {"d=","the {DIRECTORY} that has the wiki document files. If not provided, will tranform the files in the solution.",v=>transformDirectory = v},
                {"p=","the search {PATTERN} that used to find the wiki document files",v=>searchPattern=v},
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

            if (solutionFileName == null)
            {
                Console.Write("markcode: ");
                Console.WriteLine("option -s is required.");
                Console.WriteLine("Try `markcode --help' for more information.");
                return;
            }

            using (ICodeReflection r = new RoslynReflection(solutionFileName))
            {
                IMarkcodeTransform markcode = new MarkcodeTransform(r);
                if (string.IsNullOrEmpty(transformDirectory))
                {
                    markcode.TransformSolution(searchPattern);
                }
                else
                {
                    markcode.TransformDirectory(transformDirectory, searchPattern, allDirectories?SearchOption.AllDirectories: SearchOption.TopDirectoryOnly);
                }                
            }
        }

        static void ShowHelp(OptionSet p)
        {
            
            Console.WriteLine("Transform all markcode links in the wiki document files.");           
            Console.WriteLine();
            Console.WriteLine("Usage: markcode -s solution-filename [-d wiki-file-directory] [-p search-pattern] [-a] [-h|help]");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
