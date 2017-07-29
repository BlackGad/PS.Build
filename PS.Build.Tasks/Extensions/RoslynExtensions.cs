using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PS.Build.Tasks.Extensions
{
    internal static class RoslynExtensions
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

            if (data.AttributeConstructor != null)
            {
                for (var i = 0; i < data.AttributeConstructor.Parameters.Length; i++)
                {
                    var localIndex = i;
                    var constructorArgument = data.ConstructorArguments[i];

                    var tuple = constructorArgument.ExtractValue();
                    valueSequence.Add(i < data.ConstructorArguments.Length
                        ? tuple.Item2
                        : null);

                    var invalidCtors = availableCtors.Where(c => localIndex >= c.args.Length ||
                                                                 c.args[localIndex].ParameterType != tuple.Item1)
                                                     .ToList();
                    invalidCtors.ForEach(c => availableCtors.Remove(c));
                }
            }

            var ctor = availableCtors.FirstOrDefault();
            if (ctor == null)
            {
                throw new ArgumentException("There is no appropriate public constructor available");
            }

            var attribute = ctor.ctor.Invoke(valueSequence.ToArray()) as Attribute;
            foreach (var namedArgument in data.NamedArguments)
            {
                type.GetProperty(namedArgument.Key).SetValue(attribute, namedArgument.Value.ExtractValue().Item2);
            }
            return attribute;
        }

        public static Tuple<Type, object> ExtractValue(this TypedConstant constant)
        {
            Type resultType;
            object resultValue;

            var arrayType = constant.Type as IArrayTypeSymbol;
            if (arrayType != null)
            {
                var elementType = arrayType.ElementType.ResolveType();
                var array = Array.CreateInstance(elementType, constant.Values.Length);
                resultType = array.GetType();
                for (var i = 0; i < constant.Values.Length; i++)
                {
                    array.SetValue(constant.Values[i].ExtractValue().Item2, i);
                }
                resultValue = array;
            }
            else
            {
                resultType = constant.Type.ResolveType();
                resultValue = constant.Value;
            }

            return new Tuple<Type, object>(resultType, resultValue);
        }

        public static bool IsEquivalent(this ISymbol symbol, Type type)
        {
            if (symbol == null) return false;
            if (type == null) return false;

            var typeName = symbol.ContainingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
            var assemblyName = symbol.ContainingAssembly.Identity.Name;

            return type.FullName == typeName && assemblyName == type.Assembly.GetName().Name;
        }

        public static Tuple<AttributeTargets, AttributeData> ResolveAttributeData(this AttributeSyntax syntax, SemanticModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (syntax == null) throw new ArgumentNullException(nameof(syntax));
            AttributeData resolvedData;
            var attributeTargets = AttributeTargets.All;
            var attributeTarget = syntax.Parent.Parent;
            if (attributeTarget is CompilationUnitSyntax)
            {
                var defineOperator = syntax.Parent.ChildNodes().First() as AttributeTargetSpecifierSyntax;
                if (defineOperator == null) throw new InvalidOperationException("Unknown attribute specifier in CompilationUnitSyntax");
                if (defineOperator.Identifier.RawKind == (int)SyntaxKind.AssemblyKeyword)
                {
                    resolvedData = model.Compilation
                                        .Assembly
                                        .GetAttributes()
                                        .FirstOrDefault(a => a.ApplicationSyntaxReference.Span == syntax.Span);
                    attributeTargets = AttributeTargets.Assembly;
                    return new Tuple<AttributeTargets, AttributeData>(attributeTargets, resolvedData);
                }
                if (defineOperator.Identifier.RawKind == (int)SyntaxKind.ModuleKeyword)
                {
                    resolvedData = model.Compilation
                                        .SourceModule
                                        .GetAttributes()
                                        .FirstOrDefault(a => a.ApplicationSyntaxReference.Span == syntax.Span);
                    attributeTargets = AttributeTargets.Module;
                    return new Tuple<AttributeTargets, AttributeData>(attributeTargets, resolvedData);
                }
                throw new InvalidOperationException("Unknown AttributeTargetSpecifierSyntax near attribute in CompilationUnitSyntax");
            }

            var baseFieldDeclarationSyntax = attributeTarget as BaseFieldDeclarationSyntax;
            if (baseFieldDeclarationSyntax != null)
            {
                if (attributeTarget is EventFieldDeclarationSyntax) attributeTargets = AttributeTargets.Event;
                if (attributeTarget is FieldDeclarationSyntax) attributeTargets = AttributeTargets.Field;

                foreach (var variable in baseFieldDeclarationSyntax.Declaration.Variables)
                {
                    resolvedData = ModelExtensions.GetDeclaredSymbol(model, variable)?
                                                  .GetAttributes()
                                                  .FirstOrDefault(a => a.ApplicationSyntaxReference.Span == syntax.Span);
                    if (resolvedData != null) return new Tuple<AttributeTargets, AttributeData>(attributeTargets, resolvedData);
                }
                throw new InvalidOperationException("Unexpected attribute declaration in BaseFieldDeclarationSyntax");
            }

            var declaredSymbol = model.GetDeclaredSymbol(attributeTarget);
            resolvedData = declaredSymbol?.GetAttributes()
                                          .FirstOrDefault(a => a.ApplicationSyntaxReference.Span == syntax.Span);
            if (resolvedData != null)
            {
                if (attributeTarget is DelegateDeclarationSyntax) attributeTargets = AttributeTargets.Delegate;
                if (attributeTarget is ClassDeclarationSyntax) attributeTargets = AttributeTargets.Class;
                if (attributeTarget is ConstructorDeclarationSyntax) attributeTargets = AttributeTargets.Constructor;
                if (attributeTarget is PropertyDeclarationSyntax) attributeTargets = AttributeTargets.Property;
                if (attributeTarget is MethodDeclarationSyntax) attributeTargets = AttributeTargets.Method;
                if (attributeTarget is TypeParameterSyntax) attributeTargets = AttributeTargets.GenericParameter;
                if (attributeTarget is ParameterSyntax) attributeTargets = AttributeTargets.Parameter;
                if (attributeTarget is EnumDeclarationSyntax) attributeTargets = AttributeTargets.Enum;
                if (attributeTarget is InterfaceDeclarationSyntax) attributeTargets = AttributeTargets.Interface;
                if (attributeTarget is StructDeclarationSyntax) attributeTargets = AttributeTargets.Struct;
                if (attributeTarget is EnumMemberDeclarationSyntax) attributeTargets = AttributeTargets.Field;
                if (attributeTarget is EventDeclarationSyntax) attributeTargets = AttributeTargets.Event;
                if (attributeTarget is IndexerDeclarationSyntax) attributeTargets = AttributeTargets.Property;
                if (attributeTarget is AccessorDeclarationSyntax) attributeTargets = AttributeTargets.Method;

                return new Tuple<AttributeTargets, AttributeData>(attributeTargets, resolvedData);
            }

            var methodSymbol = declaredSymbol as IMethodSymbol;
            resolvedData = methodSymbol?.GetReturnTypeAttributes()
                                        .FirstOrDefault(a => a.ApplicationSyntaxReference.Span == syntax.Span);

            if (resolvedData != null)
            {
                attributeTargets = AttributeTargets.ReturnValue;
                return new Tuple<AttributeTargets, AttributeData>(attributeTargets, resolvedData);
            }

            throw new NotSupportedException($"Cannot resolve attribute data Location: {syntax.SyntaxTree.GetLineSpan(syntax.Span)}, Syntax: {syntax}");
        }

        public static Type ResolveType(this ITypeSymbol namedSymbol)
        {
            var typeName = namedSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (MapConcreteTypeToPredefinedTypeAlias.ContainsKey(typeName))
                typeName = MapConcreteTypeToPredefinedTypeAlias[typeName];

            var resultType = Type.GetType(typeName);
            var globalPrefix = "global::";
            if (resultType == null && typeName.StartsWith(globalPrefix))
            {
                resultType = Type.GetType(typeName.Substring(globalPrefix.Length));
            }
            return resultType;
        }

        #endregion
    }
}