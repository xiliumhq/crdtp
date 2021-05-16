using System.Collections.Generic;
using System.Linq;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class ObjectTypeEmitter : Emitter // TODO: use CompilationUnitEmitter
    {
        private readonly ObjectTypeInfo _typeInfo;

        public ObjectTypeEmitter(Context context, ObjectTypeInfo typeInfo)
            : base(context)
        {
            _typeInfo = typeInfo;
        }

        public override void Emit()
        {
            var domainInfo = _typeInfo.Domain;
            Check.That(domainInfo != null);

            var usingNamespaces = new HashSet<string>()
            {
                "System",
                "System.Collections.Generic",
                // "System.Threading"
                // "System.Threading.Tasks"
            };

            var typeAttributes = Context.CSharp.CreateAttributeList(_typeInfo, usingNamespaces);

            var typeMembers = new List<CS.SyntaxObject>();

            // TODO: Create Default .ctor but only if needed. (In order if more ctors will be added).

            foreach (var propertyInfo in _typeInfo.Properties)
            {
                var propertyType = propertyInfo.Type.GetFullyQualifiedName();
                if (propertyInfo.IsOptional)
                {
                    propertyType = propertyType + "?";
                }

                var propertyAttributes = Context.CSharp.CreateAttributeList(propertyInfo, usingNamespaces);

                var propertyName = propertyInfo.Name;

                // When property name can't be converted to protocol name automatically,
                // then emit JsonPropertyName attribute.
                if (Context.Options.Stj.Enabled)
                {
                    if (Context.Options.Stj.CamelCaseNamingConvention)
                    {
                        propertyName = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(propertyName);
                    }

                    var useJsonPropertyName = propertyName != propertyInfo.ProtocolName;

                    if (useJsonPropertyName)
                    {
                        propertyAttributes.Add(new CS.AttributeDecl("JsonPropertyName", new string[] {
                            Context.CSharp.CreateString(propertyInfo.ProtocolName)
                        }));
                        usingNamespaces.Add("System.Text.Json.Serialization");
                    }
                }

                var propertyDeclaration = new CS.PropertyDeclaration(
                    name: propertyInfo.Name,
                    typeModifiers: CS.TypeModifiers.Public,
                    type: propertyType,
                    attributes: propertyAttributes,
                    xmlDocumentation: Context.CSharp.CreateDocumentation(propertyInfo)
                    );
                typeMembers.Add(propertyDeclaration);
            }



            var typeDeclaration = new CS.ClassDeclaration(
                name: _typeInfo.Name,
                modifiers: CS.TypeModifiers.Public | CS.TypeModifiers.Sealed,
                members: typeMembers,
                baseType: null,
                attributes: typeAttributes,
                xmlDocumentation: Context.CSharp.CreateDocumentation(_typeInfo));

            var ns = new CS.Namespace(_typeInfo.Namespace,
                Context.CSharp.GetTypeAnalysis(_typeInfo)
                .Concat(new CS.SyntaxObject[] {
                    typeDeclaration
                }));

            var unit = Context.CSharp.CreateUnit(
                new CS.SyntaxObject[] {
                    new CS.Raw("#nullable enable"),
                    new CS.Raw("#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable."),
                }
                .Concat(usingNamespaces.OrderBy(x => x).Select(x => new CS.UsingNamespace(x)))
                .Concat(new CS.SyntaxObject[] {
                    new CS.EmptyLine(),
                    ns
                }));

            Context.OutputScope.Add(new OutputItem
            {
                Path = Context.NamingPolicy.GetOutputItemPath(domainInfo, _typeInfo),
                Category = "Compile",
                Content = Context.CSharp.GetContent(unit),
            });
        }
    }
}
