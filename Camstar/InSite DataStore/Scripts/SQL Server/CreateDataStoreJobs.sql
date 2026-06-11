------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      CreateDataStoreJobs.sql
-- DESCR:       Creates Manager Job, which will manage the other jobs
-- HISTORY:
--
--              06/15/2017      Dan Maloney      Removed block of code that starts the ODS  (US 51393)
--              07/27/2017      Dan Maloney      Removed unused variables @i_WaitTime and @v_Value and @i_ErrorStatus (US 51393)
--              10/23/2019      Dan Maloney      Modify logic to drop existing DataStore Jobs (CPR 1260)
--              12/04/2024      Dan Maloney      Replace MSDB..SYSJOBS with MSDB..SYSJOBS_VIEW
--
-- Copyright Siemens 2024
------------------------------------------------------------------------------------------------------------------------------------------------------

SET NOCOUNT ON

DECLARE @nv_JobName 	NVARCHAR(128)
DECLARE @nv_Msg			NVARCHAR(2048)
DECLARE cur_Jobs CURSOR FOR
SELECT NAME
FROM MSDB..SYSJOBS_VIEW
WHERE NAME LIKE 'csiDataStore%' + DB_NAME() +'%'
BEGIN
	SET NOCOUNT ON
		
	--Drop existing DataStore Jobs
	OPEN cur_Jobs
	FETCH NEXT FROM cur_Jobs INTO @nv_JobName
	WHILE @@FETCH_STATUS = 0
	BEGIN
		BEGIN TRY
			EXEC msdb.dbo.sp_delete_job  @job_name = @nv_JobName 
			FETCH NEXT FROM cur_Jobs INTO @nv_JobName
		END TRY
		BEGIN CATCH
			-- Drop job failed
			SET @nv_Msg = N'Unable to drop ' + @nv_JobName + N' job.  Script CreateDataStoreJobs.sql exiting';
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH
	END
	CLOSE cur_Jobs
	DEALLOCATE cur_Jobs		
		
	--  Start manager job (csiDataStoremanager)
	EXECUTE csiDataStoreVerifyJob @pnv_ProcName = 'csiDataStoreManager'
END
GO

