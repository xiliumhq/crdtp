namespace Xilium.Chromium.DevTools.Syntax
{

    using System;
    using System.IO;
    using System.Text;

    public sealed class CodeWriter : IDisposable
    {
        private readonly TextWriter _writer;
        private StringBuilder _line;
        private int _indent;

        public CodeWriter(TextWriter writer)
        {
            _writer = writer;
            _line = new StringBuilder();
            _indent = 0;
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        public void Indent()
        {
            _indent++;
        }

        public void Unindent()
        {
            _indent--;
        }

        public void WriteEol() // TODO: Should be just WriteLine()
        {
            if (_line.Length > 0)
            {
                if (_indent > 0)
                {
                    for (var i = 0; i < _indent; i++)
                    {
                        _writer.Write("\x20\x20\x20\x20");
                    }
                }
                _writer.WriteLine(_line.ToString());
                _line.Clear();
            }
            else
            {
                _writer.WriteLine();
            }
        }

        public void Write(string value)
        {
            _line.Append(value);
        }

        public void WriteLine(string value)
        {
            Write(value);
            WriteEol();
        }
    }
}
