using System;
using System.IO;

namespace PimBluePrintGenerator
{
    public static class BluePrintDataGenerator
    {
        /// <summary>Reads the data directory and returns the content of BluePrintData.g.cs.</summary>
        public static string Generate(string dataDir)
        {
            return BluePrintDataEncoder.GenerateSource(BluePrintDataFile.Read(dataDir));
        }
    }

    static class EntryPoint
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("usage: PIM1.0.BluePrintGenerator <dataDir> <outputFile>");
                return 2;
            }

            try
            {
                var source = BluePrintDataGenerator.Generate(args[0]);
                var output = args[1];
                // only write on changes, so an unchanged file does not trigger a recompile
                if (!File.Exists(output) || File.ReadAllText(output).Replace("\r\n", "\n") != source)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
                    File.WriteAllText(output, source);
                    Console.WriteLine($"BluePrintGenerator: {output} written ({source.Length} chars)");
                }
                else
                {
                    Console.WriteLine($"BluePrintGenerator: {output} is up to date");
                }
                return 0;
            }
            catch (BluePrintDataException e)
            {
                // canonical MSBuild error format, clickable in Visual Studio
                Console.Error.WriteLine($"{e.Location}: error BP0001: {e.Message}");
                return 1;
            }
        }
    }
}
