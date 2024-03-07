using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkerService1.Migrations
{
    /// <inheritdoc />
    public partial class change : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaleTip",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "SellTip",
                table: "Products",
                newName: "Message");

            migrationBuilder.AddColumn<bool>(
                name: "IsBanned",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBanned",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "Products",
                newName: "SellTip");

            migrationBuilder.AddColumn<string>(
                name: "SaleTip",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
