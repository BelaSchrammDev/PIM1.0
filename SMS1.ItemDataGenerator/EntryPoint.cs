using System;
using System.IO;

namespace SmsItemDataGenerator
{
    public static class ItemDataGenerator
    {
        /// <summary>Reads the data directory and returns the content of ItemData.g.cs.</summary>
        public static string Generate(string dataDir)
        {
            return ItemDataEncoder.GenerateSource(ItemDataFile.Read(dataDir));
        }
    }

    static class EntryPoint
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("usage: SMS1.ItemDataGenerator <dataDir> <outputFile>");
                return 2;
            }

            try
            {
                var source = ItemDataGenerator.Generate(args[0]);
                var output = args[1];
                // only write on changes, so an unchanged file does not trigger a recompile
                if (!File.Exists(output) || File.ReadAllText(output).Replace("\r\n", "\n") != source)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
                    File.WriteAllText(output, source);
                    Console.WriteLine($"ItemDataGenerator: {output} written ({source.Length} chars)");
                }
                else
                {
                    Console.WriteLine($"ItemDataGenerator: {output} is up to date");
                }
                return 0;
            }
            catch (ItemDataException e)
            {
                // canonical MSBuild error format, clickable in Visual Studio
                Console.Error.WriteLine($"{e.Location}: error ID0001: {e.Message}");
                return 1;
            }
        }
    }
}
