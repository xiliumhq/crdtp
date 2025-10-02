using System.Collections.Generic;
using System.Linq;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class StjLegacySerializerOptionsEmitter : CompilationUnitEmitter
    {
        public StjLegacySerializerOptionsEmitter(Context context)
            : base(context)
        { }

        public override bool ShouldEmit => Context.Options.Stj.Enabled
            && Context.Options.Stj.Legacy;

        protected override string GetOutputItemPath() => $"+Serialization/Stj/{WellKnownTypes.ProtocolStjLegacySerializerOptionsTypeInfo.Name}.g.cs";

        protected override string GetNamespace() => WellKnownTypes.ProtocolStjLegacySerializerOptionsTypeInfo.Namespace;

        protected override List<CS.SyntaxObject> GetNamespaceContent()
        {
            var declarations = new List<CS.SyntaxObject>();

            var typeMembers = new List<CS.SyntaxObject>();

            var thisTypeInfo = WellKnownTypes.ProtocolStjLegacySerializerOptionsTypeInfo;

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

            // GetConvertersCore
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
                    name: "GetConvertersCore",
                    parameters: null,
                    returnParameter: new CS.Parameter(null!, WellKnownTypes.GetICollectionOf(WellKnownTypes.JsonConverterTypeInfo).GetFullyQualifiedName()),
                    modifiers: CS.CSharpModifiers.Protected | CS.CSharpModifiers.Override,
                    members: methodBody
                    ));
            }

            var typeDeclaration = new CS.ClassDeclaration(
                name: thisTypeInfo.Name,
                modifiers: CS.TypeModifiers.Internal | CS.TypeModifiers.Sealed,
                members: typeMembers,
                baseType: WellKnownTypes.StjLegacySerializerOptionsTypeInfo.GetFullyQualifiedName());
            declarations.Add(typeDeclaration);

            return declarations;
        }
    }
}
