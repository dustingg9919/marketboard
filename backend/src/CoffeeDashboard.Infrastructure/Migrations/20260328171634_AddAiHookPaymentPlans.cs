using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CoffeeDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiHookPaymentPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiHookPaymentPlan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    UsageLimit = table.Column<int>(type: "integer", nullable: false),
                    ApiLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiHookPaymentPlan", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AiHookAccounts",
                keyColumn: "Id",
                keyValue: new Guid("3f1d4f83-0a8c-4a70-9f7c-5ecbfb1a4ad1"),
                column: "ExpirationTimes",
                value: 100);

            migrationBuilder.InsertData(
                table: "AiHookPaymentPlan",
                columns: new[] { "Id", "ApiLevel", "CreatedAt", "Price", "TypeName", "UpdatedAt", "UsageLimit" },
                values: new object[,]
                {
                    { new Guid("2c9cf75f-1a8f-4f3f-b2f6-0b37a5b1d020"), null, new DateTime(2026, 3, 28, 0, 0, 0, 0, DateTimeKind.Utc), 80000m, "Mở rộng", null, 400 },
                    { new Guid("6b78a0c1-7c0d-4d0c-9e67-9c1d2e2ef1d1"), null, new DateTime(2026, 3, 28, 0, 0, 0, 0, DateTimeKind.Utc), 499000m, "Chuyên Nghiệp", null, 0 },
                    { new Guid("b7ad1c12-21c3-4f6b-9bcb-57fbd7f6a2b9"), null, new DateTime(2026, 3, 28, 0, 0, 0, 0, DateTimeKind.Utc), 5000m, "Dùng thử", null, 100 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiHookPaymentPlan_TypeName",
                table: "AiHookPaymentPlan",
                column: "TypeName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiHookPaymentPlan");

            migrationBuilder.UpdateData(
                table: "AiHookAccounts",
                keyColumn: "Id",
                keyValue: new Guid("3f1d4f83-0a8c-4a70-9f7c-5ecbfb1a4ad1"),
                column: "ExpirationTimes",
                value: 20);
        }
    }
}
