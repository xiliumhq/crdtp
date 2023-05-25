using System.Collections.Generic;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal abstract class StjConverterEmitter : CompilationUnitEmitter
    {
        private const bool AlwaysEmitReadAndWrite = false; // TODO: move to options

        private readonly TypeInfo _typeInfo;
        private readonly WellKnownTypeInfo _thisTypeInfo;

        public StjConverterEmitter(Context context, TypeInfo typeInfo)
            : base(context)
        {
            _typeInfo = typeInfo;
            _thisTypeInfo = new WellKnownTypeInfo(context,
                _typeInfo.Name + "StjConverter", // TODO: use NamingPolicy StjConverter
                context.NamingPolicy.GetNamespaceName(_typeInfo.Domain));
        }

        public TypeInfo TypeInfo => _typeInfo;
        public WellKnownTypeInfo StjConverterTypeInfo => _thisTypeInfo;

        public override bool ShouldEmit => Context.Options.Stj.Enabled
            && (false // AlwaysEmitReadAndWrite
                || _typeInfo.IsSerializable
                || _typeInfo.IsDeserializable);

        protected override string GetOutputItemPath() => $"+Serialization/Stj/{_typeInfo.Domain?.Name}/{_thisTypeInfo.Name}.g.cs";

        protected override string GetNamespace() => _thisTypeInfo.Namespace;

        protected abstract IEnumerable<CS.SyntaxObject> GetReadBody();

        protected abstract IEnumerable<CS.SyntaxObject> GetWriteBody();

        protected override List<CS.SyntaxObject> GetNamespaceContent()
        {
            var declarations = new List<CS.SyntaxObject>();

            UsingNamespaces.Add("System");
            UsingNamespaces.Add("System.Text.Json");

            var typeMembers = new List<CS.SyntaxObject>();

            var thisTypeInfo = _thisTypeInfo;

            // Read
            {
                var methodBody = new List<CS.SyntaxObject>();
                if (AlwaysEmitReadAndWrite || _typeInfo.IsDeserializable)
                {
                    methodBody.AddRange(GetReadBody());
                }
                else
                {
                    methodBody.Add(new CS.Raw($"throw new NotSupportedException();"));
                }

                typeMembers.Add(new CS.MethodDeclaration(
                    name: "Read",
                    parameters: new[]{
                        new CS.Parameter("reader", "ref Utf8JsonReader"),
                        new CS.Parameter("typeToConvert", "Type"),
                        new CS.Parameter("options", "JsonSerializerOptions")
                    },
                    returnParameter: new CS.Parameter(null!, _typeInfo.GetFullyQualifiedName()),
                    modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.Override,
                    members: methodBody
                    ));
            }

            // Write
            {
                var methodBody = new List<CS.SyntaxObject>();
                if (AlwaysEmitReadAndWrite || _typeInfo.IsSerializable)
                {
                    methodBody.AddRange(GetWriteBody());
                }
                else
                {
                    methodBody.Add(new CS.Raw($"throw new NotSupportedException();"));
                }

                typeMembers.Add(new CS.MethodDeclaration(
                    name: "Write",
                    parameters: new[]{
                        new CS.Parameter("writer", "Utf8JsonWriter"),
                        new CS.Parameter("value", _typeInfo.GetFullyQualifiedName()),
                        new CS.Parameter("options", "JsonSerializerOptions")
                    },
                    returnParameter: new CS.Parameter(null!, "void"),
                    modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.Override,
                    members: methodBody
                    ));
            }

            var typeDeclaration = new CS.ClassDeclaration(
                name: thisTypeInfo.Name,
                modifiers: CS.TypeModifiers.Internal | CS.TypeModifiers.Sealed,
                members: typeMembers,
                baseType: WellKnownTypes.GetJsonConverterOf(_typeInfo).GetFullyQualifiedName());
            declarations.Add(typeDeclaration);

            return declarations;
        }
    }
}
