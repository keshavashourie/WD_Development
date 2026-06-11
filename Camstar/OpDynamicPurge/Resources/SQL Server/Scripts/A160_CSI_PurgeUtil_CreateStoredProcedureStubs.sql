/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_CreateStoredProcedure 
                     Stored Procedure creation script : create all stored procedure stubs used by the database purging scripts.
  Author           : Benny.Chia 
  Date             : 14 Sep 2015
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_GetInstance' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_GetInstance AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_CheckInstanceExist' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_CheckInstanceExist AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_GetEligibleInstances' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_GetEligibleInstances AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_ErrorLog_Record' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_ErrorLog_Record AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Global_Log' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Global_Log AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Global_LogMessage' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Global_LogMessage AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Global_LogRunStat' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Global_LogRunStat AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Global_LogArchiveStat' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Global_LogArchiveStat AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Global_GetTableColumnNames' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Global_GetTableColumnNames AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_TableSetup_SetupExists' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_TableSetup_SetupExists AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_TableSetup_ValidateSetupTable' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_TableSetup_ValidateSetupTable AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_TableSetup_SetupTableExists' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_TableSetup_SetupTableExists AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_TableSetup_ValidateSetupTable' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_TableSetup_ValidateSetupTable AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Global_TableExists' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Global_TableExists AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Global_TableColumnExists' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Global_TableColumnExists AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_TableSetup_UpdateSetup' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_TableSetup_UpdateSetup AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_TableSetup_UpdateSetupTable' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_TableSetup_UpdateSetupTable AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_TableSetup_UpdateSetupTableSkipped' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_TableSetup_UpdateSetupTableSkipped AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_TableSetup_DeleteSetup' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_TableSetup_DeleteSetup AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_CreateORAlterArchiveTable' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_CreateORAlterArchiveTable AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_CreateORAlterAllArchiveTable' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_CreateORAlterAllArchiveTable AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_CreateSP' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_CreateSP AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Execute_PurgeByInstanceId' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Execute_PurgeByInstanceId AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Execute_PurgeByInstance' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Execute_PurgeByInstance AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Execute_PurgeBySetup' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Execute_PurgeBySetup AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Execute_PurgeBySingleHierarchy' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Execute_PurgeBySingleHierarchy AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Execute_PurgeBySetupJob' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Execute_PurgeBySetupJob AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Execute_RestoreByInstanceId' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Execute_RestoreByInstanceId AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Execute_RestoreByBatchExecutionId' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Execute_RestoreByBatchExecutionId AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_HousekeepArchive' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_HousekeepArchive AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_TableSetup_GenerateObject' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_TableSetup_GenerateObject AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_DeleByTable_tpl' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_DeleByTable_tpl AS'; 
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Rest_tpl' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Rest_tpl AS'; 
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Execute_ActivateSetup' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Execute_ActivateSetup AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Execute_DeActivateSetup' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_Execute_DeActivateSetup AS';
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_CreateUserView' AND type = 'P')
   EXECUTE sp_executesql N'CREATE PROCEDURE CSI_PurgeUtil_CreateUserView AS';
GO
