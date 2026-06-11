--------------------------------------------------------------------------------
-- SCRIPT: csiDataStoreMonitor.sql
-- DESCR: Verifies and/or creates jobs in SQL Server Agent
--
-- Copyright Siemens 2023  
--
-- History:
--   12/06/2006  Updated/added copyright notice(s) (SPR S9984) Bill Lippard.
--   04/23/2007 - Updated copyright notice(s) (SPR S9984) Bill Lippard.
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiDataStoreMonitor' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiDataStoreMonitor
GO

CREATE PROCEDURE csiDataStoreMonitor
AS
   DECLARE @ErrorStatus Int
   DECLARE @sql Varchar(512)
   DECLARE @numTables Int
   DECLARE @Terminate Varchar(10)
   DECLARE @InsNum Int
   DECLARE @TableName Varchar(50)
   DECLARE @JobName Varchar(100)
   DECLARE @Description Varchar(255)
   DECLARE @JobCommand Varchar(255)
BEGIN
--
-- Copyright Siemens 2023  ------------------------------------------------------------------------------------------
--
   SET NOCOUNT ON
   SELECT @Terminate=VALUE
   FROM DATASTORESETUP
   WHERE PARAMETER='DATASTORE_TERMINATE'

   IF @Terminate='N'
   BEGIN
      -- Jobs to be created are:
      --    csiDataStoreCleanup
      --    csiDataStoreReplicator('DATASTOREUPDATES')
      --    csiDataStoreReplicator('DATASTOREINSERTSx')
      EXECUTE csiDataStoreVerifyJob 'DataStore - Cleanup', 'DataStore cleanup job', 'csiDataStoreCleanup'

      EXECUTE csiDataStoreVerifyJob 'DataStore - DATASTOREUPDATES', 'DataStore replicator for DATASTOREUPDATES', 'csiDataStoreReplicator ''DATASTOREUPDATES'' '

      SELECT @NumTables=Convert(Int,TVALUE)
      FROM INSITESITEINFO
      WHERE TNAME='DataStoreInsertTables'
  
      SET @InsNum=1
      WHILE @InsNum <= @NumTables
      BEGIN
         SET @TableName='DATASTOREINSERTS'+Convert(varchar,@InsNum)
         SET @JobName='DataStore - ' + @TableName
         SET @Description='DataStore replicator for '+@TableName
         SET @JobCommand='csiDataStoreReplicator '''+@TableName+''' '

         EXECUTE csiDataStoreVerifyJob @JobName, @Description, @JobCommand

         SET @InsNum=@InsNum+1
      END --WHILE @InsNum <= @NumTables
   END --IF @Terminate='N'
END
GO
