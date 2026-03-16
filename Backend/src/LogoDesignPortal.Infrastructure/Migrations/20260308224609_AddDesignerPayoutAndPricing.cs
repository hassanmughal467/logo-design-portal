using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignerPayoutAndPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedPrice",
                table: "LogoOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DesignCategory",
                table: "LogoOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DesignType",
                table: "LogoOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DesignerInvoiceId",
                table: "LogoOrders",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
                name: "IsDesignerInvoiced",
                table: "LogoOrders",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PriceApprovalStatus",
                table: "LogoOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DesignerInvoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DesignerId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    InvoiceNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    BillingPeriod = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IssueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignerInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignerInvoices_DesignerProfiles_DesignerId",
                        column: x => x.DesignerId,
                        principalTable: "DesignerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DesignerInvoiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    DesignerInvoiceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrderId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignerInvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignerInvoiceItems_DesignerInvoices_DesignerInvoiceId",
                        column: x => x.DesignerInvoiceId,
                        principalTable: "DesignerInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DesignerInvoiceItems_LogoOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "LogoOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(1461));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(1479));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2048));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2061));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2071));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2082));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2091));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2100));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2109));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2117));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2187));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2197));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2207));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2216));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 59, DateTimeKind.Utc).AddTicks(2225));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 74, DateTimeKind.Utc).AddTicks(9703));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 74, DateTimeKind.Utc).AddTicks(9718));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 74, DateTimeKind.Utc).AddTicks(9729));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 8, 22, 46, 2, 74, DateTimeKind.Utc).AddTicks(9739));

            migrationBuilder.CreateIndex(
                name: "IX_LogoOrders_DesignerId_IsDesignerInvoiced",
                table: "LogoOrders",
                columns: new[] { "DesignerId", "IsDesignerInvoiced" });

            migrationBuilder.CreateIndex(
                name: "IX_LogoOrders_DesignerInvoiceId",
                table: "LogoOrders",
                column: "DesignerInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignerInvoiceItems_DesignerInvoiceId_OrderId",
                table: "DesignerInvoiceItems",
                columns: new[] { "DesignerInvoiceId", "OrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DesignerInvoiceItems_OrderId",
                table: "DesignerInvoiceItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignerInvoices_DesignerId_BillingPeriod",
                table: "DesignerInvoices",
                columns: new[] { "DesignerId", "BillingPeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_DesignerInvoices_InvoiceNumber",
                table: "DesignerInvoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LogoOrders_DesignerInvoices_DesignerInvoiceId",
                table: "LogoOrders",
                column: "DesignerInvoiceId",
                principalTable: "DesignerInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogoOrders_DesignerInvoices_DesignerInvoiceId",
                table: "LogoOrders");

            migrationBuilder.DropTable(
                name: "DesignerInvoiceItems");

            migrationBuilder.DropTable(
                name: "DesignerInvoices");

            migrationBuilder.DropIndex(
                name: "IX_LogoOrders_DesignerId_IsDesignerInvoiced",
                table: "LogoOrders");

            migrationBuilder.DropIndex(
                name: "IX_LogoOrders_DesignerInvoiceId",
                table: "LogoOrders");

            migrationBuilder.DropColumn(
                name: "ApprovedPrice",
                table: "LogoOrders");

            migrationBuilder.DropColumn(
                name: "DesignCategory",
                table: "LogoOrders");

            migrationBuilder.DropColumn(
                name: "DesignType",
                table: "LogoOrders");

            migrationBuilder.DropColumn(
                name: "DesignerInvoiceId",
                table: "LogoOrders");

            migrationBuilder.DropColumn(
                name: "IsDesignerInvoiced",
                table: "LogoOrders");

            migrationBuilder.DropColumn(
                name: "PriceApprovalStatus",
                table: "LogoOrders");

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
        }
    }
}
