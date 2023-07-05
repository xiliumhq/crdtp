using System;
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
                "System.Diagnostics.CodeAnalysis",
                // "System.Threading"
                // "System.Threading.Tasks"
            };

            var typeAttributes = Context.CSharp.CreateAttributeList(_typeInfo, usingNamespaces);

            var properties = new List<CS.SyntaxObject>();
            HashSet<string>? dynamicDependencyTypes = null;
            // TODO: Create Default .ctor but only if needed. (In order if more ctors will be added).

            foreach (var propertyInfo in _typeInfo.Properties)
            {
                var propertyType = propertyInfo.Type.GetFullyQualifiedName();
                if (propertyInfo.IsOptional)
                {
                    propertyType = propertyType + "?";
                }

                if (Context.Options.Stj.Trimmable)
                {
                    AddDynamicDependencyProperty(ref dynamicDependencyTypes, propertyInfo);
                }

                var propertyAttributes = Context.CSharp.CreateAttributeList(propertyInfo, usingNamespaces);

                var propertyName = propertyInfo.Name;

                // When property name can't be converted to protocol name automatically,
                // then emit JsonPropertyName attribute.
                if (Context.Options.Stj.Enabled)
                {
                    bool emitJsonPropertyName = true;

                    if (!Context.Options.Stj.Obfuscation)
                    {
                        if (Context.Options.Stj.CamelCaseNamingConvention)
                        {
                            propertyName = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(propertyName);
                        }
                        emitJsonPropertyName = propertyName != propertyInfo.ProtocolName;
                    }

                    if (emitJsonPropertyName)
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
                properties.Add(propertyDeclaration);
            }

            CS.Constructor? ctor = null;
            if (Context.Options.Stj.Trimmable)
            {
                var ctorAttributes = new List<CS.AttributeDecl>();
                if (dynamicDependencyTypes != null)
                {
                    foreach (var typeName in dynamicDependencyTypes.OrderBy(x => x))
                    {
                        ctorAttributes.Add(new CS.AttributeDecl(
                            name: "DynamicDependency",
                            parameters: new[]
                            {
                            "DynamicallyAccessedMemberTypes.PublicConstructors "
                            + "| DynamicallyAccessedMemberTypes.PublicProperties",
                            "typeof(" + typeName + ")"
                            }));
                    }
                }

                if (ctorAttributes.Count > 0)
                {
                    ctor = new CS.Constructor(_typeInfo.Name,
                        parameters: Array.Empty<CS.Parameter>(),
                        modifiers: CS.CSharpModifiers.Public,
                        attributes: ctorAttributes,
                        xmlDocumentation: null,
                        members: null);
                }
            }


            var typeMembers = new List<CS.SyntaxObject>();
            typeMembers.AddRange(properties);
            if (ctor != null)
            {
                typeMembers.Add(ctor);
            }

            var typeDeclaration = new CS.ClassDeclaration(
                name: _typeInfo.Name,
                modifiers: CS.TypeModifiers.Public | CS.TypeModifiers.Sealed | Context.CSharp.GetDefaultTypeModifiers(),
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

        private void AddDynamicDependencyType(ref HashSet<string>? dynamicDependencyTypes,
            TypeInfo typeInfo, bool optional)
        {
            if (Context.IsStjConvertible(typeInfo))
            {
                // Not needed to add, because are convertors are explicitly registered.
                return;
            }
            else if (typeInfo is ArrayTypeInfo arrayTypeInfo)
            {
                AddDynamicDependencyType(ref dynamicDependencyTypes, arrayTypeInfo.ElementType, false);
            }
            else if (typeInfo is AnyTypeInfo)
            {
                return;
            }
            else if (typeInfo is IntrinsicTypeInfo)
            {
                return;
            }

            // shouldAdd
            dynamicDependencyTypes ??= new HashSet<string>();
            dynamicDependencyTypes.Add(typeInfo.GetFullyQualifiedName());
        }

        private void AddDynamicDependencyProperty(ref HashSet<string>? dynamicDependencyTypes, PropertyInfo propertyInfo)
        {
            AddDynamicDependencyType(ref dynamicDependencyTypes, propertyInfo.Type, propertyInfo.IsOptional);
            //if (propertyType == "System.Collections.Generic.List<ZennoLab.Browser.DevTools.Protocol.Accessibility.AXNodeId>?")
            //    ;
        }
    }
}
