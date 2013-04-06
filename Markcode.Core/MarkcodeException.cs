using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markcode.Core
{
    [Serializable]
    public class MarkcodeException : Exception
    {
        public MarkcodeException(string message, string path, string link)
            : base(message)
        {
            Path = path;
            Link = link;
        }

        public MarkcodeException(string message, string path, string link, Exception innerException)
            : base(message, innerException)
        {
            Path = path;
            Link = link;
        }

        public string Path { get; set; }
        public string Link { get; set; }

    }
}
