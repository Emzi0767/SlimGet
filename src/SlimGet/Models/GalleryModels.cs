// This file is a part of SlimGet project.
//
// Copyright 2019 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace SlimGet.Models
{
    public class GalleryAboutModel
    {
        public Uri NuGetFeedUrl { get; }
        public Uri SymbolsUrl { get; }
        public Uri SymbolsPushUrl { get; }
        public bool SymbolsEnabled { get; }
        public bool IsWritable { get; }

        public GalleryAboutModel(Uri feedUrl, Uri symbolsUrl, Uri symbolsPushUrl, bool symbolsEnabled, bool writble)
        {
            this.NuGetFeedUrl = feedUrl;
            this.SymbolsUrl = symbolsUrl;
            this.SymbolsPushUrl = symbolsPushUrl;
            this.SymbolsEnabled = symbolsEnabled;
            this.IsWritable = writble;
        }
    }

    public class GalleryIndexModel
    {
        public int PackageCount { get; }
        public int VersionCount { get; }

        public GalleryIndexModel(int pkgCount, int verCount)
        {
            this.PackageCount = pkgCount;
            this.VersionCount = verCount;
        }
    }
}
