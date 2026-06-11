ALTER PROCEDURE CSI_PurgeUtil_GetInstance 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_ErrorLog_GetInstance
                    1.1.	Used to return a unique running number depending on the passed in parameter.  
                    1.2.	If the parameter is ‘CSI_PURGEUTIL_SEQ’, then it return the next sequence from CSI_PurgeUtil_Seq sequence object.
                    1.3.	If the parameter is ‘CSI_PURGEUTIL_BATCHEXECUTIONID’, then it return the next sequence from CSI_PurgeUtil_BatchExecutionId sequence object.
                    1.4.	If the parameter is ‘CSI_PURGEUTIL_STEPNO’, then it return the next sequence from CSI_PurgeUtil_StepNo sequence object.
                    1.5.	If the parameter is ‘CSI_PURGEUTIL_MESSAGEID’, then it return the next sequence from CSI_PurgeUtil_MessageId sequence object.
                    1.6.	If the parameter is ‘CSI_PURGEUTIL_RESTOREID’, then it return the next sequence from CSI_PurgeUtil_RestoreId sequence object.
  Author           : Benny.Chia 
  Date             : 31 Mar 2014
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvNextInstanceID        NVARCHAR(16) OUTPUT
	, @pvSequenceName          NVARCHAR(40) = 'CSI_PURGEUTIL_SEQ'
	) 
AS
DECLARE  @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE  @ErrorSeverity              INT;            -- Severity.
DECLARE  @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE  @vJulianDate                NVARCHAR(9);    -- 'YYYY-DDD-'
DECLARE  @vYear                      NVARCHAR(4);    -- 'YYYY'
DECLARE  @vNextValue                 INTEGER=0;
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START'; 
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		IF NOT @pvSequenceName IN ('CSI_PURGEUTIL_SEQ', 'CSI_PURGEUTIL_BATCHEXECUTIONID', 'CSI_PURGEUTIL_STEPNO', 'CSI_PURGEUTIL_MESSAGEID', 'CSI_PURGEUTIL_RESTOREID')
			RAISERROR  ('Parameter @pvSequenceName can only be CSI_PURGEUTIL_SEQ, CSI_PURGEUTIL_BATCHEXECUTIONID or CSI_PURGEUTIL_STEPNO or CSI_PURGEUTIL_MESSAGEID or CSI_PURGEUTIL_RESTOREID', 16, 1);
		--
	    -- Last 7 characters of the sequenceid is a running number.
		SELECT @vNextValue = CAST(SUBSTRING(s.SequenceID, 10, 7) AS INTEGER)
		FROM CSI_PURGEUTIL_SEQUENCE s
		WHERE s.SequenceName = @pvSequenceName;
		--
		IF @vNextValue >= 9999999  -- Reaches its maximum value, so reset to 1.
			SET @vNextValue = 1;
		ELSE
			SET @vNextValue = @vNextValue + 1;
		--
        IF @pvSequenceName = 'CSI_PURGEUTIL_SEQ'
		BEGIN
			SET @vJulianDate = CAST(DATEPART(yyyy, GetDate()) AS NVARCHAR(4)) + '-' + RIGHT('000' + CAST(DATEPART(dy, GetDate()) AS NVARCHAR(3)),3) + '-'
			SET @pvNextInstanceID = @vJulianDate + RIGHT('0000000' + CAST(@vNextValue AS NVARCHAR(7)), 7)
		END;
		ELSE IF @pvSequenceName = 'CSI_PURGEUTIL_BATCHEXECUTIONID'
		BEGIN
			SET @vJulianDate = CAST(DATEPART(yyyy, GetDate()) AS NVARCHAR(4)) + '-' + RIGHT('000' + CAST(DATEPART(dy, GetDate()) AS NVARCHAR(3)),3) + '-'
			SET @pvNextInstanceID = @vJulianDate + RIGHT('0000000' + CAST(@vNextValue AS NVARCHAR(7)), 7)
		END;
		ELSE IF @pvSequenceName = 'CSI_PURGEUTIL_STEPNO'
		BEGIN
			SET @vJulianDate = CAST(DATEPART(yyyy, GetDate()) AS NVARCHAR(4)) + '-' + RIGHT('000' + CAST(DATEPART(dy, GetDate()) AS NVARCHAR(3)),3) + '-'
			SET @pvNextInstanceID = @vJulianDate + RIGHT('0000000' + CAST(@vNextValue AS NVARCHAR(7)), 7)
		END;
		ELSE IF @pvSequenceName = 'CSI_PURGEUTIL_MESSAGEID'
		BEGIN
			SET @vJulianDate = CAST(DATEPART(yyyy, GetDate()) AS NVARCHAR(4)) + '-' + RIGHT('000' + CAST(DATEPART(dy, GetDate()) AS NVARCHAR(3)),3) + '-'
			SET @pvNextInstanceID = @vJulianDate + RIGHT('0000000' + CAST(@vNextValue AS NVARCHAR(7)), 7)
		END;
		ELSE IF @pvSequenceName = 'CSI_PURGEUTIL_RESTOREID'
		BEGIN
			SET @vJulianDate = CAST(DATEPART(yyyy, GetDate()) AS NVARCHAR(4)) + '-' + RIGHT('000' + CAST(DATEPART(dy, GetDate()) AS NVARCHAR(3)),3) + '-'
			SET @pvNextInstanceID = @vJulianDate + RIGHT('0000000' + CAST(@vNextValue AS NVARCHAR(7)), 7)
		END;
        --------------------------------------------------------------------------
		-- Increase the sequenceid by 1	
        --------------------------------------------------------------------------
        BEGIN TRY
		    -- Increase the sequenceid by 1		
		    BEGIN TRANSACTION;
		    UPDATE CSI_PURGEUTIL_SEQUENCE
		    SET SequenceID = @pvNextInstanceID 
            WHERE SequenceName = @pvSequenceName;
		    IF @@TRANCOUNT > 0 
			    COMMIT TRANSACTION;		
	    END TRY
	    BEGIN CATCH
		    IF @@TRANCOUNT > 0 
			    ROLLBACK TRANSACTION;
	        SET @ErrorMessage = ERROR_MESSAGE(); 
	        SET @vProgID = @vObject_Name + '.' + 'Increase sequenceid by 1'; 
		    RAISERROR (@ErrorMessage, 16, 1);
	    END CATCH
        --------------------------------------------------------------------------
	    SET @vProgID = @vObject_Name + '.SUCCESSFULL';  
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
		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END;
GO
