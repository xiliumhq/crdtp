using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class AliasOfDictionaryTypeEmitter : CompilationUnitEmitter
    {
        private readonly AliasTypeInfo _typeInfo;

        public AliasOfDictionaryTypeEmitter(Context context, AliasTypeInfo typeInfo)
            : base(context)
        {
            _typeInfo = typeInfo;

            Check.That(typeInfo.UnderlyingType is DictionaryTypeInfo);
        }

        private TypeInfo TypeInfo => _typeInfo;
        private DictionaryTypeInfo UnderlyingTypeInfo => (DictionaryTypeInfo)_typeInfo.UnderlyingType;

        protected override string GetOutputItemPath()
            => Context.NamingPolicy.GetOutputItemPath(_typeInfo.Domain, _typeInfo);

        protected override string GetNamespace() => _typeInfo.Namespace;

        protected override List<CS.SyntaxObject> GetNamespaceContent()
        {
            var declarations = new List<CS.SyntaxObject>();

            var typeAttributes = Context.CSharp.CreateAttributeList(_typeInfo, UsingNamespaces);

            var typeMembers = new List<CS.SyntaxObject>();
            var typeName = _typeInfo.Name;

            //var ctorDecl = new CS.Constructor(
            //    name: typeName,
            //    parameters: new[] { new CS.Parameter("value", underlyingValueType) },
            //    modifiers: CS.CSharpModifiers.Public,
            //    attributes: null,
            //    xmlDocumentation: null,
            //    members: new CS.SyntaxObject[]
            //    {
            //        new CS.Raw($"{fieldDecl.Name} = value;")
            //    },
            //    subconstructor: null
            //);
            //typeMembers.Add(ctorDecl);

            // emit all standard list-like .ctors 

            var typeDeclaration = new CS.ClassDeclaration(
                name: typeName,
                modifiers: CS.TypeModifiers.Public | CS.TypeModifiers.Sealed | Context.CSharp.GetDefaultTypeModifiers(),
                members: typeMembers,
                baseType: UnderlyingTypeInfo.GetFullyQualifiedName(),
                attributes: typeAttributes,
                xmlDocumentation: Context.CSharp.CreateDocumentation(_typeInfo));
            declarations.AddRange(Context.CSharp.GetTypeAnalysis(_typeInfo));
            declarations.Add(typeDeclaration);

            return declarations;
        }
    }
}
