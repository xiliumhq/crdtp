using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using IO = System.IO;

namespace Xilium.Crdtp.Emitters
{
    internal sealed class ProjectEmitter : Emitter
    {
        public ProjectEmitter(Context context) : base(context) { }

        public override void Emit()
        {
            var compileItems = Context.OutputScope.Items.Where(x => x.Category == "Compile");

            // Emit warning as error when STJ generator hit in conflicting name.
            var propertyGroup = new XElement("PropertyGroup",
                new XElement("WarningsAsErrors", "$(WarningsAsErrors);SYSLIB1031"));

            var compileItemGroup = new XElement("ItemGroup", GetCompileItems(compileItems));
            var document = new XDocument(new XElement("Project", propertyGroup, compileItemGroup));

            var sb = new StringBuilder();
            using (var sw = new IO.StringWriter(sb))
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "\x20\x20",
            }))
            {
                document.Save(xw);
            }

            Context.OutputScope.Add(new OutputItem
            {
                Path = "Protocol.g.props",
                Category = "Project",
                Content = sb.ToString(),
            });
        }

        private IEnumerable<XElement> GetCompileItems(IEnumerable<OutputItem> compileItems)
        {
            return compileItems
                .Select(x => "$(MSBuildThisFileDirectory)" + IO.Path.GetRelativePath(Context.OutputScope.PhysicalPath, x.PhysicalPath))
                .OrderBy(x => x)
                .Select(x => new XElement("Compile",
                    new XAttribute("Include", x)
                    )
                );
        }
    }
}
