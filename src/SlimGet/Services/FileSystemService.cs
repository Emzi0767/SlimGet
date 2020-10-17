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
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimGet.Data;
using SlimGet.Data.Configuration;

namespace SlimGet.Services
{
    internal sealed class FileSystemService : IFileSystemService
    {
        private BlobStorageConfiguration Configuration { get; }
        private DirectoryInfo StorageRoot { get; }
        private DirectoryInfo PackageRoot { get; }
        private DirectoryInfo Temporary { get; }
        private ILogger<FileSystemService> Logger { get; }

        public FileSystemService(IOptions<BlobStorageConfiguration> blobstoreOpts, ILoggerFactory logger)
        {
            this.Configuration = blobstoreOpts.Value;
            this.Logger = logger.CreateLogger<FileSystemService>();

            this.StorageRoot = new DirectoryInfo(this.Configuration.StoragePath);
            this.PackageRoot = new DirectoryInfo(Path.Combine(this.StorageRoot.FullName, "feed"));
            this.Temporary = new DirectoryInfo(Path.Combine(this.StorageRoot.FullName, "temp"));

            if (!this.StorageRoot.Exists)
            {
                this.Logger.LogWarning("Provided package storage location '{0}' does not exist, it will be created", this.StorageRoot.FullName);
                this.StorageRoot.Create();
            }

            if (!this.PackageRoot.Exists)
            {
                this.PackageRoot.Create();
                this.Logger.LogInformation("Created package root at '{0}'", this.PackageRoot.FullName);
            }

            if (!this.Temporary.Exists)
            {
                this.Temporary.Create();
                this.Logger.LogInformation("Created temporary at '{0}'", this.Temporary.FullName);
            }
        }

        public Stream CreateTemporaryFile(TemporaryFileExtension tmpext)
        {
            var guid = Guid.NewGuid();
            var fname = $"{guid:D}.{ExtensionToString(tmpext)}";

            var fi = new FileInfo(Path.Combine(this.Temporary.FullName, fname));
            this.Logger.LogInformation("Creating temporary file '{0}'", fi.FullName);

            return File.Create(fi.FullName, 4096, FileOptions.DeleteOnClose | FileOptions.Asynchronous);
        }

        public Stream OpenPackageRead(PackageInfo package)
        {
            var pkgPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString(), "package.nupkg");
            var fi = new FileInfo(pkgPath);
            if (fi.Exists)
            {
                this.Logger.LogInformation("Opened package for reading: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);
                return fi.OpenRead();
            }

            this.Logger.LogError("Requested nonexistent package for reading: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);
            throw new FileNotFoundException();
        }

        public Stream OpenPackageWrite(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
                pkgDir.Create();

            var fi = new FileInfo(Path.Combine(pkgDir.FullName, "package.nupkg"));
            this.Logger.LogInformation("Opened package for writing: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);

            return fi.Open(FileMode.Create, FileAccess.Write);
        }

        public Stream OpenManifestRead(PackageInfo package)
        {
            var pkgPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString(), "manifest.nuspec");
            var fi = new FileInfo(pkgPath);
            if (fi.Exists)
            {
                this.Logger.LogInformation("Opened manifest for reading: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);
                return fi.OpenRead();
            }

            this.Logger.LogError("Requested nonexistent manifest for reading: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);
            throw new FileNotFoundException();
        }

        public Stream OpenManifestWrite(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
                pkgDir.Create();

            var fi = new FileInfo(Path.Combine(pkgDir.FullName, "manifest.nuspec"));
            this.Logger.LogInformation("Opened manifest for writing: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);

            return fi.Open(FileMode.Create, FileAccess.Write);
        }

        public Stream OpenSymbolsRead(PackageInfo package, Guid identifier, int age)
        {
            var pkgPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString(), $"symbols-{identifier:N}-{age:x}.pdb");
            var fi = new FileInfo(pkgPath);
            if (fi.Exists)
            {
                this.Logger.LogInformation("Opened symbols for reading: {0}/{1}/{3:D}/{4:x} ('{2}')", package.Id, package.Version, fi.FullName, identifier, age);
                return fi.OpenRead();
            }

            this.Logger.LogError("Requested nonexistent symbols for reading: {0}/{1}/{3}/{4:x} ('{2}')", package.Id, package.Version, fi.FullName, identifier, age);
            throw new FileNotFoundException();
        }

        public Stream OpenSymbolsWrite(PackageInfo package, Guid identifier, int age)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
                pkgDir.Create();

            var fi = new FileInfo(Path.Combine(pkgDir.FullName, $"symbols-{identifier:N}-{age:x}.pdb"));
            this.Logger.LogInformation("Opened manifest for writing: {0}/{1}/{3:D}/{4} ('{2}')", package.Id, package.Version, fi.FullName, identifier, age);

            return fi.Open(FileMode.Create, FileAccess.Write);
        }

        public bool DeleteWholePackage(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
                return false;

            var pkg = new FileInfo(Path.Combine(pkgDir.FullName, "package.nupkg"));
            var spec = new FileInfo(Path.Combine(pkgDir.FullName, "manifest.nuspec"));
            var pdbs = pkgDir.GetFiles("*.pdb", SearchOption.TopDirectoryOnly);
            if (!(pkg.Exists && spec.Exists))
            {
                this.Logger.LogWarning("Requested deletion of {0}/{1} but package and manifest were missing, pruning '{2}' instead", package.Id, package.Version, pkgDir.FullName);
                foreach (var pdb in pdbs) pdb.Delete();
                pkgDir.Delete(true);
                return true;
            }

            if (!pkg.Exists || !spec.Exists)
            {
                this.Logger.LogWarning("Requested deletion of {0}/{1} but package or manifest were missing", package.Id, package.Version);
                return false;
            }

            pkg.Delete();
            spec.Delete();
            foreach (var pdb in pdbs) pdb.Delete();
            pkgDir.Delete(true);
            this.Logger.LogInformation("Successfully deleted {0}/{1} from '{2}'", package.Id, package.Version, pkgDir.FullName);

            return true;
        }

        public bool DeletePackage(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
            {
                this.Logger.LogWarning("Requested deletion of package for nonexistent package {0}/{1}", package.Id, package.Version);
                return false;
            }

            var fi = new FileInfo(Path.Combine(pkgDir.FullName, "package.nupkg"));
            if (!fi.Exists)
            {
                this.Logger.LogWarning("Requested deletion of package for {0}/{1} but it was missing", package.Id, package.Version);
                return false;
            }

            fi.Delete();
            this.Logger.LogInformation("Successfully deleted package for {0}/{1}", package.Id, package.Version);

            return true;
        }

        public bool DeleteManifest(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
            {
                this.Logger.LogWarning("Requested deletion of manifest for nonexistent package {0}/{1}", package.Id, package.Version);
                return false;
            }

            var fi = new FileInfo(Path.Combine(pkgDir.FullName, "manifest.nuspec"));
            if (!fi.Exists)
            {
                this.Logger.LogWarning("Requested deletion of manifest for {0}/{1} but it was missing", package.Id, package.Version);
                return false;
            }

            fi.Delete();
            this.Logger.LogInformation("Successfully deleted manifest for {0}/{1}", package.Id, package.Version);
            return true;
        }

        public bool DeleteSymbols(PackageInfo package, Guid identifier, int age)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
            {
                this.Logger.LogWarning("Requested deletion of {2:D}/{3:x} symbols for nonexistent package {0}/{1}", package.Id, package.Version, identifier, age);
                return false;
            }

            var fi = new FileInfo(Path.Combine(pkgDir.FullName, $"symbols-{identifier:N}-{age:x}.pdb"));
            if (!fi.Exists)
            {
                this.Logger.LogWarning("Requested deletion of {2:D}{3:x} symbols for {0}/{1} but they were missing", package.Id, package.Version, identifier, age);
                return false;
            }

            fi.Delete();
            this.Logger.LogInformation("Successfully deleted {2:D}/{3:x} symbols for {0}/{1}", package.Id, package.Version, identifier, age);
            return true;
        }

        public bool HasPackage(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
            {
                this.Logger.LogInformation("Query for {0}/{1} failed (nonexistent package)", package.Id, package.Version);
                return false;
            }

            var pkg = new FileInfo(Path.Combine(pkgDir.FullName, "package.nupkg"));
            var spec = new FileInfo(Path.Combine(pkgDir.FullName, "manifest.nuspec"));

            if (!pkg.Exists)
            {
                this.Logger.LogInformation("Query for {0}/{1} failed (missing package)", package.Id, package.Version);
                return false;
            }
            if (!spec.Exists)
            {
                this.Logger.LogInformation("Query for {0}/{1} failed (missing manifest)", package.Id, package.Version);
                return false;
            }

            this.Logger.LogInformation("Query for {0}/{1} succeeded", package.Id, package.Version);
            return true;
        }

        public string GetPackageFileName(PackageInfo package)
        {
            var pkgPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString(), "package.nupkg");
            var pkg = new FileInfo(pkgPath);
            var vid = pkg.MakeRelativeTo(this.PackageRoot);

            this.Logger.LogInformation("Virtual identifier for {0}/{1} package is '{2}'", package.Id, package.Version, vid);
            return vid;
        }

        public string GetManifestFileName(PackageInfo package)
        {
            var pkgPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString(), "manifest.nuspec");
            var pkg = new FileInfo(pkgPath);
            var vid = pkg.MakeRelativeTo(this.PackageRoot);

            this.Logger.LogInformation("Virtual identifier for {0}/{1} manifest is '{2}'", package.Id, package.Version, vid);
            return vid;
        }

        public string GetSymbolsFileName(PackageInfo package, Guid identifier, int age)
        {
            var pkgPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToNormalizedString(), $"symbols-{identifier:N}-{age:x}.pdb");
            var pkg = new FileInfo(pkgPath);
            var vid = pkg.MakeRelativeTo(this.PackageRoot);

            this.Logger.LogInformation("Virtual identifier for {0}/{1}/{3:D}/{4:x} symbols is '{2}'", package.Id, package.Version, vid, identifier, age);
            return vid;
        }

        private static string ExtensionToString(TemporaryFileExtension ext)
        {
            switch (ext)
            {
                case TemporaryFileExtension.Nupkg:
                    return "nupkg";

                case TemporaryFileExtension.Nuspec:
                    return "nuspec";

                case TemporaryFileExtension.Pdb:
                    return "pdb";
            }

            return "tmp";
        }
    }
}
