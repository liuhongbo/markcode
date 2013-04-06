using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Markcode.Core
{
    public class NRefactoryReflection : ICodeReflection
    {
        private bool disposed = false;

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
            return null;
        }

        public IEnumerable<string> EnumerateFiles()
        {
            return null;
        }

        public string GetSolutionDirectory()
        {
            return null;
        }
    }
}
