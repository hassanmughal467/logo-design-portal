using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddInvoiceExchangeRateColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "ExchangeRate",
            table: "Invoices",
            type: "decimal(18,6)",
            precision: 18,
            scale: 6,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "ExchangeRateFetchedAt",
            table: "Invoices",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "ExchangeRateIsStale",
            table: "Invoices",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 290, DateTimeKind.Utc).AddTicks(2660));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 297, DateTimeKind.Utc).AddTicks(5878));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 297, DateTimeKind.Utc).AddTicks(5883));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 297, DateTimeKind.Utc).AddTicks(5888));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 6, 9, 23, 37, 31, 297, DateTimeKind.Utc).AddTicks(5893));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ExchangeRate",
            table: "Invoices");

        migrationBuilder.DropColumn(
            name: "ExchangeRateFetchedAt",
            table: "Invoices");

        migrationBuilder.DropColumn(
            name: "ExchangeRateIsStale",
            table: "Invoices");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 34, DateTimeKind.Utc).AddTicks(6579));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 38, DateTimeKind.Utc).AddTicks(3088));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 38, DateTimeKind.Utc).AddTicks(3094));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 38, DateTimeKind.Utc).AddTicks(3100));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 25, 23, 35, 52, 38, DateTimeKind.Utc).AddTicks(3104));
    }
}
