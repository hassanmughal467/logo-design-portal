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
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20260205182456_InitialCreate','8.0.0'),('20260206124650_AddPermissionSystem','8.0.0'),('20260207010050_AddMessagesReviewsSettings','8.0.0'),('20260209141859_AddPasswordResetToken','8.0.0'),('20260210104034_AddFullRegistrationFields','8.0.0'),('20260210125019_OrderManagementSystemEnhancement','8.0.0'),('20260210173325_AddOrderPriority','8.0.0'),('20260210185545_ProfessionalOrderManagementSystem','8.0.0'),('20260210192231_InvoiceSystemEnhancement','8.0.0'),('20260210195700_AddGapFillingFeatures','8.0.0'),('20260212122815_AddFileCategoryAndStatus','8.0.0'),('20260212174741_AddAllowUploadsToLogoOrder','8.0.0'),('20260212182256_AddPaymentEntity','8.0.0'),('20260305122820_AddIsRootAdminToUser','8.0.0'),('20260306000009_AddNotificationReferenceTypeAndReferenceId','8.0.0'),('20260306011208_AddNotificationIndexes','8.0.0'),('20260306011504_AddNotificationAggregationFields','8.0.0'),('20260306100301_AddMessageAdminRelayFields','8.0.0'),('20260306110947_AddPreviewBatchIdToLogoFile','8.0.0'),('20260306115028_AddPreviewBatchIdIndexToLogoFile','8.0.0'),('20260306132204_AddRedirectUrlToNotification','8.0.0'),('20260308031733_AddAnalyticsIndexesToLogoOrder','8.0.0'),('20260308034759_AddClientAnalyticsIndexToLogoOrder','8.0.0'),('20260308211008_RefactorOrderWorkflowRemoveUnusedStatuses','8.0.0'),('20260308212230_AddRevisionTrackingToLogoOrder','8.0.0'),('20260308212956_AddOrderCommentSecurityAndVisibility','8.0.0'),('20260308213647_SetVisibleToClientForExistingNonInternalComments','8.0.0'),('20260308223039_AddBillingQueueAndClientBillingType','8.0.0'),('20260308224609_AddDesignerPayoutAndPricing','8.0.0'),('20260308232822_AddStandardPriceAndDesignerInvoiceAdjustment','8.0.0'),('20260309010106_AddDesignPricingTable','8.0.0'),('20260309163126_AddInvoicedDateToLogoOrder','8.0.0'),('20260309181731_AddClientLogoPricingAndOrderFields','8.0.0'),('20260309200000_AddOrderPriceHistoryAndPriceLocked','8.0.0'),('20260309214804_AddSoftDeleteFiltersAndIndexes','8.0.0'),('20260310003349_AddPricingWorkflowFields','8.0.0'),('20260310192426_AddPriceUpdatedTrackingFields','8.0.0'),('20260310222229_AddDesignerLogoPricing','8.0.0');
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
INSERT INTO `auditlogs` VALUES ('02a17740-482c-44fc-8c23-5085cd0e9bff','Order','75ac4d55-694d-4189-811e-1100be2fb1da','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 00:45:49.861388','Logo approved. Notes: None','2026-03-08 00:45:49.864170',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('03977443-fde9-47a7-8cb4-b8441263bda7','Order','12044d25-2671-47f6-bea2-1934ca925308','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-10 01:02:05.493177','Logo approved. Notes: None','2026-03-10 01:02:05.520340',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('071813a2-6a5f-40fd-a4fe-fe236ffc5728','Order','8d6a6ed6-45e0-4121-958d-e1bc4b22c3c4','RequestRevision',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 00:42:36.162705','Revision requested: Need more contrast and different colors','2026-03-08 00:42:36.163012',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('0be1ce14-dc54-4e1b-9cdd-411208570b39','Order','1ba46150-5c19-41d1-80cc-87d6bc8e7fbb','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:10:54.561929','Logo approved. Notes: None','2026-03-07 19:10:54.562615',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('1630a789-1afb-4172-ba78-b373ddabe80b','Order','a6d9b2a7-b28d-419d-b813-5caed40939a6','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:39:33.213550','Logo approved. Notes: None','2026-03-07 18:39:33.247496',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('24932b6f-6e5b-483f-90da-819510a86474','Order','5489aa89-8bfb-45bf-ba13-1fce428f30db','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:54:34.447431','Logo approved. Notes: None','2026-03-07 18:54:34.448127',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('254e7525-5fdd-49cc-8df1-ea4a421902c0','Order','ed2e4a46-1935-4170-a80e-0aa7eb987a67','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:06:38.858148','Logo approved. Notes: None','2026-03-07 19:06:38.869872',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('258f8fde-f934-4284-af13-a0124eb20b93','Order','45761335-71fb-41a5-8205-bea14ef012d4','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:39:54.648950','Logo approved. Notes: None','2026-03-07 18:39:54.649550',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('25e81802-13c5-4ab3-a353-443e55612104','Order','d081ef89-c6dd-45f4-84fa-8ed23979264c','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-09 16:17:59.688105','Logo approved. Notes: None','2026-03-09 16:17:59.689580',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('2a407712-fa10-4635-acb0-db048f7ce349','Order','a53d1745-10df-4ca7-a33b-e3736e54c681','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 05:59:01.723074','Logo approved. Notes: None','2026-03-08 05:59:01.724538',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('308e461e-655f-4595-bf1a-6435456a5e45','Order','3c5b0459-7bcc-4818-ac24-ac9b6af11383','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 22:08:28.572401','Logo approved. Notes: None','2026-03-08 22:08:28.573462',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('31e9e6f7-d2a6-4bc4-aeba-da5f18a8e408','Order','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 23:03:37.645079','Logo approved. Notes: None','2026-03-08 23:03:37.727042',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('42703b6f-2183-48e8-9411-9cc605ffaa19','Order','477575c4-8915-4069-ad90-c7fbc30cad1c','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 00:42:37.770088','Logo approved. Notes: None','2026-03-08 00:42:37.770611',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('5054400c-c53a-4f58-b770-563b78bae658','Order','d0810784-0ea9-4b42-80e6-5bf982540e04','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-09 16:09:13.319657','Logo approved. Notes: None','2026-03-09 16:09:13.391093',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('588ad22e-6468-4698-9b15-8ff74018705d','Order','ea65a3fd-0266-4e19-ab22-7edcc14b1d73','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:33:49.683597','Logo approved. Notes: None','2026-03-07 19:33:49.684247',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('5c0740b7-e4c2-4f6e-aa4a-2da482f13053','Order','81e864db-dc69-480f-a4ee-442106d3131f','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:11:37.601952','Logo approved. Notes: None','2026-03-07 19:11:37.602558',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('5f90f6f6-9d47-483b-bf9d-b468fc82aad4','Order','9a24a9f9-ce66-4886-8a54-4371dfaf0f24','RequestRevision',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:30:00.950733','Revision requested: Need more contrast and different colors','2026-03-07 19:30:00.951331',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('603c28df-67b7-44ab-83a5-c7007a8edc46','Order','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-09 00:44:58.253590','Logo approved. Notes: None','2026-03-09 00:44:58.254301',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('6155fc92-cb2b-484d-95de-976f3606f342','Order','c962c2c9-7764-4d90-9f42-a195f60e65be','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:54:13.992921','Logo approved. Notes: None','2026-03-07 18:54:13.994062',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('630c1867-2c56-422b-b997-1a801a742ddb','Order','051d22a2-668d-42e7-a161-cfccc5122fc1','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:50:18.267806','Logo approved. Notes: None','2026-03-07 18:50:18.269647',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('6d23f85a-b607-456c-be39-6795a73c6af4','Order','f5d00657-6186-45f5-a23b-1e07777bfba8','RequestRevision',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 00:45:38.101679','Revision requested: Need more contrast and different colors','2026-03-08 00:45:38.102644',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('7c787ecc-e999-4c81-aa65-d98ce8845a32','Order','2440b10a-5ff2-4387-893b-9237f5e1193a','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:52:35.018461','Logo approved. Notes: None','2026-03-07 18:52:35.020283',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('7fa7bb9d-9238-4b86-abd3-b82fa016a377','Order','a0454a6c-9bd0-4de7-9057-a456495c9afd','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:32:51.242044','Logo approved. Notes: None','2026-03-07 18:32:51.253202',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('8822f509-2ec9-4524-b540-74e4de723bd8','Order','4c2a694f-d745-4b98-b84e-2c0a70785eb9','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 00:48:37.262765','Logo approved. Notes: None','2026-03-08 00:48:37.264581',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('93598704-36d9-4717-9387-cd357e82b5c9','Order','4908fa77-470c-4d39-bf1f-0697ed77c5fd','RequestRevision',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:24:47.795801','Revision requested: Need more contrast and different colors','2026-03-07 19:24:47.796623',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('984c2ebb-6808-4a99-a624-6bd1d3c60bd4','Order','27c4e2e1-8df3-445a-8455-aea496e62f38','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-09 09:23:56.436402','Logo approved. Notes: None','2026-03-09 09:23:56.500286',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('a034f7f3-124f-41ca-bf43-dfd6ddbce831','Order','15a5b679-0b7b-4034-a69d-a7c37c072136','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:33:26.226431','Logo approved. Notes: None','2026-03-07 19:33:26.226914',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('a371c0d0-18f3-4a01-9cab-33ae8574f284','Order','6373b85b-56cc-4edf-a7d2-0f135118a885','RequestRevision',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:50:18.453599','Revision requested: Need more contrast and different colors','2026-03-07 18:50:18.558938',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('a3ff02a6-7443-4375-a2f8-75b6678f89c2','Order','3ff7d8fe-029d-407e-8a46-73e7014f6953','RequestRevision',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:33:53.722961','Revision requested: Need more contrast and different colors','2026-03-07 19:33:53.723394',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('af69dc27-8804-4128-9158-0cfbd043bc38','Order','9baf686f-1940-4451-a8be-5099d36362a7','RequestRevision',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:52:26.844626','Revision requested: Need more contrast and different colors','2026-03-07 18:52:26.845056',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('b852098b-e48e-46f7-8943-3e007f5355a1','Order','cf2dc5e6-2797-441d-ba05-7d465abdf900','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 22:02:48.266222','Logo approved. Notes: None','2026-03-08 22:02:48.341215',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('bde1bd78-fb07-4e55-896c-c7c2f2faab65','Order','6aa76a40-7a13-4390-974a-12cd49a269a9','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:12:27.228426','Logo approved. Notes: None','2026-03-07 19:12:27.229041',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('c1209ddf-4c17-4c18-a021-77711d0b2ab4','Order','b243a4a3-81c8-4257-9e95-f41c80ec6833','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:24:34.925727','Logo approved. Notes: None','2026-03-07 19:24:34.926646',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('c2fbe28d-6c8c-4c3a-b130-a509df1384de','Order','d205666e-75f2-4fcc-b7b7-0756c7234f77','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 05:59:13.748493','Logo approved. Notes: None','2026-03-08 05:59:13.748879',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('c54085d7-d6f1-4bd0-b16c-ec1f9ef5393e','Order','eeb1d308-ce46-4ff5-b916-c76cf1e40233','RequestRevision',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:54:29.215884','Revision requested: Need more contrast and different colors','2026-03-07 18:54:29.216697',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('c5a98864-ff92-414c-8a62-935d2bc55e0d','Order','92580ddc-b335-4365-9073-8a8eb1e6b9c4','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:52:03.020250','Logo approved. Notes: None','2026-03-07 18:52:03.021814',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('c6d7760c-0e02-4f0d-87e1-588ed2a0e714','Order','8f9f351b-9dce-4e17-ba6b-57a78f75dcef','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:37:17.076864','Logo approved. Notes: None','2026-03-07 18:37:17.077767',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('cd1bd750-16e8-486d-aed1-0a6b0d8d92cd','Order','289c2878-46a8-49e8-bdbb-6b97d066246d','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 05:58:51.273286','Logo approved. Notes: None','2026-03-08 05:58:51.527862',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('d27943ef-1609-42e9-b44b-57e4058b2d1a','Order','66b58313-11cb-474c-abac-abc59aad7c5d','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:36:10.274416','Logo approved. Notes: None','2026-03-07 18:36:10.275605',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('d4415ca5-8fb7-4316-a2f1-ccb5f427d88c','Order','fe5d0015-99cd-4784-926b-218913506e95','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:24:53.579156','Logo approved. Notes: None','2026-03-07 19:24:53.579736',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('dc46258c-18cf-4204-95a0-9d12a0ea1b9e','Order','6e56c6c2-bc4d-4509-a7d0-c8651cf818eb','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:50:04.489763','Logo approved. Notes: None','2026-03-07 18:50:04.491262',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('e0d99574-c808-417b-a451-ae7b5adb4d34','Order','c21ecbfd-55ec-4645-a6f1-bd5c254f957b','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:30:06.693481','Logo approved. Notes: None','2026-03-07 19:30:06.698308',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('e824ef4b-bc09-48b3-a0f6-d1bb06cb5aa5','Order','65be75be-eebf-41c8-970e-98a85cb924cb','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 18:31:51.881195','Logo approved. Notes: None','2026-03-07 18:31:53.303610',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('e886ea86-c99b-4f31-a0eb-01dd0524fc9c','Order','29244505-ac21-4b33-a5d3-2daaf146ae17','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-07 19:29:36.779926','Logo approved. Notes: None','2026-03-07 19:29:36.780574',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('ef997399-60ea-4878-9b55-a1a009bece7c','Order','42d4ac72-d397-4886-8fd8-bab880c0bbb6','ApproveLogo',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-09 00:38:13.760183','Logo approved. Notes: None','2026-03-09 00:38:13.866118',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('ffa7abc8-1f5f-4ade-a2d5-7a90cd717e61','Order','8c17e5a2-9acb-48b6-82cc-01e82a64830c','RequestRevision',NULL,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Client','2026-03-08 00:48:34.774755','Revision requested: Need more contrast and different colors','2026-03-08 00:48:34.775302',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL);
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
INSERT INTO `clientgalleries` VALUES ('0bc709f4-eaed-4294-a053-fe29ccdb3f5f','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','6e56c6c2-bc4d-4509-a7d0-c8651cf818eb','88b70dac-759b-4fe2-a448-425fda3b6657','Files\\Permanent\\1e40863d-5db9-487f-95d8-d7e24021281c.png','1e40863d-5db9-487f-95d8-d7e24021281c.png','test-1772909403202.png','Files\\Permanent\\1e40863d-5db9-487f-95d8-d7e24021281c.png','image/png','png','2026-03-07 18:50:04.447028','2026-03-07 18:50:04.491261',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('1034ed3a-d53d-4973-a5fb-bff357f1ce34','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','ea65a3fd-0266-4e19-ab22-7edcc14b1d73','d99770a6-c461-4f04-b637-acca3294817f','Files\\Permanent\\faae98eb-9080-42b5-b699-e04cc6b71fa9.png','faae98eb-9080-42b5-b699-e04cc6b71fa9.png','test-1772912028247.png','Files\\Permanent\\faae98eb-9080-42b5-b699-e04cc6b71fa9.png','image/png','png','2026-03-07 19:33:49.652261','2026-03-07 19:33:49.684246',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('133b2646-c665-49c8-b822-eda3f450a515','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','d0810784-0ea9-4b42-80e6-5bf982540e04','18018cea-dd25-4d6d-928e-9fb120b08c66','Files\\Permanent\\b95f9755-a8d3-498d-8223-95eea381d0b2.jpg','b95f9755-a8d3-498d-8223-95eea381d0b2.jpg','Lexus.jpg','Files\\Permanent\\b95f9755-a8d3-498d-8223-95eea381d0b2.jpg','image/jpeg','jpg','2026-03-09 16:09:13.256716','2026-03-09 16:09:13.391086',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('1796ecf9-20d8-4248-af91-b77e0e279e7b','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','d0810784-0ea9-4b42-80e6-5bf982540e04','52351804-8a3a-46fb-a512-3eed486c4a51','Files\\Permanent\\8b3d5f2e-8a20-46c8-aa1c-e2116596cdb6.jpeg','8b3d5f2e-8a20-46c8-aa1c-e2116596cdb6.jpeg','image3.jpeg','Files\\Permanent\\8b3d5f2e-8a20-46c8-aa1c-e2116596cdb6.jpeg','image/jpeg','jpeg','2026-03-09 16:09:13.274737','2026-03-09 16:09:13.391092',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('1c4077a8-5111-40e2-b0ef-336b91646d3a','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','42d4ac72-d397-4886-8fd8-bab880c0bbb6','0675b72f-d052-4e44-a325-bc3e95aab9b2','Files\\Permanent\\e4723e44-428e-479f-b5e3-f8ca832c4b16.png','e4723e44-428e-479f-b5e3-f8ca832c4b16.png','WORDPRESS ELEMANTOR.png','Files\\Permanent\\e4723e44-428e-479f-b5e3-f8ca832c4b16.png','image/png','png','2026-03-09 00:38:13.695636','2026-03-09 00:38:13.866112',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('23f92adc-67ec-41cd-ab3c-aad1bec0a7c4','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','477575c4-8915-4069-ad90-c7fbc30cad1c','049beb53-db89-4c75-bc91-a88605d59a85','Files\\Permanent\\4b132a6f-391a-485e-a2dd-462c8ba9be4e.png','4b132a6f-391a-485e-a2dd-462c8ba9be4e.png','test-1772930556502.png','Files\\Permanent\\4b132a6f-391a-485e-a2dd-462c8ba9be4e.png','image/png','png','2026-03-08 00:42:37.745365','2026-03-08 00:42:37.770610',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('263a9eda-be98-4cdb-933a-59f660ab8ddb','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','15a5b679-0b7b-4034-a69d-a7c37c072136','97f705de-da0a-48b3-9b62-b78787db96a0','Files\\Permanent\\f85ab7cf-5fd9-4b08-9d74-8dc21f7b6b31.png','f85ab7cf-5fd9-4b08-9d74-8dc21f7b6b31.png','test-1772912004903.png','Files\\Permanent\\f85ab7cf-5fd9-4b08-9d74-8dc21f7b6b31.png','image/png','png','2026-03-07 19:33:26.203596','2026-03-07 19:33:26.226913',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('282e00a9-1e96-4226-9788-4bb293c4071f','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','12044d25-2671-47f6-bea2-1934ca925308','94c9a4b8-eadf-4b8d-a1b5-1eced4e13d03','Files\\Permanent\\59f1e8cf-dbc3-4595-b700-2caf6639f95a.jpeg','59f1e8cf-dbc3-4595-b700-2caf6639f95a.jpeg','image3.jpeg','Files\\Permanent\\59f1e8cf-dbc3-4595-b700-2caf6639f95a.jpeg','image/jpeg','jpeg','2026-03-10 01:02:05.448803','2026-03-10 01:02:05.520338',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('2d3f8882-72e5-471a-b2a4-112e3e662fc5','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','fe5d0015-99cd-4784-926b-218913506e95','e105ef35-0b5e-4560-b636-2a61fab33ed8','Files\\Permanent\\a3c95c90-b25b-41ac-8105-f6d48902d19d.png','a3c95c90-b25b-41ac-8105-f6d48902d19d.png','test-1772911491998.png','Files\\Permanent\\a3c95c90-b25b-41ac-8105-f6d48902d19d.png','image/png','png','2026-03-07 19:24:53.539993','2026-03-07 19:24:53.579735',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('2e1dc9f8-9d98-4521-9f55-b53734a31aa6','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','d0def505-8103-413e-b8b4-06b5f0b9f663','Files\\Permanent\\12c5092b-feae-458e-b96a-a7055ebb0302.png','12c5092b-feae-458e-b96a-a7055ebb0302.png','Z2600227 wedding curated life.PNG','Files\\Permanent\\12c5092b-feae-458e-b96a-a7055ebb0302.png','image/png','PNG','2026-03-08 23:03:37.602187','2026-03-08 23:03:37.727042',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('3051e301-e10a-42ce-867e-ee2fe4abc03e','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','289c2878-46a8-49e8-bdbb-6b97d066246d','1157bd4f-6394-45b3-abe5-f253e2294353','Files\\Permanent\\90a1c22c-9a71-4b2a-94bf-acbcb8bde1b3.png','90a1c22c-9a71-4b2a-94bf-acbcb8bde1b3.png','test-1772930810628.png','Files\\Permanent\\90a1c22c-9a71-4b2a-94bf-acbcb8bde1b3.png','image/png','png','2026-03-08 05:58:51.130958','2026-03-08 05:58:51.527860',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('33a52bc5-837e-41e6-bbff-34904449fb06','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','42d4ac72-d397-4886-8fd8-bab880c0bbb6','eb8e7de1-01f4-433e-b5d4-ba7d7582d6ce','Files\\Permanent\\827d47b5-3b84-474a-afe9-865b13d3585e.png','827d47b5-3b84-474a-afe9-865b13d3585e.png','WORDPRESS BUSINESS WEBSITE.png','Files\\Permanent\\827d47b5-3b84-474a-afe9-865b13d3585e.png','image/png','png','2026-03-09 00:38:13.716634','2026-03-09 00:38:13.866117',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('3787fa84-3b03-41dc-858b-36239bb0fb42','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','12044d25-2671-47f6-bea2-1934ca925308','927e0987-371d-4e78-8623-fa4ad518d5db','Files\\Permanent\\20b38486-08f7-4cdb-9f20-5775c817b578.jpeg','20b38486-08f7-4cdb-9f20-5775c817b578.jpeg','image4.jpeg','Files\\Permanent\\20b38486-08f7-4cdb-9f20-5775c817b578.jpeg','image/jpeg','jpeg','2026-03-10 01:02:05.434446','2026-03-10 01:02:05.520337',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('3b8d87ef-4b82-48ef-ab97-60cbf11daa52','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','a6d9b2a7-b28d-419d-b813-5caed40939a6','1ab1678f-b7f0-46be-92bf-5cc4b296c525','Files\\Permanent\\7e1a0fac-0b24-4ca5-9b7f-c2ad0fbef6ca.png','7e1a0fac-0b24-4ca5-9b7f-c2ad0fbef6ca.png','test-1772908771608.png','Files\\Permanent\\7e1a0fac-0b24-4ca5-9b7f-c2ad0fbef6ca.png','image/png','png','2026-03-07 18:39:33.154860','2026-03-07 18:39:33.247492',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('3cfa70b2-bb71-4936-8ac8-c77ed47e4ea0','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','d081ef89-c6dd-45f4-84fa-8ed23979264c','3d33ec5b-45d4-4dd7-b5ca-2eb2c613cc2c','Files\\Permanent\\55e40f81-3caa-40b7-b4d6-e4d118cf2615.jpg','55e40f81-3caa-40b7-b4d6-e4d118cf2615.jpg','Logo 3-220mm wide JB SEW.JPG','Files\\Permanent\\55e40f81-3caa-40b7-b4d6-e4d118cf2615.jpg','image/jpeg','JPG','2026-03-09 16:17:59.664147','2026-03-09 16:17:59.689574',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('3e67f163-7f6e-4292-afe6-59bd16e7cfa7','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','d081ef89-c6dd-45f4-84fa-8ed23979264c','c51927f3-4332-4782-905e-a22146e3f0cb','Files\\Permanent\\b8e76cd0-ed71-423d-ba58-43bcb398a281.jpg','b8e76cd0-ed71-423d-ba58-43bcb398a281.jpg','Logo 2 – 220mm wide JB SEW.JPG','Files\\Permanent\\b8e76cd0-ed71-423d-ba58-43bcb398a281.jpg','image/jpeg','JPG','2026-03-09 16:17:59.671716','2026-03-09 16:17:59.689577',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('46ae7455-4405-4b70-8ad0-b62d1c4cc63d','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','d081ef89-c6dd-45f4-84fa-8ed23979264c','a5368fe6-366e-43e0-9f4a-e8a7658f60ad','Files\\Permanent\\4271e657-1f64-482d-8181-86244558dc36.jpg','4271e657-1f64-482d-8181-86244558dc36.jpg','Logo 1 – 100mm wide SEW.JPG','Files\\Permanent\\4271e657-1f64-482d-8181-86244558dc36.jpg','image/jpeg','JPG','2026-03-09 16:17:59.666707','2026-03-09 16:17:59.689576',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('4741fd62-8740-445d-8a6a-9d56df29a1d0','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','236455a1-1862-41c9-b062-4843422c0969','Files\\Permanent\\740b519b-6c5d-4b02-b5fc-9cb55eec602b.jpg','740b519b-6c5d-4b02-b5fc-9cb55eec602b.jpg','image005.jpg','Files\\Permanent\\740b519b-6c5d-4b02-b5fc-9cb55eec602b.jpg','image/jpeg','jpg','2026-03-08 23:03:37.598545','2026-03-08 23:03:37.727041',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('4a3c6673-9873-4bfd-abc7-db6bd403ea9c','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','2440b10a-5ff2-4387-893b-9237f5e1193a','38d87a9b-4023-4eac-a9fc-287efd146c36','Files\\Permanent\\cbea2c4d-1de6-4b1f-b36e-0089627aa679.png','cbea2c4d-1de6-4b1f-b36e-0089627aa679.png','test-1772909550400.png','Files\\Permanent\\cbea2c4d-1de6-4b1f-b36e-0089627aa679.png','image/png','png','2026-03-07 18:52:34.929781','2026-03-07 18:52:35.020282',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('4f41b389-fe09-48e9-89c3-45920733c4ac','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','81e864db-dc69-480f-a4ee-442106d3131f','e40f51a6-2d15-4abd-a47b-41dd253be9c7','Files\\Permanent\\d132adc4-0d86-4855-8bb7-8edc0d5f258f.png','d132adc4-0d86-4855-8bb7-8edc0d5f258f.png','test-1772910695220.png','Files\\Permanent\\d132adc4-0d86-4855-8bb7-8edc0d5f258f.png','image/png','png','2026-03-07 19:11:37.531937','2026-03-07 19:11:37.602558',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('514c9ed8-7f76-424b-b972-7e6c4cfc000b','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','3c5b0459-7bcc-4818-ac24-ac9b6af11383','136d34f0-3523-410b-bc71-7f6fa0345b6d','Files\\Permanent\\6234f731-c972-4ce3-a1b6-4d2cac1bf51d.png','6234f731-c972-4ce3-a1b6-4d2cac1bf51d.png','GASS_Hero_v1.4_Green_White.png','Files\\Permanent\\6234f731-c972-4ce3-a1b6-4d2cac1bf51d.png','image/png','png','2026-03-08 22:08:28.539886','2026-03-08 22:08:28.573460',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('5b7034cd-e7b2-4fe5-8629-9c93ffd25b5f','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','cf2dc5e6-2797-441d-ba05-7d465abdf900','e55338f2-e07f-4aae-98df-4e9ab966874d','Files\\Permanent\\1c5296fb-b5de-47aa-a1d7-651b7ce0a574.png','1c5296fb-b5de-47aa-a1d7-651b7ce0a574.png','image001 (3).png','Files\\Permanent\\1c5296fb-b5de-47aa-a1d7-651b7ce0a574.png','image/png','png','2026-03-08 22:02:48.241282','2026-03-08 22:02:48.341214',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('5fa2cfbe-e908-462d-a89c-8e5f1656a994','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','3c5b0459-7bcc-4818-ac24-ac9b6af11383','68129cfe-aebb-4e64-b9f5-ce092df1affe','Files\\Permanent\\e12119c7-a6f4-4d32-b375-fd77314f9b03.jpg','e12119c7-a6f4-4d32-b375-fd77314f9b03.jpg','Wayfarer Caravan Services Logo LC SEW.JPG','Files\\Permanent\\e12119c7-a6f4-4d32-b375-fd77314f9b03.jpg','image/jpeg','JPG','2026-03-08 22:08:28.546909','2026-03-08 22:08:28.573461',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('641e15a9-1859-445d-b768-bed0350de457','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','29244505-ac21-4b33-a5d3-2daaf146ae17','f3017999-e636-49db-b585-9b02071e4a0e','Files\\Permanent\\bcbfd578-19e9-4f2a-a649-7b340e561cd5.png','bcbfd578-19e9-4f2a-a649-7b340e561cd5.png','test-1772911771703.png','Files\\Permanent\\bcbfd578-19e9-4f2a-a649-7b340e561cd5.png','image/png','png','2026-03-07 19:29:36.759442','2026-03-07 19:29:36.780573',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('64580228-f047-46b8-8461-33b3e8ea512d','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','5489aa89-8bfb-45bf-ba13-1fce428f30db','62a91f95-ff30-4ed8-8631-deab30abed9b','Files\\Permanent\\30651048-69b6-4c73-b539-430d4692beb5.png','30651048-69b6-4c73-b539-430d4692beb5.png','test-1772909671790.png','Files\\Permanent\\30651048-69b6-4c73-b539-430d4692beb5.png','image/png','png','2026-03-07 18:54:34.418479','2026-03-07 18:54:34.448126',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('6979791a-8b6c-48d8-9fc3-c57769223f7c','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','c21ecbfd-55ec-4645-a6f1-bd5c254f957b','83d4cdd5-c522-43d7-ab99-3f9bbc4a0ec1','Files\\Permanent\\00ee67e1-b242-40e8-afa9-9432148dae72.png','00ee67e1-b242-40e8-afa9-9432148dae72.png','test-1772911805663.png','Files\\Permanent\\00ee67e1-b242-40e8-afa9-9432148dae72.png','image/png','png','2026-03-07 19:30:06.643061','2026-03-07 19:30:06.698307',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('7059beb5-6b3b-4d01-acd8-98a1ca917cce','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','12044d25-2671-47f6-bea2-1934ca925308','f0f47258-1532-45da-9c1a-773dfd372b1b','Files\\Permanent\\36216158-01fb-4f0b-b3aa-84c455e8a15c.jpg','36216158-01fb-4f0b-b3aa-84c455e8a15c.jpg','USS Logo - Horizontal.jpg','Files\\Permanent\\36216158-01fb-4f0b-b3aa-84c455e8a15c.jpg','image/jpeg','jpg','2026-03-10 01:02:05.452545','2026-03-10 01:02:05.520339',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('74163009-f419-401e-ac16-9be8e699bca7','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','cf2dc5e6-2797-441d-ba05-7d465abdf900','d1c8c656-a853-44bf-a821-9f3901998ab5','Files\\Permanent\\c783291d-75c6-4554-9555-2129a7dfd926.jpg','c783291d-75c6-4554-9555-2129a7dfd926.jpg','Waylen Bay Skeleton.jpg','Files\\Permanent\\c783291d-75c6-4554-9555-2129a7dfd926.jpg','image/jpeg','jpg','2026-03-08 22:02:48.239006','2026-03-08 22:02:48.341213',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('762a49df-5238-4254-9c18-c7d4a67e3171','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','3c5b0459-7bcc-4818-ac24-ac9b6af11383','7c9c3643-47af-45bb-b5f7-0c578a49510b','Files\\Permanent\\4246f543-f464-409b-b271-f3decae3cd23.jpg','4246f543-f464-409b-b271-f3decae3cd23.jpg','CSC Logo - Black BG - Square.jpg','Files\\Permanent\\4246f543-f464-409b-b271-f3decae3cd23.jpg','image/jpeg','jpg','2026-03-08 22:08:28.553394','2026-03-08 22:08:28.573462',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('77e442d3-a911-4794-8a76-0d94336f2974','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','cf2dc5e6-2797-441d-ba05-7d465abdf900','5b7977eb-0313-4c36-87b5-fad8a4bab42e','Files\\Permanent\\3fbef068-08e0-4c8d-ac31-c9c24f6db74e.jpg','3fbef068-08e0-4c8d-ac31-c9c24f6db74e.jpg','Waylen Bay Yacht.jpg','Files\\Permanent\\3fbef068-08e0-4c8d-ac31-c9c24f6db74e.jpg','image/jpeg','jpg','2026-03-08 22:02:48.232469','2026-03-08 22:02:48.341212',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('7853484f-c791-41ed-b129-ac19072187df','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','d205666e-75f2-4fcc-b7b7-0756c7234f77','1bf7ee9e-7805-481e-b389-d13b642c0f3f','Files\\Permanent\\87ec86fe-dbec-47af-ac43-9ebb0d4461e0.png','87ec86fe-dbec-47af-ac43-9ebb0d4461e0.png','test-1772930612312.png','Files\\Permanent\\87ec86fe-dbec-47af-ac43-9ebb0d4461e0.png','image/png','png','2026-03-08 05:59:13.729928','2026-03-08 05:59:13.748878',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('7f1c56ae-629d-451b-883c-3fa36eb06a97','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','1db86b3a-f3cb-4e89-8e4b-4a2cf51929ef','Files\\Permanent\\2426dbe5-5f8a-4144-a96e-2144e017ee33.png','2426dbe5-5f8a-4144-a96e-2144e017ee33.png','Tugun Bowls Logo-03.png','Files\\Permanent\\2426dbe5-5f8a-4144-a96e-2144e017ee33.png','image/png','png','2026-03-08 23:03:37.585530','2026-03-08 23:03:37.727040',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('7f391f57-6dbb-4e75-9369-4855d1f7318a','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','a0454a6c-9bd0-4de7-9057-a456495c9afd','72962fd1-1abb-4833-967c-99e724d8dfa4','Files\\Permanent\\46da3d68-9926-4d98-ad3f-4c5463ea9e44.png','46da3d68-9926-4d98-ad3f-4c5463ea9e44.png','test-1772908364528.png','Files\\Permanent\\46da3d68-9926-4d98-ad3f-4c5463ea9e44.png','image/png','png','2026-03-07 18:32:51.159692','2026-03-07 18:32:51.253199',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('7fca9ccd-8d0e-4964-9296-a6fa722447e7','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','8f9f351b-9dce-4e17-ba6b-57a78f75dcef','0cd5da91-2947-4241-852b-b682be7b404c','Files\\Permanent\\feaf74f0-42a9-4686-b51a-513272b25ced.png','feaf74f0-42a9-4686-b51a-513272b25ced.png','test-1772908634418.png','Files\\Permanent\\feaf74f0-42a9-4686-b51a-513272b25ced.png','image/png','png','2026-03-07 18:37:17.030870','2026-03-07 18:37:17.077766',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('88906001-faa4-4013-b9fd-b52f737c3019','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','cf2dc5e6-2797-441d-ba05-7d465abdf900','936fe813-575d-4492-965e-b82a069f53ee','Files\\Permanent\\85fb17ab-6fe5-4a73-ab7a-2e60864dd18d.eps','85fb17ab-6fe5-4a73-ab7a-2e60864dd18d.eps','Cranbourne Basketball Academy - Logo WHITE.eps','Files\\Permanent\\85fb17ab-6fe5-4a73-ab7a-2e60864dd18d.eps','application/postscript','eps','2026-03-08 22:02:48.235992','2026-03-08 22:02:48.341213',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('9afd4d3b-8b00-41e2-b0c3-373117b54824','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','45761335-71fb-41a5-8205-bea14ef012d4','eaa7415f-1ab5-4014-800e-0c430337865c','Files\\Permanent\\83d9092f-2dcc-49b6-a409-e2cbd8aad76d.png','83d9092f-2dcc-49b6-a409-e2cbd8aad76d.png','test-1772908793231.png','Files\\Permanent\\83d9092f-2dcc-49b6-a409-e2cbd8aad76d.png','image/png','png','2026-03-07 18:39:54.619587','2026-03-07 18:39:54.649549',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('9ffb3608-eb7c-48da-899b-32c85dfba93d','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','cf2dc5e6-2797-441d-ba05-7d465abdf900','049847c6-5873-4bec-8e5e-24c6475dcf27','Files\\Permanent\\5fec2860-0474-414f-89aa-760507e6c8f2.jpeg','5fec2860-0474-414f-89aa-760507e6c8f2.jpeg','IMG_0976.jpeg','Files\\Permanent\\5fec2860-0474-414f-89aa-760507e6c8f2.jpeg','image/jpeg','jpeg','2026-03-08 22:02:48.222010','2026-03-08 22:02:48.341209',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('aafbe1c7-3622-4256-9a1f-8a0bcca4a705','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','42d4ac72-d397-4886-8fd8-bab880c0bbb6','4c2ec04c-6310-487f-97d1-7dc34b7ef30c','Files\\Permanent\\2603a7cc-9397-4ca2-8324-ab8b3c18eb96.jpeg','2603a7cc-9397-4ca2-8324-ab8b3c18eb96.jpeg','HMLogo.jpeg','Files\\Permanent\\2603a7cc-9397-4ca2-8324-ab8b3c18eb96.jpeg','image/jpeg','jpeg','2026-03-09 00:38:13.708339','2026-03-09 00:38:13.866114',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('b30f5afa-8c8c-4d64-839d-20aaa0f968b4','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','c962c2c9-7764-4d90-9f42-a195f60e65be','7b743005-d091-4ab5-993d-3dc2a0deed09','Files\\Permanent\\00aa80f6-fbfe-482d-a9b5-0689c02753a7.png','00aa80f6-fbfe-482d-a9b5-0689c02753a7.png','test-1772909651671.png','Files\\Permanent\\00aa80f6-fbfe-482d-a9b5-0689c02753a7.png','image/png','png','2026-03-07 18:54:13.946807','2026-03-07 18:54:13.994059',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('bd736c35-a956-48e8-bf64-0d9b0f82ca5d','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','75ac4d55-694d-4189-811e-1100be2fb1da','d5a6b1a0-178a-430e-8c84-84286d09fd92','Files\\Permanent\\e48ffccf-8fcf-4b70-ac15-9c3fe064d7cc.png','e48ffccf-8fcf-4b70-ac15-9c3fe064d7cc.png','test-1772930747611.png','Files\\Permanent\\e48ffccf-8fcf-4b70-ac15-9c3fe064d7cc.png','image/png','png','2026-03-08 00:45:49.814127','2026-03-08 00:45:49.864169',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('d1063606-b1b9-464a-9cc2-f147798c513b','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','1ba46150-5c19-41d1-80cc-87d6bc8e7fbb','84b17610-16b5-422f-8b87-ac2e151b4f42','Files\\Permanent\\137a944e-8456-444c-a015-39d99af7c5d1.png','137a944e-8456-444c-a015-39d99af7c5d1.png','test-1772910651903.png','Files\\Permanent\\137a944e-8456-444c-a015-39d99af7c5d1.png','image/png','png','2026-03-07 19:10:54.524418','2026-03-07 19:10:54.562614',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('d2df5f1e-e086-4d83-a346-eaa0ccf77be2','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','6aa76a40-7a13-4390-974a-12cd49a269a9','71dd1221-674c-4924-ad23-9fffacf27869','Files\\Permanent\\6176fc64-2368-4c40-80ca-ee92f9373c2d.png','6176fc64-2368-4c40-80ca-ee92f9373c2d.png','test-1772910744836.png','Files\\Permanent\\6176fc64-2368-4c40-80ca-ee92f9373c2d.png','image/png','png','2026-03-07 19:12:27.191551','2026-03-07 19:12:27.229040',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('d3f22bf1-22e9-44f2-970e-3ec26f42fb02','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','92580ddc-b335-4365-9073-8a8eb1e6b9c4','6b8c3d6c-95ab-45b0-9321-90e27a427e68','Files\\Permanent\\1c6652d8-386a-4713-a044-d6be85c09369.png','1c6652d8-386a-4713-a044-d6be85c09369.png','test-1772909520980.png','Files\\Permanent\\1c6652d8-386a-4713-a044-d6be85c09369.png','image/png','png','2026-03-07 18:52:02.968981','2026-03-07 18:52:03.021811',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('d7813ddc-abe2-4b84-83f4-cf821608c819','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','d0810784-0ea9-4b42-80e6-5bf982540e04','1e328fcc-0e6f-49f1-ab64-630dc4758167','Files\\Permanent\\699b6449-cdbf-4cab-9770-4b1cb97489f7.jpg','699b6449-cdbf-4cab-9770-4b1cb97489f7.jpg','USS Logo - Horizontal.jpg','Files\\Permanent\\699b6449-cdbf-4cab-9770-4b1cb97489f7.jpg','image/jpeg','jpg','2026-03-09 16:09:13.271911','2026-03-09 16:09:13.391087',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('dba1b883-c437-4a90-8ca0-e1d73e85ad74','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','ed2e4a46-1935-4170-a80e-0aa7eb987a67','4a98f728-5c29-4892-a231-2deb1e7304df','Files\\Permanent\\8e633e84-d9fd-4da8-8621-d2fd22005383.png','8e633e84-d9fd-4da8-8621-d2fd22005383.png','test-1772910396580.png','Files\\Permanent\\8e633e84-d9fd-4da8-8621-d2fd22005383.png','image/png','png','2026-03-07 19:06:38.650498','2026-03-07 19:06:38.869870',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('e7ba91ad-c9db-4655-8adb-8741ff534b18','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','42d4ac72-d397-4886-8fd8-bab880c0bbb6','94996e62-98f0-4285-bb36-0320dbd472f4','Files\\Permanent\\68f3c1a2-1362-453e-8487-49d4f7f1b7d6.jpeg','68f3c1a2-1362-453e-8487-49d4f7f1b7d6.jpeg','HMmotor.jpeg','Files\\Permanent\\68f3c1a2-1362-453e-8487-49d4f7f1b7d6.jpeg','image/jpeg','jpeg','2026-03-09 00:38:13.712129','2026-03-09 00:38:13.866115',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('eafb224c-bb54-4a23-af0a-b54f21eaccf3','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','4c2a694f-d745-4b98-b84e-2c0a70785eb9','3a2f1130-b693-4f91-8b0b-a4d36ad78091','Files\\Permanent\\0cc9857a-cd21-4f6c-9b5f-ecbe8e1b0e3b.png','0cc9857a-cd21-4f6c-9b5f-ecbe8e1b0e3b.png','test-1772930901184.png','Files\\Permanent\\0cc9857a-cd21-4f6c-9b5f-ecbe8e1b0e3b.png','image/png','png','2026-03-08 00:48:37.241474','2026-03-08 00:48:37.264581',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('ef63b25b-8b40-48c2-87a6-2204d347689b','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','d0810784-0ea9-4b42-80e6-5bf982540e04','8b74c7bb-21fd-4bf4-8f36-10fda610cfce','Files\\Permanent\\0f83638d-9d43-4241-810a-2f81948a4cb4.jpeg','0f83638d-9d43-4241-810a-2f81948a4cb4.jpeg','image4.jpeg','Files\\Permanent\\0f83638d-9d43-4241-810a-2f81948a4cb4.jpeg','image/jpeg','jpeg','2026-03-09 16:09:13.278587','2026-03-09 16:09:13.391092',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('f5bc1299-e8c0-4b04-8c57-587e963cce06','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','051d22a2-668d-42e7-a161-cfccc5122fc1','8506b57f-45bf-439d-8982-465778cb3235','Files\\Permanent\\ed2b54d0-c16a-4f8d-999a-5796531d2391.png','ed2b54d0-c16a-4f8d-999a-5796531d2391.png','test-1772909416509.png','Files\\Permanent\\ed2b54d0-c16a-4f8d-999a-5796531d2391.png','image/png','png','2026-03-07 18:50:18.238829','2026-03-07 18:50:18.269646',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('f6c600ff-976e-4e12-842a-dd6382aa97be','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','66b58313-11cb-474c-abac-abc59aad7c5d','90262251-0918-4091-b162-2c1384cefff1','Files\\Permanent\\d9d5d3a6-f3c0-4bcd-9425-2d499154c673.png','d9d5d3a6-f3c0-4bcd-9425-2d499154c673.png','test-1772908567242.png','Files\\Permanent\\d9d5d3a6-f3c0-4bcd-9425-2d499154c673.png','image/png','png','2026-03-07 18:36:10.209093','2026-03-07 18:36:10.275604',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('fb0eab4d-29e0-4bea-9630-505d1c88c5cd','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','b243a4a3-81c8-4257-9e95-f41c80ec6833','c7b91756-7c67-4e14-abdb-241cd7db5a95','Files\\Permanent\\4f6f7106-0462-40bd-b7ee-1ebd8172ee08.png','4f6f7106-0462-40bd-b7ee-1ebd8172ee08.png','test-1772911473248.png','Files\\Permanent\\4f6f7106-0462-40bd-b7ee-1ebd8172ee08.png','image/png','png','2026-03-07 19:24:34.864120','2026-03-07 19:24:34.926644',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('fe62ca94-cfe0-4565-b54f-e5777a73de99','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','65be75be-eebf-41c8-970e-98a85cb924cb','d93464af-36a7-48c7-b8cb-09052e8acc1f','Files\\Permanent\\dd781a2f-25ee-4aa4-bc23-db51a9260e28.png','dd781a2f-25ee-4aa4-bc23-db51a9260e28.png','test-1772908299773.png','Files\\Permanent\\dd781a2f-25ee-4aa4-bc23-db51a9260e28.png','image/png','png','2026-03-07 18:31:51.027344','2026-03-07 18:31:53.303606',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL),('fff05db4-3586-400f-b1aa-a0fad1bda9c9','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','a53d1745-10df-4ca7-a33b-e3736e54c681','7cc1ce74-01f2-4d20-a8fa-fd8376405e5b','Files\\Permanent\\16f9ca7a-13b8-4c80-8bb2-967fdba1ab8f.png','16f9ca7a-13b8-4c80-8bb2-967fdba1ab8f.png','test-1772930810352.png','Files\\Permanent\\16f9ca7a-13b8-4c80-8bb2-967fdba1ab8f.png','image/png','png','2026-03-08 05:59:01.709701','2026-03-08 05:59:01.724536',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `clientgalleries` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `clientlogopricings`
--

LOCK TABLES `clientlogopricings` WRITE;
/*!40000 ALTER TABLE `clientlogopricings` DISABLE KEYS */;
INSERT INTO `clientlogopricings` VALUES ('f9bc9efc-3bea-4dd4-84f2-43d5180b644e','566b3bb9-ab8b-40f3-b415-d79dfc2f48db',1,1,3.00,'USD',1,'2026-03-10 00:55:02.833769',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `clientlogopricings` ENABLE KEYS */;
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
  `BillingType` int NOT NULL DEFAULT '1',
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
INSERT INTO `clientprofiles` VALUES ('566b3bb9-ab8b-40f3-b415-d79dfc2f48db','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Next Tech Vision',NULL,NULL,NULL,NULL,NULL,'2026-03-05 15:03:38.768449','2026-03-07 17:27:05.850975',NULL,NULL,0,NULL,NULL,NULL,'Kevin sharma',NULL,NULL,NULL,NULL,NULL,1);
/*!40000 ALTER TABLE `clientprofiles` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `designerinvoiceadjustments`
--

LOCK TABLES `designerinvoiceadjustments` WRITE;
/*!40000 ALTER TABLE `designerinvoiceadjustments` DISABLE KEYS */;
/*!40000 ALTER TABLE `designerinvoiceadjustments` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `designerinvoiceitems`
--

LOCK TABLES `designerinvoiceitems` WRITE;
/*!40000 ALTER TABLE `designerinvoiceitems` DISABLE KEYS */;
INSERT INTO `designerinvoiceitems` VALUES ('037019c5-7c05-4fad-88f4-deb64ccb3724','5d16ef00-7319-47e7-a265-f7f046ba2b31','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','Order: uhoihuoh',350.00,'2026-03-09 08:10:39.503269',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL),('737ccc6c-13eb-4989-9148-f56fa6678e6f','f583f816-78c7-4fe7-9fe5-5c4be95fdaf4','d0810784-0ea9-4b42-80e6-5bf982540e04','Order: Testing 1',350.00,'2026-03-09 16:10:42.345305',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `designerinvoiceitems` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `designerinvoices`
--

LOCK TABLES `designerinvoices` WRITE;
/*!40000 ALTER TABLE `designerinvoices` DISABLE KEYS */;
INSERT INTO `designerinvoices` VALUES ('5d16ef00-7319-47e7-a265-f7f046ba2b31','489a32db-6b7b-4737-9433-c7ef19627799','DINV-202603-37111497',350.00,1,'March 2026','2026-03-09 08:10:39.208203','2026-03-09 08:46:27.272529',NULL,'2026-03-09 08:10:39.503264','2026-03-09 08:46:27.302974','da6639fc-4721-4958-940d-bf3a910b434a','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL),('f583f816-78c7-4fe7-9fe5-5c4be95fdaf4','489a32db-6b7b-4737-9433-c7ef19627799','DINV-202603-6D0422E4',350.00,0,'March 2026','2026-03-09 16:10:42.201900',NULL,NULL,'2026-03-09 16:10:42.345304',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `designerinvoices` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `designerlogopricings`
--

DROP TABLE IF EXISTS `designerlogopricings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `designerlogopricings` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `DesignerId` char(36) NOT NULL,
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
  UNIQUE KEY `IX_DesignerLogoPricings_DesignerId_DesignCategory_DesignType` (`DesignerId`,`DesignCategory`,`DesignType`),
  CONSTRAINT `FK_DesignerLogoPricings_DesignerProfiles_DesignerId` FOREIGN KEY (`DesignerId`) REFERENCES `designerprofiles` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `designerlogopricings`
--

LOCK TABLES `designerlogopricings` WRITE;
/*!40000 ALTER TABLE `designerlogopricings` DISABLE KEYS */;
/*!40000 ALTER TABLE `designerlogopricings` ENABLE KEYS */;
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
INSERT INTO `designerprofiles` VALUES ('489a32db-6b7b-4737-9433-c7ef19627799','69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,NULL,NULL,1,'2026-03-06 17:25:27.201009',NULL,NULL,NULL,0,NULL,NULL,NULL);
/*!40000 ALTER TABLE `designerprofiles` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `designpricings`
--

LOCK TABLES `designpricings` WRITE;
/*!40000 ALTER TABLE `designpricings` DISABLE KEYS */;
INSERT INTO `designpricings` VALUES ('a1000001-0000-0000-0000-000000000001',1,1,350.00,1,'2026-03-09 01:01:05.000000',NULL,NULL,NULL,0,NULL,NULL),('a1000001-0000-0000-0000-000000000002',1,2,700.00,1,'2026-03-09 01:01:05.000000',NULL,NULL,NULL,0,NULL,NULL),('a1000001-0000-0000-0000-000000000003',2,3,350.00,1,'2026-03-09 01:01:05.000000',NULL,NULL,NULL,0,NULL,NULL),('a1000001-0000-0000-0000-000000000004',2,4,0.00,1,'2026-03-09 01:01:05.000000',NULL,NULL,NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `designpricings` ENABLE KEYS */;
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
INSERT INTO `invoicelogs` VALUES ('13a5d1ba-a7eb-4fc0-a047-51c454ba524d','63789d63-8100-4faa-bef5-eeb3db8b7c25',1,'da6639fc-4721-4958-940d-bf3a910b434a','Invoice created','2026-03-09 00:38:27.294929',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL),('2d38fcb6-941c-491a-a9c7-0e58da0a8409','596d6a30-86ab-4eda-ae61-c91040c799e6',1,'da6639fc-4721-4958-940d-bf3a910b434a','Invoice created','2026-03-08 23:03:56.431688',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL),('5e0a4a20-5e53-4f2d-8bb0-265cfca83ed7','e2a24275-9323-4ce9-8d13-6d124ef8b9cf',1,'da6639fc-4721-4958-940d-bf3a910b434a','Invoice created','2026-03-09 16:09:45.999980',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL),('6a6adf46-770d-41db-b799-17e83af9150b','610483e2-8920-4c0a-94b9-5b117f42b549',1,'da6639fc-4721-4958-940d-bf3a910b434a','Invoice created','2026-03-08 22:03:20.101075',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL),('a3285626-aea4-47eb-a47c-599f65e85e24','c0a4ff35-7cb1-4dae-bb9f-c649d8ae6b47',1,'da6639fc-4721-4958-940d-bf3a910b434a','Invoice created','2026-03-08 15:13:36.932967',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL),('b8234f33-dc59-43f9-8dce-a62a2eddc20f','9a202643-b1b7-4030-a9a3-9ab924f65cd9',1,'da6639fc-4721-4958-940d-bf3a910b434a','Invoice created','2026-03-09 00:45:11.888137',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL),('bd1ce1ab-6b1e-4d5f-969e-3fee4b2019b0','ecd033ee-6914-418e-ae72-fa76c0b9e11e',1,'da6639fc-4721-4958-940d-bf3a910b434a','Invoice created','2026-03-08 22:09:20.427881',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL),('d4aeb789-bd2c-4079-8ada-2764023283ea','18af4834-537f-453d-864f-6c05326a1a31',1,'da6639fc-4721-4958-940d-bf3a910b434a','Invoice created','2026-03-09 16:18:19.039656',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL);
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
  KEY `IX_InvoiceOrders_OrderId` (`OrderId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `invoiceorders`
--

LOCK TABLES `invoiceorders` WRITE;
/*!40000 ALTER TABLE `invoiceorders` DISABLE KEYS */;
INSERT INTO `invoiceorders` VALUES ('50c73927-bcb7-4d54-af60-7b3bfe8d7267','ecd033ee-6914-418e-ae72-fa76c0b9e11e','3c5b0459-7bcc-4818-ac24-ac9b6af11383','2026-03-08 22:09:20.427877',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,2.00,'test 2'),('79025edd-1979-4240-8671-50120581f5d5','9a202643-b1b7-4030-a9a3-9ab924f65cd9','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','2026-03-09 00:45:11.888136',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,7.00,'uhoihuoh'),('8520c716-8e24-472e-a123-b8bb66dfe967','18af4834-537f-453d-864f-6c05326a1a31','d081ef89-c6dd-45f4-84fa-8ed23979264c','2026-03-09 16:18:19.039655',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,0.01,'fwertfws'),('c871bc49-1e6d-4234-a011-8d3d6bf2b64c','596d6a30-86ab-4eda-ae61-c91040c799e6','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','2026-03-08 23:03:56.431688',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,3.00,'Tets invoice'),('e90df64d-4d85-4864-b7ca-151f68d4c6a1','e2a24275-9323-4ce9-8d13-6d124ef8b9cf','d0810784-0ea9-4b42-80e6-5bf982540e04','2026-03-09 16:09:45.999979',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,2.00,'Testing 1'),('ebd3242a-6e19-43c8-959a-08d046160f7e','610483e2-8920-4c0a-94b9-5b117f42b549','cf2dc5e6-2797-441d-ba05-7d465abdf900','2026-03-08 22:03:20.101075',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,3.00,'Test 1'),('fba8ac61-30aa-4833-8394-55b4e2191a99','63789d63-8100-4faa-bef5-eeb3db8b7c25','42d4ac72-d397-4886-8fd8-bab880c0bbb6','2026-03-09 00:38:27.294929',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,2.00,'Tetsing');
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
-- Dumping data for table `invoices`
--

LOCK TABLES `invoices` WRITE;
/*!40000 ALTER TABLE `invoices` DISABLE KEYS */;
INSERT INTO `invoices` VALUES ('18af4834-537f-453d-864f-6c05326a1a31','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','INV-20260309-4EDF9CEC',0.01,0.00,0.01,'2026-03-09 16:18:19.029605','2026-04-08 16:18:19.029606',NULL,NULL,NULL,'2026-03-09 16:18:19.039654',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,NULL,1,1,NULL),('596d6a30-86ab-4eda-ae61-c91040c799e6','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','INV-20260308-D87782EB',3.00,0.00,3.00,'2026-03-08 23:03:56.292667','2026-04-07 23:03:56.292790',NULL,NULL,NULL,'2026-03-08 23:03:56.431685',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,NULL,1,1,NULL),('610483e2-8920-4c0a-94b9-5b117f42b549','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','INV-20260308-D076D28D',3.00,0.00,3.00,'2026-03-08 22:03:19.994713','2026-04-07 22:03:19.994801',NULL,NULL,NULL,'2026-03-08 22:03:20.101073',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,NULL,1,1,NULL),('63789d63-8100-4faa-bef5-eeb3db8b7c25','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','INV-20260309-533E29D5',2.00,0.00,2.00,'2026-03-09 00:38:27.127478','2026-04-08 00:38:27.127733',NULL,NULL,NULL,'2026-03-09 00:38:27.294925',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,NULL,1,1,NULL),('9a202643-b1b7-4030-a9a3-9ab924f65cd9','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','INV-20260309-5A4E7B8D',7.00,0.00,7.00,'2026-03-09 00:45:11.864130','2026-04-08 00:45:11.864131',NULL,NULL,NULL,'2026-03-09 00:45:11.888135',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,NULL,1,1,NULL),('e2a24275-9323-4ce9-8d13-6d124ef8b9cf','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','INV-20260309-114824AD',2.00,0.00,2.00,'2026-03-09 16:09:45.866543','2026-04-08 16:09:45.866628',NULL,NULL,NULL,'2026-03-09 16:09:45.999978',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,NULL,1,1,NULL),('ecd033ee-6914-418e-ae72-fa76c0b9e11e','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','INV-20260308-F00507D0',2.00,0.00,2.00,'2026-03-08 22:09:20.424246','2026-04-07 22:09:20.424247',NULL,NULL,NULL,'2026-03-08 22:09:20.427875',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,NULL,1,1,NULL);
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
-- Dumping data for table `logofiles`
--

LOCK TABLES `logofiles` WRITE;
/*!40000 ALTER TABLE `logofiles` DISABLE KEYS */;
INSERT INTO `logofiles` VALUES ('049847c6-5873-4bec-8e5e-24c6475dcf27','cf2dc5e6-2797-441d-ba05-7d465abdf900','5fec2860-0474-414f-89aa-760507e6c8f2.jpeg','IMG_0976.jpeg','Files\\Permanent\\5fec2860-0474-414f-89aa-760507e6c8f2.jpeg','image/jpeg',3462920,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 22:01:03.617747','2026-03-08 22:02:48.341218','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 22:02:48.221147','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'493888ec-f2b2-4fe0-9465-61a0627a3fcf'),('0675b72f-d052-4e44-a325-bc3e95aab9b2','42d4ac72-d397-4886-8fd8-bab880c0bbb6','e4723e44-428e-479f-b5e3-f8ca832c4b16.png','WORDPRESS ELEMANTOR.png','Files\\Permanent\\e4723e44-428e-479f-b5e3-f8ca832c4b16.png','image/png',976655,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 00:30:35.951545','2026-03-09 00:38:13.866121','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 00:38:13.694532','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'ff30982f-9bc6-422c-9187-3658bdea2a93'),('10bb80a5-7e1a-48b8-8819-6992c5d07a73','12044d25-2671-47f6-bea2-1934ca925308','e64a9e8b-62c5-4d40-aa10-d583f98fda37.jpg','MAEng-Logo.jpg','Files\\e64a9e8b-62c5-4d40-aa10-d583f98fda37.jpg','image/jpeg',776287,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-10 00:55:49.830564','2026-03-10 01:01:55.862826','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,'2026-03-10 01:01:55.858821','da6639fc-4721-4958-940d-bf3a910b434a',1,1,1,1,NULL,1,NULL),('136d34f0-3523-410b-bc71-7f6fa0345b6d','3c5b0459-7bcc-4818-ac24-ac9b6af11383','6234f731-c972-4ce3-a1b6-4d2cac1bf51d.png','GASS_Hero_v1.4_Green_White.png','Files\\Permanent\\6234f731-c972-4ce3-a1b6-4d2cac1bf51d.png','image/png',39191,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 22:06:56.820898','2026-03-08 22:08:28.573465','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 22:08:28.539790','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'080f8990-4271-41a9-b295-67e54bb8ce09'),('18018cea-dd25-4d6d-928e-9fb120b08c66','d0810784-0ea9-4b42-80e6-5bf982540e04','b95f9755-a8d3-498d-8223-95eea381d0b2.jpg','Lexus.jpg','Files\\Permanent\\b95f9755-a8d3-498d-8223-95eea381d0b2.jpg','image/jpeg',74009,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 16:08:35.662672','2026-03-09 16:09:13.391098','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 16:09:13.255268','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'02fdd1c7-75f6-47cd-b5a2-7ef6a6d0e6ff'),('1db86b3a-f3cb-4e89-8e4b-4a2cf51929ef','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','2426dbe5-5f8a-4144-a96e-2144e017ee33.png','Tugun Bowls Logo-03.png','Files\\Permanent\\2426dbe5-5f8a-4144-a96e-2144e017ee33.png','image/png',105084,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 23:02:43.835597','2026-03-08 23:03:37.727056','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 23:03:37.584217','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'1f0b487c-6e5f-487f-823c-a99dbb412321'),('1e328fcc-0e6f-49f1-ab64-630dc4758167','d0810784-0ea9-4b42-80e6-5bf982540e04','699b6449-cdbf-4cab-9770-4b1cb97489f7.jpg','USS Logo - Horizontal.jpg','Files\\Permanent\\699b6449-cdbf-4cab-9770-4b1cb97489f7.jpg','image/jpeg',276941,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 16:08:35.662669','2026-03-09 16:09:13.391098','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 16:09:13.271897','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'02fdd1c7-75f6-47cd-b5a2-7ef6a6d0e6ff'),('21839888-d150-43f2-8c3a-4559fd167b9f','4e6f84a6-f27f-4c15-a5ee-acbe8daf5d94','488cc3fe-0534-42af-86db-042674ef93e4.jpg','image005.jpg','Files\\488cc3fe-0534-42af-86db-042674ef93e4.jpg','image/jpeg',14905,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 17:52:34.701886',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,NULL,NULL,1,1,1,1,NULL,1,NULL),('236455a1-1862-41c9-b062-4843422c0969','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','740b519b-6c5d-4b02-b5fc-9cb55eec602b.jpg','image005.jpg','Files\\Permanent\\740b519b-6c5d-4b02-b5fc-9cb55eec602b.jpg','image/jpeg',14905,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 23:02:43.835598','2026-03-08 23:03:37.727056','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 23:03:37.598522','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'1f0b487c-6e5f-487f-823c-a99dbb412321'),('2bbac946-01ca-496c-b061-e626e60b25dc','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','4408cc5b-2e1f-439b-94af-41517a295d41.jpg','Waylen Bay Skeleton.jpg','Files\\4408cc5b-2e1f-439b-94af-41517a295d41.jpg','image/jpeg',104387,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-08 23:01:52.666146','2026-03-08 23:03:30.124498','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,'2026-03-08 23:03:30.119767','da6639fc-4721-4958-940d-bf3a910b434a',1,1,1,1,NULL,1,NULL),('2cb29b35-7925-48b6-a5b0-b68f218360fb','27c4e2e1-8df3-445a-8455-aea496e62f38','6063e41b-e3a4-4473-8efe-58be7cb0f1aa.jpg','image005.jpg','Files\\Permanent\\6063e41b-e3a4-4473-8efe-58be7cb0f1aa.jpg','image/jpeg',14905,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 09:22:49.159871','2026-03-09 09:23:49.930871','69fceb33-43b5-4c3c-989a-7eddf859ceef','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,'2026-03-09 09:23:49.927370','da6639fc-4721-4958-940d-bf3a910b434a',3,1,1,2,NULL,1,NULL),('37a93f16-fa55-4bd1-83c3-de3862ac74b7','27c4e2e1-8df3-445a-8455-aea496e62f38','adc9acd8-b3fd-48d9-afae-afd9191373f1.jpg','Waylen Bay Skeleton.jpg','Files\\adc9acd8-b3fd-48d9-afae-afd9191373f1.jpg','image/jpeg',104387,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 09:00:56.722939',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,NULL,NULL,1,1,1,1,NULL,1,NULL),('3d33ec5b-45d4-4dd7-b5ca-2eb2c613cc2c','d081ef89-c6dd-45f4-84fa-8ed23979264c','55e40f81-3caa-40b7-b4d6-e4d118cf2615.jpg','Logo 3-220mm wide JB SEW.JPG','Files\\Permanent\\55e40f81-3caa-40b7-b4d6-e4d118cf2615.jpg','image/jpeg',20320,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 10:43:52.666079','2026-03-09 16:17:59.689584','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 16:17:59.664130','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'5a42343f-f566-4c2b-97e3-32cbacce90e3'),('3e4ac66d-1996-4955-81a7-f3eeda2d6e3e','8d8bf938-a4d4-4ffc-9905-f8b564353640','018ebbe5-4ca0-4e89-9d37-d33f882b00e6.jpeg','HMmotor.jpeg','Files\\018ebbe5-4ca0-4e89-9d37-d33f882b00e6.jpeg','image/jpeg',228976,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 22:31:36.169836',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,NULL,NULL,1,1,1,1,NULL,1,NULL),('4c2ec04c-6310-487f-97d1-7dc34b7ef30c','42d4ac72-d397-4886-8fd8-bab880c0bbb6','2603a7cc-9397-4ca2-8324-ab8b3c18eb96.jpeg','HMLogo.jpeg','Files\\Permanent\\2603a7cc-9397-4ca2-8324-ab8b3c18eb96.jpeg','image/jpeg',47444,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 00:30:35.951548','2026-03-09 00:38:13.866121','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 00:38:13.708324','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'ff30982f-9bc6-422c-9187-3658bdea2a93'),('50821d30-f265-4987-9253-139a5c17252b','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','bc871834-fc9a-4560-b7e7-06f610796142.png','Tugun Bowls Logo-03.png','Files\\bc871834-fc9a-4560-b7e7-06f610796142.png','image/png',105084,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 00:39:17.668918','2026-03-09 00:44:48.512837','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,'2026-03-09 00:44:48.511267','da6639fc-4721-4958-940d-bf3a910b434a',1,1,1,1,NULL,1,NULL),('52351804-8a3a-46fb-a512-3eed486c4a51','d0810784-0ea9-4b42-80e6-5bf982540e04','8b3d5f2e-8a20-46c8-aa1c-e2116596cdb6.jpeg','image3.jpeg','Files\\Permanent\\8b3d5f2e-8a20-46c8-aa1c-e2116596cdb6.jpeg','image/jpeg',133847,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 16:08:35.662669','2026-03-09 16:09:13.391099','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 16:09:13.274727','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'02fdd1c7-75f6-47cd-b5a2-7ef6a6d0e6ff'),('5b7977eb-0313-4c36-87b5-fad8a4bab42e','cf2dc5e6-2797-441d-ba05-7d465abdf900','3fbef068-08e0-4c8d-ac31-c9c24f6db74e.jpg','Waylen Bay Yacht.jpg','Files\\Permanent\\3fbef068-08e0-4c8d-ac31-c9c24f6db74e.jpg','image/jpeg',122896,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 22:01:03.617730','2026-03-08 22:02:48.341218','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 22:02:48.232451','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'493888ec-f2b2-4fe0-9465-61a0627a3fcf'),('65782c49-f97e-4e57-bb3d-64cf5eaa193c','d081ef89-c6dd-45f4-84fa-8ed23979264c','cdfc74a7-3349-4c0a-808b-72474bd04fdc.png','WORDPRESS BUSINESS WEBSITE.png','Files\\cdfc74a7-3349-4c0a-808b-72474bd04fdc.png','image/png',495190,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 10:43:08.292681','2026-03-09 16:17:36.737005','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,'2026-03-09 16:17:36.736166','da6639fc-4721-4958-940d-bf3a910b434a',1,1,1,1,NULL,1,NULL),('68129cfe-aebb-4e64-b9f5-ce092df1affe','3c5b0459-7bcc-4818-ac24-ac9b6af11383','e12119c7-a6f4-4d32-b375-fd77314f9b03.jpg','Wayfarer Caravan Services Logo LC SEW.JPG','Files\\Permanent\\e12119c7-a6f4-4d32-b375-fd77314f9b03.jpg','image/jpeg',27271,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 22:06:56.820896','2026-03-08 22:08:28.573466','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 22:08:28.546879','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'080f8990-4271-41a9-b295-67e54bb8ce09'),('70109492-0eb9-4f17-b120-3248eabb7e18','d0810784-0ea9-4b42-80e6-5bf982540e04','3c9ed2c2-bf8f-45c8-9935-3a623291aabc.jpg','MAEng-Logo.jpg','Files\\3c9ed2c2-bf8f-45c8-9935-3a623291aabc.jpg','image/jpeg',776287,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 15:57:24.673429','2026-03-09 16:09:00.695187','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,'2026-03-09 16:09:00.690827','da6639fc-4721-4958-940d-bf3a910b434a',1,1,1,1,NULL,1,NULL),('7c9c3643-47af-45bb-b5f7-0c578a49510b','3c5b0459-7bcc-4818-ac24-ac9b6af11383','4246f543-f464-409b-b271-f3decae3cd23.jpg','CSC Logo - Black BG - Square.jpg','Files\\Permanent\\4246f543-f464-409b-b271-f3decae3cd23.jpg','image/jpeg',255637,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 22:06:56.820897','2026-03-08 22:08:28.573466','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 22:08:28.553371','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'080f8990-4271-41a9-b295-67e54bb8ce09'),('8b74c7bb-21fd-4bf4-8f36-10fda610cfce','d0810784-0ea9-4b42-80e6-5bf982540e04','0f83638d-9d43-4241-810a-2f81948a4cb4.jpeg','image4.jpeg','Files\\Permanent\\0f83638d-9d43-4241-810a-2f81948a4cb4.jpeg','image/jpeg',393342,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 16:08:35.662667','2026-03-09 16:09:13.391099','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 16:09:13.278569','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'02fdd1c7-75f6-47cd-b5a2-7ef6a6d0e6ff'),('927e0987-371d-4e78-8623-fa4ad518d5db','12044d25-2671-47f6-bea2-1934ca925308','20b38486-08f7-4cdb-9f20-5775c817b578.jpeg','image4.jpeg','Files\\Permanent\\20b38486-08f7-4cdb-9f20-5775c817b578.jpeg','image/jpeg',393342,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-10 01:00:41.376587','2026-03-10 01:02:05.520343','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-10 01:02:05.433574','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'e3d5ebc4-f2f9-4d8b-b891-e8ccbd0ddcfb'),('936fe813-575d-4492-965e-b82a069f53ee','cf2dc5e6-2797-441d-ba05-7d465abdf900','85fb17ab-6fe5-4a73-ab7a-2e60864dd18d.eps','Cranbourne Basketball Academy - Logo WHITE.eps','Files\\Permanent\\85fb17ab-6fe5-4a73-ab7a-2e60864dd18d.eps','application/postscript',1730174,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 22:01:03.617744','2026-03-08 22:02:48.341222','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 22:02:48.235976','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'493888ec-f2b2-4fe0-9465-61a0627a3fcf'),('94996e62-98f0-4285-bb36-0320dbd472f4','42d4ac72-d397-4886-8fd8-bab880c0bbb6','68f3c1a2-1362-453e-8487-49d4f7f1b7d6.jpeg','HMmotor.jpeg','Files\\Permanent\\68f3c1a2-1362-453e-8487-49d4f7f1b7d6.jpeg','image/jpeg',228976,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 00:30:35.951547','2026-03-09 00:38:13.866123','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 00:38:13.712112','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'ff30982f-9bc6-422c-9187-3658bdea2a93'),('94c9a4b8-eadf-4b8d-a1b5-1eced4e13d03','12044d25-2671-47f6-bea2-1934ca925308','59f1e8cf-dbc3-4595-b700-2caf6639f95a.jpeg','image3.jpeg','Files\\Permanent\\59f1e8cf-dbc3-4595-b700-2caf6639f95a.jpeg','image/jpeg',133847,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-10 01:00:41.376590','2026-03-10 01:02:05.520343','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-10 01:02:05.448784','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'e3d5ebc4-f2f9-4d8b-b891-e8ccbd0ddcfb'),('a5368fe6-366e-43e0-9f4a-e8a7658f60ad','d081ef89-c6dd-45f4-84fa-8ed23979264c','4271e657-1f64-482d-8181-86244558dc36.jpg','Logo 1 – 100mm wide SEW.JPG','Files\\Permanent\\4271e657-1f64-482d-8181-86244558dc36.jpg','image/jpeg',31153,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 10:43:52.666089','2026-03-09 16:17:59.689584','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 16:17:59.666690','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'5a42343f-f566-4c2b-97e3-32cbacce90e3'),('a8f54d4f-468a-453f-b36d-378daee9eef0','cf2dc5e6-2797-441d-ba05-7d465abdf900','9543d250-89af-42e6-ae67-9f64830f846b.jpg','Lexus.jpg','Files\\9543d250-89af-42e6-ae67-9f64830f846b.jpg','image/jpeg',74009,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-08 21:57:18.962500','2026-03-08 22:02:30.983006','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,'2026-03-08 22:02:30.975280','da6639fc-4721-4958-940d-bf3a910b434a',1,1,1,1,NULL,1,NULL),('b5158352-b5c0-4798-a48e-5d01b11e0b94','42d4ac72-d397-4886-8fd8-bab880c0bbb6','c82e9572-e209-472f-8b78-6cc299ecf8e5.jpg','image005.jpg','Files\\c82e9572-e209-472f-8b78-6cc299ecf8e5.jpg','image/jpeg',14905,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 00:29:59.602435','2026-03-09 00:38:07.060732','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,'2026-03-09 00:38:06.823504','da6639fc-4721-4958-940d-bf3a910b434a',1,1,1,1,NULL,1,NULL),('c51927f3-4332-4782-905e-a22146e3f0cb','d081ef89-c6dd-45f4-84fa-8ed23979264c','b8e76cd0-ed71-423d-ba58-43bcb398a281.jpg','Logo 2 – 220mm wide JB SEW.JPG','Files\\Permanent\\b8e76cd0-ed71-423d-ba58-43bcb398a281.jpg','image/jpeg',25374,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 10:43:52.666090','2026-03-09 16:17:59.689584','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 16:17:59.671699','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'5a42343f-f566-4c2b-97e3-32cbacce90e3'),('d0def505-8103-413e-b8b4-06b5f0b9f663','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','12c5092b-feae-458e-b96a-a7055ebb0302.png','Z2600227 wedding curated life.PNG','Files\\Permanent\\12c5092b-feae-458e-b96a-a7055ebb0302.png','image/png',48387,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 23:02:43.835596','2026-03-08 23:03:37.727057','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 23:03:37.602164','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'1f0b487c-6e5f-487f-823c-a99dbb412321'),('d1c8c656-a853-44bf-a821-9f3901998ab5','cf2dc5e6-2797-441d-ba05-7d465abdf900','c783291d-75c6-4554-9555-2129a7dfd926.jpg','Waylen Bay Skeleton.jpg','Files\\Permanent\\c783291d-75c6-4554-9555-2129a7dfd926.jpg','image/jpeg',104387,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 22:01:03.617742','2026-03-08 22:02:48.341222','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 22:02:48.238991','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'493888ec-f2b2-4fe0-9465-61a0627a3fcf'),('d2d2a3e9-abef-4850-9ac8-f4789397bb88','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','9adbafcf-c436-4dbd-861b-d17775f1d101.png','Tugun Bowls Logo-03.png','Files\\Permanent\\9adbafcf-c436-4dbd-861b-d17775f1d101.png','image/png',105084,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 00:43:39.884524','2026-03-09 00:44:48.512838','69fceb33-43b5-4c3c-989a-7eddf859ceef','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,'2026-03-09 00:44:48.511268','da6639fc-4721-4958-940d-bf3a910b434a',3,1,1,2,NULL,1,NULL),('e55338f2-e07f-4aae-98df-4e9ab966874d','cf2dc5e6-2797-441d-ba05-7d465abdf900','1c5296fb-b5de-47aa-a1d7-651b7ce0a574.png','image001 (3).png','Files\\Permanent\\1c5296fb-b5de-47aa-a1d7-651b7ce0a574.png','image/png',249397,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-08 22:01:03.617746','2026-03-08 22:02:48.341222','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-08 22:02:48.241268','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'493888ec-f2b2-4fe0-9465-61a0627a3fcf'),('eb73a149-b7a8-4db0-b939-f25d7382d861','3c5b0459-7bcc-4818-ac24-ac9b6af11383','008f0119-31f7-421c-812e-6260f19b4709.jpg','image005.jpg','Files\\008f0119-31f7-421c-812e-6260f19b4709.jpg','image/jpeg',14905,0,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-08 22:06:26.589325','2026-03-08 22:08:16.015802','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,'2026-03-08 22:08:16.015065','da6639fc-4721-4958-940d-bf3a910b434a',1,1,1,1,NULL,1,NULL),('eb8e7de1-01f4-433e-b5d4-ba7d7582d6ce','42d4ac72-d397-4886-8fd8-bab880c0bbb6','827d47b5-3b84-474a-afe9-865b13d3585e.png','WORDPRESS BUSINESS WEBSITE.png','Files\\Permanent\\827d47b5-3b84-474a-afe9-865b13d3585e.png','image/png',495190,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-09 00:30:35.951547','2026-03-09 00:38:13.866124','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-09 00:38:13.716612','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'ff30982f-9bc6-422c-9187-3658bdea2a93'),('f0f47258-1532-45da-9c1a-773dfd372b1b','12044d25-2671-47f6-bea2-1934ca925308','36216158-01fb-4f0b-b3aa-84c455e8a15c.jpg','USS Logo - Horizontal.jpg','Files\\Permanent\\36216158-01fb-4f0b-b3aa-84c455e8a15c.jpg','image/jpeg',276941,1,NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef','2026-03-10 01:00:41.376591','2026-03-10 01:02:05.520343','69fceb33-43b5-4c3c-989a-7eddf859ceef','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,'2026-03-10 01:02:05.452529','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',3,1,1,1,NULL,1,'e3d5ebc4-f2f9-4d8b-b891-e8ccbd0ddcfb');
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
  `PriceUpdatedAt` datetime(6) DEFAULT NULL,
  `PriceUpdatedByRole` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PriceUpdatedByUserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
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
-- Dumping data for table `logoorders`
--

LOCK TABLES `logoorders` WRITE;
/*!40000 ALTER TABLE `logoorders` DISABLE KEYS */;
INSERT INTO `logoorders` VALUES ('12044d25-2671-47f6-bea2-1934ca925308','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','489a32db-6b7b-4737-9433-c7ef19627799','fwsfws','fgwsgwe',7,3.00,NULL,NULL,NULL,'SATIN','2026-03-10 00:55:49.830558','2026-03-10 01:02:11.908981','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,NULL,1,350.00,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,0,0,0,NULL,0,NULL,NULL,0,350.00,1,1,NULL,0,2,350.00,NULL,0,3.00,'USD',3.00,3.00,350.00,350.00,NULL,NULL,NULL),('27c4e2e1-8df3-445a-8455-aea496e62f38','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','489a32db-6b7b-4737-9433-c7ef19627799','rfgdfg','dfhhdfcbndcf fhn',6,54.00,NULL,NULL,NULL,NULL,'2026-03-09 09:00:56.722931','2026-03-09 09:23:56.500290','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',0,NULL,NULL,NULL,1,350.00,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,0,0,0,2,0,NULL,NULL,0,350.00,1,1,NULL,0,5,350.00,NULL,0,NULL,'USD',54.00,54.00,350.00,350.00,NULL,NULL,NULL),('3c5b0459-7bcc-4818-ac24-ac9b6af11383','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','489a32db-6b7b-4737-9433-c7ef19627799','test 2','dasdfasdf svef weasf',7,2.00,NULL,NULL,NULL,NULL,'2026-03-08 22:06:26.589321','2026-03-08 22:09:18.739264','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,NULL,0,NULL,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,0,0,0,2,1,'2026-03-08 22:09:18.739264','ecd033ee-6914-418e-ae72-fa76c0b9e11e',1,NULL,NULL,NULL,NULL,0,0,NULL,NULL,0,NULL,'USD',2.00,2.00,NULL,NULL,NULL,NULL,NULL),('42d4ac72-d397-4886-8fd8-bab880c0bbb6','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','489a32db-6b7b-4737-9433-c7ef19627799','Tetsing','fsdwasdv  a efcqa',7,2.00,NULL,NULL,NULL,NULL,'2026-03-09 00:29:59.602430','2026-03-09 00:38:27.294931','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,NULL,0,NULL,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,0,0,0,2,1,'2026-03-09 00:38:26.718149','63789d63-8100-4faa-bef5-eeb3db8b7c25',1,NULL,NULL,NULL,NULL,0,0,NULL,NULL,0,NULL,'USD',2.00,2.00,NULL,NULL,NULL,NULL,NULL),('4e6f84a6-f27f-4c15-a5ee-acbe8daf5d94','566b3bb9-ab8b-40f3-b415-d79dfc2f48db',NULL,'fdgxd','fgsdfgsdg',1,350.00,NULL,NULL,NULL,NULL,'2026-03-09 17:52:34.701882',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,NULL,1,NULL,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,1,0,0,NULL,0,NULL,NULL,0,350.00,1,1,NULL,0,0,350.00,NULL,0,NULL,'USD',350.00,350.00,350.00,NULL,NULL,NULL,NULL),('8d8bf938-a4d4-4ffc-9905-f8b564353640','566b3bb9-ab8b-40f3-b415-d79dfc2f48db',NULL,'sdfgsdg','csfc',1,350.00,NULL,NULL,NULL,NULL,'2026-03-09 22:31:36.169833',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,NULL,0,NULL,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,1,0,0,NULL,0,NULL,NULL,0,NULL,1,1,NULL,0,0,NULL,NULL,0,350.00,'USD',350.00,350.00,NULL,NULL,NULL,NULL,NULL),('cf2dc5e6-2797-441d-ba05-7d465abdf900','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','489a32db-6b7b-4737-9433-c7ef19627799','Test 1','testing for the first time',7,3.00,NULL,NULL,NULL,NULL,'2026-03-08 21:57:18.962495','2026-03-08 22:03:19.653831','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,NULL,0,NULL,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,0,0,0,2,1,'2026-03-08 22:03:19.653831','610483e2-8920-4c0a-94b9-5b117f42b549',1,NULL,NULL,NULL,NULL,0,0,NULL,NULL,0,NULL,'USD',3.00,3.00,NULL,NULL,NULL,NULL,NULL),('d0810784-0ea9-4b42-80e6-5bf982540e04','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','489a32db-6b7b-4737-9433-c7ef19627799','Testing 1','Testing 23',7,2.00,NULL,NULL,NULL,NULL,'2026-03-09 15:57:24.673421','2026-03-09 16:10:42.345308','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,NULL,1,350.00,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,0,0,0,2,1,'2026-03-09 16:09:45.370282','e2a24275-9323-4ce9-8d13-6d124ef8b9cf',1,350.00,1,1,'f583f816-78c7-4fe7-9fe5-5c4be95fdaf4',1,5,350.00,NULL,0,NULL,'USD',2.00,2.00,350.00,350.00,NULL,NULL,NULL),('d081ef89-c6dd-45f4-84fa-8ed23979264c','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','489a32db-6b7b-4737-9433-c7ef19627799','fwertfws','etfgwegtgwsgv',7,0.01,NULL,NULL,NULL,NULL,'2026-03-09 10:43:08.292677','2026-03-09 16:18:19.039657','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,NULL,0,NULL,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,0,0,0,2,1,'2026-03-09 16:18:18.810054','18af4834-537f-453d-864f-6c05326a1a31',1,NULL,NULL,NULL,NULL,0,0,NULL,NULL,0,NULL,'USD',0.01,0.01,NULL,NULL,NULL,NULL,NULL),('fb116de6-7f82-4fcb-aff5-d8a95473bb1c','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','489a32db-6b7b-4737-9433-c7ef19627799','Tets invoice','Tets invoice',7,3.00,NULL,NULL,NULL,NULL,'2026-03-08 23:01:52.666143','2026-03-08 23:03:56.431689','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,NULL,0,NULL,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,0,0,0,2,1,'2026-03-08 23:03:55.973524','596d6a30-86ab-4eda-ae61-c91040c799e6',1,NULL,NULL,NULL,NULL,0,0,NULL,NULL,0,NULL,'USD',3.00,3.00,NULL,NULL,NULL,NULL,NULL),('fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','566b3bb9-ab8b-40f3-b415-d79dfc2f48db','489a32db-6b7b-4737-9433-c7ef19627799','uhoihuoh','iohuohuio [[uji',7,7.00,NULL,NULL,NULL,NULL,'2026-03-09 00:39:17.668916','2026-03-09 08:10:39.503272','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','da6639fc-4721-4958-940d-bf3a910b434a',0,NULL,NULL,NULL,1,350.00,NULL,0,2,NULL,NULL,NULL,NULL,NULL,0,0,0,NULL,NULL,NULL,NULL,0,0,0,2,1,'2026-03-09 00:45:09.994735','9a202643-b1b7-4030-a9a3-9ab924f65cd9',1,350.00,1,1,'5d16ef00-7319-47e7-a265-f7f046ba2b31',1,2,350.00,NULL,0,NULL,'USD',7.00,7.00,350.00,350.00,NULL,NULL,NULL);
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
-- Dumping data for table `notifications`
--

LOCK TABLES `notifications` WRITE;
/*!40000 ALTER TABLE `notifications` DISABLE KEYS */;
INSERT INTO `notifications` VALUES ('02498f24-ec46-4273-a78d-3e0b95776124','69fceb33-43b5-4c3c-989a-7eddf859ceef','d0810784-0ea9-4b42-80e6-5bf982540e04','Order Assigned','You have been assigned order (#ORD-D0810784)',5,1,'2026-03-09 16:08:06.121071','2026-03-09 15:57:59.400837','2026-03-09 16:08:06.121554','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'/orders/d0810784-0ea9-4b42-80e6-5bf982540e04'),('029e53f0-41ab-488e-a106-54c27e249ac9','27250b41-8cb5-40f1-bef6-ef4e114062a3','d0810784-0ea9-4b42-80e6-5bf982540e04','New Order Submitted','Kevin sharma placed a new order (#ORD-D0810784)',5,0,NULL,'2026-03-09 15:57:24.973966',NULL,NULL,NULL,0,NULL,NULL,'d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'/orders/d0810784-0ea9-4b42-80e6-5bf982540e04'),('02a04e30-2304-47fc-9e9f-40618ff0e525','da6639fc-4721-4958-940d-bf3a910b434a','d081ef89-c6dd-45f4-84fa-8ed23979264c','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-D081EF89). Please mark as completed.',5,1,'2026-03-09 16:18:13.671261','2026-03-09 16:17:59.887417','2026-03-09 16:18:13.671403','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'/orders/d081ef89-c6dd-45f4-84fa-8ed23979264c'),('033393b5-a5af-41bd-a381-aa9330b8ae1b','27250b41-8cb5-40f1-bef6-ef4e114062a3','d081ef89-c6dd-45f4-84fa-8ed23979264c','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-D081EF89). Please mark as completed.',5,0,NULL,'2026-03-09 16:17:59.822695',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'/orders/d081ef89-c6dd-45f4-84fa-8ed23979264c'),('062b4eb6-8f98-48ea-a963-1f89b60aba16','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','Order Completed','Your order (#ORD-FB116DE6) has been completed',5,1,'2026-03-09 09:06:55.475022','2026-03-08 23:03:56.000360','2026-03-09 09:06:55.506509',NULL,NULL,0,NULL,NULL,'fb116de6-7f82-4fcb-aff5-d8a95473bb1c',1,1,NULL,'/orders/fb116de6-7f82-4fcb-aff5-d8a95473bb1c'),('09d1037d-73fa-4145-9b7b-5f292cc97d12','da6639fc-4721-4958-940d-bf3a910b434a','4e6f84a6-f27f-4c15-a5ee-acbe8daf5d94','New Order Submitted','Kevin sharma placed a new order (#ORD-4E6F84A6)',5,1,'2026-03-09 17:52:49.227625','2026-03-09 17:52:35.050337','2026-03-09 17:52:49.228612',NULL,NULL,0,NULL,NULL,'4e6f84a6-f27f-4c15-a5ee-acbe8daf5d94',1,1,NULL,'/orders/4e6f84a6-f27f-4c15-a5ee-acbe8daf5d94'),('10e18a67-0075-4a3b-9093-2f79a58585cd','da6639fc-4721-4958-940d-bf3a910b434a','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','New Order Submitted','2 Kevins sharma approved the logo for order (#ORD-FB116DE6). Please mark as completed.',5,1,'2026-03-08 23:12:18.489391','2026-03-08 23:01:53.337957','2026-03-08 23:12:18.489583',NULL,NULL,0,NULL,NULL,'fb116de6-7f82-4fcb-aff5-d8a95473bb1c',1,2,'2026-03-08 23:03:37.884176','/orders/fb116de6-7f82-4fcb-aff5-d8a95473bb1c'),('139ca171-2b90-4fa3-9032-e3d05339471e','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','d081ef89-c6dd-45f4-84fa-8ed23979264c','Order Completed','Your order (#ORD-D081EF89) has been completed',5,1,'2026-03-10 01:02:33.569066','2026-03-09 16:18:18.847630','2026-03-10 01:02:33.569832',NULL,NULL,0,NULL,NULL,'d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'/orders/d081ef89-c6dd-45f4-84fa-8ed23979264c'),('172b50b9-b47e-4ba5-a366-230005a14df4','da6639fc-4721-4958-940d-bf3a910b434a','d081ef89-c6dd-45f4-84fa-8ed23979264c','Preview Ready for QA','Designer uploaded preview files for order (#ORD-D081EF89)',8,1,'2026-03-09 16:10:02.106638','2026-03-09 10:43:52.931674','2026-03-09 16:10:02.107000','69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'/orders/d081ef89-c6dd-45f4-84fa-8ed23979264c'),('19ea94c2-c107-4a5b-b1f1-55a800d0cdcd','da6639fc-4721-4958-940d-bf3a910b434a','cf2dc5e6-2797-441d-ba05-7d465abdf900','New Order Submitted','Kevin sharma placed a new order (#ORD-CF2DC5E6)',5,1,'2026-03-08 21:59:09.436065','2026-03-08 21:57:19.628280','2026-03-08 21:59:09.440388',NULL,NULL,0,NULL,NULL,'cf2dc5e6-2797-441d-ba05-7d465abdf900',1,1,NULL,'/orders/cf2dc5e6-2797-441d-ba05-7d465abdf900'),('1ca49243-c75d-477c-a345-f5b32542cc9a','27250b41-8cb5-40f1-bef6-ef4e114062a3','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','New Order Submitted','Kevin sharma placed a new order (#ORD-FD781C82)',5,0,NULL,'2026-03-09 00:39:17.868869',NULL,NULL,NULL,0,NULL,NULL,'fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,1,NULL,'/orders/fd781c82-c35c-46e0-9a8a-ba91ab7df3cb'),('21af0321-411b-411a-ab3e-46aaee51089c','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','42d4ac72-d397-4886-8fd8-bab880c0bbb6','Invoice Generated','Invoice (#INV-20260309-533E29D5) generated for order (#ORD-42D4AC72)',1,1,'2026-03-09 09:06:55.495275','2026-03-09 00:38:27.366374','2026-03-09 09:06:55.507434','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'63789d63-8100-4faa-bef5-eeb3db8b7c25',2,1,NULL,'/invoices/63789d63-8100-4faa-bef5-eeb3db8b7c25'),('278b4e3a-1ae4-43ce-a2eb-60e18a6519dd','da6639fc-4721-4958-940d-bf3a910b434a','42d4ac72-d397-4886-8fd8-bab880c0bbb6','New Order Submitted','Kevin sharma placed a new order (#ORD-42D4AC72)',5,1,'2026-03-09 08:07:36.477944','2026-03-09 00:30:00.142959','2026-03-09 08:07:36.515156',NULL,NULL,0,NULL,NULL,'42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'/orders/42d4ac72-d397-4886-8fd8-bab880c0bbb6'),('2a449c63-8b47-439c-979d-c57a1d7d34cd','da6639fc-4721-4958-940d-bf3a910b434a','42d4ac72-d397-4886-8fd8-bab880c0bbb6','Preview Ready for QA','Designer uploaded preview files for order (#ORD-42D4AC72)',8,1,'2026-03-09 08:07:36.478126','2026-03-09 00:30:36.095601','2026-03-09 08:07:36.515157','69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'/orders/42d4ac72-d397-4886-8fd8-bab880c0bbb6'),('3219b4cc-cf53-4616-816d-e80ada7ccd4f','69fceb33-43b5-4c3c-989a-7eddf859ceef','27c4e2e1-8df3-445a-8455-aea496e62f38','Order Assigned','You have been assigned order (#ORD-27C4E2E1)',5,1,'2026-03-09 15:55:44.555453','2026-03-09 09:22:14.481689','2026-03-09 15:55:44.596458','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'27c4e2e1-8df3-445a-8455-aea496e62f38',1,1,NULL,'/orders/27c4e2e1-8df3-445a-8455-aea496e62f38'),('33ed8158-8ee4-4537-b77c-825f2aead346','69fceb33-43b5-4c3c-989a-7eddf859ceef','3c5b0459-7bcc-4818-ac24-ac9b6af11383','Order Assigned','2 orders (#ORD-3C5B0459) has been completed',5,1,'2026-03-08 22:08:35.775865','2026-03-08 22:06:41.518997','2026-03-08 22:09:19.380080','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'3c5b0459-7bcc-4818-ac24-ac9b6af11383',1,2,'2026-03-08 22:09:19.256782','/orders/3c5b0459-7bcc-4818-ac24-ac9b6af11383'),('3dee45fb-004d-4dad-8cdc-ad4b45a43ba3','da6639fc-4721-4958-940d-bf3a910b434a','12044d25-2671-47f6-bea2-1934ca925308','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-12044D25). Please mark as completed.',5,1,'2026-03-10 22:42:10.352032','2026-03-10 01:02:05.811934','2026-03-10 22:42:10.467506','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'12044d25-2671-47f6-bea2-1934ca925308',1,1,NULL,'/orders/12044d25-2671-47f6-bea2-1934ca925308'),('3f8e2257-af98-472f-9147-8cd272fdbcbc','27250b41-8cb5-40f1-bef6-ef4e114062a3','12044d25-2671-47f6-bea2-1934ca925308','Preview Ready for QA','Designer uploaded preview files for order (#ORD-12044D25)',8,0,NULL,'2026-03-10 01:00:41.621424',NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'12044d25-2671-47f6-bea2-1934ca925308',1,1,NULL,'/orders/12044d25-2671-47f6-bea2-1934ca925308'),('4305f0dc-232b-4464-a8ca-04617d0c0089','27250b41-8cb5-40f1-bef6-ef4e114062a3','27c4e2e1-8df3-445a-8455-aea496e62f38','New Order Submitted','Kevin sharma placed a new order (#ORD-27C4E2E1)',5,0,NULL,'2026-03-09 09:00:57.529326',NULL,NULL,NULL,0,NULL,NULL,'27c4e2e1-8df3-445a-8455-aea496e62f38',1,1,NULL,'/orders/27c4e2e1-8df3-445a-8455-aea496e62f38'),('44fe1c38-922c-4ef4-ae84-10c9a4cee682','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','d081ef89-c6dd-45f4-84fa-8ed23979264c','Preview Files Available','Preview files were uploaded for order (#ORD-D081EF89)',8,1,'2026-03-09 16:17:57.301090','2026-03-09 16:17:36.782756','2026-03-09 16:17:57.301151',NULL,NULL,0,NULL,NULL,'d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'/orders/d081ef89-c6dd-45f4-84fa-8ed23979264c'),('45f81777-5e06-44c9-bd6c-9a928b70e15f','da6639fc-4721-4958-940d-bf3a910b434a','d081ef89-c6dd-45f4-84fa-8ed23979264c','New Order Submitted','Kevin sharma placed a new order (#ORD-D081EF89)',5,1,'2026-03-09 16:10:02.106638','2026-03-09 10:43:09.115207','2026-03-09 16:10:02.107001',NULL,NULL,0,NULL,NULL,'d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'/orders/d081ef89-c6dd-45f4-84fa-8ed23979264c'),('47866532-c2ed-41c0-a1c3-90ce771227cf','da6639fc-4721-4958-940d-bf3a910b434a','27c4e2e1-8df3-445a-8455-aea496e62f38','Final Files Uploaded','Designer uploaded final files for order (#ORD-27C4E2E1). Review and approve designer price if needed.',8,1,'2026-03-09 09:23:08.304248','2026-03-09 09:22:49.338177','2026-03-09 09:23:08.305337','69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'27c4e2e1-8df3-445a-8455-aea496e62f38',1,1,NULL,'/orders/27c4e2e1-8df3-445a-8455-aea496e62f38'),('4986390b-8d89-4256-a88f-153940531a68','27250b41-8cb5-40f1-bef6-ef4e114062a3','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','New Order Submitted','2 Kevins sharma approved the logo for order (#ORD-FB116DE6). Please mark as completed.',5,0,NULL,'2026-03-08 23:01:53.148173','2026-03-08 23:03:37.824854',NULL,NULL,0,NULL,NULL,'fb116de6-7f82-4fcb-aff5-d8a95473bb1c',1,2,'2026-03-08 23:03:37.803905','/orders/fb116de6-7f82-4fcb-aff5-d8a95473bb1c'),('49c31cd4-aa06-496e-81a8-a1a3dc5627d2','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','3c5b0459-7bcc-4818-ac24-ac9b6af11383','Order Completed','Your order (#ORD-3C5B0459) has been completed',5,1,'2026-03-09 09:06:55.495278','2026-03-08 22:09:19.052162','2026-03-09 09:06:55.507436',NULL,NULL,0,NULL,NULL,'3c5b0459-7bcc-4818-ac24-ac9b6af11383',1,1,NULL,'/orders/3c5b0459-7bcc-4818-ac24-ac9b6af11383'),('4bc6151b-6efb-4fb3-bc0c-cd50ef07457d','27250b41-8cb5-40f1-bef6-ef4e114062a3','12044d25-2671-47f6-bea2-1934ca925308','New Order Submitted','Kevin sharma placed a new order (#ORD-12044D25)',5,0,NULL,'2026-03-10 00:55:50.040053',NULL,NULL,NULL,0,NULL,NULL,'12044d25-2671-47f6-bea2-1934ca925308',1,1,NULL,'/orders/12044d25-2671-47f6-bea2-1934ca925308'),('4fdbcdca-3d50-406a-aeb3-80ecf10e4feb','27250b41-8cb5-40f1-bef6-ef4e114062a3','27c4e2e1-8df3-445a-8455-aea496e62f38','Final Files Uploaded','Designer uploaded final files for order (#ORD-27C4E2E1). Review and approve designer price if needed.',8,0,NULL,'2026-03-09 09:22:49.253431',NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'27c4e2e1-8df3-445a-8455-aea496e62f38',1,1,NULL,'/orders/27c4e2e1-8df3-445a-8455-aea496e62f38'),('505972e9-e192-45c8-a6b3-ec4d941b9d57','27250b41-8cb5-40f1-bef6-ef4e114062a3','8d8bf938-a4d4-4ffc-9905-f8b564353640','New Order Submitted','Kevin sharma placed a new order (#ORD-8D8BF938)',5,0,NULL,'2026-03-09 22:31:36.430528',NULL,NULL,NULL,0,NULL,NULL,'8d8bf938-a4d4-4ffc-9905-f8b564353640',1,1,NULL,'/orders/8d8bf938-a4d4-4ffc-9905-f8b564353640'),('51638181-12fd-4a20-ac76-33203caa7da9','27250b41-8cb5-40f1-bef6-ef4e114062a3','27c4e2e1-8df3-445a-8455-aea496e62f38','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-27C4E2E1). Please mark as completed.',5,0,NULL,'2026-03-09 09:23:56.598649',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'27c4e2e1-8df3-445a-8455-aea496e62f38',1,1,NULL,'/orders/27c4e2e1-8df3-445a-8455-aea496e62f38'),('519714f5-17c5-4d32-9459-0c89492584cb','da6639fc-4721-4958-940d-bf3a910b434a','12044d25-2671-47f6-bea2-1934ca925308','New Order Submitted','Kevin sharma placed a new order (#ORD-12044D25)',5,1,'2026-03-10 00:56:03.690505','2026-03-10 00:55:50.155807','2026-03-10 00:56:03.691705',NULL,NULL,0,NULL,NULL,'12044d25-2671-47f6-bea2-1934ca925308',1,1,NULL,'/orders/12044d25-2671-47f6-bea2-1934ca925308'),('52b9ae8f-effd-4c69-9fde-850757cab0da','69fceb33-43b5-4c3c-989a-7eddf859ceef','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','Order Assigned','You have been assigned order (#ORD-FD781C82)',5,1,'2026-03-09 00:45:24.321955','2026-03-09 00:39:37.269532','2026-03-09 00:45:24.322165','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,1,NULL,'/orders/fd781c82-c35c-46e0-9a8a-ba91ab7df3cb'),('52e6864f-0567-40ec-be90-b628e22c640d','69fceb33-43b5-4c3c-989a-7eddf859ceef','d081ef89-c6dd-45f4-84fa-8ed23979264c','Order Assigned','You have been assigned order (#ORD-D081EF89)',5,1,'2026-03-09 15:55:44.555570','2026-03-09 10:43:23.445710','2026-03-09 15:55:44.596459','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'/orders/d081ef89-c6dd-45f4-84fa-8ed23979264c'),('532949a3-276a-4963-8250-9fdbd382882a','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','42d4ac72-d397-4886-8fd8-bab880c0bbb6','Preview Files Available','Preview files were uploaded for order (#ORD-42D4AC72)',8,1,'2026-03-09 09:06:55.495278','2026-03-09 00:38:07.366397','2026-03-09 09:06:55.507437',NULL,NULL,0,NULL,NULL,'42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'/orders/42d4ac72-d397-4886-8fd8-bab880c0bbb6'),('5649bd4c-26e1-4781-ac1b-eae96b63ecdc','27250b41-8cb5-40f1-bef6-ef4e114062a3','d081ef89-c6dd-45f4-84fa-8ed23979264c','Preview Ready for QA','Designer uploaded preview files for order (#ORD-D081EF89)',8,0,NULL,'2026-03-09 10:43:52.829102',NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'/orders/d081ef89-c6dd-45f4-84fa-8ed23979264c'),('596d24b8-ee05-42a8-b599-0998cdf28c6b','da6639fc-4721-4958-940d-bf3a910b434a','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-FD781C82). Please mark as completed.',5,1,'2026-03-09 08:07:36.478127','2026-03-09 00:44:58.453348','2026-03-09 08:07:36.515158','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,1,NULL,'/orders/fd781c82-c35c-46e0-9a8a-ba91ab7df3cb'),('59f5c960-e770-430d-aa99-54c848328beb','da6639fc-4721-4958-940d-bf3a910b434a','27c4e2e1-8df3-445a-8455-aea496e62f38','New Order Submitted','Kevin sharma placed a new order (#ORD-27C4E2E1)',5,1,'2026-03-09 16:10:02.106638','2026-03-09 09:00:57.628748','2026-03-09 16:10:02.107001',NULL,NULL,0,NULL,NULL,'27c4e2e1-8df3-445a-8455-aea496e62f38',1,1,NULL,'/orders/27c4e2e1-8df3-445a-8455-aea496e62f38'),('5c52330d-7ec4-4ba7-a711-f8c7ea27d97a','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','cf2dc5e6-2797-441d-ba05-7d465abdf900','Order Completed','Your order (#ORD-CF2DC5E6) has been completed',5,1,'2026-03-08 22:05:08.060691','2026-03-08 22:03:19.687055','2026-03-08 22:05:08.060772',NULL,NULL,0,NULL,NULL,'cf2dc5e6-2797-441d-ba05-7d465abdf900',1,1,NULL,'/orders/cf2dc5e6-2797-441d-ba05-7d465abdf900'),('616c1a03-89cc-435f-ac49-56aa571d7214','da6639fc-4721-4958-940d-bf3a910b434a','d0810784-0ea9-4b42-80e6-5bf982540e04','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-D0810784). Please mark as completed.',5,1,'2026-03-09 16:09:35.327312','2026-03-09 16:09:13.583850','2026-03-09 16:09:35.327444','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'/orders/d0810784-0ea9-4b42-80e6-5bf982540e04'),('620a3d10-9287-4669-b66f-21ce5cafb9dd','da6639fc-4721-4958-940d-bf3a910b434a','27c4e2e1-8df3-445a-8455-aea496e62f38','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-27C4E2E1). Please mark as completed.',5,1,'2026-03-09 09:24:08.666230','2026-03-09 09:23:56.680032','2026-03-09 09:24:08.666590','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'27c4e2e1-8df3-445a-8455-aea496e62f38',1,1,NULL,'/orders/27c4e2e1-8df3-445a-8455-aea496e62f38'),('698cbfb8-3869-4096-b1cf-9e90e8590056','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','d081ef89-c6dd-45f4-84fa-8ed23979264c','Invoice Generated','Invoice (#INV-20260309-4EDF9CEC) generated for order (#ORD-D081EF89)',1,1,'2026-03-10 01:02:33.569068','2026-03-09 16:18:19.075469','2026-03-10 01:02:33.569849','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'18af4834-537f-453d-864f-6c05326a1a31',2,1,NULL,'/invoices/18af4834-537f-453d-864f-6c05326a1a31'),('6c5c7f46-75a2-43bf-9ae1-a191815f0fe0','27250b41-8cb5-40f1-bef6-ef4e114062a3','42d4ac72-d397-4886-8fd8-bab880c0bbb6','New Order Submitted','Kevin sharma placed a new order (#ORD-42D4AC72)',5,0,NULL,'2026-03-09 00:29:59.997236',NULL,NULL,NULL,0,NULL,NULL,'42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'/orders/42d4ac72-d397-4886-8fd8-bab880c0bbb6'),('6d42e496-98a4-4063-a41f-008c036e43ee','27250b41-8cb5-40f1-bef6-ef4e114062a3','42d4ac72-d397-4886-8fd8-bab880c0bbb6','Preview Ready for QA','Designer uploaded preview files for order (#ORD-42D4AC72)',8,0,NULL,'2026-03-09 00:30:36.048648',NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'/orders/42d4ac72-d397-4886-8fd8-bab880c0bbb6'),('6ef30131-fdba-4936-a1c5-766d65e4044f','69fceb33-43b5-4c3c-989a-7eddf859ceef','cf2dc5e6-2797-441d-ba05-7d465abdf900','Order Assigned','2 orders (#ORD-CF2DC5E6) has been completed',5,1,'2026-03-08 21:59:40.168746','2026-03-08 21:59:30.597695','2026-03-08 22:03:19.745876','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'cf2dc5e6-2797-441d-ba05-7d465abdf900',1,2,'2026-03-08 22:03:19.734128','/orders/cf2dc5e6-2797-441d-ba05-7d465abdf900'),('6f26f5d8-dba6-4ce0-9603-2ded54c10f8e','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','12044d25-2671-47f6-bea2-1934ca925308','Preview Files Available','Preview files were uploaded for order (#ORD-12044D25)',8,1,'2026-03-10 01:02:33.569069','2026-03-10 01:01:55.914942','2026-03-10 01:02:33.569849',NULL,NULL,0,NULL,NULL,'12044d25-2671-47f6-bea2-1934ca925308',1,1,NULL,'/orders/12044d25-2671-47f6-bea2-1934ca925308'),('76470f88-5ea2-44c2-8718-c9fdf5bfc6fb','27250b41-8cb5-40f1-bef6-ef4e114062a3','d081ef89-c6dd-45f4-84fa-8ed23979264c','New Order Submitted','Kevin sharma placed a new order (#ORD-D081EF89)',5,0,NULL,'2026-03-09 10:43:08.896885',NULL,NULL,NULL,0,NULL,NULL,'d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'/orders/d081ef89-c6dd-45f4-84fa-8ed23979264c'),('778cc862-21bc-442f-af72-83843560ad8a','da6639fc-4721-4958-940d-bf3a910b434a','d0810784-0ea9-4b42-80e6-5bf982540e04','Preview Ready for QA','Designer uploaded preview files for order (#ORD-D0810784)',8,1,'2026-03-09 16:08:48.664040','2026-03-09 16:08:35.834330','2026-03-09 16:08:48.664444','69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'/orders/d0810784-0ea9-4b42-80e6-5bf982540e04'),('787fb729-3fb9-436c-9917-089f2b9272a9','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','d0810784-0ea9-4b42-80e6-5bf982540e04','Order Completed','Your order (#ORD-D0810784) has been completed',5,1,'2026-03-10 01:02:33.569069','2026-03-09 16:09:45.445495','2026-03-10 01:02:33.569849',NULL,NULL,0,NULL,NULL,'d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'/orders/d0810784-0ea9-4b42-80e6-5bf982540e04'),('811bab8c-7481-4b5f-b9bb-c2f05a8358c3','da6639fc-4721-4958-940d-bf3a910b434a','8d8bf938-a4d4-4ffc-9905-f8b564353640','New Order Submitted','Kevin sharma placed a new order (#ORD-8D8BF938)',5,1,'2026-03-09 22:31:59.147743','2026-03-09 22:31:36.513664','2026-03-09 22:31:59.148650',NULL,NULL,0,NULL,NULL,'8d8bf938-a4d4-4ffc-9905-f8b564353640',1,1,NULL,'/orders/8d8bf938-a4d4-4ffc-9905-f8b564353640'),('8252afd5-340c-4da1-b44c-a4c3abbf3a1d','27250b41-8cb5-40f1-bef6-ef4e114062a3','cf2dc5e6-2797-441d-ba05-7d465abdf900','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-CF2DC5E6). Please mark as completed.',5,0,NULL,'2026-03-08 22:02:48.436312',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'cf2dc5e6-2797-441d-ba05-7d465abdf900',1,1,NULL,'/orders/cf2dc5e6-2797-441d-ba05-7d465abdf900'),('8b79d08a-9b78-4f16-9533-c545d1d507d0','da6639fc-4721-4958-940d-bf3a910b434a','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','New Order Submitted','Kevin sharma placed a new order (#ORD-FD781C82)',5,1,'2026-03-09 00:39:30.071918','2026-03-09 00:39:17.952852','2026-03-09 00:39:30.072086',NULL,NULL,0,NULL,NULL,'fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,1,NULL,'/orders/fd781c82-c35c-46e0-9a8a-ba91ab7df3cb'),('8e7b0ffb-eaa6-4fbc-899b-a855dcbc419d','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','Preview Files Available','Preview files were uploaded for order (#ORD-FD781C82)',8,1,'2026-03-09 09:06:55.495278','2026-03-09 00:44:48.577555','2026-03-09 09:06:55.507438',NULL,NULL,0,NULL,NULL,'fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,1,NULL,'/orders/fd781c82-c35c-46e0-9a8a-ba91ab7df3cb'),('8ec9c7fc-d1a2-4f13-867e-9f7a035a1e9b','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','Invoice Generated','Invoice (#INV-20260309-5A4E7B8D) generated for order (#ORD-FD781C82)',1,1,'2026-03-09 09:06:55.495279','2026-03-09 00:45:11.986000','2026-03-09 09:06:55.507439','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'9a202643-b1b7-4030-a9a3-9ab924f65cd9',2,1,NULL,'/invoices/9a202643-b1b7-4030-a9a3-9ab924f65cd9'),('8fe10896-b55f-40ca-a36d-a7aaaf86f408','da6639fc-4721-4958-940d-bf3a910b434a','12044d25-2671-47f6-bea2-1934ca925308','Preview Ready for QA','Designer uploaded preview files for order (#ORD-12044D25)',8,1,'2026-03-10 01:00:59.923076','2026-03-10 01:00:41.707796','2026-03-10 01:00:59.923226','69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'12044d25-2671-47f6-bea2-1934ca925308',1,1,NULL,'/orders/12044d25-2671-47f6-bea2-1934ca925308'),('8fef986c-f15c-41df-991b-bd9c7dd05ff8','27250b41-8cb5-40f1-bef6-ef4e114062a3','d0810784-0ea9-4b42-80e6-5bf982540e04','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-D0810784). Please mark as completed.',5,0,NULL,'2026-03-09 16:09:13.515380',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'/orders/d0810784-0ea9-4b42-80e6-5bf982540e04'),('9040f4f4-630e-446e-85c7-a6f0252af272','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','cf2dc5e6-2797-441d-ba05-7d465abdf900','Invoice Generated','Invoice (#INV-20260308-D076D28D) generated for order (#ORD-CF2DC5E6)',1,1,'2026-03-08 22:03:30.740907','2026-03-08 22:03:20.149293','2026-03-08 22:03:30.741042','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'610483e2-8920-4c0a-94b9-5b117f42b549',2,1,NULL,'/invoices/610483e2-8920-4c0a-94b9-5b117f42b549'),('949921c2-4b0e-4775-9932-cc9194db5131','69fceb33-43b5-4c3c-989a-7eddf859ceef','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','Order Assigned','2 orders (#ORD-FB116DE6) has been completed',5,1,'2026-03-08 23:02:25.288594','2026-03-08 23:02:00.468342','2026-03-08 23:03:56.061136','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'fb116de6-7f82-4fcb-aff5-d8a95473bb1c',1,2,'2026-03-08 23:03:56.050614','/orders/fb116de6-7f82-4fcb-aff5-d8a95473bb1c'),('9e8c5587-1f33-4291-a143-37cb133567e0','da6639fc-4721-4958-940d-bf3a910b434a','d0810784-0ea9-4b42-80e6-5bf982540e04','New Order Submitted','Kevin sharma placed a new order (#ORD-D0810784)',5,1,'2026-03-09 15:57:52.081147','2026-03-09 15:57:25.084789','2026-03-09 15:57:52.081334',NULL,NULL,0,NULL,NULL,'d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'/orders/d0810784-0ea9-4b42-80e6-5bf982540e04'),('a04a659f-bd1e-4b7a-a7f7-7c552981c36e','69fceb33-43b5-4c3c-989a-7eddf859ceef','12044d25-2671-47f6-bea2-1934ca925308','Order Assigned','2 orders (#ORD-12044D25) has been completed',5,1,'2026-03-10 00:59:49.535383','2026-03-10 00:59:32.522731','2026-03-10 01:02:12.061785','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'12044d25-2671-47f6-bea2-1934ca925308',1,2,'2026-03-10 01:02:12.043642','/orders/12044d25-2671-47f6-bea2-1934ca925308'),('a0d2d6ef-37cf-4dbc-9216-c347b51af529','69fceb33-43b5-4c3c-989a-7eddf859ceef','d081ef89-c6dd-45f4-84fa-8ed23979264c','Order Completed','Order (#ORD-D081EF89) has been completed',5,1,'2026-03-09 16:18:34.155265','2026-03-09 16:18:18.906035','2026-03-09 16:18:34.155378','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'/orders/d081ef89-c6dd-45f4-84fa-8ed23979264c'),('ac4a265d-ef4f-400b-b90b-9ef9e102e86d','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','12044d25-2671-47f6-bea2-1934ca925308','Order Completed','Your order (#ORD-12044D25) has been completed',5,1,'2026-03-10 01:02:33.569069','2026-03-10 01:02:11.968303','2026-03-10 01:02:33.569850',NULL,NULL,0,NULL,NULL,'12044d25-2671-47f6-bea2-1934ca925308',1,1,NULL,'/orders/12044d25-2671-47f6-bea2-1934ca925308'),('af3155e9-31b0-4135-b2ff-7a7bac837a11','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','d0810784-0ea9-4b42-80e6-5bf982540e04','Preview Files Available','Preview files were uploaded for order (#ORD-D0810784)',8,1,'2026-03-10 01:02:33.569069','2026-03-09 16:09:00.743419','2026-03-10 01:02:33.569850',NULL,NULL,0,NULL,NULL,'d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'/orders/d0810784-0ea9-4b42-80e6-5bf982540e04'),('b304fd63-f675-44cd-91cf-25559e8932cb','69fceb33-43b5-4c3c-989a-7eddf859ceef','d0810784-0ea9-4b42-80e6-5bf982540e04','Order Completed','Order (#ORD-D0810784) has been completed',5,1,'2026-03-09 16:14:54.921058','2026-03-09 16:09:45.564385','2026-03-09 16:14:54.921195','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'/orders/d0810784-0ea9-4b42-80e6-5bf982540e04'),('b46b8ac9-c53c-4120-bc70-f003da4a1b4f','69fceb33-43b5-4c3c-989a-7eddf859ceef','42d4ac72-d397-4886-8fd8-bab880c0bbb6','Order Assigned','You have been assigned order (#ORD-42D4AC72)',5,1,'2026-03-09 00:30:14.681714','2026-03-09 00:30:06.117719','2026-03-09 00:30:14.682203','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'/orders/42d4ac72-d397-4886-8fd8-bab880c0bbb6'),('bba25c1a-09c9-4505-9cd9-4cf756a9ffdf','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','42d4ac72-d397-4886-8fd8-bab880c0bbb6','Order Completed','Your order (#ORD-42D4AC72) has been completed',5,1,'2026-03-09 09:06:55.495279','2026-03-09 00:38:26.762387','2026-03-09 09:06:55.507439',NULL,NULL,0,NULL,NULL,'42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'/orders/42d4ac72-d397-4886-8fd8-bab880c0bbb6'),('bc975430-7c4c-4f29-84ad-fe2fccba799b','da6639fc-4721-4958-940d-bf3a910b434a','3c5b0459-7bcc-4818-ac24-ac9b6af11383','New Order Submitted','2 Kevins sharma approved the logo for order (#ORD-3C5B0459). Please mark as completed.',5,1,'2026-03-08 22:06:36.332167','2026-03-08 22:06:26.722172','2026-03-08 22:08:28.791998',NULL,NULL,0,NULL,NULL,'3c5b0459-7bcc-4818-ac24-ac9b6af11383',1,2,'2026-03-08 22:08:28.765886','/orders/3c5b0459-7bcc-4818-ac24-ac9b6af11383'),('c0aff4fc-15a2-4dc0-95b7-5e445bc2d6f4','27250b41-8cb5-40f1-bef6-ef4e114062a3','42d4ac72-d397-4886-8fd8-bab880c0bbb6','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-42D4AC72). Please mark as completed.',5,0,NULL,'2026-03-09 00:38:13.995675',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'/orders/42d4ac72-d397-4886-8fd8-bab880c0bbb6'),('c15fe08f-8cbe-42e6-9890-23825322f134','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','Preview Files Available','Preview files were uploaded for order (#ORD-FB116DE6)',8,1,'2026-03-09 09:06:55.495279','2026-03-08 23:03:30.179897','2026-03-09 09:06:55.507440',NULL,NULL,0,NULL,NULL,'fb116de6-7f82-4fcb-aff5-d8a95473bb1c',1,1,NULL,'/orders/fb116de6-7f82-4fcb-aff5-d8a95473bb1c'),('c17ca20e-0d08-4584-b30c-c47c407a9266','da6639fc-4721-4958-940d-bf3a910b434a','cf2dc5e6-2797-441d-ba05-7d465abdf900','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-CF2DC5E6). Please mark as completed.',5,1,'2026-03-08 22:03:49.769104','2026-03-08 22:02:48.515094','2026-03-08 22:03:49.769205','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'cf2dc5e6-2797-441d-ba05-7d465abdf900',1,1,NULL,'/orders/cf2dc5e6-2797-441d-ba05-7d465abdf900'),('c2b5567f-9509-4ae5-a4e4-4d3ff4a3dd18','da6639fc-4721-4958-940d-bf3a910b434a','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','Preview Ready for QA','Designer uploaded preview files for order (#ORD-FB116DE6)',8,1,'2026-03-08 23:03:21.590499','2026-03-08 23:02:43.990974','2026-03-08 23:03:21.590952','69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'fb116de6-7f82-4fcb-aff5-d8a95473bb1c',1,1,NULL,'/orders/fb116de6-7f82-4fcb-aff5-d8a95473bb1c'),('d097eeff-d6ce-4d7c-b728-70f9d553ba5f','27250b41-8cb5-40f1-bef6-ef4e114062a3','d0810784-0ea9-4b42-80e6-5bf982540e04','Preview Ready for QA','Designer uploaded preview files for order (#ORD-D0810784)',8,0,NULL,'2026-03-09 16:08:35.769648',NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'/orders/d0810784-0ea9-4b42-80e6-5bf982540e04'),('d879e927-dd52-4501-b71d-cb54d95d74f5','da6639fc-4721-4958-940d-bf3a910b434a','42d4ac72-d397-4886-8fd8-bab880c0bbb6','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-42D4AC72). Please mark as completed.',5,1,'2026-03-09 08:07:36.478127','2026-03-09 00:38:14.082424','2026-03-09 08:07:36.515158','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'/orders/42d4ac72-d397-4886-8fd8-bab880c0bbb6'),('da6476f9-88c1-41ba-b6ae-9e7661153a95','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','3c5b0459-7bcc-4818-ac24-ac9b6af11383','Invoice Generated','Invoice (#INV-20260308-F00507D0) generated for order (#ORD-3C5B0459)',1,1,'2026-03-09 09:06:55.495279','2026-03-08 22:09:20.687322','2026-03-09 09:06:55.507441','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'ecd033ee-6914-418e-ae72-fa76c0b9e11e',2,1,NULL,'/invoices/ecd033ee-6914-418e-ae72-fa76c0b9e11e'),('daec4b18-2028-4350-938e-8fee0c5888f8','27250b41-8cb5-40f1-bef6-ef4e114062a3','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','Preview Ready for QA','Designer uploaded preview files for order (#ORD-FB116DE6)',8,0,NULL,'2026-03-08 23:02:43.936190',NULL,'69fceb33-43b5-4c3c-989a-7eddf859ceef',NULL,0,NULL,NULL,'fb116de6-7f82-4fcb-aff5-d8a95473bb1c',1,1,NULL,'/orders/fb116de6-7f82-4fcb-aff5-d8a95473bb1c'),('dc1fe425-5020-4135-9d65-1cfb7e35fa40','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','cf2dc5e6-2797-441d-ba05-7d465abdf900','Preview Files Available','Preview files were uploaded for order (#ORD-CF2DC5E6)',8,1,'2026-03-08 22:02:41.132515','2026-03-08 22:02:31.075189','2026-03-08 22:02:41.132631',NULL,NULL,0,NULL,NULL,'cf2dc5e6-2797-441d-ba05-7d465abdf900',1,1,NULL,'/orders/cf2dc5e6-2797-441d-ba05-7d465abdf900'),('dd03c8ea-402b-4574-98cd-657cef79a329','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','27c4e2e1-8df3-445a-8455-aea496e62f38','Preview Files Available','Preview files were uploaded for order (#ORD-27C4E2E1)',8,1,'2026-03-09 15:56:47.610892','2026-03-09 09:23:49.979943','2026-03-09 15:56:47.611020',NULL,NULL,0,NULL,NULL,'27c4e2e1-8df3-445a-8455-aea496e62f38',1,1,NULL,'/orders/27c4e2e1-8df3-445a-8455-aea496e62f38'),('df1f5bd7-4b67-458a-9493-eb2f8c4366f4','27250b41-8cb5-40f1-bef6-ef4e114062a3','4e6f84a6-f27f-4c15-a5ee-acbe8daf5d94','New Order Submitted','Kevin sharma placed a new order (#ORD-4E6F84A6)',5,0,NULL,'2026-03-09 17:52:34.945385',NULL,NULL,NULL,0,NULL,NULL,'4e6f84a6-f27f-4c15-a5ee-acbe8daf5d94',1,1,NULL,'/orders/4e6f84a6-f27f-4c15-a5ee-acbe8daf5d94'),('e0577708-ae78-4cfd-bf42-f869fd4ee0fb','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','Order Completed','Your order (#ORD-FD781C82) has been completed',5,1,'2026-03-09 09:06:55.495280','2026-03-09 00:45:10.116620','2026-03-09 09:06:55.507441',NULL,NULL,0,NULL,NULL,'fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,1,NULL,'/orders/fd781c82-c35c-46e0-9a8a-ba91ab7df3cb'),('e0f06958-6ae6-4ae2-b60d-de6201718f42','27250b41-8cb5-40f1-bef6-ef4e114062a3','cf2dc5e6-2797-441d-ba05-7d465abdf900','New Order Submitted','Kevin sharma placed a new order (#ORD-CF2DC5E6)',5,0,NULL,'2026-03-08 21:57:19.467581',NULL,NULL,NULL,0,NULL,NULL,'cf2dc5e6-2797-441d-ba05-7d465abdf900',1,1,NULL,'/orders/cf2dc5e6-2797-441d-ba05-7d465abdf900'),('f006f557-66a3-4019-ba82-ca466beac117','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','d0810784-0ea9-4b42-80e6-5bf982540e04','Invoice Generated','Invoice (#INV-20260309-114824AD) generated for order (#ORD-D0810784)',1,1,'2026-03-10 01:02:33.569070','2026-03-09 16:09:46.086351','2026-03-10 01:02:33.569850','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'e2a24275-9323-4ce9-8d13-6d124ef8b9cf',2,1,NULL,'/invoices/e2a24275-9323-4ce9-8d13-6d124ef8b9cf'),('f040db1b-87b0-4d0a-9642-4ab115280402','27250b41-8cb5-40f1-bef6-ef4e114062a3','3c5b0459-7bcc-4818-ac24-ac9b6af11383','New Order Submitted','2 Kevins sharma approved the logo for order (#ORD-3C5B0459). Please mark as completed.',5,0,NULL,'2026-03-08 22:06:26.656914','2026-03-08 22:08:28.686056',NULL,NULL,0,NULL,NULL,'3c5b0459-7bcc-4818-ac24-ac9b6af11383',1,2,'2026-03-08 22:08:28.666610','/orders/3c5b0459-7bcc-4818-ac24-ac9b6af11383'),('f196bc79-fc9a-4a9c-8157-561d1dbb274d','69fceb33-43b5-4c3c-989a-7eddf859ceef','42d4ac72-d397-4886-8fd8-bab880c0bbb6','Order Completed','Order (#ORD-42D4AC72) has been completed',5,1,'2026-03-09 00:38:38.659935','2026-03-09 00:38:26.844167','2026-03-09 00:38:38.660283','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'/orders/42d4ac72-d397-4886-8fd8-bab880c0bbb6'),('f4a38a1c-1c45-4f25-a879-48c19364e7e7','27250b41-8cb5-40f1-bef6-ef4e114062a3','12044d25-2671-47f6-bea2-1934ca925308','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-12044D25). Please mark as completed.',5,0,NULL,'2026-03-10 01:02:05.728656',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'12044d25-2671-47f6-bea2-1934ca925308',1,1,NULL,'/orders/12044d25-2671-47f6-bea2-1934ca925308'),('f4e5dc8d-e82b-47b4-bb3f-07a14fe9ceba','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','3c5b0459-7bcc-4818-ac24-ac9b6af11383','Preview Files Available','Preview files were uploaded for order (#ORD-3C5B0459)',8,1,'2026-03-08 22:08:26.021699','2026-03-08 22:08:16.066863','2026-03-08 22:08:26.022129',NULL,NULL,0,NULL,NULL,'3c5b0459-7bcc-4818-ac24-ac9b6af11383',1,1,NULL,'/orders/3c5b0459-7bcc-4818-ac24-ac9b6af11383'),('f5a97d33-388b-4bd3-bcb4-5ec3802dbe98','27250b41-8cb5-40f1-bef6-ef4e114062a3','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','Client Approved Logo','Kevin sharma approved the logo for order (#ORD-FD781C82). Please mark as completed.',5,0,NULL,'2026-03-09 00:44:58.352152',NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6',NULL,0,NULL,NULL,'fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,1,NULL,'/orders/fd781c82-c35c-46e0-9a8a-ba91ab7df3cb'),('f8a7c51e-3cc3-4359-9b89-c1a61d048714','69fceb33-43b5-4c3c-989a-7eddf859ceef','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb','Order Completed','Order (#ORD-FD781C82) has been completed',5,1,'2026-03-09 08:12:15.931070','2026-03-09 00:45:10.858505','2026-03-09 08:12:15.979759','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,1,NULL,'/orders/fd781c82-c35c-46e0-9a8a-ba91ab7df3cb'),('fa7da92e-6152-463d-9ca9-cdf381e7898a','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','fb116de6-7f82-4fcb-aff5-d8a95473bb1c','Invoice Generated','Invoice (#INV-20260308-D87782EB) generated for order (#ORD-FB116DE6)',1,1,'2026-03-08 23:04:10.147024','2026-03-08 23:03:56.475756','2026-03-08 23:04:10.147158','da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL,'596d6a30-86ab-4eda-ae61-c91040c799e6',2,1,NULL,'/invoices/596d6a30-86ab-4eda-ae61-c91040c799e6');
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
INSERT INTO `orderlogs` VALUES ('0b518095-16dd-4a81-99f1-bcdd1c0b1d42','12044d25-2671-47f6-bea2-1934ca925308',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-10 00:55:49.830564',NULL,NULL,NULL,0,NULL,NULL),('1e35d174-8c11-49ef-ae88-8e7bb0f906b6','8d8bf938-a4d4-4ffc-9905-f8b564353640',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-09 22:31:36.169836',NULL,NULL,NULL,0,NULL,NULL),('4e54cb8b-0475-4807-a0d6-1de848d6a25e','27c4e2e1-8df3-445a-8455-aea496e62f38',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-09 09:00:56.722938',NULL,NULL,NULL,0,NULL,NULL),('6186c180-e9fa-4291-b0f3-ac454cc35568','d081ef89-c6dd-45f4-84fa-8ed23979264c',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-09 10:43:08.292680',NULL,NULL,NULL,0,NULL,NULL),('66d8341b-6bb0-485d-879a-8be10f319561','3c5b0459-7bcc-4818-ac24-ac9b6af11383',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-08 22:06:26.589321',NULL,NULL,NULL,0,NULL,NULL),('958a81a8-5010-4de0-b4c9-79c2da23486a','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-09 00:39:17.668918',NULL,NULL,NULL,0,NULL,NULL),('9696e376-842d-42d9-a932-889d21ff8a10','d0810784-0ea9-4b42-80e6-5bf982540e04',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-09 15:57:24.673428',NULL,NULL,NULL,0,NULL,NULL),('a83e04f1-9691-476d-a73d-4eeca55e3a90','42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-09 00:29:59.602434',NULL,NULL,NULL,0,NULL,NULL),('bcbceb9d-e1ff-46ac-ace9-fe84daa22210','cf2dc5e6-2797-441d-ba05-7d465abdf900',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-08 21:57:18.962499',NULL,NULL,NULL,0,NULL,NULL),('cb37e045-0b9b-4a6e-a7bb-3b916e7a5b0c','4e6f84a6-f27f-4c15-a5ee-acbe8daf5d94',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-09 17:52:34.701885',NULL,NULL,NULL,0,NULL,NULL),('e7e1bc8d-8592-45b7-993b-8cf2edaaf4cf','fb116de6-7f82-4fcb-aff5-d8a95473bb1c',1,NULL,1,'Client','bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','Order created with files',NULL,'2026-03-08 23:01:52.666146',NULL,NULL,NULL,0,NULL,NULL);
/*!40000 ALTER TABLE `orderlogs` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `orderpricehistories`
--

LOCK TABLES `orderpricehistories` WRITE;
/*!40000 ALTER TABLE `orderpricehistories` DISABLE KEYS */;
/*!40000 ALTER TABLE `orderpricehistories` ENABLE KEYS */;
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
INSERT INTO `orderstatushistories` VALUES ('01b424ce-0d89-4689-bc7b-3fe02ab303cf','8d8bf938-a4d4-4ffc-9905-f8b564353640',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 22:31:36.169835',NULL,NULL,NULL,0,NULL,NULL),('01bddab1-0691-4b08-b53d-15c4838d0638','fb116de6-7f82-4fcb-aff5-d8a95473bb1c',1,3,'Assigned to designer','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-08 23:02:00.374975',NULL,NULL,NULL,0,NULL,NULL),('099b24af-7e02-4887-8cb0-da7209960336','12044d25-2671-47f6-bea2-1934ca925308',1,3,'Assigned to designer','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-10 00:59:32.454148',NULL,NULL,NULL,0,NULL,NULL),('0c2c8f3b-8e5a-4186-9a2d-398fcb52ca95','cf2dc5e6-2797-441d-ba05-7d465abdf900',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-08 21:57:18.962499',NULL,NULL,NULL,0,NULL,NULL),('18359769-6ee2-489c-b2b8-2c821cfeaec5','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 00:39:17.668917',NULL,NULL,NULL,0,NULL,NULL),('1ac543c2-74ca-4043-a192-a050dcca5dec','3c5b0459-7bcc-4818-ac24-ac9b6af11383',1,3,'Assigned to designer','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-08 22:06:41.461149',NULL,NULL,NULL,0,NULL,NULL),('1c61b82e-d2c8-423f-9cc9-53fa4952ca59','cf2dc5e6-2797-441d-ba05-7d465abdf900',1,3,'Assigned to designer','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-08 21:59:30.527925',NULL,NULL,NULL,0,NULL,NULL),('3eb430f6-e97a-44d8-bda9-318bb03f96d0','12044d25-2671-47f6-bea2-1934ca925308',3,4,'Files sent to client for review','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-10 01:01:55.862823',NULL,NULL,NULL,0,NULL,NULL),('3fc72435-9e87-484b-a482-b550f05a14e2','cf2dc5e6-2797-441d-ba05-7d465abdf900',6,7,NULL,'da6639fc-4721-4958-940d-bf3a910b434a','2026-03-08 22:03:19.653829',NULL,NULL,NULL,0,NULL,NULL),('4b8968fc-dc69-4ed9-9ae6-4573a616cdc2','42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 00:29:59.602433',NULL,NULL,NULL,0,NULL,NULL),('4ff25998-dde5-46a5-ac37-7d2f9a3fe33b','42d4ac72-d397-4886-8fd8-bab880c0bbb6',1,3,'Assigned to designer','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 00:30:06.051240',NULL,NULL,NULL,0,NULL,NULL),('602c5a6d-7443-40cb-892f-b571258ac218','3c5b0459-7bcc-4818-ac24-ac9b6af11383',3,4,'Files sent to client for review','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-08 22:08:16.015799',NULL,NULL,NULL,0,NULL,NULL),('66c8faae-df74-4197-97a0-44b761b73e09','d0810784-0ea9-4b42-80e6-5bf982540e04',1,3,'Assigned to designer','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 15:57:59.340674',NULL,NULL,NULL,0,NULL,NULL),('6903c8df-acd6-4e81-94c7-a2baec74b349','4e6f84a6-f27f-4c15-a5ee-acbe8daf5d94',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 17:52:34.701885',NULL,NULL,NULL,0,NULL,NULL),('70b56c5a-eb56-4931-bba0-b9f552216a06','42d4ac72-d397-4886-8fd8-bab880c0bbb6',3,4,'Files sent to client for review','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 00:38:07.060726',NULL,NULL,NULL,0,NULL,NULL),('713bc156-6d7f-470d-873c-12fdee79713b','d0810784-0ea9-4b42-80e6-5bf982540e04',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 15:57:24.673427',NULL,NULL,NULL,0,NULL,NULL),('78eb57a6-49eb-4f50-b3f5-da886a623de9','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',6,7,NULL,'da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 00:45:09.998224',NULL,NULL,NULL,0,NULL,NULL),('8327868d-9df4-46e9-8c15-ea55dd5ea684','d081ef89-c6dd-45f4-84fa-8ed23979264c',1,3,'Assigned to designer','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 10:43:23.351412',NULL,NULL,NULL,0,NULL,NULL),('8abafa5c-f45c-4e2f-8dae-218d9916635f','12044d25-2671-47f6-bea2-1934ca925308',6,7,NULL,'da6639fc-4721-4958-940d-bf3a910b434a','2026-03-10 01:02:11.908981',NULL,NULL,NULL,0,NULL,NULL),('92777739-6a96-4fb2-8ee2-f9135a18403b','d081ef89-c6dd-45f4-84fa-8ed23979264c',6,7,NULL,'da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 16:18:18.810443',NULL,NULL,NULL,0,NULL,NULL),('9ae125e1-a155-476d-92a6-9b1c278e21a1','27c4e2e1-8df3-445a-8455-aea496e62f38',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 09:00:56.722936',NULL,NULL,NULL,0,NULL,NULL),('9eb67b72-cde3-41b0-8147-c4869b1be487','3c5b0459-7bcc-4818-ac24-ac9b6af11383',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-08 22:06:26.589321',NULL,NULL,NULL,0,NULL,NULL),('a7b3d2a6-2cf9-43de-a803-3f2ea4333064','d0810784-0ea9-4b42-80e6-5bf982540e04',3,4,'Files sent to client for review','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 16:09:00.695181',NULL,NULL,NULL,0,NULL,NULL),('a942ba85-babd-4acf-8cc9-8b6803cceb7e','27c4e2e1-8df3-445a-8455-aea496e62f38',3,4,'Files sent to client for review','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 09:23:49.930868',NULL,NULL,NULL,0,NULL,NULL),('aa969feb-28a1-4c20-a9fc-5c57dc39e0ff','12044d25-2671-47f6-bea2-1934ca925308',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-10 00:55:49.830562',NULL,NULL,NULL,0,NULL,NULL),('aaaff32f-ad04-4c2b-a482-0e3aa47681bc','d081ef89-c6dd-45f4-84fa-8ed23979264c',3,4,'Files sent to client for review','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 16:17:36.737003',NULL,NULL,NULL,0,NULL,NULL),('aadd63eb-72c9-42cf-b63c-a713a87dab87','fb116de6-7f82-4fcb-aff5-d8a95473bb1c',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-08 23:01:52.666145',NULL,NULL,NULL,0,NULL,NULL),('ac919f1a-4881-4b31-97da-f08fa3a7512e','42d4ac72-d397-4886-8fd8-bab880c0bbb6',6,7,NULL,'da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 00:38:26.718883',NULL,NULL,NULL,0,NULL,NULL),('c48497d6-e8b5-49c5-a164-38d414c0e965','3c5b0459-7bcc-4818-ac24-ac9b6af11383',6,7,NULL,'da6639fc-4721-4958-940d-bf3a910b434a','2026-03-08 22:09:18.739259',NULL,NULL,NULL,0,NULL,NULL),('c4ad66ff-a143-4b26-86b7-53d5337fada7','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',3,4,'Files sent to client for review','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 00:44:48.512818',NULL,NULL,NULL,0,NULL,NULL),('cb92805f-241b-444e-b5ff-ddb34744341c','cf2dc5e6-2797-441d-ba05-7d465abdf900',3,4,'Files sent to client for review','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-08 22:02:30.982992',NULL,NULL,NULL,0,NULL,NULL),('cca6348e-96b1-465d-847f-8ea07e973e2b','d0810784-0ea9-4b42-80e6-5bf982540e04',6,7,NULL,'da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 16:09:45.371131',NULL,NULL,NULL,0,NULL,NULL),('cebded43-c406-411f-8bea-6954c11c4ff8','fd781c82-c35c-46e0-9a8a-ba91ab7df3cb',1,3,'Assigned to designer','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 00:39:37.185699',NULL,NULL,NULL,0,NULL,NULL),('de4670b0-fd2b-4bc9-b7df-fbb3fea30973','d081ef89-c6dd-45f4-84fa-8ed23979264c',1,1,NULL,'bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','2026-03-09 10:43:08.292679',NULL,NULL,NULL,0,NULL,NULL),('eb3931d5-dbc6-490b-88ca-994f953a14f4','fb116de6-7f82-4fcb-aff5-d8a95473bb1c',3,4,'Files sent to client for review','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-08 23:03:30.124494',NULL,NULL,NULL,0,NULL,NULL),('f2543600-1379-42a5-af8c-5f09fe834844','27c4e2e1-8df3-445a-8455-aea496e62f38',1,3,'Assigned to designer','da6639fc-4721-4958-940d-bf3a910b434a','2026-03-09 09:22:14.183446',NULL,NULL,NULL,0,NULL,NULL),('fa5445c9-1412-4646-9a3a-a869ab526fe4','fb116de6-7f82-4fcb-aff5-d8a95473bb1c',6,7,NULL,'da6639fc-4721-4958-940d-bf3a910b434a','2026-03-08 23:03:55.973961',NULL,NULL,NULL,0,NULL,NULL);
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
INSERT INTO `permissions` VALUES ('10000000-0000-0000-0000-000000000001','CreateUser','Create new users','User','Create','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('10000000-0000-0000-0000-000000000002','ViewUsers','View all users','User','Read','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('10000000-0000-0000-0000-000000000003','UpdateUser','Update user information','User','Update','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('10000000-0000-0000-0000-000000000004','DeleteUser','Delete users','User','Delete','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('20000000-0000-0000-0000-000000000001','CreateDesignerProfile','Create designer profiles','DesignerProfile','Create','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('20000000-0000-0000-0000-000000000002','ViewDesignerProfiles','View designer profiles','DesignerProfile','Read','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('20000000-0000-0000-0000-000000000003','UpdateDesignerProfile','Update designer profiles','DesignerProfile','Update','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('30000000-0000-0000-0000-000000000001','CreateOrder','Create new orders','Order','Create','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('30000000-0000-0000-0000-000000000002','ViewAllOrders','View all orders in the system','Order','ReadAll','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('30000000-0000-0000-0000-000000000003','AssignOrder','Assign orders to designers','Order','Assign','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('30000000-0000-0000-0000-000000000004','UpdateOrderStatus','Update order status','Order','UpdateStatus','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('40000000-0000-0000-0000-000000000001','ManagePermissions','Manage role permissions','Permission','Manage','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('50000000-0000-0000-0000-000000000001','UploadFile','Upload files','File','Upload','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('50000000-0000-0000-0000-000000000002','DownloadFile','Download files','File','Download','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('50000000-0000-0000-0000-000000000003','DeleteFile','Delete files','File','Delete','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL);
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
INSERT INTO `rolepermissions` VALUES ('065676e8-0eed-4683-89ff-082d9db72570','22222222-2222-2222-2222-222222222222','30000000-0000-0000-0000-000000000002','2026-03-07 18:20:30.765353',NULL,NULL,NULL,0,NULL,NULL),('086e7a75-2b24-4c1e-ab90-94ccc4988ae5','22222222-2222-2222-2222-222222222222','10000000-0000-0000-0000-000000000003','2026-03-07 18:20:17.548975','2026-03-07 18:20:26.960821',NULL,NULL,1,'2026-03-07 18:20:26.960690',NULL),('4220f108-bfbf-47f0-b1e2-0b5ea24374eb','22222222-2222-2222-2222-222222222222','10000000-0000-0000-0000-000000000004','2026-03-07 18:20:18.321818','2026-03-07 18:20:27.549600',NULL,NULL,1,'2026-03-07 18:20:27.549507',NULL),('88ea140f-94a2-4197-9951-fe549acbecb7','22222222-2222-2222-2222-222222222222','20000000-0000-0000-0000-000000000001','2026-03-07 18:20:48.241986',NULL,NULL,NULL,0,NULL,NULL),('af5f6444-9935-42b8-9425-7ff0d31563bc','22222222-2222-2222-2222-222222222222','30000000-0000-0000-0000-000000000001','2026-03-07 18:20:32.069432','2026-03-07 18:20:35.055330',NULL,NULL,1,'2026-03-07 18:20:35.055237',NULL),('b0e89c76-c936-46e6-a3f1-3681b204dd32','22222222-2222-2222-2222-222222222222','30000000-0000-0000-0000-000000000004','2026-03-07 18:20:59.867915',NULL,NULL,NULL,0,NULL,NULL),('c6a1b73d-bad9-4da7-b6d9-31b37a956047','22222222-2222-2222-2222-222222222222','10000000-0000-0000-0000-000000000002','2026-03-07 18:20:16.055850','2026-03-07 18:20:26.389719',NULL,NULL,1,'2026-03-07 18:20:26.389567',NULL),('d9c400dc-3f56-4e52-a72b-ca6f4bf94966','22222222-2222-2222-2222-222222222222','30000000-0000-0000-0000-000000000003','2026-03-07 18:20:38.277858',NULL,NULL,NULL,0,NULL,NULL),('df4c8a5a-3369-4050-a431-6b8a36df8aa9','22222222-2222-2222-2222-222222222222','20000000-0000-0000-0000-000000000003','2026-03-07 18:20:44.776894',NULL,NULL,NULL,0,NULL,NULL),('f2988c0a-436b-48d6-9567-86b2441cec5b','22222222-2222-2222-2222-222222222222','10000000-0000-0000-0000-000000000001','2026-03-07 18:20:15.541039','2026-03-07 18:20:24.670025',NULL,NULL,1,'2026-03-07 18:20:24.669727',NULL),('fde02f18-7299-4bb3-9d5d-62c0ebe18089','22222222-2222-2222-2222-222222222222','20000000-0000-0000-0000-000000000002','2026-03-07 18:20:53.157075',NULL,NULL,NULL,0,NULL,NULL);
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
INSERT INTO `roles` VALUES ('11111111-1111-1111-1111-111111111111','SuperAdmin','Full system access with all permissions','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('22222222-2222-2222-2222-222222222222','Admin','Administrative access with restricted client data access','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('33333333-3333-3333-3333-333333333333','Designer','Designer access without client identity information','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL),('44444444-4444-4444-4444-444444444444','Client','Client access to their own data','2026-03-10 22:22:28.000000',NULL,NULL,NULL,0,NULL,NULL);
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
INSERT INTO `settings` VALUES ('00b2cba7-d313-4df7-8c97-de45a4611753','PayPalWebhookId','','Payment','Dummy PayPalWebhookId for testing purposes','2026-03-08 01:15:33.990656',NULL,'da6639fc-4721-4958-940d-bf3a910b434a',NULL,0,NULL,NULL),('0602daa7-3025-407b-90b0-8915994a9295','IBAN','GB82WEST12345698765432','Payment','Dummy IBAN for testing purposes','2026-02-24 23:08:27.316316',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('13ba7c03-149a-47ff-bd3d-c3460f1d6d60','PayPalClientSecret','DUMMY_PAYPAL_CLIENT_SECRET_FOR_TESTING','Payment','Dummy PayPalClientSecret for testing purposes','2026-02-24 23:08:27.316301',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('1dd5861f-e260-423b-88ea-b87037a1d475','WiseProfileId','DUMMY_WISE_PROFILE_ID_FOR_TESTING','Payment','Dummy WiseProfileId for testing purposes','2026-02-24 23:08:27.316310',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('34d7b2ae-3f6c-4eb4-802e-0b4b2837fd9b','AccountHolderName','Hawk Merchandising','Payment','Dummy AccountHolderName for testing purposes','2026-02-24 23:08:27.316313',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('3b4db36b-71d1-4c0b-aedd-b7d2e397a022','PayPalClientId','DUMMY_PAYPAL_CLIENT_ID_FOR_TESTING','Payment','Dummy PayPalClientId for testing purposes','2026-02-24 23:08:27.316270',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('94571aa7-cc51-4158-8865-92c7b2473e9d','BankName','Demo Bank','Payment','Dummy BankName for testing purposes','2026-02-24 23:08:27.316311',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('af7ed887-ce3e-42d2-85d3-11a358d9a6a0','RoutingNumber','123456789','Payment','Dummy RoutingNumber for testing purposes','2026-02-24 23:08:27.316319',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('b22675d3-218c-4b6b-bbf7-947f3a5d0a09','SWIFT','DEMOBANK123','Payment','Dummy SWIFT for testing purposes','2026-02-24 23:08:27.316318',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('b33b62a5-d43c-44b3-ad52-b00dac60c7d5','PayPalUseSandbox','true','Payment','Dummy PayPalUseSandbox for testing purposes','2026-02-24 23:08:27.316305',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('bd3ae30d-e97e-45fc-af92-4b2a692a7222','AccountNumber','1234567890','Payment','Dummy AccountNumber for testing purposes','2026-02-24 23:08:27.316314',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('c775d415-a098-49d8-adb8-5c975218f304','WiseApiKey','DUMMY_WISE_API_KEY_FOR_TESTING','Payment','Dummy WiseApiKey for testing purposes','2026-02-24 23:08:27.316307',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('d97d69b0-d459-4608-9d17-de8e043613ad','BankCurrency','USD','Payment','Dummy BankCurrency for testing purposes','2026-02-24 23:08:27.316323',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL),('e6db0d4f-f282-4dd2-b90c-0550e389e245','BranchAddress','123 Main Street, City, Country','Payment','Dummy BranchAddress for testing purposes','2026-02-24 23:08:27.316321',NULL,'00000000-0000-0000-0000-000000000000',NULL,0,NULL,NULL);
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

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES ('27250b41-8cb5-40f1-bef6-ef4e114062a3','admin@logodesign.com','hammad','HMS','$2a$11$1AZPidYDWpt3zkBxcen5.OsMlh.JEb6zX1n3UNWCzYrjmhgfrI0nK',1,'DrmSIlRuuNgs5N/4BbkdPAf11UNw8hTKIeOBqVR13PDWPuCkgKUbnVjeTDYziBb+rNaPveNH4MDYuKex30/ycg==','2026-03-08 01:48:55.517621','22222222-2222-2222-2222-222222222222','2026-03-07 17:29:47.103150','2026-03-08 00:48:55.517781',NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,NULL,0),('69fceb33-43b5-4c3c-989a-7eddf859ceef','designer@logodesign.com','Azeem ','Kamal','$2a$12$tYwLEdeFFVBV8xTlJAJwWu4QBwxHYhQzAccwk8MVS2biAQVPUu3RC',1,'PCBKB77Dq2OkS+wGHHM5N0VZM5xnxdZ6iKogpFko9bXPy7Z8ujkW4e3wRuRMYg5q54TbRGpSbG0t9nP2veXn+g==','2026-03-17 00:59:44.011132','33333333-3333-3333-3333-333333333333','2026-03-06 17:25:27.200995','2026-03-10 00:59:44.011304',NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,NULL,0),('bc5c5c41-c2cf-4dba-bcd4-ac8fac9bd4e6','client@logodesign.com','Kevin','sharma','$2a$12$2lGHqBLIryZjyQhngMOTu.6xcsECLl70n8seT.mqWQO6b2.NbEG2a',1,'GpBL4/LQ/oDTovxhNUMv28eZMymJLKNFJuJolHDmGiGNmfPpzXuiD3nP67wKcwKpKQzggDAHeXMBx8GgsYTFWA==','2026-03-17 00:58:56.306619','44444444-4444-4444-4444-444444444444','2026-03-05 15:03:38.768447','2026-03-10 00:58:56.306756',NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,NULL,0),('da6639fc-4721-4958-940d-bf3a910b434a','superadmin@logodesign.com','Super','Admin','$2a$11$TrpJjAo.7BMKzTSxKULhhuIV6BOXtI/1lvGut3ETxleGjVTBJedbG',1,'IgB9XCqruurw+GnX0zrI5NuN8alyH3C+TQ3h6LuNVrSRpFvQ0X21maAObJDlX9C5PXJIDJ0TVyiRuG43f6+lHg==','2026-03-17 22:32:55.953072','11111111-1111-1111-1111-111111111111','2026-02-24 23:08:29.624238','2026-03-10 22:32:56.159151',NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,NULL,1);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-12  3:46:46
