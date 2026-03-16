using MySqlConnector;

var connectionString = args.Length > 0
    ? string.Join(" ", args)
    : "Server=127.0.0.1;Port=3306;Database=LogoDesignPortalDb;User=root;Password=ADMIN;";

await using var conn = new MySqlConnection(connectionString);
await conn.OpenAsync();

// 1. Mark AddDesignerPayoutAndPricing as applied (fixes duplicate column error)
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
        VALUES ('20260308224609_AddDesignerPayoutAndPricing', '8.0.0');
        """;
    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine("Marked AddDesignerPayoutAndPricing as applied.");
}

// 2. Create DesignerInvoices if not exists (no FK to avoid collation issues)
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS `DesignerInvoices` (
            `Id` char(36) NOT NULL,
            `DesignerId` char(36) NOT NULL,
            `InvoiceNumber` varchar(50) NOT NULL,
            `TotalAmount` decimal(18,2) NOT NULL,
            `Status` int NOT NULL DEFAULT 0,
            `BillingPeriod` varchar(50) NOT NULL,
            `IssueDate` datetime(6) NOT NULL,
            `PaidDate` datetime(6) NULL,
            `Notes` varchar(1000) NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `UpdatedAt` datetime(6) NULL,
            `CreatedBy` char(36) NULL,
            `UpdatedBy` char(36) NULL,
            `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
            `DeletedAt` datetime(6) NULL,
            `DeletedBy` char(36) NULL,
            PRIMARY KEY (`Id`)
        );
        """;
    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine("Ensured DesignerInvoices table exists.");
}

// 3. Create DesignerInvoiceItems if not exists
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS `DesignerInvoiceItems` (
            `Id` char(36) NOT NULL,
            `DesignerInvoiceId` char(36) NOT NULL,
            `OrderId` char(36) NOT NULL,
            `Description` varchar(500) NOT NULL,
            `Amount` decimal(18,2) NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `UpdatedAt` datetime(6) NULL,
            `CreatedBy` char(36) NULL,
            `UpdatedBy` char(36) NULL,
            `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
            `DeletedAt` datetime(6) NULL,
            `DeletedBy` char(36) NULL,
            PRIMARY KEY (`Id`),
            CONSTRAINT `FK_DesignerInvoiceItems_DesignerInvoiceId` FOREIGN KEY (`DesignerInvoiceId`) REFERENCES `DesignerInvoices` (`Id`) ON DELETE CASCADE,
            CONSTRAINT `FK_DesignerInvoiceItems_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `LogoOrders` (`Id`) ON DELETE RESTRICT
        );
        """;
    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine("Ensured DesignerInvoiceItems table exists.");
}

// 4. Create DesignerInvoiceAdjustments if not exists
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS `DesignerInvoiceAdjustments` (
            `Id` char(36) NOT NULL,
            `DesignerInvoiceId` char(36) NOT NULL,
            `Description` varchar(500) NOT NULL,
            `Amount` decimal(18,2) NOT NULL,
            `CreatedAt` datetime(6) NOT NULL,
            `UpdatedAt` datetime(6) NULL,
            `CreatedBy` char(36) NULL,
            `UpdatedBy` char(36) NULL,
            `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
            `DeletedAt` datetime(6) NULL,
            `DeletedBy` char(36) NULL,
            PRIMARY KEY (`Id`),
            CONSTRAINT `FK_DesignerInvoiceAdjustments_DesignerInvoiceId` FOREIGN KEY (`DesignerInvoiceId`) REFERENCES `DesignerInvoices` (`Id`) ON DELETE CASCADE
        );
        """;
    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine("Ensured DesignerInvoiceAdjustments table exists.");
}

// 5. Add StandardPrice to LogoOrders if not exists
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'LogoOrders' AND COLUMN_NAME = 'StandardPrice'";
    var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
    if (count == 0)
    {
        cmd.CommandText = "ALTER TABLE `LogoOrders` ADD `StandardPrice` decimal(18,2) NULL";
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine("Added StandardPrice column to LogoOrders.");
    }
    else
    {
        Console.WriteLine("StandardPrice column already exists.");
    }
}

// 6. Mark our migration as applied
using (var cmd = conn.CreateCommand())
{
    cmd.CommandText = """
        INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
        VALUES ('20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment', '8.0.0');
        """;
    await cmd.ExecuteNonQueryAsync();
    Console.WriteLine("Marked AddStandardPriceAndDesignerInvoiceAdjustment as applied.");
}

Console.WriteLine("Migration applied successfully.");
