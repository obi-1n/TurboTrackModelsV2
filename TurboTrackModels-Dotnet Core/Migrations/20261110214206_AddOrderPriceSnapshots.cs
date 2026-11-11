using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurboTrackModels_Dotnet_Core.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPriceSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingCost",
                table: "Orders",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "OrderItems",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                UPDATE OrderItems
                SET UnitPrice = (
                    SELECT Price
                    FROM Products
                    WHERE Products.ProductId = OrderItems.ProductId
                );

                UPDATE Orders
                SET ShippingCost = MAX(
                    0,
                    TotalCost - COALESCE((
                        SELECT SUM(OrderItems.Quantity * OrderItems.UnitPrice)
                        FROM OrderItems
                        WHERE OrderItems.OrderId = Orders.OrderId
                    ), 0)
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "OrderItems");
        }
    }
}
