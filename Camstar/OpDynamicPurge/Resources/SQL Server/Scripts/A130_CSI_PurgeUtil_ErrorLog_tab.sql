-- CSI_PurgeUtil_ErrorLog_Record() uses CSI_PurgeUtil_ErrorLog_Tab Type.  So this SP has to be drop first before dropping the TYPE definition.
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_ErrorLog_Record' AND type = 'P')
    DROP PROCEDURE CSI_PurgeUtil_ErrorLog_Record;
IF EXISTS (SELECT name FROM sysobjects WHERE name LIKE 'TT_CSI_PurgeUtil_ErrorLog_Tab%' AND type = 'TT')
    DROP TYPE CSI_PurgeUtil_ErrorLog_Tab;
GO
CREATE TYPE CSI_PurgeUtil_ErrorLog_Tab AS TABLE ( 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_ErrorLog_Tab
  Author           : Benny.Chia 
  Date             : 31 Mar 2014
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    --------------------------------------------------------------------------
    -- Column Attributes
    --------------------------------------------------------------------------
    modulename                          nvarchar(40)
    , moduletype                          nvarchar(40)
    , creation_datetime                   datetime
    , progid                              nvarchar(255)
    , ErrMsg                              nvarchar(4000)
    , ErrCode                             nvarchar(255)
    , containerid                         char(16) null
    , resourceid                          char(16) null
	);
    --------------------------------------------------------------------------
GO
