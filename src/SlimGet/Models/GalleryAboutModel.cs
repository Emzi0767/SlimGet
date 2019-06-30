using System;

namespace SlimGet.Models
{
    public class GalleryAboutModel
    {
        public Uri NuGetFeedUrl { get; }

        public GalleryAboutModel(Uri feedUrl)
        {
            this.NuGetFeedUrl = feedUrl;
        }
    }
}
