using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Schema_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chunks_Files_FileId1",
                table: "Chunks");

            migrationBuilder.DropIndex(
                name: "IX_Chunks_FileId1",
                table: "Chunks");

            migrationBuilder.DropColumn(
                name: "FileId1",
                table: "Chunks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FileId1",
                table: "Chunks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Chunks_FileId1",
                table: "Chunks",
                column: "FileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Chunks_Files_FileId1",
                table: "Chunks",
                column: "FileId1",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
