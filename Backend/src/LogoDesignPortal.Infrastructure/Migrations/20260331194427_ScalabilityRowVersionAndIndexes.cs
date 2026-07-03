using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ScalabilityRowVersionAndIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "RowVersion",
            table: "LogoOrders",
            type: "timestamp(6)",
            rowVersion: true,
            nullable: false)
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

        migrationBuilder.AddColumn<DateTime>(
            name: "RowVersion",
            table: "Invoices",
            type: "timestamp(6)",
            rowVersion: true,
            nullable: false)
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6241));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6245));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6286));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6288));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6290));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6293));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6295));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6297));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6300));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6302));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6304));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6306));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6308));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6310));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6325));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 779, DateTimeKind.Utc).AddTicks(6327));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 781, DateTimeKind.Utc).AddTicks(2388));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 781, DateTimeKind.Utc).AddTicks(2392));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 781, DateTimeKind.Utc).AddTicks(2394));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 31, 19, 44, 26, 781, DateTimeKind.Utc).AddTicks(2398));

        migrationBuilder.CreateIndex(
            name: "IX_OrderStatusHistories_OrderId_NewStatus_CreatedAt",
            table: "OrderStatusHistories",
            columns: new[] { "OrderId", "NewStatus", "CreatedAt" });

        // Composite leads with OrderId (FK); drop redundant single-column index after composite exists.
        migrationBuilder.DropIndex(
            name: "IX_OrderStatusHistories_OrderId",
            table: "OrderStatusHistories");

        migrationBuilder.CreateIndex(
            name: "IX_Invoices_CreatedAt",
            table: "Invoices",
            column: "CreatedAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_OrderStatusHistories_OrderId",
            table: "OrderStatusHistories",
            column: "OrderId");

        migrationBuilder.DropIndex(
            name: "IX_OrderStatusHistories_OrderId_NewStatus_CreatedAt",
            table: "OrderStatusHistories");

        migrationBuilder.DropIndex(
            name: "IX_Invoices_CreatedAt",
            table: "Invoices");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "RowVersion",
            table: "Invoices");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5465));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5470));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5475));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5481));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5484));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5488));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5492));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5495));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5498));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5501));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5504));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5507));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5510));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5513));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5524));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5527));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 96, DateTimeKind.Utc).AddTicks(9768));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 96, DateTimeKind.Utc).AddTicks(9772));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 96, DateTimeKind.Utc).AddTicks(9775));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 26, 17, 52, 0, 96, DateTimeKind.Utc).AddTicks(9779));
    }
}
