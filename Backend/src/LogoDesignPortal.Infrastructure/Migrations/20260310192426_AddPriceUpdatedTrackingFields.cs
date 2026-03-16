using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceUpdatedTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PriceUpdatedAt",
                table: "LogoOrders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceUpdatedByRole",
                table: "LogoOrders",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "PriceUpdatedByUserId",
                table: "LogoOrders",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9360));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9367));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9370));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9374));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9378));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9382));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9386));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9390));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9393));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9397));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9401));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9404));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9407));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9410));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 267, DateTimeKind.Utc).AddTicks(9422));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 269, DateTimeKind.Utc).AddTicks(3015));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 269, DateTimeKind.Utc).AddTicks(3020));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 269, DateTimeKind.Utc).AddTicks(3023));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 19, 24, 24, 269, DateTimeKind.Utc).AddTicks(3026));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceUpdatedAt",
                table: "LogoOrders");

            migrationBuilder.DropColumn(
                name: "PriceUpdatedByRole",
                table: "LogoOrders");

            migrationBuilder.DropColumn(
                name: "PriceUpdatedByUserId",
                table: "LogoOrders");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2219));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2228));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2234));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2238));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2242));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2247));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2252));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2256));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2261));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2316));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2320));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2326));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2331));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2337));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 938, DateTimeKind.Utc).AddTicks(2352));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 940, DateTimeKind.Utc).AddTicks(1058));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 940, DateTimeKind.Utc).AddTicks(1063));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 940, DateTimeKind.Utc).AddTicks(1068));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 0, 33, 47, 940, DateTimeKind.Utc).AddTicks(1073));
        }
    }
}
