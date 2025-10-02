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
                var throwingMethod = MakeCommandMethod(command, getCrdtpSessionExpr, throwingMethod: true);
                domainApiTypeMembers.Add(throwingMethod);

                if (Context.Options.NonThrowingMethods)
                {
                    var nonThrowingMethod = MakeCommandMethod(command, getCrdtpSessionExpr, throwingMethod: false);
                    domainApiTypeMembers.Add(nonThrowingMethod);
                }
            }

            foreach (var eventInfo in events.OrderBy(x => x.Name))
            {
                var paramsType = eventInfo.ParametersType.GetFullyQualifiedName();
                var protocolMethod = eventInfo.ProtocolMethod;

                var eventAttributes = Context.CSharp.CreateAttributeList(eventInfo, UsingNamespaces);

                // TODO: Emit System.EventHandler for Unit type?

                var csEventNode = new CS.PropertyDeclaration( // TODO: should be event
                    name: eventInfo.Name,
                    typeModifiers: CS.TypeModifiers.Public |
#pragma warning disable CS0618 // Type or member is obsolete
                    CS.TypeModifiers.Event |
#pragma warning restore CS0618 // Type or member is obsolete
                    CS.TypeModifiers.ReadOnly,
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
                modifiers: CS.TypeModifiers.Public | CS.TypeModifiers.ReadOnly | Context.CSharp.GetDefaultTypeModifiers(),
                members: domainApiTypeMembers,
                baseType: null,
                attributes: domainApiTypeAttributes,
                xmlDocumentation: Context.CSharp.CreateDocumentation(_domainInfo));
            declarations.Add(domainApiTypeDecl);

            return declarations;
        }

        private CS.MethodDeclaration MakeCommandMethod(CommandInfo command,
            string getCrdtpSessionExpr,
            bool throwingMethod)
        {
            Check.That(command != null);

            // TODO: Make it configurable
            bool invokeSendReturnsUnit = false;

#pragma warning disable CS0219 // Variable is assigned but its value is never used
            // TODO: Add option to always generate SendCommandAsync, and get
            // result in generated method (for throwing)
            bool useSendCommandForThrowingMethod = false;
            // TODO: Add option to generate await inside command invokers to
            // make clear stacktrace.
            bool invokeUsesAwaits = false;
#pragma warning restore CS0219 // Variable is assigned but its value is never used

            var parametersType = command.ParametersType;
            var returnType = command.ReturnType;

            var parametersTypeName = command.ParametersType.GetFullyQualifiedName();
            string returnTypeName = command.ReturnType.GetFullyQualifiedName();

            var protocolMethod = command.ProtocolMethod;

            var commandAttributes = Context.CSharp.CreateAttributeList(command, UsingNamespaces);

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

            var invokeCommandMethodName = throwingMethod ? "ExecuteCommandAsync" : "SendCommandAsync";

            const string CrdtpResponseTypeName = "Xilium.Crdtp.Client.CrdtpResponse";

            CS.Parameter methodReturnParameter;
            CS.SyntaxObject[] methodBody;
            if (returnType is UnitTypeInfo)
            {
                bool invokeCommandShouldIncludeReturnTypeName;

                string methodReturnParameterTypeName;
                if (throwingMethod)
                {
                    methodReturnParameterTypeName = $"System.Threading.Tasks.Task";
                    invokeCommandShouldIncludeReturnTypeName = false;
                }
                else
                {
                    if (invokeSendReturnsUnit)
                    {
                        methodReturnParameterTypeName =
                            $"System.Threading.Tasks.Task<{CrdtpResponseTypeName}<{returnTypeName}>>";
                        invokeCommandShouldIncludeReturnTypeName = true;
                    }
                    else
                    {
                        methodReturnParameterTypeName =
                            $"System.Threading.Tasks.Task<{CrdtpResponseTypeName}>";
                        invokeCommandShouldIncludeReturnTypeName = false;
                    }
                }

                string invokeAdditionalGenericParameters = "";
                if (invokeCommandShouldIncludeReturnTypeName)
                {
                    invokeAdditionalGenericParameters += $", {returnTypeName}";
                }

                methodReturnParameter = new CS.Parameter(null!, methodReturnParameterTypeName);
                methodBody = new CS.SyntaxObject[]
                {
                            // TODO: emit JsonEncodedText command name... (
                            new CS.Raw($"return {getCrdtpSessionExpr}"),
                            new CS.Raw($"    .{invokeCommandMethodName}<{parametersTypeName}{invokeAdditionalGenericParameters}>("),
                            new CS.Raw($"        {Context.CSharp.CreateString(protocolMethod)}, {parametersExpr}, cancellationToken);"),
                };
            }
            else
            {
                string methodReturnParameterTypeName;
                if (throwingMethod)
                {
                    methodReturnParameterTypeName = $"System.Threading.Tasks.Task<{returnTypeName}>";
                }
                else
                {
                    methodReturnParameterTypeName =
                        $"System.Threading.Tasks.Task<{CrdtpResponseTypeName}<{returnTypeName}>>";
                }

                methodReturnParameter = new CS.Parameter(null!, methodReturnParameterTypeName);
                methodBody = new CS.SyntaxObject[]
                {
                        // TODO: emit JsonEncodedText command name... (
                        new CS.Raw($"return {getCrdtpSessionExpr}"),
                        new CS.Raw($"    .{invokeCommandMethodName}<{parametersTypeName}, {returnTypeName}>("),
                        new CS.Raw($"        {Context.CSharp.CreateString(protocolMethod)}, {parametersExpr}, cancellationToken);"),
                };
            }

            // TODO: Use NamingPolicy
            var methodNamePrefix = throwingMethod ? "" : "Try";
            var methodName = methodNamePrefix + command.Name + "Async";

            var method = new CS.MethodDeclaration(
                name: methodName,
                parameters: methodParameters,
                returnParameter: methodReturnParameter,
                modifiers: CS.CSharpModifiers.Public | CS.CSharpModifiers.ReadOnly,
                attributes: commandAttributes,
                xmlDocumentation: Context.CSharp.CreateDocumentation(command),
                members: methodBody);
            return method;
        }
    }
}
