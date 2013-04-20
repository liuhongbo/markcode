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

        #region Ctor

        public MarkcodeTransform(ICodeReflection reflector)
        {
            _reflector = reflector;
        }

        #endregion

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
        
        public void TransformDirectory(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories)
        {
            Console.WriteLine("Start transform dirctory " + path + " ...");

            foreach (string file in Directory.EnumerateFiles(path, searchPattern, searchOption))
            {
                TransformFile(file);
            }
        }        

        public void TransformFile(string path, string newPath = null)
        {
            Console.WriteLine("Start transform file " + path + " ...");

            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(path);

            string ext = Path.GetExtension(path);

            List<string> newLines = new List<string>();
            if (string.IsNullOrEmpty(newPath)) newPath = path;

            string pattern = @"^<!-{2,}.*\{(.+)\}.*-{2,}>$";

            Regex r = new Regex(pattern);
            bool transformed = false;
            bool skipCode = false;
            bool skippingCode = false;
            bool endMarkcode = false;

            foreach (string line in File.ReadLines(path))
            {
                if (skipCode)
                {
                    switch (ext)
                    {
                        case ".md":
                        case ".markdown":
                        case ".mdown":
                            if (!line.StartsWith("    "))
                            {
                                if (Regex.IsMatch(line, @"^<!-{2,}.*\{\?endmarkcode\}.*-{2,}>$"))
                                {
                                    skipCode = false;                                    
                                    continue;
                                }
                                else if (line != string.Empty)
                                {
                                    skipCode = false;
                                }
                            }
                            break;
                        case ".creole":
                            if (skippingCode)
                            {
                                if (Regex.IsMatch(line, @"^\{\{\{\x20*$"))
                                {
                                    skippingCode = false;
                                }
                            }
                            else
                            {
                                if (Regex.IsMatch(line, @"^<!-{2,}.*\{\?endmarkcode\}.*-{2,}>$"))
                                {
                                    skipCode = false;
                                    continue;
                                }
                                else if (Regex.IsMatch(line, @"^}}}\x20*$"))
                                {
                                    skippingCode = true;
                                }
                                else if (line != string.Empty)
                                {
                                    skipCode = false;
                                }
                            }
                            break;
                    }
                }

                if (!skipCode)
                {
                    if (endMarkcode) // make sure there is empty line after endmarkcode
                    {
                        if (line != string.Empty)
                        {
                            newLines.Add("");
                        }
                        endMarkcode = false;
                    }
                    newLines.Add(line);

                    Match m = r.Match(line);
                    if (m.Success && (m.Groups.Count == 2))
                    {
                        string link = m.Groups[1].Value;
                        string codeText = TransformLink(link);
                        //normalize here?
                        if (!string.IsNullOrEmpty(codeText))
                        {
                            switch (ext)
                            {
                                case ".md":
                                case ".markdown":
                                case ".mdown":
                                    newLines.Add("");
                                    break;
                                case ".creole":
                                    newLines.Add("{{{");
                                    break;
                            }

                            foreach (string s in codeText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable())
                            {
                                switch (ext)
                                {
                                    case ".md":
                                    case ".markdown":
                                    case ".mdown":
                                        newLines.Add("    " + s);
                                        break;
                                    case ".creole":
                                        newLines.Add(s);
                                        break;
                                }
                            }

                            switch (ext)
                            {
                                case ".md":
                                case ".markdown":
                                case ".mdown":
                                    newLines.Add("");
                                    break;
                                case ".creole":
                                    newLines.Add("}}}");
                                    break;
                            }

                            transformed = true;
                            skipCode = true;
                            skippingCode = false;
                            newLines.Add(@"<!---{?endmarkcode}--->");
                            endMarkcode = true;
                        }
                    }
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

        #region Utilities

        //http://social.msdn.microsoft.com/Forums/en-US/regexp/thread/7123e7fa-97c8-48c6-8761-737f4b01b25b
        private string GetRegexPattern(string searchPattern)
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

        #endregion
    }
}
