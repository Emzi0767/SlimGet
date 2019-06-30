using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SlimGet.Data.Database.Migrations
{
    public partial class InitialSetup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DownloadCount = table.Column<long>(nullable: false, defaultValue: 0L),
                    HasReadme = table.Column<bool>(nullable: false),
                    IsPrerelase = table.Column<bool>(nullable: false, defaultValue: false),
                    Language = table.Column<string>(nullable: true),
                    IsListed = table.Column<bool>(nullable: false, defaultValue: true),
                    MinimumClientVersion = table.Column<string>(nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    RequireLicenseAcceptance = table.Column<bool>(nullable: false, defaultValue: false),
                    Summary = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    IconUrl = table.Column<string>(nullable: true),
                    LicenseUrl = table.Column<string>(nullable: true),
                    ProjectUrl = table.Column<string>(nullable: true),
                    RepositoryUrl = table.Column<string>(nullable: true),
                    RepositoryType = table.Column<string>(nullable: true),
                    SemVerLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PKEY_PackageId", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    PackageId = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PKEY_Author", x => new { x.PackageId, x.Name });
                    table.ForeignKey(
                        name: "FKEY_Author_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    PackageId = table.Column<string>(nullable: false),
                    Tag = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PKEY_Tag", x => new { x.PackageId, x.Tag });
                    table.ForeignKey(
                        name: "FKEY_Tag_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Versions",
                columns: table => new
                {
                    PackageId = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false),
                    DownloadCount = table.Column<long>(nullable: false, defaultValue: 0L),
                    IsPrerelase = table.Column<bool>(nullable: false, defaultValue: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    IsListed = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PKEY_Version", x => new { x.PackageId, x.Version });
                    table.ForeignKey(
                        name: "FKEY_Version_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dependencies",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    TargetFramework = table.Column<string>(nullable: false),
                    PackageId = table.Column<string>(nullable: false),
                    PackageVersion = table.Column<string>(nullable: false),
                    VersionRange = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PKEY_Dependency", x => new { x.PackageId, x.PackageVersion, x.Id, x.TargetFramework });
                    table.ForeignKey(
                        name: "FKEY_Dependency_PackageId_PackageVersion",
                        columns: x => new { x.PackageId, x.PackageVersion },
                        principalTable: "Versions",
                        principalColumns: new[] { "PackageId", "Version" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Dependencies");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Versions");

            migrationBuilder.DropTable(
                name: "Packages");
        }
    }
}
