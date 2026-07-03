using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddClientAndQuoteCurrencyCode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CurrencyCode",
            table: "Quotes",
            type: "varchar(3)",
            maxLength: 3,
            nullable: false,
            defaultValue: "USD")
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "CurrencyCode",
            table: "ClientProfiles",
            type: "varchar(3)",
            maxLength: 3,
            nullable: false,
            defaultValue: "USD")
            .Annotation("MySql:CharSet", "utf8mb4");

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

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CurrencyCode",
            table: "Quotes");

        migrationBuilder.DropColumn(
            name: "CurrencyCode",
            table: "ClientProfiles");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 246, DateTimeKind.Utc).AddTicks(9894));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 246, DateTimeKind.Utc).AddTicks(9903));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 246, DateTimeKind.Utc).AddTicks(9909));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 246, DateTimeKind.Utc).AddTicks(9913));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 246, DateTimeKind.Utc).AddTicks(9918));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 246, DateTimeKind.Utc).AddTicks(9923));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 246, DateTimeKind.Utc).AddTicks(9927));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 246, DateTimeKind.Utc).AddTicks(9931));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 247, DateTimeKind.Utc).AddTicks(60));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 247, DateTimeKind.Utc).AddTicks(65));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 247, DateTimeKind.Utc).AddTicks(71));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 247, DateTimeKind.Utc).AddTicks(75));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 247, DateTimeKind.Utc).AddTicks(79));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 247, DateTimeKind.Utc).AddTicks(84));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 247, DateTimeKind.Utc).AddTicks(100));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 247, DateTimeKind.Utc).AddTicks(105));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 252, DateTimeKind.Utc).AddTicks(3018));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 252, DateTimeKind.Utc).AddTicks(3024));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 252, DateTimeKind.Utc).AddTicks(3029));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 5, 21, 20, 41, 56, 252, DateTimeKind.Utc).AddTicks(3033));
    }
}
