using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddPricingWorkflowFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Fix NULL CurrencyCode before making column NOT NULL
        migrationBuilder.Sql("UPDATE LogoOrders SET CurrencyCode = 'USD' WHERE CurrencyCode IS NULL;");

        migrationBuilder.AlterColumn<string>(
            name: "CurrencyCode",
            table: "LogoOrders",
            type: "varchar(3)",
            maxLength: 3,
            nullable: false,
            defaultValue: "USD",
            oldClrType: typeof(string),
            oldType: "varchar(3)",
            oldMaxLength: 3,
            oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<decimal>(
            name: "ClientBasePrice",
            table: "LogoOrders",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "ClientChargePrice",
            table: "LogoOrders",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "DesignerApprovedPrice",
            table: "LogoOrders",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: true);

        migrationBuilder.AddColumn<decimal>(
            name: "DesignerProposedPrice",
            table: "LogoOrders",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: true);

        // Backfill existing orders: ClientBasePrice/ClientChargePrice from ClientPrice or Price; DesignerProposedPrice/DesignerApprovedPrice from ProposedPrice/ApprovedPrice
        migrationBuilder.Sql(@"
                UPDATE LogoOrders
                SET ClientBasePrice = COALESCE(ClientPrice, Price, 0),
                    ClientChargePrice = COALESCE(ClientPrice, Price, 0),
                    DesignerProposedPrice = ProposedPrice,
                    DesignerApprovedPrice = ApprovedPrice;
            ");

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

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ClientBasePrice",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "ClientChargePrice",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "DesignerApprovedPrice",
            table: "LogoOrders");

        migrationBuilder.DropColumn(
            name: "DesignerProposedPrice",
            table: "LogoOrders");

        migrationBuilder.AlterColumn<string>(
            name: "CurrencyCode",
            table: "LogoOrders",
            type: "varchar(3)",
            maxLength: 3,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(3)",
            oldMaxLength: 3,
            oldDefaultValue: "USD")
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9713));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9720));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9725));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9730));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9735));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9740));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9747));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9752));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9755));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9761));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9767));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9773));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9777));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9781));

        migrationBuilder.UpdateData(
            table: "Permissions",
            keyColumn: "Id",
            keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 750, DateTimeKind.Utc).AddTicks(9791));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 753, DateTimeKind.Utc).AddTicks(4240));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 753, DateTimeKind.Utc).AddTicks(4247));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 753, DateTimeKind.Utc).AddTicks(4251));

        migrationBuilder.UpdateData(
            table: "Roles",
            keyColumn: "Id",
            keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
            column: "CreatedAt",
            value: new DateTime(2026, 3, 9, 21, 48, 2, 753, DateTimeKind.Utc).AddTicks(4256));
    }
}
