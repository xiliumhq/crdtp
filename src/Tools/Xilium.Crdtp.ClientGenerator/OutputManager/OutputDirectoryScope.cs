using System;
using System.Collections.Generic;
using IO = System.IO;

namespace Xilium.Crdtp
{
    public sealed class OutputDirectoryScope : OutputScope
    {
        private readonly string _path;
        private readonly string _physicalPath;
        private readonly List<OutputItem> _items = new();

        public OutputDirectoryScope(string path)
        {
            _path = path;
            _physicalPath = IO.Path.GetFullPath(path);
        }

        public override string PhysicalPath => _physicalPath;

        internal override string GetPhysicalPath(OutputItem outputItem)
        {
            return IO.Path.GetFullPath(IO.Path.Combine(_physicalPath, outputItem.Path));
        }

        public override IReadOnlyCollection<OutputItem> Items => _items;

        public override void Add(OutputItem item)
        {
            item.Attach(this);
            _items.Add(item);
        }

        public override int Commit()
        {
            Console.WriteLine($"{nameof(OutputScope)}::{nameof(Commit)}");

            var allFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int itemsUpdated = 0;

            foreach (var item in _items)
            {
                if (!string.IsNullOrEmpty(item.Content))
                {
                    // TODO: actual writes should be allowed only after validation.
                    // It is good idea to perform validation on adding instead of committing, because
                    // early error report is better.

                    var itemPhysicalPath = item.PhysicalPath;
                    if (!allFiles.Add(itemPhysicalPath))
                    {
                        throw Error.InvalidOperation("Attempt to overwrite file: \"{0}\".", itemPhysicalPath);
                    }

                    var itemExist = IO.File.Exists(itemPhysicalPath);
                    bool updateItem;

                    if (itemExist)
                    {
                        var oldContent = IO.File.ReadAllText(itemPhysicalPath);
                        updateItem = oldContent != item.Content;
                    }
                    else
                    {
                        updateItem = true;
                    }

                    if (!itemExist)
                    {
                        IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(itemPhysicalPath) ?? throw new InvalidOperationException());
                    }

                    if (updateItem)
                    {
                        IO.File.WriteAllText(itemPhysicalPath, item.Content);
                        Console.WriteLine("Written: {0}", itemPhysicalPath);
                        itemsUpdated++;
                    }
                }
            }

            return itemsUpdated;
        }
    }
}
