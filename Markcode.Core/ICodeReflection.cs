using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markcode.Core
{
    public interface ICodeReflection : IDisposable
    {
        string GetNamespaceText(string nsFullname);
        string GetTypeText(string typeFullName);
        string GetText(string fullName);
        string GetSolutionDirectory();
        IEnumerable<string> EnumerateFiles();
    }
}
