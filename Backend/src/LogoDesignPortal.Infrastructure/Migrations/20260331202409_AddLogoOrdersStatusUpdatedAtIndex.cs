using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <summary>
/// Composite index for revenue / completion analytics: filter by Status (e.g. Completed) and range on UpdatedAt.
/// </summary>
public partial class AddLogoOrdersStatusUpdatedAtIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_LogoOrders_Status_UpdatedAt",
            table: "LogoOrders",
            columns: new[] { "Status", "UpdatedAt" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_LogoOrders_Status_UpdatedAt",
            table: "LogoOrders");
    }
}
