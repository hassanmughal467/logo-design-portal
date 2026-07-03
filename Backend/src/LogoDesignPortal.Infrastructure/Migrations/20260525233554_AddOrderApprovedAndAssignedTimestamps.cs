using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOrderApprovedAndAssignedTimestamps : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "ApprovedAt",
            table: "LogoOrders",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "ApprovedBy",
            table: "LogoOrders",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

        migrationBuilder.AddColumn<DateTime>(
            name: "AssignedAt",
            table: "LogoOrders",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "AssignedBy",
            table: "LogoOrders",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

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

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ApprovedAt",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "ApprovedBy",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "AssignedAt",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "AssignedBy",
            table: "LogoOrders");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 675, DateTimeKind.Utc).AddTicks(1912));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 679, DateTimeKind.Utc).AddTicks(7487));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 679, DateTimeKind.Utc).AddTicks(7495));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 679, DateTimeKind.Utc).AddTicks(7500));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 22, 23, 40, 13, 679, DateTimeKind.Utc).AddTicks(7505));
    }
}
