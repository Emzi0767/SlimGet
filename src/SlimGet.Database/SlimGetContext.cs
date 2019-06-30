using Microsoft.EntityFrameworkCore;
using SlimGet.Data.Database;

namespace SlimGet.Services
{
    public class SlimGetContext : DbContext
    {
        private string ConnectionString { get; }

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
        {
            optionsBuilder.UseNpgsql(this.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Package
            modelBuilder.Entity<Package>()
                .Property(x => x.Id)
                .IsRequired();

            modelBuilder.Entity<Package>()
                .Property(x => x.Description)
                .HasDefaultValue(null);

            modelBuilder.Entity<Package>()
                .Property(x => x.DownloadCount)
                .IsRequired()
                .HasDefaultValue(0);

            modelBuilder.Entity<Package>()
                .Property(x => x.HasReadme)
                .IsRequired();

            modelBuilder.Entity<Package>()
                .Property(x => x.IsPrerelase)
                .IsRequired()
                .HasDefaultValue(false);

            modelBuilder.Entity<Package>()
                .Property(x => x.Language)
                .HasDefaultValue(null);

            modelBuilder.Entity<Package>()
                .Property(x => x.IsListed)
                .IsRequired()
                .HasDefaultValue(true);

            modelBuilder.Entity<Package>()
                .Property(x => x.MinimumClientVersion)
                .HasDefaultValue(null);

            modelBuilder.Entity<Package>()
                .Property(x => x.PublishedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<Package>()
                .Property(x => x.RequireLicenseAcceptance)
                .IsRequired()
                .HasDefaultValue(false);

            modelBuilder.Entity<Package>()
                .Property(x => x.Summary)
                .HasDefaultValue(null);

            modelBuilder.Entity<Package>()
                .Property(x => x.Title)
                .HasDefaultValue(null);

            modelBuilder.Entity<Package>()
                .Property(x => x.IconUrl)
                .HasDefaultValue(null);

            modelBuilder.Entity<Package>()
                .Property(x => x.LicenseUrl)
                .HasDefaultValue(null);

            modelBuilder.Entity<Package>()
                .Property(x => x.ProjectUrl)
                .HasDefaultValue(null);

            modelBuilder.Entity<Package>()
                .Property(x => x.RepositoryUrl)
                .HasDefaultValue(null);

            modelBuilder.Entity<Package>()
                .Property(x => x.RepositoryType)
                .HasDefaultValue(null);

            modelBuilder.Entity<Package>()
                .Property(x => x.SemVerLevel)
                .HasColumnType("integer")
                .IsRequired()
                .HasDefaultValue(SemVerLevel.Unknown);

            modelBuilder.Entity<Package>()
                .HasKey(x => x.Id)
                .HasName("PKEY_PackageId");

            modelBuilder.Entity<Package>()
                .Ignore(x => x.AuthorNames)
                .Ignore(x => x.TagNames);
            #endregion

            #region PackageAuthor
            modelBuilder.Entity<PackageAuthor>()
                .Property(x => x.PackageId)
                .IsRequired();

            modelBuilder.Entity<PackageAuthor>()
                .Property(x => x.Name)
                .IsRequired();

            modelBuilder.Entity<PackageAuthor>()
                .HasKey(x => new { x.PackageId, x.Name })
                .HasName("PKEY_Author");

            modelBuilder.Entity<PackageAuthor>()
                .HasOne(x => x.Package)
                .WithMany(x => x.Authors)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FKEY_Author_PackageId");
            #endregion

            #region PackageTag
            modelBuilder.Entity<PackageTag>()
                .Property(x => x.PackageId)
                .IsRequired();

            modelBuilder.Entity<PackageTag>()
                .Property(x => x.Tag)
                .IsRequired();

            modelBuilder.Entity<PackageTag>()
                .HasKey(x => new { x.PackageId, x.Tag })
                .HasName("PKEY_Tag");

            modelBuilder.Entity<PackageTag>()
                .HasOne(x => x.Package)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FKEY_Tag_PackageId");
            #endregion

            #region PackageVersion
            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.PackageId)
                .IsRequired();

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.Version)
                .IsRequired();

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.DownloadCount)
                .IsRequired()
                .HasDefaultValue(0);

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.IsPrerelase)
                .IsRequired()
                .HasDefaultValue(false);

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.PublishedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasDefaultValueSql("now()");

            modelBuilder.Entity<PackageVersion>()
                .Property(x => x.IsListed)
                .IsRequired()
                .HasDefaultValue(true);

            modelBuilder.Entity<PackageVersion>()
                .HasKey(x => new { x.PackageId, x.Version })
                .HasName("PKEY_Version");

            modelBuilder.Entity<PackageVersion>()
                .HasOne(x => x.Package)
                .WithMany(x => x.Versions)
                .HasForeignKey(x => x.PackageId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FKEY_Version_PackageId");

            modelBuilder.Entity<PackageVersion>()
                .Ignore(x => x.NuGetVersion);
            #endregion

            #region PackageDependency
            modelBuilder.Entity<PackageDependency>()
                .Property(x => x.Id)
                .IsRequired();

            modelBuilder.Entity<PackageDependency>()
                .Property(x => x.VersionRange)
                .IsRequired();

            modelBuilder.Entity<PackageDependency>()
                .Property(x => x.TargetFramework)
                .IsRequired();

            modelBuilder.Entity<PackageDependency>()
                .Property(x => x.PackageId)
                .IsRequired();

            modelBuilder.Entity<PackageDependency>()
                .HasKey(x => new { x.PackageId, x.PackageVersion, x.Id, x.TargetFramework })
                .HasName("PKEY_Dependency");

            modelBuilder.Entity<PackageDependency>()
                .HasOne(x => x.Package)
                .WithMany(x => x.Dependencies)
                .HasForeignKey(x => new { x.PackageId, x.PackageVersion })
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FKEY_Dependency_PackageId_PackageVersion");
            #endregion
        }
    }
}
