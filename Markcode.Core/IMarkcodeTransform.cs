using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Markcode.Core
{
    /// <summary>
    /// markcode transform interface
    /// </summary>
    public interface IMarkcodeTransform
    {
        void TransformSolution(string searchPattern = "*");
        void TransformDirectory(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories);
        void TransformFile(string path, string newPath = null);
        string TransformLink(string link);        
    }
}
