using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashVouchersManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInUseProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InUse",
                table: "CashVouchers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InUse",
                table: "CashVouchers");
        }
    }
}
