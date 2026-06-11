-- CSI_PurgeUtil_Global_LogArchiveStat() uses CSI_PurgeUtil_ArchiveLog_Tab Type.  So this SP has to be drop first before dropping the TYPE definition.
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Global_LogArchiveStat' AND type = 'P')
    DROP PROCEDURE CSI_PurgeUtil_Global_LogArchiveStat;
IF EXISTS (SELECT name FROM sysobjects WHERE name LIKE 'TT_CSI_PurgeUtil_ArchiveLog_Tab%' AND type = 'TT')
    DROP TYPE CSI_PurgeUtil_ArchiveLog_Tab;
GO
CREATE TYPE CSI_PurgeUtil_ArchiveLog_Tab AS TABLE ( 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_ArchiveLog_Tab
  Author           : Benny.Chia 
  Date             : 31 Mar 2014
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    --------------------------------------------------------------------------
    -- Column Attributes
    --------------------------------------------------------------------------
    SetupName					NVARCHAR(40) NOT NULL                        
 	, BatchExecutionId			CHAR(16) NULL  
	, PurgeType					NVARCHAR(30) NULL 
	, TxnDate					DATETIME 
	, Version					NVARCHAR(255)  NULL
	, ArchiveDBName			    NVARCHAR(40) NOT NULL 
	, ArchiveSchemaName			NVARCHAR(40) NOT NULL 
	);
    --------------------------------------------------------------------------
GO
