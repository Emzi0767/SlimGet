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

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data.Configuration;
using SlimGet.Services;

namespace SlimGet.Controllers
{
    public abstract class NuGetControllerBase : ControllerBase
    {
        protected SlimGetContext Database { get; }
        protected RedisService Redis { get; }
        protected IFileSystemService FileSystem { get; }
        protected BlobStorageConfiguration FileSystemConfiguration { get; }
        protected PackageStorageConfiguration PackageStorageConfiguration { get; }
        protected ILogger<NuGetControllerBase> Logger { get; }

        public NuGetControllerBase(
            SlimGetContext db,
            RedisService redis,
            IFileSystemService fs,
            IOptions<BlobStorageConfiguration> blobstoreOpts,
            IOptions<PackageStorageConfiguration> packageOpts,
            ILoggerFactory logger)
        {
            this.Database = db;
            this.Redis = redis;
            this.FileSystem = fs;
            this.FileSystemConfiguration = blobstoreOpts.Value;
            this.PackageStorageConfiguration = packageOpts.Value;
            this.Logger = logger.CreateLogger<NuGetControllerBase>();
        }
    }
}
