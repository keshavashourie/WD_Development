/* ---------------------------------------------------------------------------
  Description     : CSI_PurgeUtil_CreateTable 
                          Table creation script : create all tables used by the database purging scripts.
  Author          : Benny.Chia 
  Date            : 20 Aug 2015
  Compile in      : Source schema
  Called By       : 
  Call            : None
--------------------------------------------------------------------------- */
-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_ERRORLOG
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_ERRORLOG' AND type = 'U')
BEGIN
   create table CSI_PURGEUTIL_ERRORLOG 
    (
        CREATION_DATETIME          DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL         -- 'Record creation datetime.'
        , MODULENAME               NVARCHAR(40) NOT NULL                               -- 'Module responsible for the error.'
        , MODULEVERSION            NVARCHAR(15) NOT NULL
        , DATEINSTALLED            DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL
        , MODULETYPE               NVARCHAR(40) DEFAULT 'EXECUTION' NOT NULL           -- 'Type of error.  Can only be INSTALLATION or EXECUTION.'
        , STEPNO                   NVARCHAR(16) NOT NULL
        , PROGID                   NVARCHAR(255) NULL                                  -- 'Program that initiate the error.'
        , ERRMSG                   NVARCHAR(4000) NULL                                 -- 'Error description.'
        , ERRCODE                  NVARCHAR(255) NULL                                  -- 'Error code.'
        , CONTAINERID              CHAR(16) NULL
        , RESOURCEID               CHAR(16) NULL
    ); 
    create index CSI_PURGEUTIL_ERRORLOG_IX1 on CSI_PURGEUTIL_ERRORLOG (Modulename, Creation_Datetime);
    create index CSI_PURGEUTIL_ERRORLOG_IX2 on CSI_PURGEUTIL_ERRORLOG (Creation_Datetime);
END
GO

-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_CONFIG
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_CONFIG' AND type = 'U')
BEGIN
    create table CSI_PURGEUTIL_CONFIG 
    (
        TNAME                 NVARCHAR(100) PRIMARY KEY 
        , TVALUE              NVARCHAR(255) NULL
    ); 
END
GO
-------------------------------------------------------------------------------

-- If DebugEnabled is set to Y, debugging information will be inserted into the CSI_PURGEUTIL_ERRORLOG table.
-- INSERT INTO CSI_PURGEUTIL_CONFIG (TNAME, TVALUE) VALUES ('DebugEnabled', 'N');
-- CSI_PURGEUTIL_CONFIG table is reserved to be used in future version of the purging utility.
-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_INSTALLATION
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_INSTALLATION' AND type = 'U')
BEGIN
   create table CSI_PURGEUTIL_INSTALLATION 
    (
        MODULENAME                 NVARCHAR(40) NOT NULL
        , MODULEVERSION            NVARCHAR(15) NOT NULL
        , DATEINSTALLED            DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL
    ); 
    create index CSI_PURGEUTIL_INSTALLATION_IX1 on CSI_PURGEUTIL_INSTALLATION (DATEINSTALLED);
END
GO
-------------------------------------------------------------------------------

-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_SETUP
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_SETUP' AND type = 'U')
BEGIN
   create table CSI_PURGEUTIL_SETUP 
    (
        SetupId                     NVARCHAR(16) PRIMARY KEY 
         , SetupName                NVARCHAR(40) UNIQUE
         , SetupStatus              NVARCHAR(10) DEFAULT 'SETUP' NOT NULL -- 'Each time the setup is updated the status is set to SETUP. The ActivateSetup will set the status to ACTIVE. Only ACTIVE status will be invoked by PurgeBySetup.'
         , MainTableName            NVARCHAR(30) NOT NULL                 -- 'Root table'                         
         , MainTableInstanceCol     NVARCHAR(30) NOT NULL                 -- 'Root table Primary Key'
         , ArchiveSQLByInstanceId   NVARCHAR(1000) NULL                   -- 'Provide the SQL statements to pull out a single record from the main table to archive.'
         , ArchiveSQL               NVARCHAR(2000) NULL                   -- 'Provide the SQL statements to pull out records in one batch from the main table to archive.'
         , TxnDate                  DATETIME null 
         , RetentionPeriod          FLOAT NOT NULL                            -- 'Number of days that lots are to be retained in the database.'
         , JobNo                    BINARY(16) NULL                           -- Stores the user job number used for this setup.'
         , JobStartDate             DATETIME NULL                         -- 'Stores the next date the user job will execute for the setup.'
         , JobEndDate               DATETIME NULL                         -- 'Stores the end date the user job will stop execute for the setup.'
         , JobIntervalType          NVARCHAR(10) NULL                     -- 'Stores the user job interval type used for this setup. (hourly, daily, weekly, monthly)'
         , JobInterval              FLOAT NULL                            -- 'Stores the user job interval used for this setup.'
         , PurgeRequired            INTEGER NOT NULL                      -- Option to turn on/off purging of data. 1 denotes to delete data from its source.  0 denotes otherwise.
         , ArchiveRequired          INTEGER NOT NULL                      -- Option to turn on/off archival of data. 1 denotes copying data to archive database before deleting data from its source.  0 denotes delete data from source without archival.
         -- The number of records to commit on each table.  Smaller CommitBatchSize reduces data contention and therefore improve data concurrency.
         , CommitBatchSize          INTEGER NOT NULL  
         -- Limit the number of records eligible for purge
         , SetupBatchSize           INTEGER DEFAULT 50 NOT NULL 
         -- DMLOption is a phrase attached to the end of the DML statement as database hint.
         -- Eg : MaxDop is the number of CPUs a particular DML statement is allowed to used during execution.
         -- If the database server has a 16 CPUs configuration, database purging process will only use up to 14,
         --     leaving 2 CPUs for other usage.  
         -- '0' value denotes that purging process will not use the MaxDop feature and therefore, all DML statements
         --     will not include the usage of MaxDop feature.
         -- Purpose : to allow other processes to run while the purging process is executing.
         , DMLOption                NVARCHAR(255) DEFAULT 'OPTION (MAXDOP 14)' NOT NULL           
         -- Lock Type determines the type of lock on DELETE statements. Either Row lock or Page lock.
         -- Updating it as ' ' means that the type of lock will be decided by the database.
         , LockType                 NVARCHAR(255) DEFAULT ' ' NOT NULL
         , ValidateExecutionRule1   NVARCHAR(2000) NULL 
         , ValidateExecutionRule2   NVARCHAR(2000) NULL 
         , ValidateExecutionRule3   NVARCHAR(2000) NULL
         , BatchViewTemplate        NVARCHAR(255)  NOT NULL 
         , InstanceViewTemplate     NVARCHAR(255)  NULL
         , BatchView                NVARCHAR(255)  NOT NULL 
         , InstanceView             NVARCHAR(255)  NULL 
         , Remarks                  NVARCHAR(1000) NULL
         , Version                  NVARCHAR(255)  NULL
         , IsActive                 BIT DEFAULT 1  NOT NULL
    ); 
END
GO

--ALTER TABLE CSI_PURGEUTIL_SETUP ALTER COLUMN JobNo BINARY(16);
-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_SETUPTABLES
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_SETUPTABLES' AND type = 'U')
BEGIN
   create table CSI_PURGEUTIL_SETUPTABLES 
    (
        SetupTablesId                 NVARCHAR(16) PRIMARY KEY
        , SetupId                     NVARCHAR(16) NOT NULL
        , TableName                NVARCHAR(30) NOT NULL
        , InstanceCol1A            NVARCHAR(30) NOT NULL                 
        , InstanceCol1B            NVARCHAR(30) NULL                 
        , InstanceCol1C            NVARCHAR(30) NULL                 
        , InstanceCol2             NVARCHAR(30) NULL                 
        , InstanceParentCol        NVARCHAR(30) NULL                 
        , ParentTable              NVARCHAR(30) NULL
        , ParentTableLinkCol       NVARCHAR(30) NULL
        , TxnDate                  DATETIME null 
        -- The number of records to commit on each table.  Smaller CommitBatchSize reduces data contention and therefore improve data concurrency.
        , CommitBatchSize          INTEGER NOT NULL                      -- 
        , SQLQueryPredicate        NVARCHAR(40) NULL
        , TableLevel				  INTEGER NOT NULL
        , Remarks                  NVARCHAR(1000) NULL
    ); 
    create index CSI_PURGEUTIL_SETUPTABLES_IX1 on CSI_PURGEUTIL_SETUPTABLES (SetupId);
    create index CSI_PURGEUTIL_SETUPTABLES_IX2 on CSI_PURGEUTIL_SETUPTABLES (TableName);
END
GO

-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_SETUPTABLESSKIPPED
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_SETUPTABLESSKIPPED' AND type = 'U')
BEGIN
   create table CSI_PURGEUTIL_SETUPTABLESSKIPPED
    (
        SetupTablesId                 NVARCHAR(16) PRIMARY KEY
        , SetupId                     NVARCHAR(16) NOT NULL
        , TableName                NVARCHAR(30) NULL
        , InstanceCol1A            NVARCHAR(30) NULL                 
        , InstanceCol1B            NVARCHAR(30) NULL                 
        , InstanceCol1C            NVARCHAR(30) NULL                 
        , InstanceCol2             NVARCHAR(30) NULL                 
        , InstanceParentCol        NVARCHAR(30) NULL                 
        , ParentTable              NVARCHAR(30) NULL
        , ParentTableLinkCol       NVARCHAR(30) NULL
        , TxnDate                  DATETIME null 
        , CommitBatchSize          INTEGER NULL                      -- 
        , SQLQueryPredicate        NVARCHAR(40) NULL
        , TableLevel                  INTEGER NULL
        , Remarks                  NVARCHAR(1000) NULL
    ); 
    create index CSI_PURGEUTIL_SETUPTABLESSKIPPED_IX1 on CSI_PURGEUTIL_SETUPTABLESSKIPPED (SetupId);
    create index CSI_PURGEUTIL_SETUPTABLESSKIPPED_IX2 on CSI_PURGEUTIL_SETUPTABLESSKIPPED (TableName);
END
GO

-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_MESSAGELOGS
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_MESSAGELOGS' AND type = 'U')
BEGIN
   create table CSI_PURGEUTIL_MESSAGELOGS 
    (
        MessageId                  NVARCHAR(16) PRIMARY KEY
       , MessageDate              DATETIME NULL 
       , MessageText              NVARCHAR(500) NOT NULL
    ); 
    create index CSI_PURGEUTIL_MESSAGELOGS_IX1 on CSI_PURGEUTIL_MESSAGELOGS (MessageDate);
END
GO

-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_SEQUENCE
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_SEQUENCE' AND type = 'U')
BEGIN
    create table CSI_PURGEUTIL_SEQUENCE 
        (
             SequenceName         NVARCHAR(40) PRIMARY KEY
           , SequenceId                NVARCHAR(16) NOT NULL DEFAULT '0000000000000000' 
        ); 

    DELETE FROM CSI_PURGEUTIL_SEQUENCE;
    INSERT INTO CSI_PURGEUTIL_SEQUENCE (SequenceName) VALUES ('CSI_PURGEUTIL_SEQ')
    INSERT INTO CSI_PURGEUTIL_SEQUENCE (SequenceName) VALUES ('CSI_PURGEUTIL_BATCHEXECUTIONID')
    INSERT INTO CSI_PURGEUTIL_SEQUENCE (SequenceName) VALUES ('CSI_PURGEUTIL_STEPNO')
    INSERT INTO CSI_PURGEUTIL_SEQUENCE (SequenceName) VALUES ('CSI_PURGEUTIL_MESSAGEID')
    INSERT INTO CSI_PURGEUTIL_SEQUENCE (SequenceName) VALUES ('CSI_PURGEUTIL_RESTOREID')
END
GO
-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_INSTVALUE
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_INSTVALUE' AND type = 'U')
BEGIN
    create table CSI_PURGEUTIL_INSTVALUE 
    (
          SetupName                NVARCHAR(40) NOT NULL
        , tablename                NVARCHAR(30) NOT NULL
        , instancecol1A            NVARCHAR(30) NOT NULL 
        , instancecol1B            NVARCHAR(30) NULL 
        , instancecol1C            NVARCHAR(30) NULL 
        , instancecol2             NVARCHAR(30) NULL 
        , instancevalue1A          CHAR(16) NOT NULL   -- Usually the instancevalue is the primary key value of the mes table which is of size 16.
        , instancevalue1B          CHAR(16) NULL   
        , instancevalue1C          CHAR(16) NULL   
        , instancevalue2           CHAR(16) NULL   
        , maintableinstancevalue   CHAR(16) NULL
        , level                    INTEGER NOT NULL
        , instanceparentcol        NVARCHAR(30) NULL                 
        , parenttable              NVARCHAR(30) NULL
    ); 
    create clustered index CSI_PURGEUTIL_INSTVALUE_IX1 on CSI_PURGEUTIL_INSTVALUE (SetupName, tablename)
END
GO

-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_PURGEDLOTS
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
           WHERE name = 'CSI_PURGEUTIL_PURGEDLOTS' AND type = 'U')
    -- Create a heap table CSI_PURGEUTIL_PURGEDLOTS
    create table CSI_PURGEUTIL_PURGEDLOTS 
        (
          ContainerId              NVARCHAR(16) NOT NULL
        , ContainerName            NVARCHAR(40) NOT NULL
        , Purged_Datetime          DATETIME
        ); 
GO
-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_RUNLOG
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_RUNLOG' AND type = 'U')
BEGIN
    create table CSI_PURGEUTIL_RUNLOG 
    (
        SetupName                  NVARCHAR(40) NOT NULL  
        , RowId                    INTEGER NOT NULL                                    -- Always initialized and start from 1 per Setup for each run.                          
        , BatchExecutionId         CHAR(16) NULL  
        , RestoreId                CHAR(16) NULL                                       -- Relevant only for the restore process.   Null for purge/archive process. 
        , RunStage                 NVARCHAR(40) NULL 					               --  There are 4 run stages, namely : PREPAREDATA, VALIDATEDATA, ARCHIVEDATA, DELETEDATA
        , Action                   NVARCHAR(255) NULL                                  -- Clear Instance Table, Insert Instance Records, Archive Records, Delete Records, etc
        , TableName                NVARCHAR(30) NULL                                   -- Null if not applicable
        , glevel                   INTEGER NULL 				                       -- Only if the TableName column has a value.   Eg: Root Level table has a gLevel value of 1.
        , RecordsAffected          INTEGER                                             -- The count of records involved in ‘Action’
        , ExecutionTime            FLOAT                                               -- Duration to perform the action in milliseconds
        , CREATION_DATETIME        DATETIME 
        , SQLStatement             NVARCHAR(MAX) NULL                                  -- The actual SQL statement that was ran.
    ); 
    create index CSI_PURGEUTIL_RUNLOG_IX1 on CSI_PURGEUTIL_RUNLOG (SetupName, RowId);
END
GO

-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_RUNLOGHISTORY
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_RUNLOGHISTORY' AND type = 'U')
BEGIN
    create table CSI_PURGEUTIL_RUNLOGHISTORY 
    (
        SetupName                  NVARCHAR(40) NOT NULL  
        , RowId                    INTEGER NOT NULL                                    -- Always initialized and start from 1 per Setup for each run.                          
        , BatchExecutionId         CHAR(16) NULL  
        , RestoreId                CHAR(16) NULL                                       -- Relevant only for the restore process.   Null for purge/archive process. 
        , RunStage                 NVARCHAR(40) NULL 					               --  There are 4 run stages, namely : PREPAREDATA, VALIDATEDATA, ARCHIVEDATA, DELETEDATA
        , Action                   NVARCHAR(255) NULL                                  -- Clear Instance Table, Insert Instance Records, Archive Records, Delete Records, etc
        , TableName                NVARCHAR(30) NULL                                   -- Null if not applicable
        , glevel                   INTEGER NULL 				                       -- Only if the TableName column has a value.   Eg: Root Level table has a gLevel value of 1.
        , RecordsAffected          INTEGER                                             -- The count of records involved in ‘Action’
        , ExecutionTime            FLOAT                                               -- Duration to perform the action in milliseconds
        , CREATION_DATETIME        DATETIME 
        , SQLStatement             NVARCHAR(MAX) NULL                                 -- The actual SQL statement that was ran.
    ); 
    create index CSI_PURGEUTIL_RUNLOGHISTORY_IX1 on CSI_PURGEUTIL_RUNLOGHISTORY (SetupName, action, tablename, batchexecutionid, creation_datetime);
END
GO

-------------------------------------------------------------------------------
-- CSI_PURGEUTIL_ARCHIVELOG
-------------------------------------------------------------------------------
IF NOT EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'CSI_PURGEUTIL_ARCHIVELOG' AND type = 'U')
BEGIN
    create table CSI_PURGEUTIL_ARCHIVELOG 
    (
        SetupName					NVARCHAR(40) NOT NULL                        
        , BatchExecutionId			CHAR(16) NULL  
        , PurgeType					NVARCHAR(30) NULL 
        , TxnDate					DATETIME 
        , Version					NVARCHAR(255)  NULL
        , ArchiveDBName			    NVARCHAR(40) NOT NULL 
        , ArchiveSchemaName			NVARCHAR(40) NOT NULL 
    ); 
    create index CSI_PURGEUTIL_ARCHIVELOG_IX1 on CSI_PURGEUTIL_ARCHIVELOG (SetupName, BatchExecutionId);
END
GO

-------------------------------------------------------------------------------
-- UPDATES
-------------------------------------------------------------------------------

--Modification for 2510
IF COL_LENGTH('CSI_PURGEUTIL_SETUP', 'JobEndDate') IS NULL
BEGIN
    ALTER TABLE CSI_PURGEUTIL_SETUP ADD JobEndDate DATETIME NULL;
END
GO

IF COL_LENGTH('CSI_PURGEUTIL_SETUP', 'JobIntervalType') IS NULL
BEGIN
    ALTER TABLE CSI_PURGEUTIL_SETUP ADD JobIntervalType NVARCHAR(10) NULL;
END
GO

IF COL_LENGTH('CSI_PURGEUTIL_SETUP', 'IsActive') IS NULL
BEGIN
    ALTER TABLE CSI_PURGEUTIL_SETUP ADD IsActive BIT DEFAULT 1 NOT NULL;
END
GO

IF COL_LENGTH('CSI_PURGEUTIL_SETUP', 'SetupBatchSize') IS NULL
BEGIN
    ALTER TABLE CSI_PURGEUTIL_SETUP ADD SetupBatchSize INT DEFAULT 50 NOT NULL;
END
GO