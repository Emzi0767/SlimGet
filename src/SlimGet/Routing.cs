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
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace SlimGet
{
    public static class Routing
    {
        public const string InheritRoute = "";

        // BEGIN: NuGet API
        public const string FeedIndexRouteName = "NuGet,FeedIndex";
        public const string FeedIndexRoute = "/api/v3/index.json";

        public const string PublishPackageRouteName = "NuGet,PackagePublish";
        public const string PublishPackageRoute = "/api/v2/package";
        public const string PublishPackageModifyRouteName = "NuGet,PackagePublish,Modify";
        public const string PublishPackageModifyRoute = "{id}/{version}";

        public const string PublishSymbolsRouteName = "NuGet,SymbolsPublish";
        public const string PublishSymbolsRoute = "/api/v2/symbolpackage";

        public const string DownloadPackageRouteName = "NuGet,PackageBase";
        public const string DownloadPackageRoute = "/api/v3/flatcontainer";
        public const string DownloadPackageIndexRouteName = "NuGet,PackageBase,VersionIndex";
        public const string DownloadPackageIndexRoute = "{id}/index.json";
        public const string DownloadPackageContentsRouteName = "NuGet,PackageBase,Contents";
        public const string DownloadPackageContentsRoute = "{id}/{version}/{filename}.nupkg";
        public const string DownloadPackageManifestRouteName = "NuGet,PackageBase,Manifest";
        public const string DownloadPackageManifestRoute = "{id}/{version}/{id2}.nuspec";

        public const string DownloadSymbolsRouteName = "NuGet,SymbolBase";
        public const string DownloadSymbolsRoute = "/api/v3/symbolstore";
        public const string DownloadSymbols1StageIndexRouteName = "NuGet,SymbolBase,1StageIndex";
        public const string DownloadSymbols1StageIndexRoute = "pingme.txt";
        public const string DownloadSymbols2StageIndexRouteName = "NuGet,SymbolBase,2StageIndex";
        public const string DownloadSymbols2StageIndexRoute = "index2.txt";
        public const string DownloadSymbols1StageRouteName = "NuGet,SymbolBase,1Stage";
        public const string DownloadSymbols1StageRoute = "{file}/{sig}/{file2}";
        public const string DownloadSymbols2StageRouteName = "NuGet,SymbolBase,2Stage";
        public const string DownloadSymbols2StageRoute = "{t2prefix}/{file}/{sig}/{file2}";

        public const string RegistrationsRouteName = "NuGet,Registrations";
        public const string RegistrationsRoute = "/api/v3/registration/{mode}";
        public const string RegistrationsIndexRouteName = "NuGet,Registrations,Index";
        public const string RegistrationsIndexRoute = "{id}/index.json";
        public const string RegistrationsPageRouteName = "NuGet,Registrations,Page";
        public const string RegistrationsPageRoute = "{id}/page/{minVersion}/{maxVersion}.json";
        public const string RegistrationsLeafRouteName = "NuGet,Registrations,Leaf";
        public const string RegistrationsLeafRoute = "{id}/{version}.json";

        public const string SearchRouteName = "NuGet,Search";
        public const string SearchRoute = "/api/v3/search";
        public const string SearchQueryRouteName = "NuGet,Search,Query";
        public const string SearchQueryRoute = "query";
        public const string SearchAutocompleteRouteName = "NuGet,Search,Autocomplete";
        public const string SearchAutocompleteRoute = "autocomplete";
        // ENDOF: NuGet API

        // BEGIN: Gallery
        public const string GalleryRouteName = "Web,Gallery";
        public const string GalleryRoute = "/gallery";
        public const string GalleryIndexRouteName = "Web,Gallery,Index";
        public const string GalleryIndexRoute = "/";
        public const string GalleryPackageIndexRouteName = "Web,Gallery,PackageIndex";
        public const string GalleryPackageIndexRoute = "packages";
        public const string GalleryPackageDetailsRouteName = "Web,Gallery,PackageDetails";
        public const string GalleryPackageDetailsRoute = "packages/{id}";
        public const string GalleryPackageVersionRouteName = "Web,Gallery,PackageVersion";
        public const string GalleryPackageVersionRoute = "packages/{id}/{version}";
        public const string GalleryAboutRouteName = "Web,Gallery,About";
        public const string GalleryAboutRoute = "about";
        // ENDOF: Gallery

        // BEGIN: Misc API
        public const string DevelopmentRouteName = "Misc,Development";
        public const string DevelopmentRoute = "/api/dev";
        public const string DevelopmentTokenRouteName = "Misc,Development,Token";
        public const string DevelopmentTokenRoute = "token/issue/{username?}/{email?}";
        public const string DevelopmentWhoamiRouteName = "Misc,Development,WhoAmI";
        public const string DevelopmentWhoamiRoute = "whoami";
        public const string DevelopmentEvalRouteName = "Misc,Development,Eval";
        public const string DevelopmentEvalRoute = "eval";
        public const string DevelopmentUrlPerComponentsRouteName = "Misc,Development,Url,PerComponents";
        public const string DevelopmentUrlPerComponentsRoute = "genurl/components/{genController}/{genAction}";
        public const string DevelopmentUrlPerNameRouteName = "Misc,Development,Url,PerName";
        public const string DevelopmentUrlPerNameRoute = "genurl/name/{routeName}";

        public const string MiscApiRouteName = "Misc,API";
        public const string MiscApiRoute = "/api/misc";
        public const string MiscRevokeTokenRouteName = "Misc,API,RevokeToken";
        public const string MiscRevokeTokenRoute = "token/revoke/{token}";
        public const string MiscErrorRouteName = "Misc,API,Error";
        public const string MiscErrorRoute = "error";
        // ENDOF: Misc API

        // Name to route mapping
        internal static IReadOnlyDictionary<string, string> RouteMap { get; } = new Dictionary<string, string>()
        {
            // BEGIN: NuGet API
            [FeedIndexRouteName] = FeedIndexRoute,

            [PublishPackageRouteName] = PublishPackageRoute,
            [PublishPackageModifyRouteName] = PublishPackageModifyRoute,

            [PublishSymbolsRouteName] = PublishSymbolsRoute,

            [DownloadPackageRouteName] = DownloadPackageRoute,
            [DownloadPackageIndexRouteName] = DownloadPackageIndexRoute,
            [DownloadPackageContentsRouteName] = DownloadPackageContentsRoute,
            [DownloadPackageManifestRouteName] = DownloadPackageManifestRoute,

            [DownloadSymbolsRouteName] = DownloadSymbolsRoute,
            [DownloadSymbols1StageIndexRouteName] = DownloadSymbols1StageIndexRoute,
            [DownloadSymbols1StageRouteName] = DownloadSymbols1StageRoute,
            [DownloadSymbols2StageIndexRouteName] = DownloadSymbols2StageIndexRoute,
            [DownloadSymbols2StageRouteName] = DownloadSymbols2StageRoute,

            [RegistrationsRouteName] = RegistrationsRoute,
            [RegistrationsIndexRouteName] = RegistrationsIndexRoute,
            [RegistrationsPageRouteName] = RegistrationsPageRoute,
            [RegistrationsLeafRouteName] = RegistrationsLeafRoute,

            [SearchRouteName] = SearchRoute,
            [SearchQueryRouteName] = SearchQueryRoute,
            [SearchAutocompleteRouteName] = SearchAutocompleteRoute,
            // ENDOF: NuGet API

            // BEGIN: Gallery
            [GalleryRouteName] = GalleryRoute,
            [GalleryIndexRouteName] = GalleryIndexRoute,
            [GalleryPackageIndexRouteName] = GalleryPackageIndexRoute,
            [GalleryPackageDetailsRouteName] = GalleryPackageDetailsRoute,
            [GalleryPackageVersionRouteName] = GalleryPackageVersionRoute,
            [GalleryAboutRouteName] = GalleryAboutRoute,
            // ENDOF: Gallery

            // BEGIN: Misc API
            [DevelopmentRouteName] = DevelopmentRoute,
            [DevelopmentTokenRouteName] = DevelopmentTokenRoute,
            [DevelopmentWhoamiRouteName] = DevelopmentWhoamiRoute,
            [DevelopmentEvalRouteName] = DevelopmentEvalRoute,
            [DevelopmentUrlPerComponentsRouteName] = DevelopmentUrlPerComponentsRoute,
            [DevelopmentUrlPerNameRouteName] = DevelopmentUrlPerNameRoute,

            [MiscApiRouteName] = MiscApiRoute,
            [MiscRevokeTokenRouteName] = MiscRevokeTokenRoute,
            [MiscErrorRouteName] = MiscErrorRoute
            // ENDOF: Misc API
        };
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SlimGetRouteAttribute : RouteAttribute
    {
        public SlimGetRouteAttribute(string routeName)
            : base(GetRouteTemplate(routeName))
        {
            if (!string.IsNullOrWhiteSpace(routeName))
                this.Name = routeName;
        }

        private static string GetRouteTemplate(string routeName)
            => !string.IsNullOrWhiteSpace(routeName) && Routing.RouteMap.TryGetValue(routeName, out var routeTemplate)
                  ? routeTemplate
                  : routeName;
    }
}
