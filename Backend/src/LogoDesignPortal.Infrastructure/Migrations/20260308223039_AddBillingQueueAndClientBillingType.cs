using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddBillingQueueAndClientBillingType : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "BillingEligible",
            table: "LogoOrders",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "CompletedDate",
            table: "LogoOrders",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "InvoiceId",
            table: "LogoOrders",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

        migrationBuilder.AddColumn<bool>(
            name: "IsInvoiced",
            table: "LogoOrders",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "BillingPeriod",
            table: "Invoices",
            type: "varchar(100)",
            maxLength: 100,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<int>(
            name: "BillingType",
            table: "ClientProfiles",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9634));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9649));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9658));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9673));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9684));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9692));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9710));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9722));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9733));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9745));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9824));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9835));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9846));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9861));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 389, DateTimeKind.Utc).AddTicks(9874));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 396, DateTimeKind.Utc).AddTicks(4246));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 396, DateTimeKind.Utc).AddTicks(4626));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 396, DateTimeKind.Utc).AddTicks(4644));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 22, 30, 34, 396, DateTimeKind.Utc).AddTicks(4651));

        migrationBuilder.CreateIndex(
            name: "IX_LogoOrders_ClientId_BillingEligible_IsInvoiced",
            table: "LogoOrders",
            columns: new[] { "ClientId", "BillingEligible", "IsInvoiced" });

        // Backfill: Completed orders (Status=7) with InvoiceOrder -> IsInvoiced=true, BillingEligible=true, InvoiceId
        migrationBuilder.Sql(@"
                UPDATE LogoOrders o
                INNER JOIN (SELECT OrderId, MIN(InvoiceId) as InvoiceId FROM InvoiceOrders WHERE OrderId IS NOT NULL AND IsDeleted = 0 GROUP BY OrderId) io ON io.OrderId = o.Id
                SET o.BillingEligible = 1, o.IsInvoiced = 1, o.CompletedDate = COALESCE(o.UpdatedAt, o.CreatedAt), o.InvoiceId = io.InvoiceId
                WHERE o.Status = 7 AND o.IsDeleted = 0;
            ");

        // Backfill: Completed orders without invoice -> BillingEligible=true, IsInvoiced=false
        migrationBuilder.Sql(@"
                UPDATE LogoOrders o
                LEFT JOIN (SELECT DISTINCT OrderId FROM InvoiceOrders WHERE OrderId IS NOT NULL AND IsDeleted = 0) io ON io.OrderId = o.Id
                SET o.BillingEligible = 1, o.CompletedDate = COALESCE(o.UpdatedAt, o.CreatedAt)
                WHERE o.Status = 7 AND o.IsDeleted = 0 AND io.OrderId IS NULL;
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_LogoOrders_ClientId_BillingEligible_IsInvoiced",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "BillingEligible",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "CompletedDate",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "InvoiceId",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "IsInvoiced",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "BillingPeriod",
            table: "Invoices");

        migrationBuilder.DropColumn(
            name: "BillingType",
            table: "ClientProfiles");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(949));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(954));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(957));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(960));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(965));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(968));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(971));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(974));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(977));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(981));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(983));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(986));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(989));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(992));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 222, DateTimeKind.Utc).AddTicks(1006));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 223, DateTimeKind.Utc).AddTicks(4209));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 223, DateTimeKind.Utc).AddTicks(4213));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 223, DateTimeKind.Utc).AddTicks(4217));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 21, 36, 46, 223, DateTimeKind.Utc).AddTicks(4220));
    }
}
