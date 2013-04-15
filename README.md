# Markcode

------------------------

Mark the source code in markdown document as a link and use markcode as a tool to insert/update the source code automatically. This file README.md is used as an example to test and demostrate the use of markcode.

## Purpose

Markcode works for the wiki documents that have a lot of references to source code.

* When write the documents, just use the markcode links point to the source code. Markcode will update the documents and embedded the source code right after the markcode link.

* When source code changed, markcode can update the documents to reflect the most recent changes.

* When source code changed and the markcode link broken, markcode will give the warning message to help update the links.

* For some application documents that heavily rely on the source code, for example, the api documents, We can write the real api samples and test them. That will make sure the code quoted in the documents by markcode links are correct. 

## Syntax

markcode link is coded in the html comments that wont affect the final markup display.

    <!---{markcode-link}--->

The comment itself should be a seperated paragraph which means there should be a blank line after the comment.

Triple dashes are recommended since some markup engines like [pandoc][pandoc] will ignore triple  dash comment and no footprint left in the final wiki documents.

### 1. file link

    <!---{filepath}--->

For example,

    <!---{./Markcode.Core/MarkcodeException.cs}--->

<!---{./Markcode.Core/MarkcodeException.cs}--->

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

<!---{?endmarkcode}--->
File path can be a absolute path or a relative path. If it is a relative path, markcode will use the solution's directory as the default current directory. The default current directory can be overrided as an option to markcode command line.

### 2. identifier link

    <!---{fully-qualified-name-of-identifier}--->

identifier could be namespace, type, memeber, variable etc. For example,

    <!---{Markcode.Core.RoslynReflection.GetText}--->

<!---{Markcode.Core.RoslynReflection.GetText}--->

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

<!---{?endmarkcode}--->


### 3. selection

selection is used to select part of the souce code.

### 3.1. region selection

    <!--- {markcode-link#region} --->

In c sharp, region lets you specify a block of code. You can use region to select part of the source code. Region selection can be used with file link or identifier link. 

### 3.2. line selection

    <!---{markcode-link#line}--->

Use line selection to select 1 line of souce. Line selection can be used with file link or identifier link. When used with identifier link, the line number is a relative number to the beginning of the code block. it is not the line number of that source file. The first line is line 1.

### 3.3. line range selection

    <!---{markcode-link#line1->line2}--->

Use line range selection to select multiple lines of soure code. Line range selection can be used with file link or identifier link. When used with identifier link, the line number is a relative number to the beginning of the code block. It is not the line number of that source file. The first line is line 1.

Line related selections are not recommended since source code changes will most likely break the links.

## Markcode command tool

## How it works


<!---{Markcode.Core.RoslynReflection#Fields}--->

        private const string SELECTOR = "#";
        private IWorkspace _workspace;
        private bool disposed = false;
        

<!---{?endmarkcode}--->

<!---{Markcode.Core.RoslynReflection.GetTextLines#3->6}--->

        string pattern = @"^\s*(?<line1>\d+)\s*(-\>\s*(?<line2>\d+)\s*)?$";
        Match m = Regex.Match(selection, pattern, RegexOptions.ExplicitCapture);
        int line1 = 0, line2 = 0;
        if (m.Success)

<!---{?endmarkcode}--->


<!---{Markcode.Core.RoslynReflection.GetTextLines#8}--->

            if (m.Groups["line1"].Success)

<!---{?endmarkcode}--->

<!---{Markcode.Core.RoslynReflection#Utilities 1->20}--->

        public string GetIdentifierText(string fullName)
        {
            string[] names = fullName.Trim().Split('.');
            string param = null;
            if (names.Length == 0) return null;
            string lastName = names[names.Length - 1];
            string pattern = @"^(?<name>\w+)" +
                             @"(\((?<params>.+)\))*" +
                             @"\s*$";
            Match m = Regex.Match(lastName, pattern);
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

<!---{?endmarkcode}--->
[pandoc]: http://johnmacfarlane.net/pandoc/ "a universal document converter"
