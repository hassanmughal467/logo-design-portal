using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class DropOrphanReferenceWebsiteColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ReferenceWebsite was added to LogoOrders by migration 20260331222128, but the
        // entity property was later removed without a corresponding drop migration,
        // leaving an orphaned column in every database that ran that migration. The
        // current model snapshot no longer tracks the column, so EF cannot scaffold
        // this drop from a model diff — it is authored explicitly here (supported EF
        // migration customization). Migration replay order guarantees the column
        // exists at this point in the chain.
        migrationBuilder.DropColumn(
            name: "ReferenceWebsite",
            table: "LogoOrders");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 867, DateTimeKind.Utc).AddTicks(9571));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 900, DateTimeKind.Utc).AddTicks(6693));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 900, DateTimeKind.Utc).AddTicks(6713));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 900, DateTimeKind.Utc).AddTicks(6723));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 7, 3, 23, 10, 9, 900, DateTimeKind.Utc).AddTicks(6733));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Restores the column exactly as migration 20260331222128 created it.
        migrationBuilder.AddColumn<string>(
            name: "ReferenceWebsite",
            table: "LogoOrders",
            type: "varchar(2000)",
            maxLength: 2000,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

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
}
