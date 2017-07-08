using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PS.Build.Tasks.Extensions
{
    public static class RoslynExtensions
    {
        #region Constants

        private const string SystemNamespace = "System.";

        private static readonly Dictionary<string, string> MapConcreteTypeToPredefinedTypeAlias =
            new Dictionary<string, string>
            {
                { "short", SystemNamespace + nameof(Int16) },
                { "int", SystemNamespace + nameof(Int32) },
                { "long", SystemNamespace + nameof(Int64) },
                { "ushort", SystemNamespace + nameof(UInt16) },
                { "uint", SystemNamespace + nameof(UInt32) },
                { "ulong", SystemNamespace + nameof(UInt64) },
                { "object", SystemNamespace + nameof(Object) },
                { "byte", SystemNamespace + nameof(Byte) },
                { "sbyte", SystemNamespace + nameof(SByte) },
                { "char", SystemNamespace + nameof(Char) },
                { "bool", SystemNamespace + nameof(Boolean) },
                { "float", SystemNamespace + nameof(Single) },
                { "double", SystemNamespace + nameof(Double) },
                { "decimal", SystemNamespace + nameof(Decimal) },
                { "string", SystemNamespace + nameof(String) }
            };

        #endregion

        #region Static members

        public static Attribute CreateAttribute(this AttributeData data, Type type)
        {
            var ctorArgumentsWithError = data.ConstructorArguments.Where(a => a.Kind == TypedConstantKind.Error).ToList();
            if (ctorArgumentsWithError.Any())
            {
                throw new ArgumentException($"{data} attribute arguments could not be parsed");
            }

            var valueSequence = new List<object>();

            var availableCtors = type.GetConstructors().Select(c => new
            {
                ctor = c,
                args = c.GetParameters()
            }).ToList();

            for (var i = 0; i < data.AttributeConstructor.Parameters.Length; i++)
            {
                var localIndex = i;
                var constructorArgument = data.ConstructorArguments[i];
                var argumentTypeName = constructorArgument.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (MapConcreteTypeToPredefinedTypeAlias.ContainsKey(argumentTypeName))
                    argumentTypeName = MapConcreteTypeToPredefinedTypeAlias[argumentTypeName];

                var argumentType = Type.GetType(argumentTypeName);
                var globalPrefix = "global::";
                if (argumentType == null && argumentTypeName.StartsWith(globalPrefix))
                {
                    argumentType = Type.GetType(argumentTypeName.Substring(globalPrefix.Length));
                }

                valueSequence.Add(i < data.ConstructorArguments.Length
                    ? constructorArgument.Value
                    : null);

                var invalidCtors = availableCtors.Where(c => localIndex >= c.args.Length ||
                                                             c.args[localIndex].ParameterType != argumentType)
                                                 .ToList();
                invalidCtors.ForEach(c => availableCtors.Remove(c));
            }
            var ctor = availableCtors.FirstOrDefault();
            if (ctor == null)
            {
                throw new ArgumentException("There is no appropriate public constructor available");
            }

            var attribute = ctor.ctor.Invoke(valueSequence.ToArray()) as Attribute;
            foreach (var namedArgument in data.NamedArguments)
            {
                type.GetProperty(namedArgument.Key).SetValue(attribute, namedArgument.Value.Value);
            }
            return attribute;
        }

        public static bool IsEquivalent(this ISymbol symbol, Type type)
        {
            if (symbol == null) return false;
            if (type == null) return false;

            var typeName = symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            var assemblyName = symbol.ContainingAssembly.Identity.Name;

            return type.FullName == typeName && assemblyName == type.Assembly.GetName().Name;
        }

        public static AttributeData ResolveAttributeData(this AttributeSyntax syntax, SemanticModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (syntax == null) throw new ArgumentNullException(nameof(syntax));

            var attributeTarget = syntax.Parent.Parent;
            if (attributeTarget is CompilationUnitSyntax)
            {
                return model.Compilation
                            .Assembly
                            .GetAttributes()
                            .FirstOrDefault(a => a.ApplicationSyntaxReference.Span == syntax.Span);
            }

            var fieldDeclarationSyntax = attributeTarget as FieldDeclarationSyntax;
            if (fieldDeclarationSyntax != null)
            {
                foreach (var variable in fieldDeclarationSyntax.Declaration.Variables)
                {
                    var resolvedData = model.GetDeclaredSymbol(variable)?
                                            .GetAttributes()
                                            .FirstOrDefault(a => a.ApplicationSyntaxReference.Span == syntax.Span);
                    if (resolvedData != null) return resolvedData;
                }
            }
            else
            {
                return model.GetDeclaredSymbol(attributeTarget)?
                            .GetAttributes()
                            .FirstOrDefault(a => a.ApplicationSyntaxReference.Span == syntax.Span);
            }

            return null;
        }

        #endregion
    }
}