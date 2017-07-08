using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PS.Build.Tasks
{
    class SuspiciousAttributeSyntaxes : Dictionary<AttributeSyntax, List<Type>>
    {
    }
}