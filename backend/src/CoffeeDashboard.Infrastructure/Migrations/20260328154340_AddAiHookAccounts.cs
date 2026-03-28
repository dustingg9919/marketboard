using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeDashboard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiHookAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AiHookAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PaymentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpirationTimes = table.Column<int>(type: "integer", nullable: false),
                    BankAccount = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiHookAccounts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AiHookAccounts",
                columns: new[] { "Id", "ApiKey", "BankAccount", "BankName", "CreatedAt", "ExpirationDate", "ExpirationTimes", "Password", "PaymentType", "UpdatedAt", "Username" },
                values: new object[] { new Guid("3f1d4f83-0a8c-4a70-9f7c-5ecbfb1a4ad1"), null, null, null, new DateTime(2026, 3, 28, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), 20, "sieu", "Dùng thử", null, "sieu" });

            migrationBuilder.CreateIndex(
                name: "IX_AiHookAccounts_Username",
                table: "AiHookAccounts",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiHookAccounts");
        }
    }
}
