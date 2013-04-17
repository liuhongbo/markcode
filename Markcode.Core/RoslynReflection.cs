using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.CSharp;
using Roslyn.Services.Formatting;
using Roslyn.Services.MetadataAsSource;

namespace Markcode.Core
{
    public class RoslynReflection : ICodeReflection
    {
        #region Fields
     
        private const string SELECTOR = "#";

        private IWorkspace _workspace;
        private Compilation _currentCompilation;

        private bool disposed = false;

        #endregion

        #region Ctor

        public RoslynReflection(string solutionFileName)
        {
            _workspace = Workspace.LoadSolution(solutionFileName);             
        }

        public RoslynReflection(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);            
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {           
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                    _workspace.Dispose();
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.               

                // Note disposing has been done.
                disposed = true;

            }
        }

        #endregion

        #region ICodeReflection

        public string GetText(string fullName)
        {
            string pattern = @"^(?<link>[^" + SELECTOR + @"]+)" +
                             @"(" +
                             SELECTOR +
                             @"(?<selection>[^" + SELECTOR + @"]+)" +
                             @")?$";

            Match m = Regex.Match(fullName, pattern, RegexOptions.ExplicitCapture);
            string text = null;
            if (m.Success)
            {
                if (m.Groups["link"].Success)
                {
                    string link = m.Groups["link"].Value;                    

                    if (File.Exists(link))
                    {
                        text = File.ReadAllText(link);
                    }
                    else
                    {
                        text = GetIdentifierText(link);
                    }

                    if (m.Groups["selection"].Success)
                    {
                        text = GetTextSelection(text, m.Groups["selection"].Value);
                    }
                }
            }

            return text;
        }        

        public IEnumerable<string> EnumerateFiles()
        {            
            foreach (IProject p in _workspace.CurrentSolution.Projects)
            {
                foreach (IDocument d in p.Documents.Where(c=>c.FilePath!=null))
                {
                    yield return d.FilePath;
                }
            }
        }

        public string GetSolutionDirectory()
        {
            return Path.GetDirectoryName(_workspace.CurrentSolution.FilePath);
        }

        #endregion

        #region Utilities

        public string GetIdentifierText(string fullName)
        {
            string[] names = fullName.Trim().Split('.');
            string param = null;

            if (names.Length == 0) return null;

            string lastName = names[names.Length - 1];
            string pattern = @"^(?<name>\w+)" +
                             @"(\((?<params>.+)\))*" +
                             @"\s*$";

            Match m = Regex.Match(lastName, pattern, RegexOptions.ExplicitCapture);
            if (m.Success)
            {
                if (m.Groups["name"].Success)
                {
                    names[names.Length - 1] = m.Groups["name"].Value;
                }

                if (m.Groups["params"].Success)
                {
                    param = m.Groups["params"].Value;
                }
            }


            Symbol s = null;

            for (int i = 0; i < names.Length - 1; i++)
            {
                s = GetSymbol(s, names[i]);
                if (s == null) return null;
            }

            if (string.IsNullOrEmpty(param))
            {
                s = GetSymbol(s, names[names.Length - 1]);
            }
            else
            {
                s = GetSymbol(s, names[names.Length - 1], param);
            }

            if (s == null) return null;

            string text = string.Empty;

            switch (s.Kind)
            {
                case SymbolKind.Namespace:
                    NamespaceSymbol ns = (s as NamespaceSymbol);
                    if (ns.Locations.Any(l => l.IsInSource))
                    {
                        foreach (SyntaxNode node in ns.DeclaringSyntaxNodes)
                        {
                            text += node.Format(FormattingOptions.GetDefaultOptions()).GetFormattedRoot().ToFullString();
                            text += "\r\n";
                        }
                    }
                    else
                    {
                    }
                    break;
                case SymbolKind.NamedType:
                    NamedTypeSymbol ts = (s as NamedTypeSymbol);
                    if (ts.Locations.Any(l => l.IsInSource))
                    {
                        foreach (SyntaxNode node in ts.DeclaringSyntaxNodes)
                        {
                            text += node.Format(FormattingOptions.GetDefaultOptions()).GetFormattedRoot().ToFullString();
                            text += "\r\n";
                        }
                    }
                    else
                    {
                        //        ts as ISymbol .GenerateSyntax()

                    }
                    break;
                case SymbolKind.Method:
                    MethodSymbol ms = (s as MethodSymbol);

                    if (ms.Locations.Any(l => l.IsInSource))
                    {
                        foreach (SyntaxNode node in ms.DeclaringSyntaxNodes)
                        {
                            text += node.Format(FormattingOptions.GetDefaultOptions()).GetFormattedRoot().ToFullString();
                            text += "\r\n";
                        }
                    }
                    else
                    {
                    }
                    break;
                case SymbolKind.Local:
                    LocalSymbol ls = (s as LocalSymbol);
                    if (ls.Locations.Any(l => l.IsInSource))
                    {
                        foreach (SyntaxNode node in ls.DeclaringSyntaxNodes)
                        {
                            text += node.Format(FormattingOptions.GetDefaultOptions()).GetFormattedRoot().ToFullString();
                            text += "\r\n";
                        }
                    }
                    else
                    {
                    }
                    break;
            }

            return text;
        }        

        Symbol GetSymbol(Symbol s, string name, string param)
        {
            string typeName = "_"+ Guid.NewGuid().ToString().Replace("-","");
            string ns = s.ContainingSymbol.ToString();
            SyntaxTree tree = SyntaxTree.ParseText(@"namespace " + ns  +  "{ " +
                                                        " class " + typeName + " { " + 
                                                            "void " + name + "(" + param + "){} " +
                                                        "}" +
                                                    "}");
            if (_currentCompilation == null) return null;

            Compilation compilation = Compilation.Create("method")
                                                 .AddReferences(_currentCompilation.References)
                                                 .AddSyntaxTrees(_currentCompilation.SyntaxTrees.AsEnumerable())
                                                 .AddSyntaxTrees(tree);

            Symbol s0 = compilation.GetTypeByMetadataName(ns + "." + typeName);
            
            s0 = GetSymbol(s0, name);

            string displayString = s0.ToDisplayString().Replace(typeName,s.Name);

            return GetSymbols(s, name).FirstOrDefault(c => (c.Kind == SymbolKind.Method) && (c.ToDisplayString() == displayString));
        }

        IEnumerable<Symbol> GetSymbols(Symbol s, string name)
        {
            if (s == null)
            {
                foreach (IProject project in _workspace.CurrentSolution.Projects)
                {
                    Compilation compilation = project.GetCompilation() as Compilation;
                    if (compilation == null) return null;
                    _currentCompilation = compilation;
                    IEnumerable<Symbol> c = GetSymbols(compilation.GlobalNamespace, name);
                    if (c != null) return c;
                }
            }
            else
            {
                return GetMembers(s).Where(m => m.Name == name);
            }
            return ReadOnlyArray<Symbol>.Empty.AsEnumerable();
        }

        Symbol GetSymbol(Symbol s, string name)
        {
            if (s == null)
            {   
                foreach (IProject project in _workspace.CurrentSolution.Projects)
                {
                    Compilation compilation = project.GetCompilation() as Compilation;
                    if (compilation == null) return null;
                    _currentCompilation = compilation;
                    Symbol c = GetSymbol(compilation.GlobalNamespace, name);
                    if (c != null) return c;
                }
            }
            else
            {
                return GetMembers(s).FirstOrDefault(m => m.Name == name);                
            }
            return null;
        }

        static ReadOnlyArray<Symbol> GetMembers(Symbol s)
        {
            ReadOnlyArray<Symbol> l = ReadOnlyArray<Symbol>.Empty;
            switch (s.Kind)
            {
                case SymbolKind.Alias:
                    break;
                case SymbolKind.ArrayType:
                    l = (s as ArrayTypeSymbol).GetMembers();
                    break;
                case SymbolKind.Assembly:
                    break;
                case SymbolKind.DynamicType:
                    l = (s as DynamicTypeSymbol).GetMembers();
                    break;
                case SymbolKind.ErrorType:
                    l = (s as ErrorTypeSymbol).GetMembers();
                    break;
                case SymbolKind.Event:
                    break;
                case SymbolKind.Field:
                    break;
                case SymbolKind.Label:
                    break;
                case SymbolKind.Local:
                    break;
                case SymbolKind.Method:        
                    //(s as MethodSymbol)
                    break;
                case SymbolKind.NamedType:
                    l = (s as NamedTypeSymbol).GetMembers();
                    break;
                case SymbolKind.Namespace:
                    l = (s as NamespaceSymbol).GetMembers();
                    break;
                case SymbolKind.NetModule:
                    break;
                case SymbolKind.Parameter:
                    break;
                case SymbolKind.PointerType:
                    l = (s as PointerTypeSymbol).GetMembers();
                    break;
                case SymbolKind.Property:
                    break;
                case SymbolKind.RangeVariable:
                    break;
                case SymbolKind.TypeParameter:
                    l = (s as TypeParameterSymbol).GetMembers();
                    break;
            }

            return l;
        }

        private string GetTextSelection(string text, string selection)
        {
            string pattern = @"^\s*(?<region>\D\w+)?\s*(?<line>[0-9\s\-\>]+)?$";
            Match m = Regex.Match(selection, pattern, RegexOptions.ExplicitCapture);

            if (m.Success)
            {
                if (m.Groups["region"].Success)
                {
                    text = GetTextRegion(text, m.Groups["region"].Value);
                }

                if (m.Groups["line"].Success)
                {
                    text = GetTextLines(text, m.Groups["line"].Value);
                }
            }
            return text;    
        }

        private string GetTextLines(string text, string selection)
        {
            string pattern = @"^\s*(?<line1>\d+)\s*(-\>\s*(?<line2>\d+)\s*)?$";
            Match m = Regex.Match(selection, pattern, RegexOptions.ExplicitCapture);
            int line1 = 0, line2 = 0;
            if (m.Success)
            {
                if (m.Groups["line1"].Success)
                {
                    line1 = int.Parse(m.Groups["line1"].Value);
                }

                if (m.Groups["line2"].Success)
                {
                    line2 = int.Parse(m.Groups["line2"].Value);
                }
            }

            if (line1 > 0)
            {
                string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (line2 > line1)
                {
                    if (lines.Length >= line2)
                    {
                        string s = string.Empty;
                        for (int i = line1; i <= line2; i++)
                        {
                            if (s != string.Empty)
                            {
                                s += System.Environment.NewLine;
                            }
                            s += lines[i - 1];                            
                        }
                        return s;
                    }
                }
                else
                {
                    if (lines.Length >= line1)
                    {
                        return lines[line1 - 1];
                    }
                }
            }
            return null;
        }

        private string GetTextRegion(string text, string region)
        {            
            string pattern = @"#region\s+" + region + @"(?<region>((?!#region|#endregion).)+(((?'Open'#region)((?!#region|#endregion).)*)+((?'-Open'#endregion)((?!#region|#endregion).)*)+)*(?(Open)(?!)))#endregion";
            Match m = Regex.Match(text, pattern, RegexOptions.Singleline | RegexOptions.ExplicitCapture);
            if (m.Success)
            {
                if (m.Groups["region"].Success)
                {
                    return m.Groups["region"].Value;
                }
            }
            return null;
        }

        #endregion
    }
}
