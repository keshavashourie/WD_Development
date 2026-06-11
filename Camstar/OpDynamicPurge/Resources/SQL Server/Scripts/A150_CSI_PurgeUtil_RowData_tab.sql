IF EXISTS (SELECT name FROM sysobjects WHERE name LIKE 'CSI_PurgeUtil_CreateSP' AND type = 'P')
    DROP PROCEDURE CSI_PurgeUtil_CreateSP;
GO
IF EXISTS (SELECT name FROM sysobjects WHERE name LIKE 'TT_CSI_PurgeUtil_RowData_Tab%' AND type = 'TT')
    DROP TYPE CSI_PurgeUtil_RowData_Tab;
GO
CREATE TYPE CSI_PurgeUtil_RowData_Tab AS TABLE ( 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_RowData_Tab
  Author           : Benny.Chia 
  Date             : 15 Sep 2015
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */    
    --------------------------------------------------------------------------
    -- Column Attributes
    --------------------------------------------------------------------------
	RowID                               int IDENTITY(1,1) PRIMARY KEY
    , RowData                           NVARCHAR(MAX)
    , RowType                           NVARCHAR(16) DEFAULT 'NORMAL'  -- Future use.
	);
    --------------------------------------------------------------------------
GO
