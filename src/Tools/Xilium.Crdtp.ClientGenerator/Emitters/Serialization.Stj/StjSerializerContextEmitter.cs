using System.Collections.Generic;
using System.Linq;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class StjSerializerContextEmitter : CompilationUnitEmitter
    {
        public StjSerializerContextEmitter(Context context)
            : base(context)
        { }

        public override bool ShouldEmit => Context.Options.Stj.Enabled;

        protected override string GetOutputItemPath() => $"+Serialization/Stj/{WellKnownTypes.ProtocolStjSerializerContext.Name}.g.cs";

        protected override string GetNamespace() => WellKnownTypes.ProtocolStjSerializerContext.Namespace;

        protected override List<CS.SyntaxObject> GetPrologue()
        {
            var basePrologue = base.GetPrologue();
            basePrologue.Add(new CS.Raw("#pragma warning disable CS0618 // Obosolete members"));
            return basePrologue;
        }

        protected override List<CS.SyntaxObject> GetNamespaceContent()
        {
            UsingNamespaces.Add("System");
            UsingNamespaces.Add("System.Text.Json.Serialization");

            var declarations = new List<CS.SyntaxObject>();

            /*
            var typeMembers = new List<CS.SyntaxObject>();

            var thisTypeInfo = WellKnownTypes.ProtocolStjSerializerContext;

            var attributes = new List<CS.AttributeDecl>();

            attributes.Add(new CS.AttributeDecl(
                "JsonSourceGenerationOptions",
                namedParameters: new Dictionary<string, string>()
                {
                    { "DefaultIgnoreCondition", "JsonIgnoreCondition.WhenWritingNull" },
                    { "GenerationMode", "JsonSourceGenerationMode.Metadata" },
                    { "PropertyNamingPolicy", "JsonKnownNamingPolicy.CamelCase" },
                }));

            foreach (var typeInfo in Context.StjSerializerContextTypes
                .OrderBy(x => x.GetFullyQualifiedName()))
            {
                if (!typeInfo.UseInSerializationContext())
                    continue;

                var attribute = new CS.AttributeDecl(
                    "JsonSerializable",
                    new[] { $"typeof({typeInfo.GetFullyQualifiedName()})" },
                    new Dictionary<string, string>()
                    {
                        { "TypeInfoPropertyName", $"\"{typeInfo.GetTypeInfoPropertyName()}\"" },
                    });
                attributes.Add(attribute);

                if (Context.StjSerializerContextOptionalTypes.Contains(typeInfo)
                    && typeInfo.IsValueType)
                {
                    var attribute2 = new CS.AttributeDecl(
                        "JsonSerializable",
                        new[] { $"typeof({typeInfo.GetFullyQualifiedName()}?)" },
                        new Dictionary<string, string>()
                        {
                            { "TypeInfoPropertyName", $"\"NullableOf{typeInfo.GetTypeInfoPropertyName()}\"" },
                        });
                    attributes.Add(attribute2);
                }

            }

            var typeDeclaration = new CS.ClassDeclaration(
                name: thisTypeInfo.Name,
                modifiers: CS.TypeModifiers.Internal | CS.TypeModifiers.Sealed | CS.TypeModifiers.Partial,
                members: typeMembers,
                baseType: WellKnownTypes.JsonSerializerContext.GetFullyQualifiedName(),
                attributes: attributes);
            declarations.Add(typeDeclaration);
            */

            return declarations;
        }
    }
}
