using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlimGet.Models
{
    public sealed class FeedServiceModel
    {
        /// <summary>
        /// Gets the base URL of the resource.
        /// </summary>
        [JsonProperty("@id")]
        public Uri BaseUrl { get; }

        /// <summary>
        /// Gets the type of the resource.
        /// </summary>
        [JsonProperty("@type")]
        public string Type { get; }

        /// <summary>
        /// Gets the human-readable comment for the resource.
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get; }

        public FeedServiceModel(Uri baseUrl, string type, string comment)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentNullException(nameof(type), "Service type cannot be null or empty.");

            this.BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl), "Service base URL cannot be null.");
            this.Type = type;
            this.Comment = !string.IsNullOrWhiteSpace(comment) ? comment : string.Empty;
        }
    }

    public sealed class FeedIndexModel
    {
        /// <summary>
        /// Gets the version of the feed index.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; }

        /// <summary>
        /// Gets the services available in the feed.
        /// </summary>
        [JsonProperty("services")]
        public IEnumerable<FeedServiceModel> Services { get; }

        // @context

        public FeedIndexModel(string version, IEnumerable<FeedServiceModel> services)
        {
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentNullException(nameof(version), "Version cannot be null or empty.");

            this.Version = version;
            this.Services = services ?? throw new ArgumentException(nameof(services), "Service collection cannot be null.");
        }
    }
}
