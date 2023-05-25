using System.Collections.Generic;
using System.Linq;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal abstract class AliasTypeEmitter : CompilationUnitEmitter
    {
        private readonly AliasTypeInfo _typeInfo;

        public AliasTypeEmitter(Context context, AliasTypeInfo typeInfo)
            : base(context)
        {
            _typeInfo = typeInfo;
        }

        protected TypeInfo TypeInfo => _typeInfo;
        protected TypeInfo UnderlyingTypeInfo => _typeInfo.UnderlyingType;

        protected bool IsEquatable => true;

        protected override string GetOutputItemPath()
            => Context.NamingPolicy.GetOutputItemPath(_typeInfo.Domain, _typeInfo);

        protected override string GetNamespace() => _typeInfo.Namespace;

        // TODO: Review. For primitive types (int, double, etc) it should emit more clean code.
        // For strings its probably okay. Generally this methods should avoid to call
        // runtime GetUnderlyingValue() method.
        private string GetDefaultValueExpr()
        {
            if (UnderlyingTypeInfo is StringTypeInfo)
            {
                return "\"\"";
            }
            else throw Error.Unreachable();
        }

        private string GetUnderlyingValueExpr(string getValueExpr)
        {
            if (UnderlyingTypeInfo.IsValueType)
            {
                return getValueExpr;
            }
            else
            {
                return $"{getValueExpr} ?? " + GetDefaultValueExpr();
            }
        }

        private string GetHashCodeExpr(string getValueExpr)
        {
            if (UnderlyingTypeInfo is IntegerTypeInfo)
            {
                return getValueExpr;
            }
            else
            {
                return $"({GetUnderlyingValueExpr(getValueExpr)}).GetHashCode()";
            }
        }

        private string GetToStringExpr(string getValueExpr)
        {
            if (UnderlyingTypeInfo is StringTypeInfo)
            {
                return "GetUnderlyingValue()";
            }
            else
            {
                return "GetUnderlyingValue().ToString()";
            }
        }

        public string GetEqualsExpr(string getValueExpr)
        {
            return $"GetUnderlyingValue() == other.GetUnderlyingValue()";
        }

        public string GetOpEqExpr(string getValueExpr)
        {
            return $"x.GetUnderlyingValue() == y.GetUnderlyingValue()";
        }

        public string GetOpNeqExpr(string getValueExpr)
        {
            return $"x.GetUnderlyingValue() != y.GetUnderlyingValue()";
        }

        protected override List<CS.SyntaxObject> GetNamespaceContent()
        {
            var declarations = new List<CS.SyntaxObject>();

            var typeAttributes = Context.CSharp.CreateAttributeList(_typeInfo, UsingNamespaces);

            var typeMembers = new List<CS.SyntaxObject>();
            var typeName = _typeInfo.Name;

            var underlyingValueType = _typeInfo.UnderlyingType.GetFullyQualifiedName();

            // Because struct can be default-initialized, then if it wraps reference type,
            // then mark field as nullable.
            var underlyingValueFieldType = underlyingValueType;
            if (!UnderlyingTypeInfo.IsValueType) { underlyingValueFieldType += "?"; }

            // Making Nodes
            var fieldDecl = new CS.FieldDeclaration("_value",
                modifiers: CS.CSharpModifiers.Private | CS.CSharpModifiers.ReadOnly,
                type: underlyingValueFieldType);
            typeMembers.Add(fieldDecl);

            var valueExpr = fieldDecl.Name;

            var ctorDecl = new CS.Constructor(
                name: typeName,
                parameters: new[] { new CS.Parameter("value", underlyingValueType) },
                modifiers: CS.CSharpModifiers.Public,
                attributes: null,
                xmlDocumentation: null,
                members: new CS.SyntaxObject[]
                {
                    new CS.Raw($"{fieldDecl.Name} = value;")
                },
                subconstructor: null
            );
            typeMembers.Add(ctorDecl);

            var aggressiveInliningAttr = new CS.AttributeDecl("System.Runtime.CompilerServices.MethodImpl",
                new[] { "System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining" });
            var getUnderlyingValueDecl = new CS.MethodDeclaration(
                "GetUnderlyingValue",
                parameters: null,
                returnParameter: new CS.Parameter(null!, type: underlyingValueType),
                modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.ReadOnly,
                attributes: new[] { aggressiveInliningAttr },
                xmlDocumentation: null,
                members: new CS.SyntaxObject[]
                {
                    new CS.Raw($"return {GetUnderlyingValueExpr(valueExpr)};")
                });
            typeMembers.Add(getUnderlyingValueDecl);

            if (IsEquatable)
            {
                var getHashCodeDecl = new CS.MethodDeclaration(
                    "GetHashCode",
                    parameters: null,
                    returnParameter: new CS.Parameter(null!, type: "int"),
                    modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.Override,
                    attributes: null,
                    xmlDocumentation: null,
                    members: new CS.SyntaxObject[]
                    {
                    new CS.Raw($"return {GetHashCodeExpr(valueExpr)};")
                    });
                typeMembers.Add(getHashCodeDecl);

                var toStringDecl = new CS.MethodDeclaration(
                    "ToString",
                    parameters: null,
                    returnParameter: new CS.Parameter(null!, type: "string"),
                    modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.Override,
                    attributes: null,
                    xmlDocumentation: null,
                    members: new CS.SyntaxObject[]
                    {
                    new CS.Raw($"return {GetToStringExpr(valueExpr)};")
                    });
                typeMembers.Add(toStringDecl);

                var objectEqualsDecl = new CS.MethodDeclaration(
                    "Equals",
                    parameters: new[] { new CS.Parameter("obj", "object?") },
                    returnParameter: new CS.Parameter(null!, type: "bool"),
                    modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.Override,
                    attributes: null,
                    xmlDocumentation: null,
                    members: new CS.SyntaxObject[]
                    {
                        new CS.Raw($"if (obj is {typeName} other) return Equals(other);"),
                        new CS.Raw($"return false;"),
                    });
                typeMembers.Add(objectEqualsDecl);

                var equalsDecl = new CS.MethodDeclaration(
                    "Equals",
                    parameters: new[] { new CS.Parameter("other", typeName) },
                    returnParameter: new CS.Parameter(null!, type: "bool"),
                    modifiers: CS.CSharpModifiers.Public,
                    attributes: null,
                    xmlDocumentation: null,
                    members: new CS.SyntaxObject[]
                    {
                        new CS.Raw($"return {GetEqualsExpr(valueExpr)};"),
                    });
                typeMembers.Add(equalsDecl);

                var opEqDecl = new CS.MethodDeclaration(
                    "operator==",
                    parameters: new[] { new CS.Parameter("x", typeName), new CS.Parameter("y", typeName) },
                    returnParameter: new CS.Parameter(null!, type: "bool"),
                    modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.Static,
                    attributes: null,
                    xmlDocumentation: null,
                    members: new CS.SyntaxObject[]
                    {
                    new CS.Raw($"return {GetOpEqExpr(valueExpr)};"),
                    });
                typeMembers.Add(opEqDecl);

                var opNeqDecl = new CS.MethodDeclaration(
                    "operator!=",
                    parameters: new[] { new CS.Parameter("x", typeName), new CS.Parameter("y", typeName) },
                    returnParameter: new CS.Parameter(null!, type: "bool"),
                    modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.Static,
                    attributes: null,
                    xmlDocumentation: null,
                    members: new CS.SyntaxObject[]
                    {
                    new CS.Raw($"return {GetOpNeqExpr(valueExpr)};"),
                    });
                typeMembers.Add(opNeqDecl);
            }

            var typeDeclaration = new CS.StructDeclaration(
                name: typeName,
                modifiers: CS.TypeModifiers.Public | CS.TypeModifiers.ReadOnly | Context.CSharp.GetDefaultTypeModifiers(),
                members: typeMembers,
                baseType: WellKnownTypes.GetIEquatableOf(TypeInfo).GetFullyQualifiedName(),
                attributes: typeAttributes,
                xmlDocumentation: Context.CSharp.CreateDocumentation(_typeInfo));
            declarations.AddRange(Context.CSharp.GetTypeAnalysis(_typeInfo));
            declarations.Add(typeDeclaration);

            return declarations;
        }
    }
}
