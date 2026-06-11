------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:     csiDataStoreCheckTerminate.sql
-- DESCR:      Function to return DataStore status.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiDataStoreCheckTerminate' AND Type = 'FN')
	DROP FUNCTION csiDataStoreCheckTerminate
GO

CREATE FUNCTION csiDataStoreCheckTerminate 
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreCheckTerminate
-- Params:      None
-- Returns:	CHAR(1)  'Y' or 'N' 
-- Descr:       Function to return DataStore status. Executes on the ODS	
--		

-- HISTORY:
--      06/25/2016      Dan Maloney     New function
--      07/14/2017      Dan Maloney     Removed unused variables @BIT_FALSE, @BIT_TRUE, @I_LOG_LEVEL_MAX, @I_LOG_LEVEL_MIN (US 51393)
--      07/14/2017      Dan Maloney     Removed unused variables @BI_LOG_LEVEL_ERROR, @i_ErrorNumber, @i_ErrorSeverity, @i_ErrorState (US 51393)
--      07/14/2017      Dan Maloney     Removed unused variables @nv_Err, @nv_ErrorMessage (US 51393)
--
-- Copyright Siemens 2023  
-----------------------------------------------------------------------------------------------------------------------------------------------------
()
RETURNS CHAR(1)
AS
BEGIN
        DECLARE @c_DataStore_Terminate          CHAR(1) 
        DECLARE @nv_Loc                         NVARCHAR(64) 
        DECLARE	@nv_Msg                         NVARCHAR(MAX) 

	SET @nv_Loc = N'csiDataStoreCheckTerminate'
	SET @nv_MSG = N'Get DATASTORE_TERMINATE value'

	SELECT 
	@c_DataStore_Terminate = VALUE 
	FROM DataStoreSetUp WITH(NOLOCK)
	WHERE Parameter = 'DATASTORE_TERMINATE'

	RETURN @c_DataStore_Terminate	
END
GO
