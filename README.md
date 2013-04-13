# Markcode

------------------------

Mark the source code in markdown document as a link and use markcode as a tool to create/update the source code automatically. This file README.md is also used to test and demostrate the use of markcode.

## Purpose

Markcode works for the wiki documents that have a lot of references to source code.

* When write the documents, just use the markcode links point to the source code. Markcode will update the documents and embedded the source code right after the markcode link.

* When source code changed, markcode can update the documents to reflect the source code most recent changes.

* When source code changed and the markcode link broken, markcode will give the warning message to help update the links.

* For some application documents that heavily rely on the source code, for example, the api documents, We can write the real api samples and test them. That will make sure the code quoted in the documents by markcode links are correct. 

## Syntax

markcode link is coded in the html comments that wont affect the final markup display.

    <!---{markcode-link}--->

The comment itself should be a seperated paragraph which means there should be a blank line after the comment.

Triple dashes are recommended since some markup engines like [pandoc][pandoc] will ignore triple  dash comment.

### 1. file link

    <!---{filepath}--->

For example,

    <!---{./Markcode.Core/xxx.cs}--->

File path can be a absolute path or a relative path. If it is a relative path, markcode will use the document file's directory as the default current directory. The current directory can be set as an option to markcode.

### 2. file function link
    
	<!---{filepath>function}--->


### 3. identifier link

    <!---{fully-qualified-name-of-identifier}--->

identifier could be namespace, type, memeber, variable etc. For example,

    <!---{Markcode.Core.RoslynReflection.GetText}--->

<!---{Markcode.Core.RoslynReflection.GetText}--->

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
                names[names.Length - 1] = m.Groups["name"].Value;
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

<!---{?endmarkcode}--->
### 4. selection

selection is used to select part of the souce code.

### 4.1. region selection

    <!--- {markcode-link#region} --->

you can use region to select part of the code. region selection can be used with file link or identifier link.

### 4.2. line selection

    <!---{markcode-link:line}--->

### 4.3. line range selection

    <!---{markcode-link:line1 line2}--->

line related links are not recommended since source code changes will most likely break the links. 

## Markcode command tool

## How it works


<!---{Markcode.Core.RoslynReflection#Fields}--->

        IWorkspace _workspace;
        private bool disposed = false;
        

<!---{?endmarkcode}--->
[pandoc]: http://johnmacfarlane.net/pandoc/ "a universal document converter"
