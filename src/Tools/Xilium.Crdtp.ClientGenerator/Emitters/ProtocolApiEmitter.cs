using System.Collections.Generic;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class ProtocolApiEmitter : CompilationUnitEmitter
    {
        private readonly StjSerializationContextFactoryEmitter _stjSerializerOptionsEmitter;

        public ProtocolApiEmitter(Context context, StjSerializationContextFactoryEmitter stjSerializerOptionsEmitter)
            : base(context)
        {
            _stjSerializerOptionsEmitter = stjSerializerOptionsEmitter;
        }

        protected override string GetOutputItemPath() => "ProtocolApi.g.cs";

        protected override string GetNamespace() => Context.Options.Namespace;

        protected override List<CS.SyntaxObject> GetNamespaceContent()
        {
            var declarations = new List<CS.SyntaxObject>();

            var typeMembers = new List<CS.SyntaxObject>();

            var crdtpSessionFieldDecl = new CS.FieldDeclaration(
                name: "_session",
                modifiers: CS.CSharpModifiers.Private | CS.CSharpModifiers.ReadOnly,
                type: Context.Options.CrdtpSessionTypeName);
            typeMembers.Add(crdtpSessionFieldDecl);

            // .ctor
            {
                var ctorMembers = new List<CS.SyntaxObject>();
                ctorMembers.Add(new CS.Raw($"{crdtpSessionFieldDecl.Name} = session;"));

                if (_stjSerializerOptionsEmitter.ShouldEmit)
                {
                    ctorMembers.Add(
                        new CS.Raw($"_session.UseSerializationContextFactory({WellKnownTypes.ProtocolStjSerializationContextFactoryTypeInfo.GetFullyQualifiedName()}.Instance);")
                    );
                }

                typeMembers.Add(new CS.Constructor(
                    name: WellKnownTypes.ProtocolApiTypeInfo.Name,
                    parameters: new[] { new CS.Parameter("session", Context.Options.CrdtpSessionTypeName) },
                    modifiers: CS.CSharpModifiers.Public,
                    attributes: null,
                    xmlDocumentation: null,
                    members: ctorMembers,
                    subconstructor: null
                    ));
            }

            typeMembers.Add(new CS.MethodDeclaration(
                name: "GetCrdtpSession",
                parameters: null,
                returnParameter: new CS.Parameter(null!, Context.Options.CrdtpSessionTypeName),
                modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.ReadOnly,
                arrowExpression: new CS.ArrowExpressionClause($"{crdtpSessionFieldDecl.Name}")
                ));

            foreach (var domainInfo in Context.GetReachableNodes<DomainInfo>())
            {
                var domainApiTypeInfo = new DomainApiTypeInfo(Context, domainInfo);

                var propAttributes = new List<CS.AttributeDecl>();
                if (domainInfo.IsDeprecated)
                {
                    propAttributes.Add(new CS.AttributeDecl("Obsolete"));
                    UsingNamespaces.Add("System");
                }

                // TODO(dmitry.azaraev): (High) Emit MethodImplOptions(AggressiveInlining) for domain accessors.
                // using System.Runtime.CompilerServices;
                //public AccessibilityDomain Accessibility
                //{
                //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                //    get => new Accessibility.AccessibilityDomain(this);
                //}

                var domainApiTypeName = domainApiTypeInfo.GetFullyQualifiedName();
                // TODO: simplify names

                var domainApiParams = Context.Options.UseApi2
                    ? "_session"
                    : "this";

                var propDecl = new CS.PropertyDeclaration(
                    name: domainInfo.Name,
                    typeModifiers: CS.TypeModifiers.Public,
                    type: domainApiTypeName,
                    arrowExpression: new CS.ArrowExpressionClause(
                        $"new {domainApiTypeName}({domainApiParams})"),
                    attributes: propAttributes,
                    xmlDocumentation: Context.CSharp.CreateDocumentation(domainInfo)
                    );
                typeMembers.Add(propDecl);
            }

            CS.SyntaxObject typeDeclaration;
            if (Context.Options.StructProtocolApi)
            {
                typeDeclaration = new CS.StructDeclaration(
                    name: Context.Options.ProtocolApiTypeName,
                    modifiers: CS.TypeModifiers.Public | CS.TypeModifiers.ReadOnly | Context.CSharp.GetDefaultTypeModifiers(),
                    members: typeMembers);
            }
            else
            {
                typeDeclaration = new CS.ClassDeclaration(
                    name: Context.Options.ProtocolApiTypeName,
                    modifiers: CS.TypeModifiers.Partial,
                    members: typeMembers);
            }
            declarations.Add(typeDeclaration);

            return declarations;
        }
    }
}
