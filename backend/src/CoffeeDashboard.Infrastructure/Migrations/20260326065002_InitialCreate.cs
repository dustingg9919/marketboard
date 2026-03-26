using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MarketAssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SecondaryValue = table.Column<decimal>(type: "numeric", nullable: true),
                    SecondaryUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Change = table.Column<decimal>(type: "numeric", nullable: true),
                    ChangePercent = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketSnapshots_MarketAssets_MarketAssetId",
                        column: x => x.MarketAssetId,
                        principalTable: "MarketAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiAccounts_Name",
                table: "ApiAccounts",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketAssets_Code",
                table: "MarketAssets",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketSnapshots_MarketAssetId_SnapshotDate",
                table: "MarketSnapshots",
                columns: new[] { "MarketAssetId", "SnapshotDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiAccounts");

            migrationBuilder.DropTable(
                name: "MarketSnapshots");

            migrationBuilder.DropTable(
                name: "MarketAssets");
        }
    }
}
