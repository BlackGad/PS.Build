using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace PS.Build.Tasks
{
    class AdaptationUsage
    {
        #region Static members

        public static MethodInfo GetPostBuildMethod(Type t)
        {
            var method = t.GetMethod("PostBuild", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var parameters = method?.GetParameters();
            if (parameters?.Length != 1) return null;
            if (parameters.First().ParameterType != typeof(IServiceProvider)) return null;
            return method;
        }

        public static MethodInfo GetPreBuildMethod(Type t)
        {
            var method = t.GetMethod("PreBuild", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var parameters = method?.GetParameters();
            if (parameters?.Length != 1) return null;
            if (parameters.First().ParameterType != typeof(IServiceProvider)) return null;
            return method;
        }

        #endregion

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
            PreBuildMethod = GetPreBuildMethod(type);
            PostBuildMethod = GetPostBuildMethod(type);
        }

        #endregion

        #region Properties

        public SyntaxNode AssociatedSyntaxNode { get; }

        public Attribute Attribute { get; set; }
        public AttributeData AttributeData { get; }
        public MethodInfo PostBuildMethod { get; set; }

        public MethodInfo PreBuildMethod { get; set; }
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