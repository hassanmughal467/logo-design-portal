using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddInvoicedDateToLogoOrder : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "InvoicedDate",
            table: "LogoOrders",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8039));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8049));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8056));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8062));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8071));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8077));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8083));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8090));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8097));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8103));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8128));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8135));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8141));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8147));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 409, DateTimeKind.Utc).AddTicks(8154));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 416, DateTimeKind.Utc).AddTicks(1271));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 416, DateTimeKind.Utc).AddTicks(1281));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 416, DateTimeKind.Utc).AddTicks(1288));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 16, 31, 21, 416, DateTimeKind.Utc).AddTicks(1294));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "InvoicedDate",
            table: "LogoOrders");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6843));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6855));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6861));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6865));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6869));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6874));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6878));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6882));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6886));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6890));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6893));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6897));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6901));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6904));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 694, DateTimeKind.Utc).AddTicks(6914));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 696, DateTimeKind.Utc).AddTicks(2640));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 696, DateTimeKind.Utc).AddTicks(2646));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 696, DateTimeKind.Utc).AddTicks(2653));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 1, 1, 5, 696, DateTimeKind.Utc).AddTicks(2743));
    }
}
