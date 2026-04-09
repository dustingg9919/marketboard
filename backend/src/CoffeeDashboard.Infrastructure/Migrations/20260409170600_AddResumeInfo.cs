using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResumeInfo",
                columns: table => new
                {
                    Object = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResumeInfo", x => x.Object);
                });

            migrationBuilder.InsertData(
                table: "ResumeInfo",
                columns: new[] { "Object", "CreatedAt", "UpdatedAt", "Value" },
                values: new object[]
                {
                    "gemini_api_key",
                    new DateTime(2026, 4, 9, 0, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    "AIzaSyC45ufgYsXLFZG-pTvena8BVPemQXwOLj0"
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResumeInfo");
        }
    }
}
