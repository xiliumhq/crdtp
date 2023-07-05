using System.Collections.Generic;
using System.Linq;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class StjSerializationContextFactoryEmitter : CompilationUnitEmitter
    {
        public StjSerializationContextFactoryEmitter(Context context)
            : base(context)
        { }

        public override bool ShouldEmit => Context.Options.Stj.Enabled;

        protected override string GetOutputItemPath() => $"+Serialization/Stj/{WellKnownTypes.ProtocolStjSerializationContextFactoryTypeInfo.Name}.g.cs";

        protected override string GetNamespace() => WellKnownTypes.ProtocolStjSerializationContextFactoryTypeInfo.Namespace;

        protected override List<CS.SyntaxObject> GetNamespaceContent()
        {
            var declarations = new List<CS.SyntaxObject>();

            var typeMembers = new List<CS.SyntaxObject>();

            var thisTypeInfo = WellKnownTypes.ProtocolStjSerializationContextFactoryTypeInfo;

            // public static readonly StjSerializerOptions Instance = new ProtocolStjSerializerOptions();
            var instanceFieldDecl = new CS.FieldDeclaration(
                name: "Instance",
                modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.Static | CS.CSharpModifiers.ReadOnly,
                type: thisTypeInfo.Name,
                initialValue: $"new {thisTypeInfo.Name}()");
            typeMembers.Add(instanceFieldDecl);

            // .ctor
            typeMembers.Add(new CS.Constructor(
                name: thisTypeInfo.Name,
                parameters: null,
                modifiers: CS.CSharpModifiers.Private,
                attributes: null,
                xmlDocumentation: null,
                members: null,
                subconstructor: null
                ));

            // CreateJsonSerializerContext
            {
                var methodBody = new List<CS.SyntaxObject>()
                {
                    new CS.Raw($"return new {WellKnownTypes.ProtocolStjSerializerContext.GetFullyQualifiedName()}(options);"),
                };
                typeMembers.Add(new CS.MethodDeclaration(
                    name: "CreateJsonSerializerContext",
                    parameters: new[]
                    {
                        new CS.Parameter("options", WellKnownTypes.JsonSerializerOptions.GetFullyQualifiedName() + "?")
                    },
                    returnParameter: new CS.Parameter(null!, WellKnownTypes.JsonSerializerContext.GetFullyQualifiedName()),
                    modifiers: CS.CSharpModifiers.Protected | CS.CSharpModifiers.Override,
                    members: methodBody
                    ));
            }

            // GetJsonConverters
            {
                var methodBody = new List<CS.SyntaxObject>();
                methodBody.Add(new CS.Raw($"return new {WellKnownTypes.JsonConverterTypeInfo.GetFullyQualifiedName()}[]"));
                methodBody.Add(new CS.Raw("{"));
                foreach (var x in Context
                    .StjConverterEmitters
                    .Where(x => x.ShouldEmit)
                    .OrderBy(x => x.StjConverterTypeInfo.GetFullyQualifiedName()))
                {
                    methodBody.Add(new CS.Raw($"    new {x.StjConverterTypeInfo.GetFullyQualifiedName()}(),"));
                }
                methodBody.Add(new CS.Raw("};"));

                typeMembers.Add(new CS.MethodDeclaration(
                    name: "GetJsonConverters",
                    parameters: null,
                    returnParameter: new CS.Parameter(null!, WellKnownTypes.JsonConverterTypeInfo.GetFullyQualifiedName() + "[]"),
                    modifiers: CS.CSharpModifiers.Protected | CS.CSharpModifiers.Override,
                    members: methodBody
                    ));
            }

            var typeDeclaration = new CS.ClassDeclaration(
                name: thisTypeInfo.Name,
                modifiers: CS.TypeModifiers.Internal | CS.TypeModifiers.Sealed,
                members: typeMembers,
                baseType: WellKnownTypes.StjSerializationContextFactoryTypeInfo.GetFullyQualifiedName());
            declarations.Add(typeDeclaration);

            return declarations;
        }
    }
}
