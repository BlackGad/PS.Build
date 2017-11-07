using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var baseType = t;
            while (baseType != null)
            {
                var method = baseType.GetMethod("PostBuild", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var parameters = method?.GetParameters();
                if (parameters?.Length == 1 && parameters.First().ParameterType == typeof(IServiceProvider))
                {
                    return method;
                }

                baseType = baseType.BaseType;
            }
            return null;
        }

        public static MethodInfo GetPreBuildMethod(Type t)
        {
            var baseType = t;
            while (baseType != null)
            {
                var method = baseType.GetMethod("PreBuild", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var parameters = method?.GetParameters();
                if (parameters?.Length == 1 && parameters.First().ParameterType == typeof(IServiceProvider))
                {
                    return method;
                }

                baseType = baseType.BaseType;
            }
            return null;
        }

        public static MethodInfo[] GetSetupMethods(Type t)
        {
            var result = new List<MethodInfo>();
            var baseType = t;
            while (baseType != null)
            {
                var method = baseType.GetMethod("Setup", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var parameters = method?.GetParameters();
                if (parameters?.Length == 1 && parameters.First().ParameterType == typeof(IServiceProvider))
                    result.Add(method);

                baseType = baseType.BaseType;
            }
            return result.ToArray();
        }

        #endregion

        #region Constructors

        public AdaptationUsage(SemanticModel semanticModel,
                               SyntaxTree syntaxTree,
                               SyntaxNode associatedSyntaxNode,
                               AttributeData attributeData,
                               AttributeTargets attributeTargets,
                               Type type)
        {
            SemanticModel = semanticModel;
            SyntaxTree = syntaxTree;
            AssociatedSyntaxNode = associatedSyntaxNode;
            AttributeData = attributeData;
            Type = type;

            AttributeTargets = attributeTargets;
            PreBuildMethod = GetPreBuildMethod(type);
            PostBuildMethod = GetPostBuildMethod(type);
            SetupMethods = GetSetupMethods(type);
        }

        #endregion

        #region Properties

        public SyntaxNode AssociatedSyntaxNode { get; }
        public Attribute Attribute { get; set; }
        public AttributeData AttributeData { get; }
        public AttributeTargets AttributeTargets { get; }
        public MethodInfo PostBuildMethod { get; }

        public MethodInfo PreBuildMethod { get; }
        public SemanticModel SemanticModel { get; }
        public MethodInfo[] SetupMethods { get; }

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