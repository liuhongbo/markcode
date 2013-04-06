using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Markcode.Core
{
    public class MarkcodeTransform :IMarkcodeTransform
    {
        private ICodeReflection _reflector;

        public MarkcodeTransform(ICodeReflection reflector)
        {
            _reflector = reflector;
        }

        public void TransformSolution(string searchPattern = "*")
        {
            //foreach (string file in _reflector.EnumerateFiles())
            //{
            //    string pattern = GetRegexPattern(searchPattern);
            //    if (Regex.IsMatch(Path.GetFileName(file), pattern))
            //    {
            //        TransformFile(file);
            //    }
            //}
            TransformDirectory(_reflector.GetSolutionDirectory(), searchPattern);
        }

        //http://social.msdn.microsoft.com/Forums/en-US/regexp/thread/7123e7fa-97c8-48c6-8761-737f4b01b25b
        public string GetRegexPattern(string searchPattern)
        {
            // escape regex metacharacters
            string pattern = Regex.Escape(searchPattern);

            // anchor for exact match (otherwise it'll yield partial matches)
            // replace desired characters (note escaped metacharacters)
            pattern = "^"    // anchor beginning of string
                        + pattern.Replace(@"\?", ".")     // single char
                                .Replace('_', '.')         // single char
                                .Replace(@"\*", ".*")     // wildcard
                        + "$";    // anchor end of string

            // this replaces multiple dots with a quantifier, for example "..." => ".{3}"
            // it's an optional step to enhance the regex pattern but the behavior is the same
            // unless someone will see the pattern it can be skipped as there's no value added
            pattern = Regex.Replace(pattern, @"(?<!\\)(\.){2,}", m => String.Concat(".{", m.Length, "}"));

            return pattern;
        }

        public void TransformDirectory(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            foreach (string file in Directory.EnumerateFiles(path, searchPattern, searchOption))
            {
                TransformFile(file);
            }
        }

        public void TransformFile(string path, string newPath = null)
        {
            List<string> newLines = new List<string>();
            if (string.IsNullOrEmpty(newPath)) newPath = path;

            string pattern = @"^<!-{2,}.*\{(.+)\}.*-{2,}>$";

            Regex r = new Regex(pattern);
            bool transformed = false;
            bool skipcode = false;

            foreach (string line in File.ReadLines(path))
            {
                if (skipcode)
                {
                    if (!line.StartsWith("    "))
                    {
                        if (!Regex.IsMatch(line, @"^<!-{2,}.*\{markcodeend\}.*-{2,}>$"))
                        {
                            skipcode = false;
                        }
                    }
                }

                if (!skipcode)
                    newLines.Add(line);

                Match m = r.Match(line);
                if (m.Success && (m.Groups.Count == 2))
                {
                    string link = m.Groups[1].Value;
                    foreach (string s in TransformLink(link).Split(new char[]{'\r','\n'}, StringSplitOptions.RemoveEmptyEntries).AsEnumerable())
                    {
                        newLines.Add("    "+s);
                        transformed = true;
                        skipcode = true;
                    }
                    newLines.Add(@"<!---{endmarkcode}--->");
                }                
            }

            if (transformed)
            {
                File.WriteAllLines(newPath, newLines.AsEnumerable());
            }
            else
            {
                if (path != newPath)
                {
                    File.Copy(path, newPath);
                }
            }
        }

        public string TransformLink(string link)
        {
            return _reflector.GetText(link);
        }

        public string TransformString(string s)
        {
            return null;
        }

        public void TransformStream(StreamReader reader, StreamWriter writer)
        {
        }       

    }
}
