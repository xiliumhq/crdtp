using System.Collections.Generic;
using System.Linq;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    // TODO: (?) Instead StjSerializerOptionsEmitter try to use JsonConverterAttribute, if them will not be too many them there is should be better than registering StjSerializerOptions
    internal sealed class StjSerializerOptionsEmitter : CompilationUnitEmitter
    {
        public StjSerializerOptionsEmitter(Context context)
            : base(context)
        { }

        public override bool ShouldEmit
            => Context.Options.Stj.Enabled
            && Context.StjConverterEmitters.Where(x => x.ShouldEmit).Count() > 0;

        protected override string GetOutputItemPath() => $"+Serialization/Stj/{WellKnownTypes.ProtocolStjSerializerOptionsTypeInfo.Name}.g.cs";

        protected override string GetNamespace() => WellKnownTypes.ProtocolStjSerializerOptionsTypeInfo.Namespace;

        protected override List<CS.SyntaxObject> GetNamespaceContent()
        {
            var declarations = new List<CS.SyntaxObject>();

            var typeMembers = new List<CS.SyntaxObject>();

            var thisTypeInfo = WellKnownTypes.ProtocolStjSerializerOptionsTypeInfo;

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

            // GetConverters
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
                baseType: WellKnownTypes.StjSerializerOptionsTypeInfo.GetFullyQualifiedName());
            declarations.Add(typeDeclaration);

            return declarations;
        }
    }
}
