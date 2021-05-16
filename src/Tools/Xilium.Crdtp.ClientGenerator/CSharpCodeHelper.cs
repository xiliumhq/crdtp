using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xilium.Crdtp.Model;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp
{
    internal sealed class CSharpCodeHelper
    {
        private readonly Context _context;

        internal CSharpCodeHelper(Context context)
        {
            _context = context;
        }

        public IEnumerable<string> GetAutomaticallyGeneratedFileNotice() => new[] {
            "",
            "DO NOT MODIFY. THIS IS AUTOMATICALLY GENERATED FILE.",
            "",
        };

        public CS.Unit CreateUnit(CS.SyntaxObject node)
            => CreateUnit(new[] { node });

        public CS.Unit CreateUnit(IEnumerable<CS.SyntaxObject> nodes)
        {
            var comments = GetAutomaticallyGeneratedFileNotice()
                .Select(x => new CS.Comment(x));
            return new CS.Unit(comments.Concat(nodes));
        }

        public List<CS.AttributeDecl> CreateAttributeList(MemberInfo memberInfo, HashSet<string>? namespaces)
        {
            var attributes = new List<CS.AttributeDecl>();
            if (memberInfo.IsDeprecated)
            {
                attributes.Add(new CS.AttributeDecl("Obsolete"));
                namespaces?.Add("System");
            }
            return attributes;
        }

        public CS.XmlDocumentation? CreateDocumentation(MemberInfo memberInfo)
        {
            if (!_context.Options.EmitDocumentation) return null;

            var sb = new StringBuilder();

            if (memberInfo.IsDeprecated)
            {
                sb.Append("[Deprecated] ");
            }

            if (memberInfo.IsExperimental)
            {
                sb.Append("[Experimental] ");
            }

            var first = true;
            foreach (var line in memberInfo.Description)
            {
                if (first) first = false;
                else sb.AppendLine();
                sb.Append(line.Trim());
            }

            if (sb.Length > 0)
            {
                return new CS.XmlDocumentation(sb.ToString());
            }
            else
            {
                return null;
            }
        }

        public CS.XmlDocumentation? CreateDocumentation(string? summary)
        {
            if (!_context.Options.EmitDocumentation) return null;
            return new CS.XmlDocumentation(summary);
        }

        public IEnumerable<CS.SyntaxObject> GetTypeAnalysis(TypeInfo typeInfo)
        {
            if (!_context.Options.EmitTypeAnalysis) return Enumerable.Empty<CS.SyntaxObject>();

            return new CS.SyntaxObject[]
            {
                new CS.Comment("Type Analysis:"),
                new CS.Comment(string.Format("       IsReachable: {0}", typeInfo.IsReachable ? "true" : "false")),
                new CS.Comment(string.Format("    IsSerializable: {0}", typeInfo.IsSerializable ? "true" : "false")),
                new CS.Comment(string.Format("  IsDeserializable: {0}", typeInfo.IsDeserializable ? "true" : "false")),
                new CS.EmptyLine(),
            };
        }

        public string CreateString(string value)
        {
            return "\"" + value + "\"";
        }

        public string GetContent(CS.Unit unit)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var cw = new CS.CodeWriter(sw))
            using (var csw = new CS.CSharpWriter(cw))
            {
                csw.Write(unit);
            }
            return sb.ToString();
        }
    }
}
