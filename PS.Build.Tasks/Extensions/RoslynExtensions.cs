using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PS.Build.Tasks.Extensions
{
    internal static class RoslynExtensions
    {
        #region Static members

        public static Attribute CreateAttribute(this AttributeData data)
        {
            var ctorArgumentsWithError = data.ConstructorArguments.Where(a => a.Kind == TypedConstantKind.Error).ToList();
            if (ctorArgumentsWithError.Any())
            {
                throw new ArgumentException($"{data} attribute arguments could not be parsed");
            }

            var type = data.AttributeClass.ResolveType();
            var args = data.ConstructorArguments.Select(a => a.ExtractValue()).ToArray();
            var attribute = Activator.CreateInstance(type,
                                                     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                                                     null,
                                                     args,
                                                     null);
            foreach (var namedArgument in data.NamedArguments)
            {
                type.GetProperty(namedArgument.Key).SetValue(attribute, namedArgument.Value.ExtractValue());
            }

            return (Attribute)attribute;
        }

        public static object ExtractValue(this TypedConstant constant)
        {
            var arrayType = constant.Type as IArrayTypeSymbol;
            if (arrayType == null) return constant.Value;

            var elementType = arrayType.ElementType.ResolveType();
            var array = Array.CreateInstance(elementType, constant.Values.Length);
            for (var i = 0; i < constant.Values.Length; i++)
            {
                array.SetValue(constant.Values[i].ExtractValue(), i);
            }
            return array;
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

        public static Type ResolveType(this ISymbol symbol)
        {
            if (symbol.ContainingAssembly == null) return null;

            var typeSymbol = symbol as ITypeSymbol;
            if (typeSymbol != null)
            {
                switch (typeSymbol.SpecialType)
                {
                    case SpecialType.System_Object:
                        return typeof(object);
                    case SpecialType.System_Enum:
                        return typeof(Enum);
                    case SpecialType.System_MulticastDelegate:
                        return typeof(MulticastDelegate);
                    case SpecialType.System_Delegate:
                        return typeof(Delegate);
                    case SpecialType.System_Void:
                        return typeof(void);
                    case SpecialType.System_Boolean:
                        return typeof(bool);
                    case SpecialType.System_Char:
                        return typeof(char);
                    case SpecialType.System_SByte:
                        return typeof(sbyte);
                    case SpecialType.System_Byte:
                        return typeof(byte);
                    case SpecialType.System_Int16:
                        return typeof(short);
                    case SpecialType.System_UInt16:
                        return typeof(ushort);
                    case SpecialType.System_Int32:
                        return typeof(int);
                    case SpecialType.System_UInt32:
                        return typeof(uint);
                    case SpecialType.System_Int64:
                        return typeof(long);
                    case SpecialType.System_UInt64:
                        return typeof(ulong);
                    case SpecialType.System_Decimal:
                        return typeof(decimal);
                    case SpecialType.System_Single:
                        return typeof(float);
                    case SpecialType.System_Double:
                        return typeof(double);
                    case SpecialType.System_String:
                        return typeof(string);
                    case SpecialType.System_IntPtr:
                        return typeof(IntPtr);
                    case SpecialType.System_UIntPtr:
                        return typeof(UIntPtr);
                    case SpecialType.System_DateTime:
                        return typeof(DateTime);
                    case SpecialType.System_ArgIterator:
                        return typeof(ArgIterator);
                }
            }

            var containingType = symbol.ContainingType ??
                                 (symbol as INamedTypeSymbol)?.ConstructedFrom;

            if (containingType == null) return null;
            var typeName = string.Join(",",
                                       containingType.ToDisplayString(),
                                       symbol.ContainingAssembly.ToDisplayString());
            return Type.GetType(typeName);
        }

        #endregion
    }
}