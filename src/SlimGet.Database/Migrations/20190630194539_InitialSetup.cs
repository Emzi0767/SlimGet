using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SlimGet.Data.Database.Migrations
{
    public partial class InitialSetup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    email = table.Column<string>(nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_userid", x => x.id);
                    table.UniqueConstraint("ukey_email", x => x.email);
                });

            migrationBuilder.CreateTable(
                name: "packages",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    id_lowercase = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    download_count = table.Column<long>(nullable: false, defaultValue: 0L),
                    has_readme = table.Column<bool>(nullable: false),
                    prerelease = table.Column<bool>(nullable: false, defaultValue: false),
                    language = table.Column<string>(nullable: true),
                    listed = table.Column<bool>(nullable: false, defaultValue: true),
                    min_client_version = table.Column<string>(nullable: true),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    require_license_acceptance = table.Column<bool>(nullable: false, defaultValue: false),
                    summary = table.Column<string>(nullable: true),
                    title = table.Column<string>(nullable: true),
                    icon_url = table.Column<string>(nullable: true),
                    license_url = table.Column<string>(nullable: true),
                    project_url = table.Column<string>(nullable: true),
                    repository_url = table.Column<string>(nullable: true),
                    repository_type = table.Column<string>(nullable: true),
                    semver_level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    owner_id = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_packageid", x => x.id);
                    table.UniqueConstraint("ukey_packageidlower", x => x.id_lowercase);
                    table.ForeignKey(
                        name: "fkey_package_ownerid",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tokens",
                columns: table => new
                {
                    value = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_token_value", x => x.value);
                    table.ForeignKey(
                        name: "fkey_token_userid",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package_authors",
                columns: table => new
                {
                    package_id = table.Column<string>(nullable: false),
                    name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_author", x => new { x.package_id, x.name });
                    table.ForeignKey(
                        name: "fkey_author_packageid",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package_tags",
                columns: table => new
                {
                    package_id = table.Column<string>(nullable: false),
                    tag = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_tag", x => new { x.package_id, x.tag });
                    table.ForeignKey(
                        name: "fkey_tag_packageid",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package_versions",
                columns: table => new
                {
                    package_id = table.Column<string>(nullable: false),
                    version = table.Column<string>(nullable: false),
                    download_count = table.Column<long>(nullable: false, defaultValue: 0L),
                    prerelease = table.Column<bool>(nullable: false, defaultValue: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    listed = table.Column<bool>(nullable: false, defaultValue: true),
                    package_filename = table.Column<string>(nullable: false),
                    manifest_filename = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_version", x => new { x.package_id, x.version });
                    table.ForeignKey(
                        name: "fkey_version_packageid",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package_dependencies",
                columns: table => new
                {
                    package_id = table.Column<string>(nullable: false),
                    package_version = table.Column<string>(nullable: false),
                    id = table.Column<string>(nullable: false),
                    target_framework = table.Column<string>(nullable: false),
                    version_range = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_dependency", x => new { x.package_id, x.package_version, x.id, x.target_framework });
                    table.ForeignKey(
                        name: "fkey_dependency_packageid_packageversion",
                        columns: x => new { x.package_id, x.package_version },
                        principalTable: "package_versions",
                        principalColumns: new[] { "package_id", "version" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_packages_owner_id",
                table: "packages",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_tokens_user_id",
                table: "tokens",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "package_authors");

            migrationBuilder.DropTable(
                name: "package_dependencies");

            migrationBuilder.DropTable(
                name: "package_tags");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "package_versions");

            migrationBuilder.DropTable(
                name: "packages");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
