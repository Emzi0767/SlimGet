using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlimGet.Models
{
    public sealed class SearchResponseModel
    {
        [JsonProperty("totalHits", NullValueHandling = NullValueHandling.Include)]
        public int TotalResultCount { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Include)]
        public IEnumerable<SearchResultModel> ResultPage { get; set; }
    }

    public sealed class SearchResultModel
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Include)]
        public string Id { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Include)]
        public string Version { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("versions", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<SearchVersionModel> Versions { get; set; }

        [JsonProperty("authors", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Authors { get; set; }

        [JsonProperty("iconUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string IconUrl { get; set; }

        [JsonProperty("licenseUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string LicenseUrl { get; set; }

        [JsonProperty("owners", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Owners { get; set; }

        [JsonProperty("projectUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string ProjectUrl { get; set; }

        [JsonProperty("registration", NullValueHandling = NullValueHandling.Ignore)]
        public string RegistrationUrl { get; set; }

        [JsonProperty("summary", NullValueHandling = NullValueHandling.Ignore)]
        public string Summary { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Tags { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("totalDownloads", NullValueHandling = NullValueHandling.Ignore)]
        public long DownloadCount { get; set; }

        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsVerified { get; set; } = false;
    }

    public sealed class SearchVersionModel
    {
        [JsonProperty("@id", NullValueHandling = NullValueHandling.Include)]
        public string RegistrationLeafUrl { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty("downloads", NullValueHandling = NullValueHandling.Ignore)]
        public long DownloadCount { get; set; }
    }
}
