using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogoDesignPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotesSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET @quote_id_col_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'LogoOrders'
                      AND COLUMN_NAME = 'QuoteId'
                );
                SET @quote_id_add_sql := IF(
                    @quote_id_col_exists = 0,
                    'ALTER TABLE `LogoOrders` ADD `QuoteId` char(36) NULL',
                    'SELECT 1'
                );
                PREPARE quote_id_add_stmt FROM @quote_id_add_sql;
                EXECUTE quote_id_add_stmt;
                DEALLOCATE PREPARE quote_id_add_stmt;
                """);

            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS `Quotes` (
                    `Id` char(36) NOT NULL,
                    `ClientId` char(36) NOT NULL,
                    `LogoName` varchar(200) NOT NULL,
                    `Description` varchar(2000) NOT NULL,
                    `AttachmentsJson` longtext NULL,
                    `RequestedBudget` decimal(18,2) NULL,
                    `AdminQuotedPrice` decimal(18,2) NULL,
                    `AdminNotes` varchar(2000) NULL,
                    `Status` int NOT NULL DEFAULT 1,
                    `ConvertedOrderId` char(36) NULL,
                    `CreatedAt` datetime(6) NOT NULL,
                    `UpdatedAt` datetime(6) NULL,
                    `CreatedBy` char(36) NULL,
                    `UpdatedBy` char(36) NULL,
                    `IsDeleted` tinyint(1) NOT NULL,
                    `DeletedAt` datetime(6) NULL,
                    `DeletedBy` char(36) NULL,
                    PRIMARY KEY (`Id`)
                ) CHARACTER SET utf8mb4;
                """);

            migrationBuilder.Sql(
                """
                SET @cp_charset := (
                    SELECT CHARACTER_SET_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'ClientProfiles'
                      AND COLUMN_NAME = 'Id'
                    LIMIT 1
                );
                SET @cp_collation := (
                    SELECT COLLATION_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'ClientProfiles'
                      AND COLUMN_NAME = 'Id'
                    LIMIT 1
                );
                SET @quotes_clientid_sql := CONCAT(
                    'ALTER TABLE `Quotes` MODIFY COLUMN `ClientId` char(36) CHARACTER SET ',
                    @cp_charset,
                    ' COLLATE ',
                    @cp_collation,
                    ' NOT NULL'
                );
                PREPARE quotes_clientid_stmt FROM @quotes_clientid_sql;
                EXECUTE quotes_clientid_stmt;
                DEALLOCATE PREPARE quotes_clientid_stmt;
                """);

            migrationBuilder.Sql(
                """
                SET @fk_quotes_clients_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE()
                      AND CONSTRAINT_NAME = 'FK_Quotes_ClientProfiles_ClientId'
                      AND TABLE_NAME = 'Quotes'
                );
                SET @fk_quotes_clients_sql := IF(
                    @fk_quotes_clients_exists = 0,
                    'ALTER TABLE `Quotes` ADD CONSTRAINT `FK_Quotes_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `ClientProfiles` (`Id`) ON DELETE RESTRICT',
                    'SELECT 1'
                );
                PREPARE fk_quotes_clients_stmt FROM @fk_quotes_clients_sql;
                EXECUTE fk_quotes_clients_stmt;
                DEALLOCATE PREPARE fk_quotes_clients_stmt;
                """);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5465));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5470));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5475));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5481));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5484));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5488));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5492));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5495));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5498));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5501));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5504));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5507));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5510));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5513));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5524));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 93, DateTimeKind.Utc).AddTicks(5527));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 96, DateTimeKind.Utc).AddTicks(9768));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 96, DateTimeKind.Utc).AddTicks(9772));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 96, DateTimeKind.Utc).AddTicks(9775));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 52, 0, 96, DateTimeKind.Utc).AddTicks(9779));

            migrationBuilder.Sql(
                """
                SET @ix_logoorders_quoteid_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'LogoOrders'
                      AND INDEX_NAME = 'IX_LogoOrders_QuoteId'
                );
                SET @ix_logoorders_quoteid_sql := IF(
                    @ix_logoorders_quoteid_exists = 0,
                    'CREATE UNIQUE INDEX `IX_LogoOrders_QuoteId` ON `LogoOrders` (`QuoteId`)',
                    'SELECT 1'
                );
                PREPARE ix_logoorders_quoteid_stmt FROM @ix_logoorders_quoteid_sql;
                EXECUTE ix_logoorders_quoteid_stmt;
                DEALLOCATE PREPARE ix_logoorders_quoteid_stmt;
                """);

            migrationBuilder.Sql(
                """
                SET @ix_quotes_clientid_status_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Quotes'
                      AND INDEX_NAME = 'IX_Quotes_ClientId_Status'
                );
                SET @ix_quotes_clientid_status_sql := IF(
                    @ix_quotes_clientid_status_exists = 0,
                    'CREATE INDEX `IX_Quotes_ClientId_Status` ON `Quotes` (`ClientId`, `Status`)',
                    'SELECT 1'
                );
                PREPARE ix_quotes_clientid_status_stmt FROM @ix_quotes_clientid_status_sql;
                EXECUTE ix_quotes_clientid_status_stmt;
                DEALLOCATE PREPARE ix_quotes_clientid_status_stmt;
                """);

            migrationBuilder.Sql(
                """
                SET @ix_quotes_createdat_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.STATISTICS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Quotes'
                      AND INDEX_NAME = 'IX_Quotes_CreatedAt'
                );
                SET @ix_quotes_createdat_sql := IF(
                    @ix_quotes_createdat_exists = 0,
                    'CREATE INDEX `IX_Quotes_CreatedAt` ON `Quotes` (`CreatedAt`)',
                    'SELECT 1'
                );
                PREPARE ix_quotes_createdat_stmt FROM @ix_quotes_createdat_sql;
                EXECUTE ix_quotes_createdat_stmt;
                DEALLOCATE PREPARE ix_quotes_createdat_stmt;
                """);

            migrationBuilder.Sql(
                """
                SET @quotes_charset := (
                    SELECT CHARACTER_SET_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Quotes'
                      AND COLUMN_NAME = 'Id'
                    LIMIT 1
                );
                SET @quotes_collation := (
                    SELECT COLLATION_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'Quotes'
                      AND COLUMN_NAME = 'Id'
                    LIMIT 1
                );
                SET @logoorders_quoteid_sql := CONCAT(
                    'ALTER TABLE `LogoOrders` MODIFY COLUMN `QuoteId` char(36) CHARACTER SET ',
                    @quotes_charset,
                    ' COLLATE ',
                    @quotes_collation,
                    ' NULL'
                );
                PREPARE logoorders_quoteid_stmt FROM @logoorders_quoteid_sql;
                EXECUTE logoorders_quoteid_stmt;
                DEALLOCATE PREPARE logoorders_quoteid_stmt;
                """);

            migrationBuilder.Sql(
                """
                SET @fk_logoorders_quotes_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE()
                      AND CONSTRAINT_NAME = 'FK_LogoOrders_Quotes_QuoteId'
                      AND TABLE_NAME = 'LogoOrders'
                );
                SET @fk_logoorders_quotes_sql := IF(
                    @fk_logoorders_quotes_exists = 0,
                    'ALTER TABLE `LogoOrders` ADD CONSTRAINT `FK_LogoOrders_Quotes_QuoteId` FOREIGN KEY (`QuoteId`) REFERENCES `Quotes` (`Id`) ON DELETE SET NULL',
                    'SELECT 1'
                );
                PREPARE fk_logoorders_quotes_stmt FROM @fk_logoorders_quotes_sql;
                EXECUTE fk_logoorders_quotes_stmt;
                DEALLOCATE PREPARE fk_logoorders_quotes_stmt;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LogoOrders_Quotes_QuoteId",
                table: "LogoOrders");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_LogoOrders_QuoteId",
                table: "LogoOrders");

            migrationBuilder.Sql(
                """
                SET @quote_id_col_exists := (
                    SELECT COUNT(*)
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = DATABASE()
                      AND TABLE_NAME = 'LogoOrders'
                      AND COLUMN_NAME = 'QuoteId'
                );
                SET @quote_id_drop_sql := IF(
                    @quote_id_col_exists > 0,
                    'ALTER TABLE `LogoOrders` DROP COLUMN `QuoteId`',
                    'SELECT 1'
                );
                PREPARE quote_id_drop_stmt FROM @quote_id_drop_sql;
                EXECUTE quote_id_drop_stmt;
                DEALLOCATE PREPARE quote_id_drop_stmt;
                """);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(1818));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2062));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2065));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2070));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2072));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2076));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2078));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2082));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2085));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2088));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2091));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2093));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2095));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2098));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2115));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 613, DateTimeKind.Utc).AddTicks(2118));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 615, DateTimeKind.Utc).AddTicks(4277));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 615, DateTimeKind.Utc).AddTicks(4281));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 615, DateTimeKind.Utc).AddTicks(4283));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 17, 33, 8, 615, DateTimeKind.Utc).AddTicks(4285));
        }
    }
}
