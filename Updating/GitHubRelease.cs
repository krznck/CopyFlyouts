namespace CopyFlyouts.UpdateInfrastructure
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// Simple class that maps to the GitHub release info JSON.
    /// Used for the <see cref="UpdateChecker"/>.
    /// </summary>
    public class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }
    }
}
