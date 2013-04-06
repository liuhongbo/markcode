# Markcode

------------------------

Mark the source code in markdown document as a link and use markcode as a tool to create/update the source code automatically

## Purpose

Markcode works for the wiki documents that have a lot of references to source code.

* When write the documents, just use the markcode links point to the source code. Markcode will update the documents and embedded the source code right after the markcode link.

* When source code changed, markcode can update the documents to reflect the source code most recent changes.

* When source code changed and the markcode link broken, markcode will give the warning message to help update the links.

* For some application documents that heavily rely on the source code, for example, the api documents, We can write the real api samples and test them. That will make sure the code quoted in the documents by markcode links are correct. 

## Syntax

markcode link is embedded in the html comments that wont affect the final markup

    <!---{markcode link}--->

Triple dashes are recommended since some markup engines like [pandoc][pandoc] will ignore triple  dash comment.

### 1. file link

    <!---{filepath}--->

For example,

    <!---{./Markcode.Core/xxx.cs}--->

File path can be a absolute path or a relative path. If it is a relative path, markcode will use the document file's directory as the default current directory. The current directory can be set as an option to markcode.

##### 1.1 region link

    <!--- {filepath#region} --->

##### 1.2 line range link

    <!---{filepath:line1 line2}--->

##### 1.3 line link

    <!---{filepath:line}--->

line related links are not recommended since source code changes will most likely break the links. 

##### 1.4 function link
    
	<!---{filepath>function}--->

###### 1.4.1 function line link

    <!---{filepath>function:line}--->

###### 1.4.2 function line range link
	
	<!---{filepath>function:line1 line2}--->

### namespace link

    <!---{namespace}--->

### type link

    <!---{fully-qualified-name-of-type}--->

The link is the fully qualified name of the Type, including the namespace of the Type but not the assembly.

#### type region link

    <!---{fully-qualified-name-of-type#region}--->

### member link

    <!---{fully-qualified-name-of-member}--->	

#### member region link

    <!---{fully-qualified-name-of-member#region}--->

## How it works

<!---{Markcode.Core.IMarkcodeTransform}--->

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
<!---{endmarkcode}--->

[pandoc]: http://johnmacfarlane.net/pandoc/ "a universal document converter"
