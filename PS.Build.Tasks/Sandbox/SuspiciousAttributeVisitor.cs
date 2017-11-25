using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PS.Build.Tasks.Extensions;

namespace PS.Build.Tasks
{
    class SuspiciousAttributeVisitor : CSharpSyntaxRewriter
    {
        private readonly Dictionary<string, List<Type>> _associationMap;

        #region Constructors

        public SuspiciousAttributeVisitor(IEnumerable<Type> adaptationTypes)
        {
            SuspiciousAttributeSyntaxes = new SuspiciousAttributeSyntaxes();
            _associationMap = adaptationTypes.CreateAssociationMap();
        }

        #endregion

        #region Properties

        public SuspiciousAttributeSyntaxes SuspiciousAttributeSyntaxes { get; }

        #endregion

        #region Override members

        public override SyntaxNode VisitAttribute(AttributeSyntax node)
        {
            var result = (AttributeSyntax)base.VisitAttribute(node);
            var attributeClass = result.Name.ToFullString();
            if (_associationMap.ContainsKey(attributeClass))
                SuspiciousAttributeSyntaxes.Add(result, _associationMap[attributeClass]);
            return result;
        }

        #endregion
    }
}