using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PS.Build.Tasks
{
    class CompileItemRewriterVisitor : CSharpSyntaxRewriter
    {
        private readonly AdaptationUsage[] _usages;

        #region Constructors

        public CompileItemRewriterVisitor(IEnumerable<AdaptationUsage> usages)
        {
            _usages = usages.ToArray();
        }

        #endregion

        #region Override members

        public override SyntaxNode VisitAttribute(AttributeSyntax node)
        {
            if (_usages.Any(u => u.AttributeData.ApplicationSyntaxReference.Span.Start == node.SpanStart)) return null;
            return base.VisitAttribute(node);
        }

        public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
        {
            var result = (AttributeListSyntax)base.VisitAttributeList(node);
            return result.Attributes.Any() ? result : null;
        }

        #endregion
    }
}