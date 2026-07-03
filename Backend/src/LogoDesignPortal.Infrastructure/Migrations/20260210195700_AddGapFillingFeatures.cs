using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddGapFillingFeatures : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "DeactivatedAt",
            table: "Users",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "DeactivatedBy",
            table: "Users",
            type: "char(36)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "FailedLoginAttempts",
            table: "Users",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "LockoutEnd",
            table: "Users",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Notes",
            table: "DesignerProfiles",
            type: "longtext",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Notes",
            table: "ClientProfiles",
            type: "longtext",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                EntityType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                EntityId = table.Column<Guid>(type: "char(36)", nullable: false),
                Action = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                PreviousValue = table.Column<string>(type: "longtext", nullable: true),
                NewValue = table.Column<string>(type: "longtext", nullable: true),
                PerformedByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                PerformedByRole = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
            });

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2922));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2926));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2927));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2980));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2982));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2983));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2985));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2988));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2989));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2991));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2993));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2994));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2996));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2998));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 854, DateTimeKind.Utc).AddTicks(2999));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 855, DateTimeKind.Utc).AddTicks(380));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 855, DateTimeKind.Utc).AddTicks(382));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 855, DateTimeKind.Utc).AddTicks(384));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 56, 59, 855, DateTimeKind.Utc).AddTicks(385));

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_EntityType_EntityId",
            table: "AuditLogs",
            columns: new[] { "EntityType", "EntityId" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_PerformedByUserId",
            table: "AuditLogs",
            column: "PerformedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_Timestamp",
            table: "AuditLogs",
            column: "Timestamp");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AuditLogs");

        migrationBuilder.DropColumn(
            name: "DeactivatedAt",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "DeactivatedBy",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "FailedLoginAttempts",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "LockoutEnd",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "Notes",
            table: "DesignerProfiles");

        migrationBuilder.DropColumn(
            name: "Notes",
            table: "ClientProfiles");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4641));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4644));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4646));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4648));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4650));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4652));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4654));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4655));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4657));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4659));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4661));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4662));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4664));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4666));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 921, DateTimeKind.Utc).AddTicks(4674));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 922, DateTimeKind.Utc).AddTicks(1232));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 922, DateTimeKind.Utc).AddTicks(1238));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 922, DateTimeKind.Utc).AddTicks(1240));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 10, 19, 22, 30, 922, DateTimeKind.Utc).AddTicks(1242));
    }
}
