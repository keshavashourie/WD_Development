------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      UpdateDataStoreTables.sql
-- DESCR:       Updates the Datastore tables in the data store database.  Executes on the ODS
-- HISTORY:
--              11/22/2005                       Initial build. Using HotFix 34088 as the baseline.
--              01/30/2006                       Changes to stored procedures.
--              12/06/2006                       Added/updated copyright notice(s) (SPR S9984) Bill Lippard.
--              04/23/2007                       Updated copyright notice(s) (SPR S9984) Bill Lippard.
--              03/24/2008                       Updated the version number(SPR S12904). Purushotham Neelakantachar
--              06/13/2016      Dan Maloney      Changed variable naming to prefix with data type.  Example @sql Varchar(512) has become @nv_Sql NVARCHAR(4000)
--              07/25/2017      Dan Malney       Added THROW to catch block (US 51393)
--              07/25/2017      Dan Malney       Added ERROR_NUMBER() to @nv_ErrorMessage in catch block (US 51393)
--              07/27/2017      Dan Maloney      Removed unused variables @nv_Err, @nv_Msg, @nv_Sql
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------


DECLARE @i_ErrorNumber          INT 
DECLARE @i_ErrorStatus          INT
DECLARE @i_ErrorSeverity        INT  
DECLARE @i_ErrorState           INT 
DECLARE @v_Version              VARCHAR(10)
DECLARE @nv_ErrorMessage        NVARCHAR(4000)
BEGIN
	SET NOCOUNT ON
	BEGIN TRY
		SET @i_ErrorStatus = 0
		SET @v_Version = NULL

		BEGIN TRAN
			UPDATE DataStoreSetUp SET VALUE = @v_Version WHERE Parameter = 'VERSION'
			UPDATE DataStoreSetUp SET VALUE = '0', DESCRIPTION = 'Number of transactions to process before committing. Used by REPLICATOR process. Default=0.  Not adjustable for SQL Server' WHERE Parameter = 'INSERT_UPDATE_BATCH_SIZE'
		COMMIT TRAN
	END TRY
	BEGIN CATCH
    		SELECT   
        	@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - Line :' + CONVERT(NVARCHAR(10), ERROR_LINE()) + N' - ' + ERROR_MESSAGE(),     
        	@i_ErrorSeverity = ERROR_SEVERITY(),  
        	@i_ErrorState = ERROR_STATE(),
		@i_ErrorNumber = ERROR_NUMBER()

		IF (XACT_STATE()) = -1 OR @@TRANCOUNT > 0
			ROLLBACK TRAN;

    		THROW;
	END CATCH
END
GO
