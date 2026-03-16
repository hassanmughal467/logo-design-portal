using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SetVisibleToClientForExistingNonInternalComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill: existing non-internal comments were previously visible to clients
            migrationBuilder.Sql("UPDATE OrderComments SET VisibleToClient = 1 WHERE IsInternal = 0;");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert: set VisibleToClient = 0 for non-Designer comments (we can't reverse perfectly)
            migrationBuilder.Sql("UPDATE OrderComments SET VisibleToClient = 0 WHERE CommentType != 3;");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4117));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4124));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4129));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4133));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4137));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4142));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4149));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4153));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4157));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4160));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4164));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4168));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4172));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4176));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 635, DateTimeKind.Utc).AddTicks(4187));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 641, DateTimeKind.Utc).AddTicks(6228));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 641, DateTimeKind.Utc).AddTicks(6236));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 641, DateTimeKind.Utc).AddTicks(6241));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 21, 29, 54, 641, DateTimeKind.Utc).AddTicks(6246));
        }
    }
}
