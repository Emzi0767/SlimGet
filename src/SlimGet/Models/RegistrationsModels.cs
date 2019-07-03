using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlimGet.Models
{
    public sealed class RegistrationsLeafModel
    {
        [JsonProperty("@id", NullValueHandling = NullValueHandling.Include)]
        public string LeafUrl { get; set; }

        // CatalogEntry

        [JsonProperty("listed")]
        public bool IsListed { get; set; }

        [JsonProperty("packageContent", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentUrl { get; set; }

        [JsonIgnore]
        public DateTimeOffset PublishedAt { get; set; }
        [JsonProperty("published")]
        public string PublishedAtString => this.PublishedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

        [JsonProperty("registration", NullValueHandling = NullValueHandling.Ignore)]
        public string RegistrationIndexUrl { get; set; }
    }

    public sealed class RegistrationsPageModel
    {
        [JsonProperty("@id", NullValueHandling = NullValueHandling.Include)]
        public string PageUrl { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<RegistrationsLeafModel> Items { get; set; }

        [JsonProperty("lower", NullValueHandling = NullValueHandling.Include)]
        public string MinVersion { get; set; }

        [JsonProperty("upper", NullValueHandling = NullValueHandling.Include)]
        public string MaxVersion { get; set; }

        [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
        public string IndexUrl { get; set; }
    }
}
