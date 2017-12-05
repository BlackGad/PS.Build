using System;
using System.Collections.Generic;
using System.Threading;
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

        public SuspiciousAttributeVisitor(IEnumerable<Type> adaptationTypes) : base(true)
        {
            SuspiciousAttributeSyntaxes = new SuspiciousAttributeSyntaxes();
            _associationMap = adaptationTypes.CreateAssociationMap();
            IsChanged = new ManualResetEvent(false);
        }

        #endregion

        #region Properties

        public int AdaptationRegionEntries { get; set; }

        public ManualResetEvent IsChanged { get; }

        public SuspiciousAttributeSyntaxes SuspiciousAttributeSyntaxes { get; }

        #endregion

        #region Override members

        public override SyntaxNode VisitAttribute(AttributeSyntax node)
        {
            var result = (AttributeSyntax)base.VisitAttribute(node);
            var attributeClass = result.Name.ToFullString();
            if (_associationMap.ContainsKey(attributeClass))
            {
                SuspiciousAttributeSyntaxes.Manage(result, _associationMap[attributeClass]);
                IsChanged.Set();
            }
            return result;
        }

        #endregion
    }
}