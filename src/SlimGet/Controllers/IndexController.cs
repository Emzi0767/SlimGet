using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Models;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    [ApiController, AllowAnonymous]
    public sealed class IndexController : NuGetControllerBase
    {
        public IndexController(SlimGetContext db, RedisService redis, IOptions<StorageConfiguration> storcfg)
            : base(db, redis, storcfg)
        { }

        [Route("/api/v3/index.json"), HttpGet]
        public IActionResult Index()
        {
            var packagePublish = this.CreateServiceModels(this.Url.AbsoluteUrl("Push", "PackagePublish", this.HttpContext).ToUri(), "PackagePublish", "2.0.0");
            var symbolPackagePublish = this.CreateServiceModels(this.Url.AbsoluteUrl("Push", "SymbolPublish", this.HttpContext).ToUri(), "SymbolPackagePublish", "4.9.0");
            var searchQuery = this.CreateServiceModels(this.Url.AbsoluteUrl("Search", "Search", this.HttpContext).ToUri(), "SearchQueryService", "", "3.0.0-beta", "3.0.0-rc");
            var searchAutocomplete = this.CreateServiceModels(this.Url.AbsoluteUrl("Autocomplete", "Search", this.HttpContext).ToUri(), "SearchAutocompleteService", "", "3.0.0-beta", "3.0.0-rc");
            var registrationsBase = this.CreateServiceModels(this.Url.AbsoluteUrl("Dummy", "RegistrationsBase", this.HttpContext).ToUri(), "RegistrationsBase", "", "3.0.0-beta", "3.0.0-rc");
            var registrationsBaseGz = this.CreateServiceModels(this.Url.AbsoluteUrl("DummyGz", "RegistrationsBase", this.HttpContext).ToUri(), "RegistrationsBase", "3.4.0");
            var registrationsBaseSemVer2 = this.CreateServiceModels(this.Url.AbsoluteUrl("DummySemVer", "RegistrationsBase", this.HttpContext).ToUri(), "RegistrationsBase", "3.6.0");
            var packageDetailsUriTemplate = this.CreateServiceModels(this.Url.AbsoluteUrl("Packages", "Gallery", this.HttpContext, new { id = "{id}", version = "{version}" }).Replace("%7B", "{").Replace("%7D", "}").ToUri(), "PackageDetailsUriTemplate", "5.1.0");
            var packageBaseAddress = this.CreateServiceModels(this.Url.AbsoluteUrl("Dummy", "PackageBase", this.HttpContext).ToUri(), "PackageBaseAddress", "3.0.0");

            var services = packagePublish
                .Concat(symbolPackagePublish)
                .Concat(searchQuery)
                .Concat(searchAutocomplete)
                .Concat(registrationsBase)
                .Concat(registrationsBaseGz)
                .Concat(registrationsBaseSemVer2)
                .Concat(packageDetailsUriTemplate)
                .Concat(packageBaseAddress);

            var index = new FeedIndexModel("3.0.0", services);
            return this.Json(index);
        }

        private IEnumerable<FeedServiceModel> CreateServiceModels(Uri baseUrl, string type, params string[] versions)
        {
            foreach (var version in versions)
            {
                var xtype = !string.IsNullOrWhiteSpace(type) ? $"{type}/{version}" : type;
                yield return new FeedServiceModel(baseUrl, xtype, null);
            }
        }
    }
}
