using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPermissionSystem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Permissions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                Resource = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                Action = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
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
                table.PrimaryKey("PK_Permissions", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "RolePermissions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
                PermissionId = table.Column<Guid>(type: "char(36)", nullable: false),
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
                table.PrimaryKey("PK_RolePermissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_RolePermissions_Permissions_PermissionId",
                    column: x => x.PermissionId,
                    principalTable: "Permissions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RolePermissions_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            table: "Permissions",
            columns: new[] { "Id", "Action", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "Resource", "UpdatedAt", "UpdatedBy" },
            values: new object[,]
            {
                { new Guid("10000000-0000-0000-0000-000000000001"), "Create", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5634), null, null, null, "Create new users", false, "CreateUser", "User", null, null },
                { new Guid("10000000-0000-0000-0000-000000000002"), "Read", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5646), null, null, null, "View all users", false, "ViewUsers", "User", null, null },
                { new Guid("10000000-0000-0000-0000-000000000003"), "Update", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5651), null, null, null, "Update user information", false, "UpdateUser", "User", null, null },
                { new Guid("10000000-0000-0000-0000-000000000004"), "Delete", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5656), null, null, null, "Delete users", false, "DeleteUser", "User", null, null },
                { new Guid("20000000-0000-0000-0000-000000000001"), "Create", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5660), null, null, null, "Create designer profiles", false, "CreateDesignerProfile", "DesignerProfile", null, null },
                { new Guid("20000000-0000-0000-0000-000000000002"), "Read", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5666), null, null, null, "View designer profiles", false, "ViewDesignerProfiles", "DesignerProfile", null, null },
                { new Guid("20000000-0000-0000-0000-000000000003"), "Update", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5670), null, null, null, "Update designer profiles", false, "UpdateDesignerProfile", "DesignerProfile", null, null },
                { new Guid("30000000-0000-0000-0000-000000000001"), "Create", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5674), null, null, null, "Create new orders", false, "CreateOrder", "Order", null, null },
                { new Guid("30000000-0000-0000-0000-000000000002"), "ReadAll", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5679), null, null, null, "View all orders in the system", false, "ViewAllOrders", "Order", null, null },
                { new Guid("30000000-0000-0000-0000-000000000003"), "Assign", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5683), null, null, null, "Assign orders to designers", false, "AssignOrder", "Order", null, null },
                { new Guid("30000000-0000-0000-0000-000000000004"), "UpdateStatus", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5687), null, null, null, "Update order status", false, "UpdateOrderStatus", "Order", null, null },
                { new Guid("40000000-0000-0000-0000-000000000001"), "Manage", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5693), null, null, null, "Manage role permissions", false, "ManagePermissions", "Permission", null, null },
                { new Guid("50000000-0000-0000-0000-000000000001"), "Upload", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5755), null, null, null, "Upload files", false, "UploadFile", "File", null, null },
                { new Guid("50000000-0000-0000-0000-000000000002"), "Download", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5763), null, null, null, "Download files", false, "DownloadFile", "File", null, null },
                { new Guid("50000000-0000-0000-0000-000000000003"), "Delete", new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(5768), null, null, null, "Delete files", false, "DeleteFile", "File", null, null }
            });

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(9161));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(9167));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(9171));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 6, 12, 46, 49, 134, DateTimeKind.Utc).AddTicks(9175));

        migrationBuilder.CreateIndex(
            name: "IX_Permissions_Name",
            table: "Permissions",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_PermissionId",
            table: "RolePermissions",
            column: "PermissionId");

        migrationBuilder.CreateIndex(
            name: "IX_RolePermissions_RoleId_PermissionId",
            table: "RolePermissions",
            columns: new[] { "RoleId", "PermissionId" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RolePermissions");

        migrationBuilder.DropTable(
            name: "Permissions");

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 5, 18, 24, 55, 680, DateTimeKind.Utc).AddTicks(2061));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 5, 18, 24, 55, 680, DateTimeKind.Utc).AddTicks(2068));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 5, 18, 24, 55, 680, DateTimeKind.Utc).AddTicks(2073));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 5, 18, 24, 55, 680, DateTimeKind.Utc).AddTicks(2079));
    }
}
