using System;

namespace SlimGet.Models
{
    public class GalleryAboutModel
    {
        public Uri NuGetFeedUrl { get; }
        public Uri SymbolsUrl { get; }
        public bool SymbolsEnabled { get; }

        public GalleryAboutModel(Uri feedUrl, Uri symbolsUrl, bool symbolsEnabled)
        {
            this.NuGetFeedUrl = feedUrl;
            this.SymbolsUrl = symbolsUrl;
            this.SymbolsEnabled = symbolsEnabled;
        }
    }
}
