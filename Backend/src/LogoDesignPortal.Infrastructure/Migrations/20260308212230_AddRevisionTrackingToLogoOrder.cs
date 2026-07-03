using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddRevisionTrackingToLogoOrder : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "AllowExtraRevisions",
            table: "LogoOrders",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<int>(
            name: "RevisionCount",
            table: "LogoOrders",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "RevisionLimit",
            table: "LogoOrders",
            type: "int",
            nullable: true);

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6068));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6074));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6079));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6083));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6087));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6092));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6097));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6101));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6105));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6109));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6113));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6116));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6121));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6125));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 373, DateTimeKind.Utc).AddTicks(6139));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 375, DateTimeKind.Utc).AddTicks(3195));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 375, DateTimeKind.Utc).AddTicks(3202));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 375, DateTimeKind.Utc).AddTicks(3207));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 22, 28, 375, DateTimeKind.Utc).AddTicks(3211));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AllowExtraRevisions",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "RevisionCount",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "RevisionLimit",
            table: "LogoOrders");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9148));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9153));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9157));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9161));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9164));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9168));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9171));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9174));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9178));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9181));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9186));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9189));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9193));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9196));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 421, DateTimeKind.Utc).AddTicks(9211));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 423, DateTimeKind.Utc).AddTicks(5394));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 423, DateTimeKind.Utc).AddTicks(5399));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 423, DateTimeKind.Utc).AddTicks(5402));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 10, 7, 423, DateTimeKind.Utc).AddTicks(5406));
    }
}
