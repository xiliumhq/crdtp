using System.Collections.Generic;
using System.Linq;
using CS = Xilium.Chromium.DevTools.Syntax;

namespace Xilium.Crdtp.Emitters
{
    internal abstract class CompilationUnitEmitter : Emitter
    {
        protected CompilationUnitEmitter(Context context)
            : base(context)
        {
            UsingNamespaces = new();
        }

        protected HashSet<string> UsingNamespaces { get; private set; }

        public virtual bool ShouldEmit => true;

        public sealed override void Emit()
        {
            if (!ShouldEmit) return;

            var unit = CreateUnit(GetPrologue(), GetContent());

            Context.OutputScope.Add(new OutputItem
            {
                Path = GetOutputItemPath(),
                Category = "Compile",
                Content = Context.CSharp.GetContent(unit),
            });
        }

        protected virtual List<CS.SyntaxObject> GetPrologue()
        {
            var members = new List<CS.SyntaxObject>();
            // TODO: disable warnings only when need
            //members.Add(
            //    new CS.Raw("#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.")
            //    );
            return members;
        }

        protected abstract List<CS.SyntaxObject> GetNamespaceContent();

        protected virtual List<CS.SyntaxObject> GetContent()
        {
            var content = new CS.Namespace(GetNamespace(), GetNamespaceContent());
            return new List<CS.SyntaxObject>() { content };
        }

        private CS.Unit CreateUnit(IEnumerable<CS.SyntaxObject> prologue, IEnumerable<CS.SyntaxObject> content)
        {
            var unit = Context.CSharp.CreateUnit(
                new CS.SyntaxObject[] {
                    new CS.Raw("#nullable enable"),
                }
                .Concat(prologue)
                .Concat(UsingNamespaces.OrderBy(x => x).Select(x => new CS.UsingNamespace(x)))
                .Concat(new CS.SyntaxObject[] {
                    new CS.EmptyLine()
                })
                .Concat(content));
            return unit;
        }

        protected abstract string GetOutputItemPath();

        protected virtual string GetNamespace() => Context.Options.Namespace;
    }
}
