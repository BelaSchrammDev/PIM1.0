using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PimBluePrintTool
{
    /// <summary>
    /// Locations of the game data and the workshop mods.
    /// Found via the Steam installation (registry + libraryfolders.vdf), can be overridden by options.
    /// </summary>
    public sealed class GamePaths
    {
        public const string AppId = "244850";

        /// <summary>...\SpaceEngineers\Content\Data</summary>
        public string GameData;

        /// <summary>Directories that contain one folder per workshop id (Steam libraries, steamcmd).</summary>
        public readonly List<string> WorkshopRoots = new List<string>();

        /// <summary>Data folder of a workshop item, null if it is not downloaded.</summary>
        public string FindMod(string workshopId)
        {
            foreach (var root in WorkshopRoots)
            {
                var dir = Path.Combine(root, workshopId);
                if (Directory.Exists(dir)) return dir;
            }
            return null;
        }

        public IEnumerable<string> AllInstalledMods() =>
            WorkshopRoots.Where(Directory.Exists)
                .SelectMany(Directory.GetDirectories)
                .Where(d => Path.GetFileName(d).All(char.IsDigit))
                .GroupBy(Path.GetFileName).Select(g => g.First());

        public static GamePaths Detect(string gameDir, IEnumerable<string> extraWorkshopRoots, string steamCmdDir)
        {
            var paths = new GamePaths();
            var libraries = SteamLibraries();

            if (gameDir == null)
                gameDir = libraries.Select(l => Path.Combine(l, "steamapps", "common", "SpaceEngineers"))
                                   .FirstOrDefault(Directory.Exists);
            if (gameDir != null)
                paths.GameData = Directory.Exists(Path.Combine(gameDir, "Content", "Data"))
                    ? Path.Combine(gameDir, "Content", "Data")
                    : gameDir;   // the Data folder itself was given

            foreach (var lib in libraries)
                paths.WorkshopRoots.Add(Path.Combine(lib, "steamapps", "workshop", "content", AppId));
            if (steamCmdDir != null)
                paths.WorkshopRoots.Add(Path.Combine(steamCmdDir, "steamapps", "workshop", "content", AppId));
            paths.WorkshopRoots.AddRange(extraWorkshopRoots);

            return paths;
        }

        /// <summary>Steam library folders (the Steam folder itself first).</summary>
        static List<string> SteamLibraries()
        {
            var result = new List<string>();
            var steam = SteamPath();
            if (steam == null) return result;
            result.Add(steam);

            var vdf = Path.Combine(steam, "steamapps", "libraryfolders.vdf");
            if (File.Exists(vdf))
            {
                foreach (Match m in Regex.Matches(File.ReadAllText(vdf), "\"path\"\\s+\"([^\"]+)\""))
                {
                    var lib = m.Groups[1].Value.Replace("\\\\", "\\");
                    if (!result.Any(r => string.Equals(Path.GetFullPath(r), Path.GetFullPath(lib), StringComparison.OrdinalIgnoreCase)))
                        result.Add(lib);
                }
            }
            return result;
        }

        static string SteamPath()
        {
            if (OperatingSystem.IsWindows())
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    var path = key?.GetValue("SteamPath") as string;
                    if (path != null && Directory.Exists(path)) return path.Replace('/', '\\');
                }
                var defaultPath = @"C:\Program Files (x86)\Steam";
                return Directory.Exists(defaultPath) ? defaultPath : null;
            }
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var linux = Path.Combine(home, ".steam", "steam");
            return Directory.Exists(linux) ? linux : null;
        }
    }
}
