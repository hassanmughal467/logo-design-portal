using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddAnalyticsIndexesToLogoOrder : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Note: Do not drop IX_LogoOrders_ClientId or IX_LogoOrders_DesignerId - they are required by MySQL for FK constraints.
        // The new composite indexes will be created alongside them.

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3784));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3791));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3795));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3800));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3806));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3811));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3815));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3820));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3823));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3828));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3831));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3837));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3841));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3845));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 184, DateTimeKind.Utc).AddTicks(3857));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 185, DateTimeKind.Utc).AddTicks(9081));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 185, DateTimeKind.Utc).AddTicks(9087));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 185, DateTimeKind.Utc).AddTicks(9091));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 17, 32, 185, DateTimeKind.Utc).AddTicks(9095));

        migrationBuilder.CreateIndex(
            name: "IX_LogoOrders_ClientId_CreatedAt",
            table: "LogoOrders",
            columns: new[] { "ClientId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_LogoOrders_CreatedAt",
            table: "LogoOrders",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_LogoOrders_DesignerId_Status",
            table: "LogoOrders",
            columns: new[] { "DesignerId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_LogoOrders_Status",
            table: "LogoOrders",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_LogoOrders_Status_CreatedAt",
            table: "LogoOrders",
            columns: new[] { "Status", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_LogoOrders_UpdatedAt",
            table: "LogoOrders",
            column: "UpdatedAt");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_LogoOrders_ClientId_CreatedAt",
            table: "LogoOrders");

        migrationBuilder.DropIndex(
            name: "IX_LogoOrders_CreatedAt",
            table: "LogoOrders");

        migrationBuilder.DropIndex(
            name: "IX_LogoOrders_DesignerId_Status",
            table: "LogoOrders");

        migrationBuilder.DropIndex(
            name: "IX_LogoOrders_Status",
            table: "LogoOrders");

        migrationBuilder.DropIndex(
            name: "IX_LogoOrders_Status_CreatedAt",
            table: "LogoOrders");

        migrationBuilder.DropIndex(
            name: "IX_LogoOrders_UpdatedAt",
            table: "LogoOrders");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3131));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3136));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3143));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3148));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3152));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3158));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3161));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3165));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3168));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3176));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3182));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3187));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3190));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3193));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 300, DateTimeKind.Utc).AddTicks(3275));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 302, DateTimeKind.Utc).AddTicks(7293));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 302, DateTimeKind.Utc).AddTicks(7300));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 302, DateTimeKind.Utc).AddTicks(7303));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 6, 13, 22, 3, 302, DateTimeKind.Utc).AddTicks(7307));

        // IX_LogoOrders_ClientId and IX_LogoOrders_DesignerId are recreated by FK constraints - no need to recreate here
    }
}
