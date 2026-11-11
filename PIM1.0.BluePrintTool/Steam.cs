using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace PimBluePrintTool
{
    public sealed class WorkshopDetails
    {
        public string Id;
        /// <summary>Steam result code: 1 = ok, 9 = not found (deleted), others: hidden/no access.</summary>
        public int Result;
        public string Title;
        public DateTime Updated;
        public long FileSize;
        public int Subscriptions;

        public bool Available => Result == 1;
    }

    /// <summary>Optional online functions. Only used on explicit request (status, --titles, download).</summary>
    public static class Steam
    {
        const string DetailsUrl = "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/";
        const int BatchSize = 100;   // the API accepts about 100 ids per request

        /// <summary>
        /// Details of workshop items via the public Steam Web API (no API key needed).
        /// Sends only the workshop ids.
        /// </summary>
        public static Dictionary<string, WorkshopDetails> FetchDetails(IList<string> workshopIds)
        {
            var result = new Dictionary<string, WorkshopDetails>();
            using (var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("PIM-BluePrintTool");
                var ids = workshopIds.Distinct().ToList();
                for (int start = 0; start < ids.Count; start += BatchSize)
                {
                    if (start > 0) Thread.Sleep(500);   // avoid rate limiting
                    var batch = ids.Skip(start).Take(BatchSize).ToList();
                    var form = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("itemcount", batch.Count.ToString()) };
                    for (int i = 0; i < batch.Count; i++)
                        form.Add(new KeyValuePair<string, string>($"publishedfileids[{i}]", batch[i]));

                    var response = http.PostAsync(DetailsUrl, new FormUrlEncodedContent(form)).GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    using (var doc = JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()))
                    {
                        foreach (var item in doc.RootElement.GetProperty("response").GetProperty("publishedfiledetails").EnumerateArray())
                        {
                            var details = new WorkshopDetails
                            {
                                Id = String(item, "publishedfileid"),
                                Result = (int)Number(item, "result"),
                                Title = String(item, "title"),
                                Updated = DateTimeOffset.FromUnixTimeSeconds(Number(item, "time_updated")).LocalDateTime,
                                FileSize = Number(item, "file_size"),
                                Subscriptions = (int)Number(item, "subscriptions"),
                            };
                            result[details.Id] = details;
                        }
                    }
                }
            }
            return result;
        }

        // the API returns numbers sometimes as strings
        static string String(JsonElement e, string name)
        {
            JsonElement p;
            if (!e.TryGetProperty(name, out p)) return "";
            return p.ValueKind == JsonValueKind.String ? p.GetString() : p.ValueKind == JsonValueKind.Number ? p.GetInt64().ToString() : "";
        }

        static long Number(JsonElement e, string name)
        {
            JsonElement p;
            long value;
            if (!e.TryGetProperty(name, out p)) return 0;
            if (p.ValueKind == JsonValueKind.Number) return p.GetInt64();
            return p.ValueKind == JsonValueKind.String && long.TryParse(p.GetString(), out value) ? value : 0;
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
