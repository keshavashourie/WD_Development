ALTER PROCEDURE CSI_PurgeUtil_ErrorLog_Record 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_ErrorLog_Record
                     Insert a record into the error table.
  Author           : Benny.Chia 
  Date             : 31 Mar 2014
  Compile in       : Source schema
  Called By        : CSI_PurgeUtil_Global_Log()
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvCSI_PurgeUtil_ErrorLog_Tab     CSI_PurgeUtil_ErrorLog_Tab READONLY
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @iDurationOfLogData          INT=30;  -- in terms of days
DECLARE @iNoOfHistoricalRecs         INT=0;
--
DECLARE @vContainerId                CHAR(16);
DECLARE @vResourceId                 CHAR(16);
--
DECLARE @vModuleVersion              NVARCHAR(15);
DECLARE @pvNextInstanceID           NVARCHAR(16);
BEGIN 
	SET NOCOUNT ON;
	--PRINT 'CSI_PurgeUtil_ErrorLog_Record.START';
    --PRINT '@@TRANCOUNT(CSI_PurgeUtil_ErrorLog_Record):' + STR(@@TRANCOUNT);
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
        --
		---------------------------------------------------------------------------
	    BEGIN TRY 
			-----------------------------------------------------------------------
			-- Get ModuleVersion
			-----------------------------------------------------------------------
			BEGIN TRY
				SELECT @vModuleVersion = i.moduleversion
				FROM CSI_PURGEUTIL_INSTALLATION i 
				WHERE i.dateinstalled = (select max(i1.dateinstalled)  
									     from CSI_PURGEUTIL_INSTALLATION i1);
				SET @vModuleVersion = IsNull(@vModuleVersion, '00.00.00');
			END TRY
			BEGIN CATCH
				SET @vModuleVersion = '00.00.00';
			END CATCH
			--
            --PRINT 'CSI_PurgeUtil_ErrorLog_Record.Get ModuleVersion';
			BEGIN TRANSACTION
            SAVE TRANSACTION SavePoint1;
            EXECUTE CSI_PurgeUtil_GetInstance @pvNextInstanceID OUTPUT, 'CSI_PURGEUTIL_STEPNO';
            --PRINT '@pvNextInstanceID=' + @pvNextInstanceID;
			INSERT INTO CSI_PURGEUTIL_ERRORLOG (
				modulename
				, moduleversion
				, moduletype
				, creation_datetime
				, StepNo
				, progId
				, ErrMsg
				, ErrCode
				, containerid
				, resourceid
				) 
			SELECT IsNull(modulename, 'No Module')
			     , IsNull(@vModuleVersion, 'No Version')
				 , 'EXECUTION' --IsNull(moduletype, 'EXECUTION')
				 , IsNull(creation_datetime, CURRENT_TIMESTAMP)  --  SYSDATETIME()
				 , IsNull(@pvNextInstanceID, '0000000000000000')
				 , progId
				 , ErrMsg
  				 , ErrCode
				 , containerid
				 , resourceid
			FROM @pvCSI_PurgeUtil_ErrorLog_Tab;
            --
		    IF @@TRANCOUNT > 0
			    COMMIT TRANSACTION;
 	    END TRY
		BEGIN CATCH
		    IF @@TRANCOUNT > 0
		        ROLLBACK TRANSACTION;
			-- Use RAISERROR inside the CATCH block to return error information about the original error that caused execution to jump to the CATCH block.
			SET @ErrorMessage = ' INSERT INTO CSI_PURGEUTIL_ERRORLOG ERROR : '; --SET @ErrorSeverity = ERROR_SEVERITY(); SET @ErrorState = ERROR_STATE();
			RAISERROR (@ErrorMessage, 16, 1);
		END CATCH
        BEGIN TRY
		    ----------------------------------------------------------------------
		    -- Housekeep this table based on date of historical records (keep for <iDurationOfLogData> days)
		    ----------------------------------------------------------------------
		    SELECT @iNoOfHistoricalRecs = COUNT(*) FROM CSI_PURGEUTIL_ERRORLOG
		    WHERE CREATION_DATETIME < (GetDate() - @iDurationOfLogData);    
		    IF @iNoOfHistoricalRecs > 0  -- found
            BEGIN
			    BEGIN TRANSACTION
                SAVE TRANSACTION SavePoint1;
			    DELETE FROM CSI_PURGEUTIL_ERRORLOG 
			    WHERE CREATION_DATETIME < (GetDate() - @iDurationOfLogData)
			    ;
    			IF @@TRANCOUNT > 0
	    			COMMIT TRANSACTION;
			END
			--
        END TRY    
		BEGIN CATCH
			SET @ErrorMessage = ' DELETE FROM CSI_PURGEUTIL_ERRORLOG ERROR : ' + ERROR_MESSAGE(); SET @ErrorSeverity = ERROR_SEVERITY(); SET @ErrorState = ERROR_STATE();
			RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
		END CATCH
        --------------------------------------------------------------------------
		RETURN 0;                   
	END TRY
	BEGIN CATCH
        IF XACT_STATE() = -1 -- Transaction is doomed, Rollback everything.
            ROLLBACK TRANSACTION;
        IF XACT_STATE() = 1 --Transaction is commitable, we can rollback to a save point
            ROLLBACK TRANSACTION SavePoint1 ; 
  		-- Use RAISERROR inside the CATCH block to return error information about the original error that caused execution to jump to the CATCH block.
		IF @ErrorMessage IS NULL
		    SET @ErrorMessage  = ERROR_MESSAGE();
		SET @ErrorSeverity = ERROR_SEVERITY(); SET @ErrorState = ERROR_STATE();
		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END;
GO