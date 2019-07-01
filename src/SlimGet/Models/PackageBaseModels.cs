using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlimGet.Models
{
    public sealed class PackageVersionList
    {
        [JsonProperty("versions")]
        public IEnumerable<string> Versions { get; }

        public PackageVersionList(IEnumerable<string> versions)
        {
            this.Versions = versions;
        }
    }
}
