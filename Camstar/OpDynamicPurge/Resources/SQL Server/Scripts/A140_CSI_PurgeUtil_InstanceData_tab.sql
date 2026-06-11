--The following sql statement will display a list of stored procedures that are using this table type as parameter.
SELECT o.name, o.type, p.name
FROM sys.parameters p
INNER JOIN sys.objects o ON o.object_id = p.object_id AND o.type = 'P' -- stored procedure
WHERE p.name = '@pvCSI_PurgeUtil_Instance_Tab';
--
-- CSI_PurgeUtil_Dele_tpl() uses CSI_PurgeUtil_Instance_Tab Type.  So this SP has to be drop first before dropping the TYPE definition.
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_Dele_tpl' AND type = 'P')
    DROP PROCEDURE CSI_PurgeUtil_Dele_tpl;
--
IF EXISTS (SELECT name FROM sysobjects WHERE name = 'CSI_PurgeUtil_DeleByTable_tpl' AND type = 'P')
    DROP PROCEDURE CSI_PurgeUtil_DeleByTable_tpl;
--
-- Due to User View referecing CSI_PurgeUtil_Instance_Tab table type, this will not be drop and recreate again 
IF NOT EXISTS (SELECT name FROM sysobjects WHERE name LIKE 'TT_CSI_PurgeUtil_Instance_Tab%' AND type = 'TT')
    CREATE TYPE CSI_PurgeUtil_Instance_Tab AS TABLE ( 
    /* ---------------------------------------------------------------------------
      Description      : CSI_PurgeUtil_Instance_Tab
      Author           : Benny.Chia 
      Date             : 31 Mar 2014
      Compile in       : Source schema
      Called By        : 
      Call             : None
    --------------------------------------------------------------------------- */    
        --------------------------------------------------------------------------
        -- Column Attributes
        --------------------------------------------------------------------------
        RowID                               int IDENTITY(1,1) PRIMARY KEY
        , InstanceID                             char(16) UNIQUE
        );
        --------------------------------------------------------------------------
GO
