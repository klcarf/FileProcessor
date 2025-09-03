using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Metadata.Migrations.DatabaseStorageDb
{
    /// <inheritdoc />
    public partial class DatabaseStorageDbContext_v10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Storages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<string>(type: "text", nullable: false),
                    ChunkOrder = table.Column<int>(type: "integer", nullable: false),
                    ChunkData = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Storages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Storages_CreateDate",
                table: "Storages",
                column: "CreateDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Storages");
        }
    }
}
