using System;
using System.Collections.Generic;
using System.Linq;

namespace SlimGet.Data.Database
{
    public class Package
    {
        public string Id { get; set; }
        public string IdLowercase { get; set; }
        public string Description { get; set; }
        public long DownloadCount { get; set; }
        public bool HasReadme { get; set; }
        public bool IsPrerelase { get; set; }
        public string Language { get; set; }
        public bool IsListed { get; set; }
        public string MinimumClientVersion { get; set; }
        public DateTime? PublishedAt { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Summary { get; set; }
        public string Title { get; set; }
        public string IconUrl { get; set; }
        public string LicenseUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string RepositoryUrl { get; set; }
        public string RepositoryType { get; set; }
        public SemVerLevel SemVerLevel { get; set; }
        public string OwnerId { get; set; }
        
        public List<PackageVersion> Versions { get; set; }
        public List<PackageAuthor> Authors { get; set; }
        public List<PackageTag> Tags { get; set; }
        public User Owner { get; set; }

        public IEnumerable<string> AuthorNames => this.Authors.Select(x => x.Name);
        public IEnumerable<string> TagNames => this.Tags.Select(x => x.Tag);
    }
}
