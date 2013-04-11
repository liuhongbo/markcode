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

        IWorkspace _workspace;
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

        public string GetTypeText(string typeFullName)
        {
            return null;
        }

        public string GetNamespaceText(string nsFullname)
        {
            return null;
        }

        public string GetText(string fullName)
        {
            string[] names = fullName.Trim().Split('.');
            string param = null;
            string selector = null;
            string selection = null;
            if (names.Length == 0) return null;
            
            string lastName = names[names.Length - 1];
            string pattern = @"^(?<name>\w+)(\((?<params>.+)\))*((?<selector>[#:])(?<selection>.+))*\s*$";

            Match m = Regex.Match(lastName, pattern);
            if (m.Success)
            {
                if (m.Groups["name"].Success)
                {
                    lastName = m.Groups["name"].Value;
                }

                if (m.Groups["params"].Success)
                {
                    param = m.Groups["params"].Value;
                }

                if (m.Groups["selector"].Success)
                {
                    selector = m.Groups["selector"].Value;
                }

                if (m.Groups["selection"].Success)
                {
                    selection = m.Groups["selection"].Value;
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

            return GetTextSelection(text, selector, selection);
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

        #region Utilities


        Symbol GetSymbol(Symbol s, string name, string param)
        {
            foreach (Symbol c in GetSymbols(s, name))
            {
                if (c.Kind == SymbolKind.Method)
                {
                    MethodSymbol ms = c as MethodSymbol;
                    
                }
            }

            return null;
        }

        IEnumerable<Symbol> GetSymbols(Symbol s, string name)
        {
            if (s == null)
            {
                foreach (IProject project in _workspace.CurrentSolution.Projects)
                {
                    Compilation compilation = project.GetCompilation() as Compilation;
                    if (compilation == null) return null;
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

        private string GetTextSelection(string text, string selector, string selection)
        {
            switch (selector)
            {
                case "#":
                    return GetTextRegion(text, selection);                    
                case ":":
                    return GetTextLines(text, selection);                    
                default:
                    return text;                    
            }           
        }

        private string GetTextLines(string text, string selection)
        {
            throw new NotImplementedException();
        }

        private string GetTextRegion(string text, string selection)
        {
            string pattern = @"\A\Z";
            return null;
        }

        #endregion
    }
}
