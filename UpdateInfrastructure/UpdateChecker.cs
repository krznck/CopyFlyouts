using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace copy_flyouts.UpdateInfrastructure
{
    public class UpdateChecker
    {
        private readonly HttpClient client = new HttpClient();
        // note - adding a token like this straight into source code is bad, but it will be fine so long as the repo is private.
        // by the time this repo is publicized, the token will be expired or deleted
        private readonly string personalAccessToken = "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";

        public async Task<GitHubRelease> GetLatestReleaseAsync()
        {
            try
            {
                var url = $"https://api.github.com/repos/krznck/copy-flyouts/releases/latest";
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", personalAccessToken);
                var response = await client.GetStringAsync(url);
                var release = JsonSerializer.Deserialize<GitHubRelease>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return release;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching release info: {ex.Message}");
                return null;
            }
        }
    }
}
