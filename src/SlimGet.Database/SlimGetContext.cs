using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SlimGet.Data.Database;

namespace SlimGet.Services
{
    public sealed class SlimGetContext : DbContext
    {
        private string ConnectionString { get; }

        public DbSet<User> Users { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<PackageAuthor> Authors { get; set; }
        public DbSet<PackageTag> Tags { get; set; }
        public DbSet<PackageVersion> Versions { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageDependency> Dependencies { get; set; }

        public SlimGetContext(ConnectionStringProvider csp)
        {
            this.ConnectionString = csp.ConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(this.ConnectionString, pgopts => pgopts.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region User
            modelBuilder.Entity<User>()
                .ToTable("users");

            modelBuilder.Entity<User>()
                .Property(x => x.Id)
                .IsRequired()
                .HasColumnName("id");

            modelBuilder.Entity<User>()
                .Property(x => x.Email)
                .IsRequired()
                .HasColumnName("email");

            modelBuilder.Entity<User>()
                .Property(x => x.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            modelBuilder.Entity<User>()
                .HasKey(x => x.Id)
                .HasName("pkey_user_id");

            modelBuilder.Entity<User>()
                .HasAlternateKey(x => x.Email)
                .HasName("ukey_email");
            #endregion

            #region Token
            modelBuilder.Entity<Token>()
                .ToTable("tokens");

            modelBuilder.Entity<Token>()
                .Property(x => x.Guid)
                .HasColumnType("uuid")
                .IsRequired()
                .HasColumnName("guid");

            modelBuilder.Entity<Token>()
                .Property(x => x.IssuedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("issued_at");

            modelBuilder.Entity<Token>()
                .Property(x => x.UserId)
                .IsRequired()
                .HasColumnName("user_id");

            modelBuilder.Entity<Token>()
                .HasKey(x => x.Guid)
                .HasName("pkey_token_value");

            modelBuilder.Entity<Token>()
                .HasIndex(x => x.UserId)
                .HasName("ix_token_owner");

            modelBuilder.Entity<Token>()
                .HasOne(x => x.User)
                .WithMany(x => x.Tokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fkey_token_userid");
            #endregion

            #region Package
            modelBuilder.Entity<Package>()
                .ToTable("packages");

            modelBuilder.Entity<Package>()
                .Property(x => x.Id)
                .IsRequired()
                .HasColumnName("id");

            modelBuilder.Entity<Package>()
                .Property(x => x.IdLowercase)
                .IsRequired()
                .HasColumnName("id_lowercase");

            modelBuilder.Entity<Package>()
                .Property(x => x.Description)
                .HasDefaultValue(null)
                .HasColumnName("description");

            modelBuilder.Entity<Package>()
                .Property(x => x.DownloadCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("download_count");

            modelBuilder.Entity<Package>()
                .Property(x => x.HasReadme)
                .IsRequired()
                .HasColumnName("has_readme");

            modelBuilder.Entity<Package>()
                .Property(x => x.IsPrerelase)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("prerelease");

            modelBuilder.Entity<Package>()
                .Property(x => x.Language)
                .HasDefaultValue(null)
                .HasColumnName("language");

            modelBuilder.Entity<Package>()
                .Property(x => x.IsListed)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("listed");

            modelBuilder.Entity<Package>()
                .Property(x => x.MinimumClientVersion)
                .HasDefaultValue(null)
                .HasColumnName("min_client_version");

            modelBuilder.Entity<Package>()
                .Property(x => x.PublishedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("published_at");

            modelBuilder.Entity<Package>()
                .Property(x => x.RequireLicenseAcceptance)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("require_license_acceptance");

            modelBuilder.Entity<Package>()
                .Property(x => x.Summary)
                .HasDefaultValue(null)
                .HasColumnName("summary");

            modelBuilder.Entity<Package>()
                .Property(x => x.Title)
                .HasDefaultValue(null)
                .HasColumnName("title");

            modelBuilder.Entity<Package>()
                .Property(x => x.IconUrl)
                .HasDefaultValue(null)
                .HasColumnName("icon_url");

            modelBuilder.Entity<Package>()
                .Property(x => x.LicenseUrl)
                .HasDefaultValue(null)
                .HasColumnName("license_url");

            modelBuilder.Entity<Package>()
                .Property(x => x.ProjectUrl)
                .HasDefaultValue(null)
                .HasColumnName("project_url");

            modelBuilder.Entity<Package>()
                .Property(x => x.RepositoryUrl)
                .HasDefaultValue(null)
                .HasColumnName("repository_url");

            modelBuilder.Entity<Package>()
                .Property(x => x.RepositoryType)
                .HasDefaultValue(null)
                .HasColumnName("repository_type");

            modelBuilder.Entity<Package>()
                .Property(x => x.SemVerLevel)
                .HasColumnType("integer")
                .IsRequired()
                .HasDefaultValue(SemVerLevel.Unknown)
                .HasColumnName("semver_level");

            modelBuilder.Entity<Package>()
                .Property(x => x.OwnerId)
                .IsRequired()
                .HasColumnName("owner_id");

            modelBuilder.Entity<Package>()
                .HasKey(x => x.Id)
                .HasName("pkey_package_id");

            modelBuilder.Entity<Package>()
                .HasAlternateKey(x => x.IdLowercase)
                .HasName("ukey_package_idlower");

            modelBuilder.Entity<Package>()
                .HasIndex(x => x.IdLowercase)
                .HasName("ix_package_idlower");

            modelBuilder.Entity<Package>()
                .HasIndex(x => x.OwnerId)
                .HasName("ix_package_owner");

            modelBuilder.Entity<Package>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.Packages)
                .HasForeignKey(x => x.OwnerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fkey_package_ownerid");

            modelBuilder.Entity<Package>()
                .Ignore(x => x.AuthorNames)
                .Ignore(x => x.TagNames);
            #endregion

            #region PackageAuthor
            modelBuilder.Entity<PackageAuthor>()
                .ToTable("package_authors");

            modelBuilder.Entity<PackageAuthor>()
                .Property(x => x.PackageId)
                .IsRequired()
                .HasColumnName("package_id");

            modelBuilder.Entity<PackageAuthor>()
                .Property(x => x.Name)
                .IsRequired()
                .HasColumnName("name");

            modelBuilder.Entity<PackageAuthor>()
                .HasKey(x => new { x.PackageId, x.Name })
                .HasName("pkey_author");

            modelBuilder.Entity<PackageAuthor>()
                .HasOne(x => x.Package)
                .WithMany(x => x.Authors)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fkey_author_packageid");
            #endregion

            #region PackageTag
            modelBuilder.Entity<PackageTag>()
                .ToTable("package_tags");

            modelBuilder.Entity<PackageTag>()
                .Property(x => x.PackageId)
                .IsRequired()
                .HasColumnName("package_id");

            modelBuilder.Entity<PackageTag>()
                .Property(x => x.Tag)
                .IsRequired()
                .HasColumnName("tag");

            modelBuilder.Entity<PackageTag>()
                .HasKey(x => new { x.PackageId, x.Tag })
                .HasName("pkey_tag");

            modelBuilder.Entity<PackageTag>()
                .HasOne(x => x.Package)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fkey_tag_packageid");
            #endregion

            #region PackageVersion
            modelBuilder.Entity<PackageVersion>()
                .ToTable("package_versions");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.PackageId)
                .IsRequired()
                .HasColumnName("package_id");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.Version)
                .IsRequired()
                .HasColumnName("version");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.DownloadCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnName("download_count");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.IsPrerelase)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("prerelease");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.PublishedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasDefaultValueSql("now()")
                .HasColumnName("published_at");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.IsListed)
                .IsRequired()
                .HasDefaultValue(true)
                .HasColumnName("listed");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.PackageFileName)
                .IsRequired()
                .HasColumnName("package_filename");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.ManifestFileName)
                .IsRequired()
                .HasColumnName("manifest_filename");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.SymbolsFileName)
                .HasDefaultValue(null)
                .HasColumnName("symbols_filename");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.SymbolsIdentifier)
                .HasColumnType("uuid")
                .HasDefaultValue(null)
                .HasColumnName("symbols_id");

            modelBuilder.Entity<PackageVersion>()
                .HasKey(x => new { x.PackageId, x.Version })
                .HasName("pkey_version");

            modelBuilder.Entity<PackageVersion>()
                .HasIndex(x => x.PackageId)
                .HasName("ix_version_packageid");

            modelBuilder.Entity<PackageVersion>()
                .HasIndex(x => x.SymbolsIdentifier)
                .HasName("ix_version_symbolsid");

            modelBuilder.Entity<PackageVersion>()
                .HasOne(x => x.Package)
                .WithMany(x => x.Versions)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fkey_version_packageid");

            modelBuilder.Entity<PackageVersion>()
                .Ignore(x => x.NuGetVersion);
            #endregion

            #region PackageDependency
            modelBuilder.Entity<PackageDependency>()
                .ToTable("package_dependencies");

            modelBuilder.Entity<PackageDependency>()
                .Property(x => x.Id)
                .IsRequired()
                .HasColumnName("id");

            modelBuilder.Entity<PackageDependency>()
                .Property(x => x.VersionRange)
                .IsRequired()
                .HasColumnName("version_range");

            modelBuilder.Entity<PackageDependency>()
                .Property(x => x.TargetFramework)
                .IsRequired()
                .HasColumnName("target_framework");

            modelBuilder.Entity<PackageDependency>()
                .Property(x => x.PackageId)
                .IsRequired()
                .HasColumnName("package_id");

            modelBuilder.Entity<PackageDependency>()
                .Property(x => x.PackageVersion)
                .IsRequired()
                .HasColumnName("package_version");

            modelBuilder.Entity<PackageDependency>()
                .HasKey(x => new { x.PackageId, x.PackageVersion, x.Id, x.TargetFramework })
                .HasName("pkey_dependency");

            modelBuilder.Entity<PackageDependency>()
                .HasOne(x => x.Package)
                .WithMany(x => x.Dependencies)
                .HasForeignKey(x => new { x.PackageId, x.PackageVersion })
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fkey_dependency_packageid_packageversion");
            #endregion
        }
    }
}
