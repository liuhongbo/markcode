using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Markcode.Core
{
    public interface IMarkcodeTransform
    {
        void TransformSolution(string searchPattern = "*");
        void TransformDirectory(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.AllDirectories);
        void TransformFile(string path, string newPath = null);
        string TransformString(string s);
        void TransformStream(StreamReader reader, StreamWriter writer); 
        string TransformLink(string link);
    }
}
