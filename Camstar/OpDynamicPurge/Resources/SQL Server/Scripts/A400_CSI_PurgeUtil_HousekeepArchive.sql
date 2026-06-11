ALTER PROCEDURE CSI_PurgeUtil_HousekeepArchive 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_HousekeepArchive
                     Delete archive records permanently.
  Author           : Benny.Chia 
  Date             : 28 Sep 2015
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvArchivedb                  NVARCHAR(40)
	, @pvArchiveSchemaName			NVARCHAR(40)
	, @pvArchiveRetentionPeriod		INT 
	) 
AS
DECLARE @vMessage                   NVARCHAR(4000)=''; -- Message text.
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE	@vTableDictStmt				  nvarchar(max);
DECLARE	@vTablename					  NVARCHAR(30)=''; 
DECLARE @vArchiveTableCount           INT=0;
DECLARE @vDeleteArchiveRecordsStmt    NVARCHAR(4000);
BEGIN 
	SET NOCOUNT ON;
	--SET XACT_ABORT ON;
	SET @vProgID = @vObject_Name + '.START(ArchiveRetentionPeriod=' + CAST(@pvArchiveRetentionPeriod AS NVARCHAR(10)) + ')';  
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		IF @pvArchivedb IS Null 
			RAISERROR ('Archive DB must not be blank', 16, 1);
		IF @pvArchiveSchemaName IS Null 
			RAISERROR ('Archive Schema Name must not be blank', 16, 1);
		IF @pvArchiveRetentionPeriod IS Null 
			RAISERROR ('Archive Retention Period must not be blank', 16, 1);
		---------------------------------------------------------------------------
		---------------------------------------------------------------------------
        -- Log a start message 
        SET @vMessage = 'Deletion of archive records started. '; EXECUTE CSI_PurgeUtil_Global_LogMessage @vMessage;
	    BEGIN TRY 
			SET @vTableDictStmt = 
				'DECLARE curArchiveTable CURSOR FOR 
				SELECT Tablename   = so.name  
				FROM ' + @pvArchivedb + '.SYS.objects So 
				WHERE SO.TYPE =''U'' 
                ORDER BY so.name
				';
			EXECUTE sp_executesql @vTableDictStmt;	
		END TRY
		BEGIN CATCH
			SET @ErrorMessage = ERROR_MESSAGE();
			SET @vProgID = @vObject_Name + '.' + 'Check table dictionary in the archive database and schema.'; 
    		RAISERROR (@ErrorMessage, 16, 1);
		END CATCH
		--
		IF CURSOR_STATUS('local','curArchiveTable') >= -1
		BEGIN
			CLOSE curArchiveTable;
			DEALLOCATE curArchiveTable;
		END;
		--
		OPEN curArchiveTable;
		FETCH NEXT FROM curArchiveTable
		INTO @vTableName;
		--
		WHILE @@FETCH_STATUS = 0
		BEGIN
			BEGIN TRANSACTION;
			BEGIN TRY
				--PRINT 'Table ' + CAST(@vArchiveTableCount AS NVARCHAR(10)) + ' => ' + @vTablename;
				--
				SET @vArchiveTableCount = @vArchiveTableCount + 1;
				SET @vDeleteArchiveRecordsStmt =  ' DELETE FROM ' + @pvArchivedb + '.' + @pvArchiveSchemaName + '.' + @vTablename;
			    SET @vDeleteArchiveRecordsStmt += ' WHERE purge_datetime < (GetDate() - ' + CAST(@pvArchiveRetentionPeriod AS NVARCHAR(10)) + ');'; 
				EXECUTE sp_executesql @vDeleteArchiveRecordsStmt;	
				--PRINT @vDeleteArchiveRecordsStmt;
				--PRINT 'Table ' + CAST(@vArchiveTableCount AS NVARCHAR(10)) + ' : Deleted ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' records in ' + @pvArchivedb + '.' + @pvArchiveSchemaName + '.' + @vTablename;
				--
				SET @vProgID = @vObject_Name + '.Table ' + CAST(@vArchiveTableCount AS NVARCHAR(10)) + ' : Deleted ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' records in ' + @pvArchivedb + '.' + @pvArchiveSchemaName + '.' + @vTablename; EXECUTE CSI_PurgeUtil_Global_Log @pvProgID=@vProgID; 
				--
				IF @@TRANCOUNT > 0  
					COMMIT TRANSACTION;
			END TRY
			BEGIN CATCH
				SET @ErrorMessage = ERROR_MESSAGE(); 
				SET @vProgID = @vObject_Name + '.' + '@@vDeleteArchiveRecordsStmt=' + @vDeleteArchiveRecordsStmt;
        		RAISERROR (@ErrorMessage, 16, 1);
			END CATCH;
			---------------------------------------------------------------
			-- Get the next table to delete
			---------------------------------------------------------------
			FETCH NEXT FROM curArchiveTable
			INTO @vTableName;
		END;  -- Column loop
		CLOSE curArchiveTable;
		DEALLOCATE curArchiveTable;
		--
        --------------------------------------------------------------------------
        -- Log a completion message 
        SET @vMessage = 'Deletion of archive records completed. '; EXECUTE CSI_PurgeUtil_Global_LogMessage @vMessage;
        --
	    SET @vProgID = @vObject_Name + '.SUCCESSFUL'; 
		RETURN 0;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 
			ROLLBACK TRANSACTION;
		IF @ErrorMessage IS NULL
		    SET @ErrorMessage  = ERROR_MESSAGE();
		IF @ErrorSeverity IS NULL
		    SET @ErrorSeverity = ERROR_SEVERITY(); 
		IF @ErrorState IS NULL
		    SET @ErrorState = ERROR_STATE();
		IF @vProgID IS NULL
		    SET @vProgID = @vObject_Name + '.OTHER ERROR'; 
		EXECUTE CSI_PurgeUtil_Global_Log @pvProgID=@vProgID, @pvErrMsg=@ErrorMessage; 
		IF CURSOR_STATUS('local','curArchiveTable') >= -1
		BEGIN
			CLOSE curArchiveTable;
			DEALLOCATE curArchiveTable;
		END;
		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END;
GO
