using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.CSharp;

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
            if (names.Length == 0) return null;

            Symbol s = null;

            foreach (string name in names)
            {
                s = GetSymbol(name, s);
                if (s == null) return null;
            }

            if (s == null) return null;

            string text = string.Empty;

            switch (s.Kind)
            {
                case SymbolKind.NamedType:
                    NamedTypeSymbol ts = (s as NamedTypeSymbol);
                    foreach (Location location in ts.Locations)
                    {
                        if (location.IsInSource)
                        {
                            text += location.SourceTree.GetText();
                            text += "\r\n";
                        }
                        else if (location.IsInMetadata)
                        {
                         
                        }
                    }
                    break;
                case SymbolKind.Method:
                    break;
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

        #region Utilities

        Symbol GetSymbol(string name, Symbol s)
        {
            if (s == null)
            {
                foreach (IProject project in _workspace.CurrentSolution.Projects)
                {
                    Compilation compilation = project.GetCompilation() as Compilation;
                    if (compilation == null) return null;
                    Symbol c = GetSymbol(name, compilation.GlobalNamespace);
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

        #endregion
    }
}
