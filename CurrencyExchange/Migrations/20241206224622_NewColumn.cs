using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CurrencyExchange.Migrations
{
    public partial class NewColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "Transactions",
                type: "decimal(18,10)",
                precision: 18,
                scale: 10,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentRate",
                table: "Currencies",
                type: "decimal(18,12)",
                precision: 18,
                scale: 12,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Transactions");

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,10)",
                oldPrecision: 18,
                oldScale: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentRate",
                table: "Currencies",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,12)",
                oldPrecision: 18,
                oldScale: 12);
        }
    }
}
