using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashVouchersManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashVouchers",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IssuingStoreId = table.Column<int>(type: "INTEGER", nullable: false),
                    RedemptionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IssuingSaleId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    RedemptionSaleId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashVouchers_Code",
                table: "CashVouchers",
                column: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashVouchers");
        }
    }
}
