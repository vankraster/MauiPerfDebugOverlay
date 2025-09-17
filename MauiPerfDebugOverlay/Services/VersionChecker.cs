using System.Text.Json;

namespace MauiPerfDebugOverlay.Services
{ 
    public class VersionChecker
    {
        public static string NewVersionLabelText = string.Empty;
        private const string NugetUrl = "https://api.nuget.org/v3-flatcontainer/performancedebugoverlay/index.json";

        public static string? GetLatestNugetVersionForMajor(int major)
        {
            try
            {
                using var client = new HttpClient();
                var response = client.GetStringAsync(NugetUrl).Result;

                using var doc = JsonDocument.Parse(response);
                if (doc.RootElement.TryGetProperty("versions", out var versions))
                {
                    var filtered = versions.EnumerateArray()
                                           .Select(v => v.GetString())
                                           .Where(v => !string.IsNullOrEmpty(v))
                                           .Where(v => Version.TryParse(v, out var ver) && ver.Major == major)
                                           .Select(v => Version.Parse(v))
                                           .OrderBy(v => v)
                                           .ToList();

                    return filtered.Count > 0 ? filtered.Last().ToString() : null;
                }
            }
            catch
            {
                // ignorăm erorile
            }
            return null;
        }


        public static bool IsNewerVersionAvailable(string currentVersion, string latestVersion)
        {
            if (string.IsNullOrEmpty(latestVersion))
                return false;

            try
            {
                var current = new Version(currentVersion);
                var latest = new Version(latestVersion);
                return latest > current;
            }
            catch
            {
                return false;
            }
        }
    }

}
