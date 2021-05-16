using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xilium.Crdtp.Pdl.Syntax;

namespace Xilium.Crdtp.Pdl
{
    public static class PdlFileReader
    {
        public static ProtocolSyntax Read(IEnumerable<string> files)
        {
            ProtocolSyntax? result = null;
            foreach (var file in files)
            {
                var protocol = ParsePdlFile(file);
                if (result == null)
                {
                    result = protocol;
                    continue;
                }

                // Merge PDL Domains
                if (!string.Equals(result.Version.ToString(), protocol.Version.ToString()))
                {
                    // TODO: Use Logging/Diagnostics
                    Console.WriteLine($"Warning: Different versions. The base version is {result.Version}, the current is {protocol.Version}");
                }

                foreach (var domain in protocol.Domains)
                {
                    if (result.Domains.Any(x => x.Name == domain.Name))
                    {
                        throw Error.InvalidOperation("Duplicate Domain: \"{0}\".", domain.Name);
                    }
                    result.Domains.Add(domain);
                }
            }

            Check.That(result != null);
            return result;
        }

        private static ProtocolSyntax ParsePdlFile(string path)
        {
            Console.WriteLine("Reading: " + path); // TODO: Use Logging

            using var stream = File.OpenRead(path);
            using var reader = new StreamReader(stream);
            return PdlReader.Parse(reader);
        }
    }
}
