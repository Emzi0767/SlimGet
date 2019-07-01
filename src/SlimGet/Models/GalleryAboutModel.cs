using System;

namespace SlimGet.Models
{
    public class GalleryAboutModel
    {
        public Uri NuGetFeedUrl { get; }
        public Uri SymbolsUrl { get; }

        public GalleryAboutModel(Uri feedUrl, Uri symbolsUrl)
        {
            this.NuGetFeedUrl = feedUrl;
            this.SymbolsUrl = symbolsUrl;
        }
    }
}
