namespace Xilium.Chromium.DevTools.Syntax
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    public sealed class CSharpWriter : IDisposable
    {
        private readonly CodeWriter _writer;

        public CSharpWriter(CodeWriter writer)
        {
            _writer = writer;
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        public void Write(Unit node)
        {
            Visit(node);
        }

        private void Visit(Unit node)
        {
            foreach (var member in node.Members)
            {
                Visit(member);
            }
        }

        private void Visit(SyntaxObject node)
        {
            if (node is CodeBlock)
            {
                Visit((CodeBlock)node);
            }
            else if (node is Namespace)
            {
                Visit((Namespace)node);
            }
            else if (node is PropertyDeclaration)
            {
                Visit((PropertyDeclaration)node);
            }
            else if (node is FieldDeclaration)
            {
                Visit((FieldDeclaration)node);
            }
            else if (node is ClassDeclaration)
            {
                Visit((ClassDeclaration)node);
            }
            else if (node is InterfaceDeclaration)
            {
                Visit((InterfaceDeclaration)node);
            }
            else if (node is StructDeclaration)
            {
                Visit((StructDeclaration)node);
            }
            else if (node is EnumDeclaration)
            {
                Visit((EnumDeclaration)node);
            }
            else if (node is EnumValue)
            {
                Visit((EnumValue)node);
            }
            else if (node is MethodDeclaration)
            {
                Visit((MethodDeclaration)node);
            }
            else if (node is Constructor)
            {
                Visit((Constructor)node);
            }
            else if (node is Subconstructor)
            {
                Visit((Subconstructor)node);
            }
            else if (node is UsingNamespace)
            {
                Visit((UsingNamespace)node);
            }
            else if (node is EmptyLine)
            {
                Visit((EmptyLine)node);
            }
            else if (node is Raw)
            {
                Visit((Raw)node);
            }
            else if (node is Comment)
            {
                Visit((Comment)node);
            }
            else if (node is IfStatement)
            {
                Visit((IfStatement)node);
            }
            else if (node is WhileDeclaration)
            {
                Visit((WhileDeclaration)node);
            }
            else throw new InvalidOperationException();
        }

        private void Visit(CodeBlock node)
        {
            if (node.Indent) BeginBlock();
            foreach (var x in node.Members)
            {
                Visit(x);
            }
            if (node.Indent) EndBlock(node.TerminateStatement);
        }

        private void Visit(Namespace node)
        {
            Visit(node.XmlDocumentation);
            _writer.WriteLine($"namespace {node.Name}");
            BeginBlock();
            foreach (var x in node.Members)
            {
                Visit(x);
            }
            EndBlock();
        }

        private void Visit(ClassDeclaration node)
        {
            Visit(node.XmlDocumentation);
            Visit(node.Attributes);
            _writer.WriteLine($"{GetTypeModifiers(node.Modifiers)}class {node.Name}");
            if (node.BaseType != null)
            {
                _writer.Indent();
                _writer.WriteLine($": {node.BaseType}");
                _writer.Unindent();
            }
            BeginBlock();
            bool emitEmptyLine = false;
            foreach (var x in node.Members)
            {
                if (emitEmptyLine) _writer.WriteEol();
                else emitEmptyLine = true;
                Visit(x);
            }
            EndBlock();
        }

        private void Visit(InterfaceDeclaration node)
        {
            Visit(node.XmlDocumentation);
            Visit(node.Attributes);
            _writer.WriteLine($"{GetTypeModifiers(node.Modifiers)}interface {node.Name}");
            if (node.BaseType != null)
            {
                _writer.Indent();
                _writer.WriteLine($": {node.BaseType}");
                _writer.Unindent();
            }
            BeginBlock();
            bool emitEmptyLine = false;
            foreach (var x in node.Members)
            {
                if (emitEmptyLine) _writer.WriteEol();
                else emitEmptyLine = true;
                Visit(x);
            }
            EndBlock();
        }

        private void Visit(StructDeclaration node)
        {
            Visit(node.XmlDocumentation);
            Visit(node.Attributes);
            _writer.WriteLine($"{GetTypeModifiers(node.Modifiers)}struct {node.Name}");
            if (node.BaseType != null)
            {
                _writer.Indent();
                _writer.WriteLine($": {node.BaseType}");
                _writer.Unindent();
            }
            BeginBlock();
            bool emitEmptyLine = false;
            foreach (var x in node.Members)
            {
                if (emitEmptyLine) _writer.WriteEol();
                else emitEmptyLine = true;
                Visit(x);
            }
            EndBlock();
        }

        private void Visit(EnumDeclaration node)
        {
            Visit(node.XmlDocumentation);
            Visit(node.Attributes);
            _writer.Write($"{GetTypeModifiers(node.Modifiers)}enum {node.Name}");
            if (node.BaseType != null)
            {
                _writer.Write(" : ");
                _writer.Write(node.BaseType);
            }
            _writer.WriteEol();
            BeginBlock();
            // bool emitEmptyLine = false;
            foreach (var x in node.Members)
            {
                // TODO: Emit empty lines only if attributes or comments are emitted.
                //if (emitEmptyLine) _writer.WriteEol();
                //else emitEmptyLine = true;
                Visit(x);
            }
            EndBlock();
        }

        private void Visit(EnumValue node)
        {
            Visit(node.XmlDocumentation);
            Visit(node.Attributes);
            _writer.Write($"{node.Name}");
            if (node.Value != null)
            {
                _writer.Write(" = ");
                _writer.Write(node.Value);
            }
            _writer.WriteLine(",");
        }

        private void Visit(IEnumerable<AttributeDecl> attributes, bool inline = false)
        {
            if (attributes == null) return;
            foreach (var x in attributes)
            {
                Visit(x, inline);
            }
        }

        private void Visit(AttributeDecl node, bool inline)
        {
            _writer.Write("[");
            _writer.Write(node.Name);
            if (node.Parameters.Count() > 0 || node.NamedParameters.Count > 0)
            {
                _writer.Write("(");
                bool needsComma = false;
                foreach (var x in node.Parameters)
                {
                    if (needsComma) _writer.Write(", ");
                    else needsComma = true;
                    _writer.Write(x);
                }

                foreach (var kv in node.NamedParameters)
                {
                    if (needsComma) _writer.Write(", ");
                    else needsComma = true;
                    _writer.Write($"{kv.Key} = {kv.Value}");
                }

                _writer.Write(")");
            }
            _writer.Write("]");
            if (!inline) _writer.WriteEol();
        }

        private void Visit(FieldDeclaration node)
        {
            Visit(node.XmlDocumentation);
            Visit(node.Attributes);
            _writer.Write($"{GetCSharpModifiers(node.Modifiers)}{node.Type} {node.Name}");
            if (string.IsNullOrEmpty(node.InitialValue))
                _writer.WriteLine(";");
            else
                _writer.WriteLine($" = {node.InitialValue};");
        }

        private void Visit(PropertyDeclaration node)
        {
            Visit(node.XmlDocumentation);
            Visit(node.Attributes);
            _writer.Write($"{GetTypeModifiers(node.TypeModifiers)}{node.Type} {node.Name} ");
            if (node.ArrowExpression != null)
            {
                _writer.WriteLine($"=> {node.ArrowExpression.ExpressionBody};");
            }
            else
            {
                var hasBlock = node.AccessorList.Any(f => f.Members.Any());
                if (hasBlock)
                {
                    _writer.WriteEol();
                    BeginBlock();
                    foreach (var accessor in node.AccessorList)
                        Visit(accessor);
                    EndBlock();
                }
                else
                {
                    _writer.Write("{");
                    foreach (var accessor in node.AccessorList)
                    {
                        _writer.Write(" ");
                        Visit(accessor);
                    }
                    _writer.WriteLine(" }");
                }
            }
        }

        private void Visit(AccessorDeclaration accessor)
        {
            _writer.Write($"{GetTypeModifiers(accessor.TypeModifiers)}{accessor.Keyword()}");
            if (accessor.Members.Any())
            {
                _writer.WriteEol();
                BeginBlock();
                foreach (var m in accessor.Members)
                    Visit(m);
                EndBlock();
            }
            else
            {
                var body = accessor.ArrowExpression?.ExpressionBody;
                _writer.Write($"{(string.IsNullOrWhiteSpace(body) ? "" : $" => {body}")};");
            }
        }

        private void Visit(XmlDocumentation? node)
        {
            if (node == null) return;

            // TODO: Use XmlWriter to build whole doc comment.
            if (!string.IsNullOrWhiteSpace(node.Summary))
            {
                var summaryText = EscapeXml(node.Summary);

                if (summaryText.Length > 40 || summaryText.IndexOf('\n') > -1)
                {
                    _writer.WriteLine("/// <summary>");
                    foreach (var summaryLine in summaryText.Split('\n'))
                        _writer.WriteLine($"/// {summaryLine.Replace("\r", string.Empty, StringComparison.OrdinalIgnoreCase)}");
                    _writer.WriteLine("/// </summary>");
                }
                else
                {
                    _writer.WriteLine($"/// <summary>{summaryText}</summary>");
                }
            }

            foreach (var param in node.ParamList)
            {
                if (string.IsNullOrEmpty(param.Name))
                    throw new InvalidOperationException("param.Name == null");

                if (!string.IsNullOrWhiteSpace(param.Description))
                {
                    var paramDescriptionText = EscapeXml(param.Description);

                    const int MAX_LINE = 120;
                    var len = 27 + param.Name.Length + paramDescriptionText?.Length ?? 0;
                    var isMultiLine = paramDescriptionText?.Contains('\n');

                    if (len > MAX_LINE || isMultiLine.HasValue && isMultiLine.Value)
                    {
                        _writer.WriteLine($"/// <param name=\"{param.Name}\">");
                        if (!string.IsNullOrEmpty(paramDescriptionText))
                        {
                            foreach (var line in paramDescriptionText.Split('\n'))
                                _writer.WriteLine($"/// {line.Replace("\r", string.Empty, StringComparison.OrdinalIgnoreCase)}");
                        }
                        _writer.WriteLine("/// </param>");
                    }
                    else
                    {
                        _writer.Write($"/// <param name=\"{param.Name}\">");
                        if (!string.IsNullOrEmpty(paramDescriptionText))
                        {
                            _writer.Write(paramDescriptionText);
                        }
                        _writer.WriteLine("</param>");
                    }
                }
            }
        }

        private void Visit(Constructor node)
        {
            Visit(node.XmlDocumentation);
            Visit(node.Attributes);
            _writer.WriteLine($"{GetCSharpModifiers(node.Modifiers)}{node.Name}(");
            _writer.Indent();
            var first = true;
            foreach (var p in node.Parameters)
            {
                if (first) first = false;
                else
                {
                    _writer.Write(", ");
                    _writer.WriteEol();
                }
                Visit(p);
            }
            _writer.Write(")");
            Visit(node.Subconstructor);

            // Assume constructors always have body.
            {
                _writer.WriteEol();
                _writer.Unindent();
                BeginBlock();
                foreach (var m in node.Members)
                    Visit(m);
                EndBlock();
            }
        }

        private void Visit(Subconstructor? node)
        {
            if (node == null) return;
            _writer.Write($" : {GetAccessKeywords(node.Keyword)}(");
            var first = true;
            foreach (var p in node.Parameters)
            {
                if (first) first = false;
                else _writer.Write(", ");
                _writer.Write(p.Name);
            }
            _writer.Write(")");
        }

        private void Visit(MethodDeclaration node)
        {
            Visit(node.XmlDocumentation);
            Visit(node.Attributes);
            var returnType = (string.IsNullOrEmpty(node.ReturnParameter?.Type) ? "" : $"{node.ReturnParameter.Type} ");
            _writer.WriteLine($"{GetCSharpModifiers(node.Modifiers)}{returnType}{node.Name}(");
            _writer.Indent();
            var first = true;
            foreach (var p in node.Parameters)
            {
                if (first) first = false;
                else
                {
                    _writer.Write(", ");
                    _writer.WriteEol();
                }
                Visit(p);
            }
            _writer.Write(")");
            if (node.ArrowExpression != null)
            {
                _writer.WriteLine($" => {node.ArrowExpression.ExpressionBody};");
                _writer.Unindent();
            }
            else
            {
                if (node.Members.Count() == 0)
                {
                    _writer.WriteLine(";");
                    _writer.Unindent();
                }
                else
                {
                    _writer.WriteEol();
                    _writer.Unindent();
                    BeginBlock();
                    foreach (var m in node.Members)
                        Visit(m);
                    EndBlock();
                }
            }
        }

        private void Visit(Parameter node)
        {
            Visit(node.Attributes, true);
            _writer.Write(node.Type);
            _writer.Write(" ");
            _writer.Write(node.Name);
            if (!string.IsNullOrWhiteSpace(node.DefaultValue))
                _writer.Write($" = {node.DefaultValue}");
        }

        private void Visit(UsingNamespace node)
        {
            _writer.WriteLine($"using {node.Name};");
        }

        private void Visit(EmptyLine node)
        {
            if (node.Eol) _writer.WriteEol();
        }

        private void Visit(Comment node)
        {
            _writer.Write("//");
            if (!string.IsNullOrEmpty(node.Value))
            {
                _writer.Write(" ");
                _writer.Write(node.Value);
            }
            _writer.WriteEol();
        }

        private void Visit(Raw node)
        {
            _writer.WriteLine(node.Value);
        }

        private void Visit(IfStatement node)
        {
            _writer.WriteLine($"if({node.Condition})");
            BeginBlock();
            foreach (var m in node.Members)
                Visit(m);
            EndBlock();
        }

        private void Visit(WhileDeclaration node)
        {
            _writer.WriteLine($"while({node.Condition})");
            BeginBlock();
            foreach (var m in node.Members)
                Visit(m);
            EndBlock();
        }

        private string GetTypeModifiers(TypeModifiers modifiers, bool trailingSpace = true)
        {
            var sb = new StringBuilder();
            if ((modifiers & TypeModifiers.Public) != 0) sb.AppendWithLeadingSpace("public");
            if ((modifiers & TypeModifiers.Protected) != 0) sb.AppendWithLeadingSpace("protected");
            if ((modifiers & TypeModifiers.Private) != 0) sb.AppendWithLeadingSpace("private");
            if ((modifiers & TypeModifiers.Internal) != 0) sb.AppendWithLeadingSpace("internal");
            if ((modifiers & TypeModifiers.Static) != 0) sb.AppendWithLeadingSpace("static");
            if ((modifiers & TypeModifiers.Abstract) != 0) sb.AppendWithLeadingSpace("abstract");
            if ((modifiers & TypeModifiers.Sealed) != 0) sb.AppendWithLeadingSpace("sealed");
            if ((modifiers & TypeModifiers.ReadOnly) != 0) sb.AppendWithLeadingSpace("readonly");
            if ((modifiers & TypeModifiers.Partial) != 0) sb.AppendWithLeadingSpace("partial");
#pragma warning disable CS0618 // Type or member is obsolete
            if ((modifiers & TypeModifiers.Event) != 0) sb.AppendWithLeadingSpace("event");
#pragma warning restore CS0618 // Type or member is obsolete
            if (sb.Length > 0 && trailingSpace) sb.Append(' ');
            return sb.ToString();
        }

        private string GetCSharpModifiers(CSharpModifiers modifiers, bool trailingSpace = true)
        {
            var sb = new StringBuilder();
            if ((modifiers & CSharpModifiers.Public) != 0) sb.AppendWithLeadingSpace("public");
            if ((modifiers & CSharpModifiers.Protected) != 0) sb.AppendWithLeadingSpace("protected");
            if ((modifiers & CSharpModifiers.Private) != 0) sb.AppendWithLeadingSpace("private");
            if ((modifiers & CSharpModifiers.Internal) != 0) sb.AppendWithLeadingSpace("internal");
            if ((modifiers & CSharpModifiers.Static) != 0) sb.AppendWithLeadingSpace("static");
            if ((modifiers & CSharpModifiers.Abstract) != 0) sb.AppendWithLeadingSpace("abstract");
            if ((modifiers & CSharpModifiers.Sealed) != 0) sb.AppendWithLeadingSpace("sealed");
            if ((modifiers & CSharpModifiers.Partial) != 0) sb.AppendWithLeadingSpace("partial");
            if ((modifiers & CSharpModifiers.Async) != 0) sb.AppendWithLeadingSpace("async");
            if ((modifiers & CSharpModifiers.Const) != 0) sb.AppendWithLeadingSpace("const");
            if ((modifiers & CSharpModifiers.Event) != 0) sb.AppendWithLeadingSpace("event");
            if ((modifiers & CSharpModifiers.Extern) != 0) sb.AppendWithLeadingSpace("extern");
            if ((modifiers & CSharpModifiers.New) != 0) sb.AppendWithLeadingSpace("new");
            if ((modifiers & CSharpModifiers.Override) != 0) sb.AppendWithLeadingSpace("override");
            if ((modifiers & CSharpModifiers.ReadOnly) != 0) sb.AppendWithLeadingSpace("readonly");
            if ((modifiers & CSharpModifiers.Unsafe) != 0) sb.AppendWithLeadingSpace("unsafe");
            if ((modifiers & CSharpModifiers.Virtual) != 0) sb.AppendWithLeadingSpace("virtual");
            if ((modifiers & CSharpModifiers.Volatile) != 0) sb.AppendWithLeadingSpace("volatile");
            if (sb.Length > 0 && trailingSpace) sb.Append(' ');
            return sb.ToString();
        }

        private string GetAccessKeywords(AccessKeywords keyword)
        {
            switch (keyword)
            {
                case AccessKeywords.Base:
                    return "base";

                case AccessKeywords.This:
                    return "this";

                default:
                    return "";
            }
        }

        private void BeginBlock()
        {
            _writer.WriteLine("{");
            _writer.Indent();
        }

        public void EndBlock(bool terminateStatement = false)
        {
            _writer.Unindent();
            _writer.Write("}");
            _writer.WriteLine(terminateStatement ? ";" : "");
        }

        [return: NotNullIfNotNull("text")]
        private static string? EscapeXml(string? text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var summaryTextBuilder = new StringBuilder();
            using (var writer = System.Xml.XmlWriter.Create(summaryTextBuilder,
                new System.Xml.XmlWriterSettings
                {
                    ConformanceLevel = System.Xml.ConformanceLevel.Fragment,
                }))
            {
                writer.WriteString(text);
            }
            return summaryTextBuilder.ToString();
        }
    }
}
