-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: LogoDesignPortalDb
-- ------------------------------------------------------
-- Server version	8.0.45

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
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
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20260205182456_InitialCreate','8.0.0'),('20260206124650_AddPermissionSystem','8.0.0'),('20260207010050_AddMessagesReviewsSettings','8.0.0'),('20260209141859_AddPasswordResetToken','8.0.0'),('20260210104034_AddFullRegistrationFields','8.0.0'),('20260210125019_OrderManagementSystemEnhancement','8.0.0'),('20260210173325_AddOrderPriority','8.0.0'),('20260210185545_ProfessionalOrderManagementSystem','8.0.0'),('20260210192231_InvoiceSystemEnhancement','8.0.0'),('20260210195700_AddGapFillingFeatures','8.0.0'),('20260212122815_AddFileCategoryAndStatus','8.0.0'),('20260212174741_AddAllowUploadsToLogoOrder','8.0.0'),('20260212182256_AddPaymentEntity','8.0.0');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `auditlogs`
--

LOCK TABLES `auditlogs` WRITE;
/*!40000 ALTER TABLE `auditlogs` DISABLE KEYS */;
/*!40000 ALTER TABLE `auditlogs` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `clientgalleries`
--

LOCK TABLES `clientgalleries` WRITE;
/*!40000 ALTER TABLE `clientgalleries` DISABLE KEYS */;
/*!40000 ALTER TABLE `clientgalleries` ENABLE KEYS */;
UNLOCK TABLES;

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
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ClientProfiles_UserId` (`UserId`),
  CONSTRAINT `FK_ClientProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `clientprofiles`
--

LOCK TABLES `clientprofiles` WRITE;
/*!40000 ALTER TABLE `clientprofiles` DISABLE KEYS */;
/*!40000 ALTER TABLE `clientprofiles` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `designerprofiles`
--

LOCK TABLES `designerprofiles` WRITE;
/*!40000 ALTER TABLE `designerprofiles` DISABLE KEYS */;
/*!40000 ALTER TABLE `designerprofiles` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `invoicelogs`
--

LOCK TABLES `invoicelogs` WRITE;
/*!40000 ALTER TABLE `invoicelogs` DISABLE KEYS */;
/*!40000 ALTER TABLE `invoicelogs` ENABLE KEYS */;
UNLOCK TABLES;

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
  UNIQUE KEY `IX_InvoiceOrders_InvoiceId_OrderId` (`InvoiceId`,`OrderId`),
  KEY `IX_InvoiceOrders_OrderId` (`OrderId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `invoiceorders`
--

LOCK TABLES `invoiceorders` WRITE;
/*!40000 ALTER TABLE `invoiceorders` DISABLE KEYS */;
/*!40000 ALTER TABLE `invoiceorders` ENABLE KEYS */;
UNLOCK TABLES;

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
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Invoices_InvoiceNumber` (`InvoiceNumber`),
  KEY `IX_Invoices_ClientId` (`ClientId`),
  KEY `IX_Invoices_LogoOrderId` (`LogoOrderId`),
  CONSTRAINT `FK_Invoices_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `clientprofiles` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Invoices_LogoOrders_LogoOrderId` FOREIGN KEY (`LogoOrderId`) REFERENCES `logoorders` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `invoices`
--

LOCK TABLES `invoices` WRITE;
/*!40000 ALTER TABLE `invoices` DISABLE KEYS */;
/*!40000 ALTER TABLE `invoices` ENABLE KEYS */;
UNLOCK TABLES;

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
  PRIMARY KEY (`Id`),
  KEY `IX_LogoFiles_OrderId` (`OrderId`),
  KEY `IX_LogoFiles_ApprovedBy` (`ApprovedBy`),
  CONSTRAINT `FK_LogoFiles_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_LogoFiles_Users_ApprovedBy` FOREIGN KEY (`ApprovedBy`) REFERENCES `users` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `logofiles`
--

LOCK TABLES `logofiles` WRITE;
/*!40000 ALTER TABLE `logofiles` DISABLE KEYS */;
/*!40000 ALTER TABLE `logofiles` ENABLE KEYS */;
UNLOCK TABLES;

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
  PRIMARY KEY (`Id`),
  KEY `IX_LogoOrders_ClientId` (`ClientId`),
  KEY `IX_LogoOrders_DesignerId` (`DesignerId`),
  CONSTRAINT `FK_LogoOrders_ClientProfiles_ClientId` FOREIGN KEY (`ClientId`) REFERENCES `clientprofiles` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_LogoOrders_DesignerProfiles_DesignerId` FOREIGN KEY (`DesignerId`) REFERENCES `designerprofiles` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `logoorders`
--

LOCK TABLES `logoorders` WRITE;
/*!40000 ALTER TABLE `logoorders` DISABLE KEYS */;
/*!40000 ALTER TABLE `logoorders` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `messages`
--

LOCK TABLES `messages` WRITE;
/*!40000 ALTER TABLE `messages` DISABLE KEYS */;
/*!40000 ALTER TABLE `messages` ENABLE KEYS */;
UNLOCK TABLES;

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
  PRIMARY KEY (`Id`),
  KEY `IX_Notifications_OrderId` (`OrderId`),
  KEY `IX_Notifications_UserId_IsRead` (`UserId`,`IsRead`),
  CONSTRAINT `FK_Notifications_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE SET NULL,
  CONSTRAINT `FK_Notifications_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notifications`
--

LOCK TABLES `notifications` WRITE;
/*!40000 ALTER TABLE `notifications` DISABLE KEYS */;
/*!40000 ALTER TABLE `notifications` ENABLE KEYS */;
UNLOCK TABLES;

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
  PRIMARY KEY (`Id`),
  KEY `IX_OrderComments_CreatedBy` (`CreatedBy`),
  KEY `IX_OrderComments_OrderId` (`OrderId`),
  CONSTRAINT `FK_OrderComments_LogoOrders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `logoorders` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_OrderComments_Users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `users` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ordercomments`
--

LOCK TABLES `ordercomments` WRITE;
/*!40000 ALTER TABLE `ordercomments` DISABLE KEYS */;
/*!40000 ALTER TABLE `ordercomments` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `orderlogs`
--

LOCK TABLES `orderlogs` WRITE;
/*!40000 ALTER TABLE `orderlogs` DISABLE KEYS */;
/*!40000 ALTER TABLE `orderlogs` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `orderrevisions`
--

LOCK TABLES `orderrevisions` WRITE;
/*!40000 ALTER TABLE `orderrevisions` DISABLE KEYS */;
/*!40000 ALTER TABLE `orderrevisions` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `orderstatushistories`
--

LOCK TABLES `orderstatushistories` WRITE;
/*!40000 ALTER TABLE `orderstatushistories` DISABLE KEYS */;
/*!40000 ALTER TABLE `orderstatushistories` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `payments`
--

LOCK TABLES `payments` WRITE;
/*!40000 ALTER TABLE `payments` DISABLE KEYS */;
/*!40000 ALTER TABLE `payments` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `permissions`
--

LOCK TABLES `permissions` WRITE;
/*!40000 ALTER TABLE `permissions` DISABLE KEYS */;
INSERT INTO `permissions` VALUES ('10000000-0000-0000-0000-000000000001','CreateUser','Create new users','User','Create','2026-02-12 18:22:55.056168',NULL,NULL,NULL,0,NULL,NULL),('10000000-0000-0000-0000-000000000002','ViewUsers','View all users','User','Read','2026-02-12 18:22:55.056169',NULL,NULL,NULL,0,NULL,NULL),('10000000-0000-0000-0000-000000000003','UpdateUser','Update user information','User','Update','2026-02-12 18:22:55.056169',NULL,NULL,NULL,0,NULL,NULL),('10000000-0000-0000-0000-000000000004','DeleteUser','Delete users','User','Delete','2026-02-12 18:22:55.056169',NULL,NULL,NULL,0,NULL,NULL),('20000000-0000-0000-0000-000000000001','CreateDesignerProfile','Create designer profiles','DesignerProfile','Create','2026-02-12 18:22:55.056169',NULL,NULL,NULL,0,NULL,NULL),('20000000-0000-0000-0000-000000000002','ViewDesignerProfiles','View designer profiles','DesignerProfile','Read','2026-02-12 18:22:55.056170',NULL,NULL,NULL,0,NULL,NULL),('20000000-0000-0000-0000-000000000003','UpdateDesignerProfile','Update designer profiles','DesignerProfile','Update','2026-02-12 18:22:55.056170',NULL,NULL,NULL,0,NULL,NULL),('30000000-0000-0000-0000-000000000001','CreateOrder','Create new orders','Order','Create','2026-02-12 18:22:55.056170',NULL,NULL,NULL,0,NULL,NULL),('30000000-0000-0000-0000-000000000002','ViewAllOrders','View all orders in the system','Order','ReadAll','2026-02-12 18:22:55.056170',NULL,NULL,NULL,0,NULL,NULL),('30000000-0000-0000-0000-000000000003','AssignOrder','Assign orders to designers','Order','Assign','2026-02-12 18:22:55.056171',NULL,NULL,NULL,0,NULL,NULL),('30000000-0000-0000-0000-000000000004','UpdateOrderStatus','Update order status','Order','UpdateStatus','2026-02-12 18:22:55.056171',NULL,NULL,NULL,0,NULL,NULL),('40000000-0000-0000-0000-000000000001','ManagePermissions','Manage role permissions','Permission','Manage','2026-02-12 18:22:55.056171',NULL,NULL,NULL,0,NULL,NULL),('50000000-0000-0000-0000-000000000001','UploadFile','Upload files','File','Upload','2026-02-12 18:22:55.056172',NULL,NULL,NULL,0,NULL,NULL),('50000000-0000-0000-0000-000000000002','DownloadFile','Download files','File','Download','2026-02-12 18:22:55.056172',NULL,NULL,NULL,0,NULL,NULL),('50000000-0000-0000-0000-000000000003','DeleteFile','Delete files','File','Delete','2026-02-12 18:22:55.056173',NULL,NULL,NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `permissions` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `reviews`
--

LOCK TABLES `reviews` WRITE;
/*!40000 ALTER TABLE `reviews` DISABLE KEYS */;
/*!40000 ALTER TABLE `reviews` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `revisionfiles`
--

LOCK TABLES `revisionfiles` WRITE;
/*!40000 ALTER TABLE `revisionfiles` DISABLE KEYS */;
/*!40000 ALTER TABLE `revisionfiles` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `rolepermissions`
--

LOCK TABLES `rolepermissions` WRITE;
/*!40000 ALTER TABLE `rolepermissions` DISABLE KEYS */;
/*!40000 ALTER TABLE `rolepermissions` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES ('11111111-1111-1111-1111-111111111111','SuperAdmin','Full system access with all permissions','2026-02-12 18:22:55.057138',NULL,NULL,NULL,0,NULL,NULL),('22222222-2222-2222-2222-222222222222','Admin','Administrative access with restricted client data access','2026-02-12 18:22:55.057139',NULL,NULL,NULL,0,NULL,NULL),('33333333-3333-3333-3333-333333333333','Designer','Designer access without client identity information','2026-02-12 18:22:55.057139',NULL,NULL,NULL,0,NULL,NULL),('44444444-4444-4444-4444-444444444444','Client','Client access to their own data','2026-02-12 18:22:55.057139',NULL,NULL,NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `settings`
--

LOCK TABLES `settings` WRITE;
/*!40000 ALTER TABLE `settings` DISABLE KEYS */;
INSERT INTO `settings` VALUES ('0602daa7-3025-407b-90b0-8915994a9295','IBAN','GB82WEST12345698765432','Payment','Dummy IBAN for testing purposes','2026-02-24 23:08:27.316316',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('13ba7c03-149a-47ff-bd3d-c3460f1d6d60','PayPalClientSecret','DUMMY_PAYPAL_CLIENT_SECRET_FOR_TESTING','Payment','Dummy PayPalClientSecret for testing purposes','2026-02-24 23:08:27.316301',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('1dd5861f-e260-423b-88ea-b87037a1d475','WiseProfileId','DUMMY_WISE_PROFILE_ID_FOR_TESTING','Payment','Dummy WiseProfileId for testing purposes','2026-02-24 23:08:27.316310',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('34d7b2ae-3f6c-4eb4-802e-0b4b2837fd9b','AccountHolderName','Hawk Merchandising','Payment','Dummy AccountHolderName for testing purposes','2026-02-24 23:08:27.316313',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('3b4db36b-71d1-4c0b-aedd-b7d2e397a022','PayPalClientId','DUMMY_PAYPAL_CLIENT_ID_FOR_TESTING','Payment','Dummy PayPalClientId for testing purposes','2026-02-24 23:08:27.316270',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('94571aa7-cc51-4158-8865-92c7b2473e9d','BankName','Demo Bank','Payment','Dummy BankName for testing purposes','2026-02-24 23:08:27.316311',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('af7ed887-ce3e-42d2-85d3-11a358d9a6a0','RoutingNumber','123456789','Payment','Dummy RoutingNumber for testing purposes','2026-02-24 23:08:27.316319',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('b22675d3-218c-4b6b-bbf7-947f3a5d0a09','SWIFT','DEMOBANK123','Payment','Dummy SWIFT for testing purposes','2026-02-24 23:08:27.316318',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('b33b62a5-d43c-44b3-ad52-b00dac60c7d5','PayPalUseSandbox','true','Payment','Dummy PayPalUseSandbox for testing purposes','2026-02-24 23:08:27.316305',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('bd3ae30d-e97e-45fc-af92-4b2a692a7222','AccountNumber','1234567890','Payment','Dummy AccountNumber for testing purposes','2026-02-24 23:08:27.316314',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('c775d415-a098-49d8-adb8-5c975218f304','WiseApiKey','DUMMY_WISE_API_KEY_FOR_TESTING','Payment','Dummy WiseApiKey for testing purposes','2026-02-24 23:08:27.316307',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('d97d69b0-d459-4608-9d17-de8e043613ad','BankCurrency','USD','Payment','Dummy BankCurrency for testing purposes','2026-02-24 23:08:27.316323',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('e6db0d4f-f282-4dd2-b90c-0550e389e245','BranchAddress','123 Main Street, City, Country','Payment','Dummy BranchAddress for testing purposes','2026-02-24 23:08:27.316321',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `settings` ENABLE KEYS */;
UNLOCK TABLES;

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
  `UpdatedBy` char(36) DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL,
  `DeletedAt` datetime(6) DEFAULT NULL,
  `DeletedBy` char(36) DEFAULT NULL,
  `PasswordResetToken` longtext,
  `PasswordResetTokenExpiryTime` datetime(6) DEFAULT NULL,
  `InvoiceEmail` varchar(256) DEFAULT NULL,
  `SecondaryEmail` varchar(256) DEFAULT NULL,
  `DeactivatedAt` datetime(6) DEFAULT NULL,
  `DeactivatedBy` char(36) DEFAULT NULL,
  `FailedLoginAttempts` int NOT NULL DEFAULT '0',
  `LockoutEnd` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Users_Email` (`Email`),
  KEY `IX_Users_RoleId` (`RoleId`),
  CONSTRAINT `FK_Users_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `roles` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES ('da6639fc-4721-4958-940d-bf3a910b434a','superadmin@logodesign.com','Super','Admin','$2a$11$TrpJjAo.7BMKzTSxKULhhuIV6BOXtI/1lvGut3ETxleGjVTBJedbG',1,'6xg3DSs0rCdyA8voNWsqEGL1yNDDWcCJbnKCzalwuS/nH/EcBVU7Z8xD4bu+1/AVmsDvQ4hS5cFXzDLR8Sp2Qg==','2026-03-03 23:11:06.076862','11111111-1111-1111-1111-111111111111','2026-02-24 23:08:29.624238','2026-02-24 23:11:06.124099',NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,NULL);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'LogoDesignPortalDb'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-02-25 19:26:51
