using System;
using System.Collections.Generic;
using System.Linq;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class DomainApiEmitter : CompilationUnitEmitter
    {
        private readonly DomainInfo _domainInfo;
        private readonly DomainApiTypeInfo _domainApiTypeInfo;

        public DomainApiEmitter(Context context, DomainInfo domainInfo)
            : base(context)
        {
            _domainInfo = domainInfo;
            _domainApiTypeInfo = new DomainApiTypeInfo(context, domainInfo);
        }

        protected override string GetOutputItemPath()
            => Context.NamingPolicy.GetOutputItemPath(_domainInfo, _domainApiTypeInfo);

        protected override string GetNamespace()
            => _domainApiTypeInfo.Namespace;

        protected override List<CS.SyntaxObject> GetNamespaceContent()
        {
            var declarations = new List<CS.SyntaxObject>();

            var commands = _domainInfo.Commands.Where(x => x.IsReachable);
            var events = _domainInfo.Events.Where(x => x.IsReachable);

            var domainApiTypeAttributes = Context.CSharp.CreateAttributeList(_domainInfo, UsingNamespaces);
            var domainApiTypeMembers = new List<CS.SyntaxObject>();

            var sessionTypeName = Context.Options.UseApi2
                ? Context.Options.CrdtpSessionTypeName
                : WellKnownTypes.ProtocolApiTypeInfo.GetFullyQualifiedName();

            var sessionFieldDecl = new CS.FieldDeclaration(
                "_session",
                CS.CSharpModifiers.Private | CS.CSharpModifiers.ReadOnly,
                sessionTypeName);
            domainApiTypeMembers.Add(sessionFieldDecl);

            domainApiTypeMembers.Add(new CS.Constructor(
                name: _domainApiTypeInfo.Name,
                parameters: new[] { new CS.Parameter("session", sessionTypeName) },
                modifiers: CS.CSharpModifiers.Internal,
                attributes: null,
                xmlDocumentation: null,
                members: new CS.SyntaxObject[]
                {
                    new CS.Raw($"{sessionFieldDecl.Name} = session;")
                },
                subconstructor: null
                ));
            var getCrdtpSessionExpr = Context.Options.UseApi2
                ? sessionFieldDecl.Name
                : $"{sessionFieldDecl.Name}.GetCrdtpSession()";

            foreach (var command in commands.OrderBy(x => x.Name))
            {
                var parametersType = command.ParametersType;
                var returnType = command.ReturnType;

                var parametersTypeName = command.ParametersType.GetFullyQualifiedName();
                var returnTypeName = command.ReturnType.GetFullyQualifiedName();
                var protocolMethod = command.ProtocolMethod;

                var commandAttributes = Context.CSharp.CreateAttributeList(command, UsingNamespaces);

                // TODO: emit Try/non-Try methods

                var methodParameters = new List<CS.Parameter>();
                string parametersExpr;
                {
                    if (parametersType is UnitTypeInfo)
                    {
                        parametersExpr = $"default({parametersTypeName})!";
                    }
                    else
                    {
                        var parametersParameter = new CS.Parameter("parameters", parametersTypeName);
                        methodParameters.Add(parametersParameter);
                        parametersExpr = parametersParameter.Name;
                    }
                    methodParameters.Add(new CS.Parameter("cancellationToken", "System.Threading.CancellationToken", defaultValue: "default"));
                }

                CS.Parameter methodReturnParameter;
                CS.SyntaxObject[] methodBody;
                if (returnType is UnitTypeInfo)
                {
                    methodReturnParameter = new CS.Parameter(null!, "System.Threading.Tasks.Task");
                    methodBody = new CS.SyntaxObject[]
                    {
                        // TODO: emit JsonEncodedText command name... (
                        new CS.Raw($"return {getCrdtpSessionExpr}"),
                        new CS.Raw($"    .ExecuteCommandAsync<{parametersTypeName}>("),
                        new CS.Raw($"        {Context.CSharp.CreateString(protocolMethod)}, {parametersExpr}, cancellationToken);"),
                    };
                }
                else
                {
                    methodReturnParameter = new CS.Parameter(null!, $"System.Threading.Tasks.Task<{returnTypeName}>");
                    methodBody = new CS.SyntaxObject[]
                    {
                        // TODO: emit JsonEncodedText command name... (
                        new CS.Raw($"return {getCrdtpSessionExpr}"),
                        new CS.Raw($"    .ExecuteCommandAsync<{parametersTypeName}, {returnTypeName}>("),
                        new CS.Raw($"        {Context.CSharp.CreateString(protocolMethod)}, {parametersExpr}, cancellationToken);"),
                    };
                }

                var method = new CS.MethodDeclaration(
                    name: command.Name + "Async",
                    parameters: methodParameters,
                    returnParameter: methodReturnParameter,
                    modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.ReadOnly,
                    attributes: commandAttributes,
                    xmlDocumentation: Context.CSharp.CreateDocumentation(command),
                    members: methodBody);
                domainApiTypeMembers.Add(method);
            }

            foreach (var eventInfo in events.OrderBy(x => x.Name))
            {
                var paramsType = eventInfo.ParametersType.GetFullyQualifiedName();
                var protocolMethod = eventInfo.ProtocolMethod;

                var eventAttributes = Context.CSharp.CreateAttributeList(eventInfo, UsingNamespaces);

                // TODO: Emit System.EventHandler for Unit type?

                var csEventNode = new CS.PropertyDeclaration( // TODO: should be event
                    name: eventInfo.Name,
                    typeModifiers: CS.TypeModifiers.Public | CS.TypeModifiers.Event | CS.TypeModifiers.ReadOnly,
                    type: $"System.EventHandler<{paramsType}>",
                    accessorList: new CS.AccessorDeclaration[]
                    {
                        new CS.AddAccessorDeclaration(CS.TypeModifiers.None, new CS.SyntaxObject[] {
                            new CS.Raw($"{getCrdtpSessionExpr}.AddEventHandler({Context.CSharp.CreateString(eventInfo.ProtocolMethod)}, value);") // TODO: we might want use JsonEncodedText instead of strings
                        }),
                        new CS.RemoveAccessorDeclaration(CS.TypeModifiers.None, new CS.SyntaxObject[] {
                            new CS.Raw($"{getCrdtpSessionExpr}.RemoveEventHandler({Context.CSharp.CreateString(eventInfo.ProtocolMethod)}, value);")
                        }),
                    },
                    attributes: eventAttributes,
                    xmlDocumentation: Context.CSharp.CreateDocumentation(eventInfo)
                    );
                domainApiTypeMembers.Add(csEventNode);
            }

            var domainApiTypeDecl = new CS.StructDeclaration(
                name: _domainApiTypeInfo.Name,
                modifiers: CS.TypeModifiers.Public | CS.TypeModifiers.ReadOnly,
                members: domainApiTypeMembers,
                baseType: null,
                attributes: domainApiTypeAttributes,
                xmlDocumentation: Context.CSharp.CreateDocumentation(_domainInfo));
            declarations.Add(domainApiTypeDecl);

            return declarations;
        }
    }
}
