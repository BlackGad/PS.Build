using System;
using Microsoft.CodeAnalysis;

namespace PS.Build.Tasks
{
    class AdaptationUsage
    {
        #region Constructors

        public AdaptationUsage(SemanticModel semanticModel,
                               SyntaxTree syntaxTree,
                               SyntaxNode associatedSyntaxNode,
                               AttributeData attributeData,
                               Type type)
        {
            SemanticModel = semanticModel;
            SyntaxTree = syntaxTree;
            AssociatedSyntaxNode = associatedSyntaxNode;
            AttributeData = attributeData;
            Type = type;
        }

        #endregion

        #region Properties

        public SyntaxNode AssociatedSyntaxNode { get; }
        public AttributeData AttributeData { get; }
        public SemanticModel SemanticModel { get; }

        public SyntaxTree SyntaxTree { get; }
        public Type Type { get; }

        #endregion

        #region Override members

        public override string ToString()
        {
            return $"({AssociatedSyntaxNode.GetType().Name}) {AttributeData}";
        }

        #endregion
    }
}