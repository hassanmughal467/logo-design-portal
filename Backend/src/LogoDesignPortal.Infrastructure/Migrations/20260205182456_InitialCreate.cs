using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Roles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
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
                table.PrimaryKey("PK_Roles", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                FirstName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                PasswordHash = table.Column<string>(type: "longtext", nullable: false),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                RefreshToken = table.Column<string>(type: "longtext", nullable: true),
                RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
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
                table.PrimaryKey("PK_Users", x => x.Id);
                table.ForeignKey(
                    name: "FK_Users_Roles_RoleId",
                    column: x => x.RoleId,
                    principalTable: "Roles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ClientProfiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                CompanyName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                Country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                PostalCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
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
                table.PrimaryKey("PK_ClientProfiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_ClientProfiles_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DesignerProfiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                Specialization = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                Bio = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                HourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                IsAvailable = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
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
                table.PrimaryKey("PK_DesignerProfiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_DesignerProfiles_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "LogoOrders",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                ClientId = table.Column<Guid>(type: "char(36)", nullable: false),
                DesignerId = table.Column<Guid>(type: "char(36)", nullable: true),
                Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Deadline = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                Requirements = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                ColorPreferences = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                StylePreferences = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
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
                table.PrimaryKey("PK_LogoOrders", x => x.Id);
                table.ForeignKey(
                    name: "FK_LogoOrders_ClientProfiles_ClientId",
                    column: x => x.ClientId,
                    principalTable: "ClientProfiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_LogoOrders_DesignerProfiles_DesignerId",
                    column: x => x.DesignerId,
                    principalTable: "DesignerProfiles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "Invoices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                OrderId = table.Column<Guid>(type: "char(36)", nullable: false),
                InvoiceNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                IssueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsPaid = table.Column<bool>(type: "tinyint(1)", nullable: false),
                PaymentMethod = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
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
                table.PrimaryKey("PK_Invoices", x => x.Id);
                table.ForeignKey(
                    name: "FK_Invoices_LogoOrders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "LogoOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "LogoFiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                OrderId = table.Column<Guid>(type: "char(36)", nullable: false),
                FileName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                OriginalFileName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                FilePath = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                ContentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                FileSize = table.Column<long>(type: "bigint", nullable: false),
                IsFinalVersion = table.Column<bool>(type: "tinyint(1)", nullable: false),
                Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                UploadedBy = table.Column<Guid>(type: "char(36)", nullable: false),
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
                table.PrimaryKey("PK_LogoFiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_LogoFiles_LogoOrders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "LogoOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrderStatusHistories",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                OrderId = table.Column<Guid>(type: "char(36)", nullable: false),
                PreviousStatus = table.Column<int>(type: "int", nullable: false),
                NewStatus = table.Column<int>(type: "int", nullable: false),
                Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                ChangedBy = table.Column<Guid>(type: "char(36)", nullable: false),
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
                table.PrimaryKey("PK_OrderStatusHistories", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrderStatusHistories_LogoOrders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "LogoOrders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.InsertData(
            table: "Roles",
            columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
            values: new object[,]
            {
                { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 2, 5, 18, 24, 55, 680, DateTimeKind.Utc).AddTicks(2061), null, null, null, "Full system access with all permissions", false, "SuperAdmin", null, null },
                { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2026, 2, 5, 18, 24, 55, 680, DateTimeKind.Utc).AddTicks(2068), null, null, null, "Administrative access with restricted client data access", false, "Admin", null, null },
                { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2026, 2, 5, 18, 24, 55, 680, DateTimeKind.Utc).AddTicks(2073), null, null, null, "Designer access without client identity information", false, "Designer", null, null },
                { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2026, 2, 5, 18, 24, 55, 680, DateTimeKind.Utc).AddTicks(2079), null, null, null, "Client access to their own data", false, "Client", null, null }
            });

        migrationBuilder.CreateIndex(
            name: "IX_ClientProfiles_UserId",
            table: "ClientProfiles",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DesignerProfiles_UserId",
            table: "DesignerProfiles",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Invoices_InvoiceNumber",
            table: "Invoices",
            column: "InvoiceNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Invoices_OrderId",
            table: "Invoices",
            column: "OrderId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_LogoFiles_OrderId",
            table: "LogoFiles",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_LogoOrders_ClientId",
            table: "LogoOrders",
            column: "ClientId");

        migrationBuilder.CreateIndex(
            name: "IX_LogoOrders_DesignerId",
            table: "LogoOrders",
            column: "DesignerId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderStatusHistories_OrderId",
            table: "OrderStatusHistories",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_Roles_Name",
            table: "Roles",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_RoleId",
            table: "Users",
            column: "RoleId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Invoices");

        migrationBuilder.DropTable(
            name: "LogoFiles");

        migrationBuilder.DropTable(
            name: "OrderStatusHistories");

        migrationBuilder.DropTable(
            name: "LogoOrders");

        migrationBuilder.DropTable(
            name: "ClientProfiles");

        migrationBuilder.DropTable(
            name: "DesignerProfiles");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "Roles");
    }
}
