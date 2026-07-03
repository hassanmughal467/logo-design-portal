using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddOrderCommentSecurityAndVisibility : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "CommentType",
            table: "OrderComments",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<bool>(
            name: "IsReadByAdmin",
            table: "OrderComments",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsReadByClient",
            table: "OrderComments",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsReadByDesigner",
            table: "OrderComments",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "VisibleToClient",
            table: "OrderComments",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

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

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CommentType",
            table: "OrderComments");

        migrationBuilder.DropColumn(
            name: "IsReadByAdmin",
            table: "OrderComments");

        migrationBuilder.DropColumn(
            name: "IsReadByClient",
            table: "OrderComments");

        migrationBuilder.DropColumn(
            name: "IsReadByDesigner",
            table: "OrderComments");

        migrationBuilder.DropColumn(
            name: "VisibleToClient",
            table: "OrderComments");

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
}
