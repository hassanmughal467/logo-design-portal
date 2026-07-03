using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class RefactorOrderWorkflowRemoveUnusedStatuses : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Migrate orders with removed status values (Pending=10, Paid=11, Processing=12, Failed=16, Archived=17) to CancelledByAdmin (14)
        migrationBuilder.Sql(@"
                UPDATE LogoOrders
                SET Status = 14
                WHERE Status IN (10, 11, 12, 16, 17)
                AND IsDeleted = 0;
            ");

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

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6466));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6469));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6472));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6535));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6539));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6541));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6544));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6547));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6549));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6552));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6554));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6557));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6559));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6562));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 773, DateTimeKind.Utc).AddTicks(6573));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 775, DateTimeKind.Utc).AddTicks(981));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 775, DateTimeKind.Utc).AddTicks(986));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 775, DateTimeKind.Utc).AddTicks(989));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 8, 3, 47, 58, 775, DateTimeKind.Utc).AddTicks(992));
    }
}
