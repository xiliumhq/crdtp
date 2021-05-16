using System.Collections.Generic;
using System.Linq;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class StjEnumConverterEmitter : StjConverterEmitter
    {
        private readonly EnumTypeInfo _enumTypeInfo;

        public StjEnumConverterEmitter(Context context, EnumTypeInfo enumTypeInfo)
            : base(context, enumTypeInfo)
        {
            _enumTypeInfo = enumTypeInfo;
        }
        protected override IEnumerable<CS.SyntaxObject> GetReadBody()
        {
            yield return new CS.Raw("var value = reader.GetString() ?? throw new JsonException();");
            yield return new CS.Raw("var result = value switch");
            yield return new CS.Raw("{");
            foreach (var enumMember in _enumTypeInfo.Members.OrderBy(x => x.Name))
            {
                yield return new CS.Raw($"    {Context.CSharp.CreateString(enumMember.ProtocolName)} => {_enumTypeInfo.GetFullyQualifiedName()}.{enumMember.Name},");
            }
            yield return new CS.Raw("    _ => throw new NotSupportedException(value),");
            yield return new CS.Raw("};");
            yield return new CS.Raw("return result;");
        }

        protected override IEnumerable<CS.SyntaxObject> GetWriteBody()
        {
            yield return new CS.Raw("var serialized = value switch");
            yield return new CS.Raw("{");
            foreach (var enumMember in _enumTypeInfo.Members.OrderBy(x => x.Name))
            {
                yield return new CS.Raw($"    {_enumTypeInfo.GetFullyQualifiedName()}.{enumMember.Name} => {Context.CSharp.CreateString(enumMember.ProtocolName)},");
            }
            yield return new CS.Raw($"    _ => throw new NotSupportedException({Context.CSharp.CreateString(_enumTypeInfo.Name)} + \"::\" + value.ToString()),");
            yield return new CS.Raw("};");
            yield return new CS.Raw("writer.WriteStringValue(serialized);");
        }
    }
}
