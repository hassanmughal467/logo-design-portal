using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPaymentEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                InvoiceId = table.Column<Guid>(type: "char(36)", nullable: false),
                PaymentMethod = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, defaultValue: "USD"),
                Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                PaymentLink = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                TransactionId = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                ErrorMessage = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                ReturnUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                CancelUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                InvoiceId1 = table.Column<Guid>(type: "char(36)", nullable: true),
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
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Payments_Invoices_InvoiceId",
                    column: x => x.InvoiceId,
                    principalTable: "Invoices",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Payments_Invoices_InvoiceId1",
                    column: x => x.InvoiceId1,
                    principalTable: "Invoices",
                    principalColumn: "Id");
            });

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1685));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1691));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1694));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1697));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1699));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1702));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1704));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1707));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1709));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1711));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1714));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1717));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1720));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1723));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 56, DateTimeKind.Utc).AddTicks(1738));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 57, DateTimeKind.Utc).AddTicks(1386));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 57, DateTimeKind.Utc).AddTicks(1390));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 57, DateTimeKind.Utc).AddTicks(1392));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 18, 22, 55, 57, DateTimeKind.Utc).AddTicks(1395));

        migrationBuilder.CreateIndex(
            name: "IX_Payments_CreatedAt",
            table: "Payments",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_InvoiceId",
            table: "Payments",
            column: "InvoiceId");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_InvoiceId1",
            table: "Payments",
            column: "InvoiceId1");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_Status",
            table: "Payments",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_TransactionId",
            table: "Payments",
            column: "TransactionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Payments");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8215));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8219));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8223));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8226));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8229));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8233));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8236));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8239));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8242));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8245));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8247));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8250));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8254));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8258));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 863, DateTimeKind.Utc).AddTicks(8268));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 864, DateTimeKind.Utc).AddTicks(8812));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 864, DateTimeKind.Utc).AddTicks(8817));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 864, DateTimeKind.Utc).AddTicks(8824));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 2, 12, 17, 47, 39, 864, DateTimeKind.Utc).AddTicks(8827));
    }
}
