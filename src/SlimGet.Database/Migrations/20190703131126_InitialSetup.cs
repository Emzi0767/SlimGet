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
using Microsoft.EntityFrameworkCore.Migrations;

namespace SlimGet.Data.Database.Migrations
{
    public partial class InitialSetup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

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
                    table.PrimaryKey("pkey_user_id", x => x.id);
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
                    table.PrimaryKey("pkey_package_id", x => x.id);
                    table.UniqueConstraint("ukey_package_idlower", x => x.id_lowercase);
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
                    guid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(nullable: false),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_token_value", x => x.guid);
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
                    version_lowercase = table.Column<string>(nullable: false),
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
                    min_version = table.Column<string>(nullable: true),
                    min_version_inclusive = table.Column<bool>(nullable: false, defaultValue: false),
                    max_version = table.Column<string>(nullable: true),
                    max_version_inclusive = table.Column<bool>(nullable: false, defaultValue: false)
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

            migrationBuilder.CreateTable(
                name: "package_frameworks",
                columns: table => new
                {
                    package_id = table.Column<string>(nullable: false),
                    package_version = table.Column<string>(nullable: false),
                    framework = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_framework", x => new { x.package_id, x.package_version, x.framework });
                    table.ForeignKey(
                        name: "fkey_framework_packageid_packageversion",
                        columns: x => new { x.package_id, x.package_version },
                        principalTable: "package_versions",
                        principalColumns: new[] { "package_id", "version" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package_binaries",
                columns: table => new
                {
                    package_id = table.Column<string>(nullable: false),
                    package_version = table.Column<string>(nullable: false),
                    framework = table.Column<string>(nullable: false),
                    name = table.Column<string>(nullable: false),
                    length = table.Column<long>(nullable: false),
                    sha256_hash = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_binary_packageid_packageversion_framework", x => new { x.package_id, x.package_version, x.framework, x.name });
                    table.ForeignKey(
                        name: "fkey_binary_packageid_packageversion",
                        columns: x => new { x.package_id, x.package_version },
                        principalTable: "package_versions",
                        principalColumns: new[] { "package_id", "version" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fkey_binary_framework",
                        columns: x => new { x.package_id, x.package_version, x.framework },
                        principalTable: "package_frameworks",
                        principalColumns: new[] { "package_id", "package_version", "framework" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package_symbols",
                columns: table => new
                {
                    package_id = table.Column<string>(nullable: false),
                    package_version = table.Column<string>(nullable: false),
                    framework = table.Column<string>(nullable: false),
                    binary_name = table.Column<string>(nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    age = table.Column<int>(nullable: false),
                    signature = table.Column<string>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    file_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pkey_symbols_packageid_packageversion_framework_id_kind", x => new { x.package_id, x.package_version, x.framework, x.binary_name, x.id, x.kind });
                    table.ForeignKey(
                        name: "fkey_symbols_packageid_packageversion_framework_binaryname",
                        columns: x => new { x.package_id, x.package_version, x.framework, x.binary_name },
                        principalTable: "package_binaries",
                        principalColumns: new[] { "package_id", "package_version", "framework", "name" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_binary_hash",
                table: "package_binaries",
                column: "sha256_hash")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "ix_symbols_id",
                table: "package_symbols",
                column: "id")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "ix_symbols_sig",
                table: "package_symbols",
                column: "signature")
                .Annotation("Npgsql:IndexMethod", "hash");

            migrationBuilder.CreateIndex(
                name: "ix_version_packageid",
                table: "package_versions",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "ix_package_idlower",
                table: "packages",
                column: "id_lowercase");

            migrationBuilder.CreateIndex(
                name: "ix_package_owner",
                table: "packages",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_token_owner",
                table: "tokens",
                column: "user_id");

            // ADDED MANUALLY
            migrationBuilder.Sql("CREATE INDEX ix_package_trgm_id ON packages USING gin (id gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX ix_package_trgm_title ON packages USING gin (title gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX ix_package_trgm_desc ON packages USING gin (description gin_trgm_ops);");
            migrationBuilder.Sql("CREATE INDEX ix_package_trgm_tags ON package_tags USING gin (tag gin_trgm_ops);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "package_authors");

            migrationBuilder.DropTable(
                name: "package_dependencies");

            migrationBuilder.DropTable(
                name: "package_symbols");

            migrationBuilder.DropTable(
                name: "package_tags");

            migrationBuilder.DropTable(
                name: "tokens");

            migrationBuilder.DropTable(
                name: "package_binaries");

            migrationBuilder.DropTable(
                name: "package_frameworks");

            migrationBuilder.DropTable(
                name: "package_versions");

            migrationBuilder.DropTable(
                name: "packages");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
