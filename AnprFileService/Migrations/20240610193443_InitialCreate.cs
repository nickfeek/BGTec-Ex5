using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnprFileService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CountryOfVehicle = table.Column<string>(type: "TEXT", nullable: false),
                    RegNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ConfidenceLevel = table.Column<string>(type: "TEXT", nullable: false),
                    CameraName = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<int>(type: "INTEGER", nullable: false),
                    Time = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageFilename = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_CameraName",
                table: "Files",
                column: "CameraName");

            migrationBuilder.CreateIndex(
                name: "IX_Files_Date",
                table: "Files",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Files_Path",
                table: "Files",
                column: "Path",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");
        }
    }
}
