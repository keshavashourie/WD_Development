-- CSI_PurgeUtil_Global_LogRunStat() uses CSI_PurgeUtil_RunLog_Tab Type.  So this SP has to be drop first before dropping the TYPE definition.
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Global_LogRunStat' AND type = 'P')
    DROP PROCEDURE CSI_PurgeUtil_Global_LogRunStat;
IF EXISTS (SELECT name FROM sysobjects WHERE name LIKE 'TT_CSI_PurgeUtil_RunLog_Tab%' AND type = 'TT')
    DROP TYPE CSI_PurgeUtil_RunLog_Tab;
GO
CREATE TYPE CSI_PurgeUtil_RunLog_Tab AS TABLE ( 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_RunLog_Tab
  Author           : Benny.Chia 
  Date             : 23 Feb 2016
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    --------------------------------------------------------------------------
    -- Column Attributes
    --------------------------------------------------------------------------
    RowID                                 int IDENTITY(1,1) PRIMARY KEY
    , SetupName                           nvarchar(40)
    , BatchExecutionId                    char(16)
    , RestoreId                           char(16)
    , RunStage                            nvarchar(40)
    , Action                              nvarchar(255)
    , TableName                           nvarchar(30)
	, gLevel                              integer
    , RecordsAffected                     integer
    , ExecutionTime                       float
    , creation_datetime                   datetime
    , SQLStatement                        nvarchar(max)
	);
    --------------------------------------------------------------------------
GO
