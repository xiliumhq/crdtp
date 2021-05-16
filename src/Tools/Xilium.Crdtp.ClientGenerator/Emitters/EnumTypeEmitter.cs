using System.Collections.Generic;
using System.Linq;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class EnumTypeEmitter : CompilationUnitEmitter
    {
        private readonly EnumTypeInfo _typeInfo;

        public EnumTypeEmitter(Context context, EnumTypeInfo typeInfo)
            : base(context)
        {
            _typeInfo = typeInfo;
        }

        protected override string GetOutputItemPath()
            => Context.NamingPolicy.GetOutputItemPath(_typeInfo.Domain, _typeInfo);

        protected override string GetNamespace() => _typeInfo.Namespace;

        protected override List<CS.SyntaxObject> GetNamespaceContent()
        {
            var declarations = new List<CS.SyntaxObject>();

            var typeAttributes = Context.CSharp.CreateAttributeList(_typeInfo, UsingNamespaces);

            var typeMembers = new List<CS.SyntaxObject>();

            // TODO: Create Default .ctor but only if needed. (In order if more ctors will be added).

            var isFirstMember = true;
            foreach (var enumMemberInfo in _typeInfo.Members)
            {
                var enumMemberDecl = new CS.EnumValue(
                    enumMemberInfo.Name,
                    value: isFirstMember ? "1" : null,
                    attributes: null,
                    xmlDocumentation: null);
                typeMembers.Add(enumMemberDecl);

                isFirstMember = false;
            }

            var typeDeclaration = new CS.EnumDeclaration(
                name: _typeInfo.Name,
                modifiers: CS.TypeModifiers.Public,
                members: typeMembers,
                baseType: null,
                attributes: typeAttributes,
                xmlDocumentation: Context.CSharp.CreateDocumentation(_typeInfo));
            declarations.AddRange(Context.CSharp.GetTypeAnalysis(_typeInfo));
            declarations.Add(typeDeclaration);

            return declarations;
        }
    }
}
