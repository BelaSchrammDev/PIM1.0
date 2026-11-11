using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace PimBluePrintTool
{
    /// <summary>Optional online functions. Only used on explicit request (--titles, download).</summary>
    public static class Steam
    {
        /// <summary>
        /// Titles of workshop items via the public Steam Web API (no API key needed).
        /// Sends only the workshop ids.
        /// </summary>
        public static Dictionary<string, string> FetchTitles(IList<string> workshopIds)
        {
            var result = new Dictionary<string, string>();
            if (workshopIds.Count == 0) return result;

            var form = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("itemcount", workshopIds.Count.ToString()) };
            for (int i = 0; i < workshopIds.Count; i++)
                form.Add(new KeyValuePair<string, string>($"publishedfileids[{i}]", workshopIds[i]));

            using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("PIM-BluePrintTool");
                var response = http.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/",
                    new FormUrlEncodedContent(form)).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                using (var doc = JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()))
                {
                    foreach (var item in doc.RootElement.GetProperty("response").GetProperty("publishedfiledetails").EnumerateArray())
                    {
                        JsonElement title;
                        if (item.TryGetProperty("title", out title))
                            result[item.GetProperty("publishedfileid").GetString()] = title.GetString();
                    }
                }
            }
            return result;
        }

        /// <summary>steamcmd executable: given path, or found in PATH.</summary>
        public static string FindSteamCmd(string option)
        {
            if (option != null)
            {
                if (File.Exists(option)) return Path.GetFullPath(option);
                foreach (var exe in new[] { "steamcmd.exe", "steamcmd.sh", "steamcmd" })
                    if (File.Exists(Path.Combine(option, exe))) return Path.GetFullPath(Path.Combine(option, exe));
                return null;
            }
            foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
                foreach (var exe in new[] { "steamcmd.exe", "steamcmd.sh", "steamcmd" })
                {
                    try { if (File.Exists(Path.Combine(dir, exe))) return Path.Combine(dir, exe); }
                    catch (ArgumentException) { }
                }
            return null;
        }

        /// <summary>
        /// Downloads workshop items with steamcmd (anonymous login). The files end up in
        /// &lt;steamcmd&gt;/steamapps/workshop/content/244850/&lt;id&gt;, which the tool searches too.
        /// </summary>
        public static int Download(string steamCmd, IEnumerable<string> workshopIds)
        {
            var args = "+login anonymous " + string.Concat(workshopIds.Select(id => $"+workshop_download_item {GamePaths.AppId} {id} ")) + "+quit";
            Console.WriteLine($"{steamCmd} {args}");
            using (var process = Process.Start(new ProcessStartInfo(steamCmd, args) { UseShellExecute = false }))
            {
                process.WaitForExit();
                return process.ExitCode;
            }
        }
    }
}
