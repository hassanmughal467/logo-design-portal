-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: logodesignportaldb
-- ------------------------------------------------------
-- Server version	8.0.45

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `auditlogs`
--

DROP TABLE IF EXISTS `auditlogs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `auditlogs` (
  `Id` char(36) NOT NULL,
  `EntityType` varchar(50) NOT NULL,
  `EntityId` char(36) NOT NULL,
  `Action` varchar(50) NOT NULL,
  `PreviousValue` longtext,
  `NewValue` longtext,
  `PerformedByUserId` char(36) DEFAULT NULL,
  `PerformedByRole` varchar(50) DEFAULT NULL,
  `Timestamp` datetime(6) NOT NULL,
  `Notes` varchar(1000) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_AuditLogs_EntityType_EntityId` (`EntityType`,`EntityId`),
  KEY `IX_AuditLogs_PerformedByUserId` (`PerformedByUserId`),
  KEY `IX_AuditLogs_Timestamp` (`Timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `clientgalleries`
--

DROP TABLE IF EXISTS `clientgalleries`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clientgalleries` (
  `Id` char(36) NOT NULL,
  `ClientId` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `FileId` char(36) NOT NULL,
  `PreviewImagePath` varchar(1000) NOT NULL,
  `FileName` varchar(500) NOT NULL,
  `OriginalFileName` varchar(500) NOT NULL,
  `FilePath` varchar(1000) NOT NULL,
  `ContentType` varchar(100) NOT NULL,
  `Format` varchar(50) DEFAULT NULL,
  `ApprovedAt` datetime(6) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientGalleries_ClientId` (`ClientId`),
  KEY `IX_ClientGalleries_FileId` (`FileId`),
  KEY `IX_ClientGalleries_OrderId` (`OrderId`),
  CONSTRAINT `FK_ClientGalleries_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `clientprofiles` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ClientGalleries_LogoFiles_FileId` FOREIGN KEY (`FileId`) REFERENCES `logofiles` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_ClientGalleries_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `clientlogopricings`
--

DROP TABLE IF EXISTS `clientlogopricings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clientlogopricings` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `ClientId` char(36) NOT NULL,
  `DesignCategory` int NOT NULL,
  `DesignType` int NOT NULL,
  `Price` decimal(18,2) NOT NULL,
  `CurrencyCode` varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'USD',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `UpdatedBy` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ClientLogoPricings_ClientId_DesignCategory_DesignType` (`ClientId`,`DesignCategory`,`DesignType`),
  CONSTRAINT `FK_ClientLogoPricings_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `clientprofiles` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `clientprofiles`
--

DROP TABLE IF EXISTS `clientprofiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clientprofiles` (
  `Id` char(36) NOT NULL,
  `UserId` char(36) NOT NULL,
  `CompanyName` varchar(200) NOT NULL,
  `PhoneNumber` varchar(20) DEFAULT NULL,
  `Address` varchar(500) DEFAULT NULL,
  `City` varchar(100) DEFAULT NULL,
  `Country` varchar(100) DEFAULT NULL,
  `PostalCode` varchar(20) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `Cell` varchar(20) DEFAULT NULL,
  `ContactName` varchar(200) DEFAULT NULL,
  `Fax` varchar(20) DEFAULT NULL,
  `Reference` varchar(200) DEFAULT NULL,
  `State` varchar(100) DEFAULT NULL,
  `Website` varchar(500) DEFAULT NULL,
  `Notes` longtext,
  `BillingType` int NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ClientProfiles_UserId` (`UserId`),
  CONSTRAINT `FK_ClientProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `designerinvoiceadjustments`
--

DROP TABLE IF EXISTS `designerinvoiceadjustments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `designerinvoiceadjustments` (
  `Id` char(36) NOT NULL,
  `DesignerInvoiceId` char(36) NOT NULL,
  `Description` varchar(500) NOT NULL,
  `Amount` decimal(18,2) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0',
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_DesignerInvoiceAdjustments_DesignerInvoiceId` (`DesignerInvoiceId`),
  CONSTRAINT `FK_DesignerInvoiceAdjustments_DesignerInvoiceId` FOREIGN KEY (`DesignerInvoiceId`) REFERENCES `designerinvoices` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `designerinvoiceitems`
--

DROP TABLE IF EXISTS `designerinvoiceitems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `designerinvoiceitems` (
  `Id` char(36) NOT NULL,
  `DesignerInvoiceId` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `Description` varchar(500) NOT NULL,
  `Amount` decimal(18,2) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0',
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_DesignerInvoiceItems_DesignerInvoiceId` (`DesignerInvoiceId`),
  KEY `FK_DesignerInvoiceItems_OrderId` (`OrderId`),
  CONSTRAINT `FK_DesignerInvoiceItems_DesignerInvoiceId` FOREIGN KEY (`DesignerInvoiceId`) REFERENCES `designerinvoices` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_DesignerInvoiceItems_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `designerinvoices`
--

DROP TABLE IF EXISTS `designerinvoices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `designerinvoices` (
  `Id` char(36) NOT NULL,
  `DesignerId` char(36) NOT NULL,
  `InvoiceNumber` varchar(50) NOT NULL,
  `TotalAmount` decimal(18,2) NOT NULL,
  `Status` int NOT NULL DEFAULT '0',
  `BillingPeriod` varchar(50) NOT NULL,
  `IssueDate` datetime(6) NOT NULL,
  `PaidDate` datetime(6) DEFAULT NULL,
  `Notes` varchar(1000) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0',
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `designerprofiles`
--

DROP TABLE IF EXISTS `designerprofiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `designerprofiles` (
  `Id` char(36) NOT NULL,
  `UserId` char(36) NOT NULL,
  `Specialization` varchar(200) DEFAULT NULL,
  `Bio` varchar(1000) DEFAULT NULL,
  `HourlyRate` decimal(18,2) DEFAULT NULL,
  `IsAvailable` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `Notes` longtext,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_DesignerProfiles_UserId` (`UserId`),
  CONSTRAINT `FK_DesignerProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `designpricings`
--

DROP TABLE IF EXISTS `designpricings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `designpricings` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `DesignCategory` int NOT NULL,
  `DesignType` int NOT NULL,
  `DefaultPrice` decimal(18,2) NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `UpdatedBy` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_DesignPricings_DesignCategory_DesignType` (`DesignCategory`,`DesignType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `invoicelogs`
--

DROP TABLE IF EXISTS `invoicelogs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invoicelogs` (
  `Id` char(36) NOT NULL,
  `InvoiceId` char(36) NOT NULL,
  `Action` int NOT NULL,
  `PerformedBy` char(36) DEFAULT NULL,
  `Notes` varchar(1000) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_InvoiceLogs_CreatedAt` (`CreatedAt`),
  KEY `IX_InvoiceLogs_InvoiceId` (`InvoiceId`),
  CONSTRAINT `FK_InvoiceLogs_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `invoices` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `invoiceorders`
--

DROP TABLE IF EXISTS `invoiceorders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invoiceorders` (
  `Id` char(36) NOT NULL,
  `InvoiceId` char(36) NOT NULL,
  `OrderId` char(36) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `Amount` decimal(18,2) NOT NULL DEFAULT '0.00',
  `Description` varchar(500) NOT NULL DEFAULT '',
  PRIMARY KEY (`Id`),
  KEY `IX_InvoiceOrders_OrderId` (`OrderId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `invoices`
--

DROP TABLE IF EXISTS `invoices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invoices` (
  `Id` char(36) NOT NULL,
  `ClientId` char(36) NOT NULL,
  `InvoiceNumber` varchar(50) NOT NULL,
  `Amount` decimal(18,2) NOT NULL,
  `TaxAmount` decimal(18,2) NOT NULL,
  `TotalAmount` decimal(18,2) NOT NULL,
  `IssueDate` datetime(6) NOT NULL,
  `DueDate` datetime(6) DEFAULT NULL,
  `PaidDate` datetime(6) DEFAULT NULL,
  `PaymentMethod` varchar(50) DEFAULT NULL,
  `Notes` varchar(1000) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `LogoOrderId` char(36) DEFAULT NULL,
  `Status` int NOT NULL DEFAULT '1',
  `BillingType` int NOT NULL DEFAULT '1',
  `BillingPeriod` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Invoices_InvoiceNumber` (`InvoiceNumber`),
  KEY `IX_Invoices_ClientId` (`ClientId`),
  KEY `IX_Invoices_LogoOrderId` (`LogoOrderId`),
  KEY `IX_Invoices_ClientId_Status` (`ClientId`,`Status`),
  KEY `IX_Invoices_PaidDate` (`PaidDate`),
  KEY `IX_Invoices_Status` (`Status`),
  CONSTRAINT `FK_Invoices_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `clientprofiles` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Invoices_LogoOrders_LogoOrderId` FOREIGN KEY (`LogoOrderId`) REFERENCES `logoorders` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `logofiles`
--

DROP TABLE IF EXISTS `logofiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `logofiles` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `FileName` varchar(500) NOT NULL,
  `OriginalFileName` varchar(500) NOT NULL,
  `FilePath` varchar(1000) NOT NULL,
  `ContentType` varchar(100) NOT NULL,
  `FileSize` bigint NOT NULL,
  `IsFinalVersion` tinyint(1) NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `UploadedBy` char(36) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `ApprovedAt` datetime(6) DEFAULT NULL,
  `ApprovedBy` char(36) DEFAULT NULL,
  `FileType` int NOT NULL DEFAULT '1',
  `IsAdminApproved` tinyint(1) NOT NULL DEFAULT '0',
  `IsVisibleToClient` tinyint(1) NOT NULL DEFAULT '0',
  `VersionNumber` int NOT NULL DEFAULT '0',
  `FileCategory` int DEFAULT NULL,
  `FileStatus` int NOT NULL DEFAULT '1',
  `PreviewBatchId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_LogoFiles_OrderId` (`OrderId`),
  KEY `IX_LogoFiles_ApprovedBy` (`ApprovedBy`),
  KEY `IX_LogoFiles_PreviewBatchId` (`PreviewBatchId`),
  CONSTRAINT `FK_LogoFiles_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_LogoFiles_Users_ApprovedBy` FOREIGN KEY (`ApprovedBy`) REFERENCES `users` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `logoorders`
--

DROP TABLE IF EXISTS `logoorders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `logoorders` (
  `Id` char(36) NOT NULL,
  `ClientId` char(36) NOT NULL,
  `DesignerId` char(36) DEFAULT NULL,
  `Title` varchar(200) NOT NULL,
  `Description` varchar(2000) NOT NULL,
  `Status` int NOT NULL DEFAULT '1',
  `Price` decimal(18,2) NOT NULL,
  `Deadline` datetime(6) DEFAULT NULL,
  `Requirements` varchar(2000) DEFAULT NULL,
  `ColorPreferences` varchar(500) DEFAULT NULL,
  `StylePreferences` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `Instructions` longtext,
  `PriceApproved` tinyint(1) NOT NULL DEFAULT '0',
  `ProposedPrice` decimal(18,2) DEFAULT NULL,
  `RequiredFormats` varchar(500) DEFAULT NULL,
  `RequiresPriceApproval` tinyint(1) NOT NULL DEFAULT '0',
  `Priority` int NOT NULL DEFAULT '2',
  `ArchivedAt` datetime(6) DEFAULT NULL,
  `ArchivedBy` char(36) DEFAULT NULL,
  `CancellationReason` varchar(2000) DEFAULT NULL,
  `CancelledAt` datetime(6) DEFAULT NULL,
  `CancelledBy` char(36) DEFAULT NULL,
  `IsArchived` tinyint(1) NOT NULL DEFAULT '0',
  `IsCancelledByUser` tinyint(1) NOT NULL DEFAULT '0',
  `IsRefunded` tinyint(1) NOT NULL DEFAULT '0',
  `RefundAmount` decimal(18,2) DEFAULT NULL,
  `RefundReason` varchar(2000) DEFAULT NULL,
  `RefundedAt` datetime(6) DEFAULT NULL,
  `RefundedBy` char(36) DEFAULT NULL,
  `AllowUploads` tinyint(1) NOT NULL DEFAULT '1',
  `AllowExtraRevisions` tinyint(1) NOT NULL DEFAULT '0',
  `RevisionCount` int NOT NULL DEFAULT '0',
  `RevisionLimit` int DEFAULT NULL,
  `BillingEligible` tinyint(1) NOT NULL DEFAULT '0',
  `CompletedDate` datetime(6) DEFAULT NULL,
  `InvoiceId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `IsInvoiced` tinyint(1) NOT NULL DEFAULT '0',
  `ApprovedPrice` decimal(18,2) DEFAULT NULL,
  `DesignCategory` int DEFAULT NULL,
  `DesignType` int DEFAULT NULL,
  `DesignerInvoiceId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `IsDesignerInvoiced` tinyint(1) NOT NULL DEFAULT '0',
  `PriceApprovalStatus` int NOT NULL DEFAULT '0',
  `StandardPrice` decimal(18,2) DEFAULT NULL,
  `InvoicedDate` datetime(6) DEFAULT NULL,
  `PriceLocked` tinyint(1) NOT NULL DEFAULT '0',
  `ClientPrice` decimal(18,2) DEFAULT NULL,
  `CurrencyCode` varchar(3) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT 'USD',
  `ClientBasePrice` decimal(18,2) NOT NULL DEFAULT '0.00',
  `ClientChargePrice` decimal(18,2) NOT NULL DEFAULT '0.00',
  `DesignerApprovedPrice` decimal(18,2) DEFAULT NULL,
  `DesignerProposedPrice` decimal(18,2) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_LogoOrders_ClientId` (`ClientId`),
  KEY `IX_LogoOrders_DesignerId` (`DesignerId`),
  KEY `IX_LogoOrders_ClientId_CreatedAt` (`ClientId`,`CreatedAt`),
  KEY `IX_LogoOrders_CreatedAt` (`CreatedAt`),
  KEY `IX_LogoOrders_DesignerId_Status` (`DesignerId`,`Status`),
  KEY `IX_LogoOrders_Status` (`Status`),
  KEY `IX_LogoOrders_Status_CreatedAt` (`Status`,`CreatedAt`),
  KEY `IX_LogoOrders_UpdatedAt` (`UpdatedAt`),
  KEY `IX_LogoOrders_ClientId_Status` (`ClientId`,`Status`),
  KEY `IX_LogoOrders_ClientId_BillingEligible_IsInvoiced` (`ClientId`,`BillingEligible`,`IsInvoiced`),
  CONSTRAINT `FK_LogoOrders_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `clientprofiles` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_LogoOrders_DesignerProfiles_DesignerId` FOREIGN KEY (`DesignerId`) REFERENCES `designerprofiles` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `messages`
--

DROP TABLE IF EXISTS `messages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `messages` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) DEFAULT NULL,
  `SenderId` char(36) NOT NULL,
  `RecipientId` char(36) DEFAULT NULL,
  `Content` varchar(2000) NOT NULL,
  `IsRead` tinyint(1) NOT NULL,
  `ReadAt` datetime(6) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `ForwardedByAdmin` tinyint(1) NOT NULL DEFAULT '0',
  `ForwardedToMessageId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `IsRejected` tinyint(1) NOT NULL DEFAULT '0',
  `OriginalSenderRole` int DEFAULT NULL,
  `RejectedAt` datetime(6) DEFAULT NULL,
  `RejectedBy` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `RequiresAdminApproval` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_Messages_OrderId` (`OrderId`),
  KEY `IX_Messages_RecipientId` (`RecipientId`),
  KEY `IX_Messages_SenderId` (`SenderId`),
  CONSTRAINT `FK_Messages_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Messages_Users_RecipientId` FOREIGN KEY (`RecipientId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Messages_Users_SenderId` FOREIGN KEY (`SenderId`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `notifications`
--

DROP TABLE IF EXISTS `notifications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notifications` (
  `Id` char(36) NOT NULL,
  `UserId` char(36) NOT NULL,
  `OrderId` char(36) DEFAULT NULL,
  `Title` varchar(200) NOT NULL,
  `Message` varchar(1000) NOT NULL,
  `Type` int NOT NULL DEFAULT '1',
  `IsRead` tinyint(1) NOT NULL,
  `ReadAt` datetime(6) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `ReferenceId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `ReferenceType` int NOT NULL DEFAULT '1',
  `AggregationCount` int NOT NULL DEFAULT '1',
  `LastOccurrenceAt` datetime(6) DEFAULT NULL,
  `RedirectUrl` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Notifications_OrderId` (`OrderId`),
  KEY `IX_Notifications_UserId_IsRead` (`UserId`,`IsRead`),
  KEY `IX_Notifications_UserId_CreatedAt` (`UserId`,`CreatedAt`),
  KEY `IX_Notifications_UserId_ReferenceType_CreatedAt` (`UserId`,`ReferenceType`,`CreatedAt`),
  CONSTRAINT `FK_Notifications_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_Notifications_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ordercomments`
--

DROP TABLE IF EXISTS `ordercomments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ordercomments` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `Content` varchar(2000) NOT NULL,
  `CreatedBy` char(36) NOT NULL,
  `IsInternal` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `CommentType` int NOT NULL DEFAULT '1',
  `IsReadByAdmin` tinyint(1) NOT NULL DEFAULT '0',
  `IsReadByClient` tinyint(1) NOT NULL DEFAULT '0',
  `IsReadByDesigner` tinyint(1) NOT NULL DEFAULT '0',
  `VisibleToClient` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_OrderComments_CreatedBy` (`CreatedBy`),
  KEY `IX_OrderComments_OrderId` (`OrderId`),
  CONSTRAINT `FK_OrderComments_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_OrderComments_Users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `orderlogs`
--

DROP TABLE IF EXISTS `orderlogs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orderlogs` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `Action` int NOT NULL,
  `PreviousStatus` int DEFAULT NULL,
  `NewStatus` int DEFAULT NULL,
  `PerformedBy` varchar(100) DEFAULT NULL,
  `PerformedById` char(36) DEFAULT NULL,
  `Note` varchar(2000) DEFAULT NULL,
  `Metadata` longtext,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_OrderLogs_Action` (`Action`),
  KEY `IX_OrderLogs_CreatedAt` (`CreatedAt`),
  KEY `IX_OrderLogs_OrderId` (`OrderId`),
  CONSTRAINT `FK_OrderLogs_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `orderpricehistories`
--

DROP TABLE IF EXISTS `orderpricehistories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orderpricehistories` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `OldPrice` decimal(18,2) NOT NULL,
  `NewPrice` decimal(18,2) NOT NULL,
  `ChangedBy` char(36) NOT NULL,
  `ChangedAt` datetime(6) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_OrderPriceHistories_OrderId` (`OrderId`),
  CONSTRAINT `FK_OrderPriceHistories_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `orderrevisions`
--

DROP TABLE IF EXISTS `orderrevisions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orderrevisions` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `Instructions` longtext NOT NULL,
  `RequestedBy` char(36) NOT NULL,
  `IsResolved` tinyint(1) NOT NULL,
  `ResolvedAt` datetime(6) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_OrderRevisions_OrderId` (`OrderId`),
  CONSTRAINT `FK_OrderRevisions_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `orderstatushistories`
--

DROP TABLE IF EXISTS `orderstatushistories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orderstatushistories` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `PreviousStatus` int NOT NULL,
  `NewStatus` int NOT NULL,
  `Notes` varchar(1000) DEFAULT NULL,
  `ChangedBy` char(36) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_OrderStatusHistories_OrderId` (`OrderId`),
  CONSTRAINT `FK_OrderStatusHistories_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `payments`
--

DROP TABLE IF EXISTS `payments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payments` (
  `Id` char(36) NOT NULL,
  `InvoiceId` char(36) NOT NULL,
  `PaymentMethod` varchar(50) NOT NULL,
  `Amount` decimal(18,2) NOT NULL,
  `Currency` varchar(10) NOT NULL DEFAULT 'USD',
  `Status` int NOT NULL DEFAULT '0',
  `PaymentLink` varchar(1000) DEFAULT NULL,
  `TransactionId` varchar(200) DEFAULT NULL,
  `CompletedAt` datetime(6) DEFAULT NULL,
  `ErrorMessage` varchar(1000) DEFAULT NULL,
  `ReturnUrl` varchar(500) DEFAULT NULL,
  `CancelUrl` varchar(500) DEFAULT NULL,
  `ExpiresAt` datetime(6) DEFAULT NULL,
  `InvoiceId1` char(36) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Payments_CreatedAt` (`CreatedAt`),
  KEY `IX_Payments_InvoiceId` (`InvoiceId`),
  KEY `IX_Payments_InvoiceId1` (`InvoiceId1`),
  KEY `IX_Payments_Status` (`Status`),
  KEY `IX_Payments_TransactionId` (`TransactionId`),
  CONSTRAINT `FK_Payments_Invoices_InvoiceId` FOREIGN KEY (`InvoiceId`) REFERENCES `invoices` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Payments_Invoices_InvoiceId1` FOREIGN KEY (`InvoiceId1`) REFERENCES `invoices` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `permissions`
--

DROP TABLE IF EXISTS `permissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `permissions` (
  `Id` char(36) NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) NOT NULL,
  `Resource` varchar(100) NOT NULL,
  `Action` varchar(50) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Permissions_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `reviews`
--

DROP TABLE IF EXISTS `reviews`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `reviews` (
  `Id` char(36) NOT NULL,
  `OrderId` char(36) NOT NULL,
  `ClientId` char(36) NOT NULL,
  `Rating` int NOT NULL,
  `Comment` varchar(1000) DEFAULT NULL,
  `IsPublished` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Reviews_ClientId` (`ClientId`),
  KEY `IX_Reviews_OrderId` (`OrderId`),
  CONSTRAINT `FK_Reviews_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `clientprofiles` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Reviews_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `revisionfiles`
--

DROP TABLE IF EXISTS `revisionfiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `revisionfiles` (
  `Id` char(36) NOT NULL,
  `RevisionId` char(36) NOT NULL,
  `FileName` varchar(500) NOT NULL,
  `OriginalFileName` varchar(500) NOT NULL,
  `FilePath` varchar(1000) NOT NULL,
  `ContentType` varchar(100) NOT NULL,
  `FileSize` bigint NOT NULL,
  `FileType` int NOT NULL DEFAULT '1',
  `Description` varchar(1000) DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_RevisionFiles_RevisionId` (`RevisionId`),
  CONSTRAINT `FK_RevisionFiles_OrderRevisions_RevisionId` FOREIGN KEY (`RevisionId`) REFERENCES `orderrevisions` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `rolepermissions`
--

DROP TABLE IF EXISTS `rolepermissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `rolepermissions` (
  `Id` char(36) NOT NULL,
  `RoleId` char(36) NOT NULL,
  `PermissionId` char(36) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_RolePermissions_RoleId_PermissionId` (`RoleId`,`PermissionId`),
  KEY `IX_RolePermissions_PermissionId` (`PermissionId`),
  CONSTRAINT `FK_RolePermissions_Permissions_PermissionId` FOREIGN KEY (`PermissionId`) REFERENCES `permissions` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_RolePermissions_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `Id` char(36) NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Description` varchar(500) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Roles_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `settings`
--

DROP TABLE IF EXISTS `settings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `settings` (
  `Id` char(36) NOT NULL,
  `Key` varchar(100) NOT NULL,
  `Value` longtext NOT NULL,
  `Category` varchar(50) NOT NULL,
  `Description` longtext,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Settings_Key_Category` (`Key`,`Category`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `Id` char(36) NOT NULL,
  `Email` varchar(256) NOT NULL,
  `FirstName` varchar(100) NOT NULL,
  `LastName` varchar(100) NOT NULL,
  `PasswordHash` longtext NOT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `RefreshToken` longtext,
  `RefreshTokenExpiryTime` datetime(6) DEFAULT NULL,
  `RoleId` char(36) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) DEFAULT NULL,
  `CreatedBy` char(36) DEFAULT NULL,
  `UpdatedBy` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `PasswordResetToken` longtext,
  `PasswordResetTokenExpiryTime` datetime(6) DEFAULT NULL,
  `InvoiceEmail` varchar(256) DEFAULT NULL,
  `SecondaryEmail` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `DeactivatedAt` datetime(6) DEFAULT NULL,
  `DeactivatedBy` char(36) DEFAULT NULL,
  `FailedLoginAttempts` int NOT NULL DEFAULT '0',
  `LockoutEnd` datetime(6) DEFAULT NULL,
  `IsRootAdmin` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Users_Email` (`Email`),
  KEY `IX_Users_RoleId` (`RoleId`),
  CONSTRAINT `FK_Users_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-10 21:51:33
