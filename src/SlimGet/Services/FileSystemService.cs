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
        private FileSystemConfiguration Configuration { get; }
        private DirectoryInfo StorageRoot { get; }
        private DirectoryInfo PackageRoot { get; }
        private DirectoryInfo Temporary { get; }
        private ILogger<FileSystemService> Logger { get; }

        public FileSystemService(IOptions<StorageConfiguration> sconf, ILoggerFactory logger)
        {
            this.Configuration = sconf.Value.FileSystem;
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

        public Stream CreateTemporaryFile()
        {
            var guid = Guid.NewGuid();
            var fname = $"{guid:D}.tmp";

            var fi = new FileInfo(Path.Combine(this.Temporary.FullName, fname));
            this.Logger.LogInformation("Creating temporary file '{0}'", fi.FullName);

            return File.Create(fi.FullName, 4096, FileOptions.DeleteOnClose | FileOptions.Asynchronous);
        }

        public Stream OpenPackageRead(PackageInfo package)
        {
            var pkgPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToString(), "package.nupkg");
            var fi = new FileInfo(pkgPath);
            if (fi.Exists)
            {
                this.Logger.LogInformation("Opened package for reading: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);
                return fi.OpenRead();
            }

            this.Logger.LogError("Requested nonexistent package for writing: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);
            throw new FileNotFoundException();
        }

        public Stream OpenPackageWrite(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
                pkgDir.Create();

            var fi = new FileInfo(Path.Combine(pkgDir.FullName, "package.nupkg"));
            this.Logger.LogInformation("Opened package for writing: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);

            return fi.Open(FileMode.Create, FileAccess.Write);
        }

        public Stream OpenManifestRead(PackageInfo package)
        {
            var pkgPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToString(), "manifest.nuspec");
            var fi = new FileInfo(pkgPath);
            if (fi.Exists)
            {
                this.Logger.LogInformation("Opened manifest for reading: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);
                return fi.OpenRead();
            }

            this.Logger.LogError("Requested nonexistent manifest for writing: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);
            throw new FileNotFoundException();
        }

        public Stream OpenManifestWrite(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
                pkgDir.Create();

            var fi = new FileInfo(Path.Combine(pkgDir.FullName, "manifest.nuspec"));
            this.Logger.LogInformation("Opened manifest for writing: {0}/{1} ('{2}')", package.Id, package.Version, fi.FullName);

            return fi.Open(FileMode.Create, FileAccess.Write);
        }

        public bool DeleteWholePackage(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToString());
            var pkgDir = new DirectoryInfo(pkgDirPath);
            if (!pkgDir.Exists)
                return false;

            var pkg = new FileInfo(Path.Combine(pkgDir.FullName, "package.nupkg"));
            var spec = new FileInfo(Path.Combine(pkgDir.FullName, "manifest.nuspec"));
            if (!(pkg.Exists && spec.Exists))
            {
                this.Logger.LogWarning("Requested deletion of {0}/{1} but package and manifest were missing, pruning '{2}' instead", package.Id, package.Version, pkgDir.FullName);
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
            pkgDir.Delete(true);
            this.Logger.LogInformation("Successfully deleted {0}/{1} from '{2}'", package.Id, package.Version, pkgDir.FullName);

            return true;
        }

        public bool DeletePackage(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToString());
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
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToString());
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

        public bool HasPackage(PackageInfo package)
        {
            var pkgDirPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToString());
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
            var pkgPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToString(), "package.nupkg");
            var pkg = new FileInfo(pkgPath);
            var vid = pkg.MakeRelativeTo(this.PackageRoot);

            this.Logger.LogInformation("Virtual identifier for {0}/{1} package is '{2}'", package.Id, package.Version, vid);
            return vid;
        }

        public string GetManifestFileName(PackageInfo package)
        {
            var pkgPath = Path.Combine(this.PackageRoot.FullName, package.Id.ToLowerInvariant(), package.Version.ToString(), "manifest.nuspec");
            var pkg = new FileInfo(pkgPath);
            var vid = pkg.MakeRelativeTo(this.PackageRoot);

            this.Logger.LogInformation("Virtual identifier for {0}/{1} manifest is '{2}'", package.Id, package.Version, vid);
            return vid;
        }
    }
}
