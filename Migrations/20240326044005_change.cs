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
            migrationBuilder.CreateTable(
                name: "hhyys",
                columns: table => new
                {
                    skuCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    productName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    spec = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    factoryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approvalNO = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    middlePackAmount = table.Column<int>(type: "int", nullable: true),
                    bigPackAmount = table.Column<int>(type: "int", nullable: true),
                    units = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stockState = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amount = table.Column<int>(type: "int", nullable: true),
                    price = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    priceTip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noSellTip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    saleTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    source = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    isdeleted = table.Column<int>(type: "int", nullable: false),
                    url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hhyys", x => x.skuCode);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hhyys");
        }
    }
}
