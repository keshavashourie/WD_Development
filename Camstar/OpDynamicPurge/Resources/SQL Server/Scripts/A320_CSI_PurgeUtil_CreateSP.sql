ALTER PROCEDURE CSI_PurgeUtil_CreateSP 
    ( @pvRowData_Tab                       CSI_PurgeUtil_RowData_Tab READONLY
	) 
AS
DECLARE @ErrorMessage                NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE @vData                      NVARCHAR(4000);
DECLARE @vRowType                   NVARCHAR(16);
DECLARE @vRowsToProcess             INTEGER;
DECLARE @vCurrentRow                INTEGER=0;
DECLARE @vS00 NVARCHAR(MAX)=''; 
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START'; 
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
        -- Find out the number of rows in the table @vScriptsDELEData_tab.
        SELECT @vRowsToProcess = COUNT(RowID)  
        FROM @pvRowData_Tab; 
        --PRINT '@vRowsToProcess : ' + STR(@vRowsToProcess);
        --
        SET @vCurrentRow = 0;
        SET @vS00 = '';
        WHILE @vCurrentRow < @vRowsToProcess
        BEGIN
            SET @vCurrentRow = @vCurrentRow + 1;
            SELECT DISTINCT
                @vData = IsNull(RowData, '')
                , @vRowType = RowType
            FROM @pvRowData_Tab
            WHERE RowID = @vCurrentRow;
            SET @vS00 = @vS00 + @vData;
            --PRINT @vData;
        END
        EXECUTE sp_executesql @vS00;
		--
		---------------------------------------------------------------------------
	    SET @vProgID = @vObject_Name + '.SUCCESSFULL'; 
		RETURN 0;
	END TRY
	BEGIN CATCH
		IF @ErrorMessage IS NULL
		    SET @ErrorMessage  = ERROR_MESSAGE();
		IF @ErrorSeverity IS NULL
		    SET @ErrorSeverity = ERROR_SEVERITY(); 
		IF @ErrorState IS NULL
		    SET @ErrorState = ERROR_STATE();
		IF @vProgID IS NULL
		    SET @vProgID = @vObject_Name + '.OTHER ERROR'; 
		EXECUTE CSI_PurgeUtil_Global_Log @pvProgID=@vProgID, @pvErrMsg=@ErrorMessage; 
        -- Print out the generated SP codes
        SET @vCurrentRow = 0;
        WHILE @vCurrentRow < @vRowsToProcess
        BEGIN
            SET @vCurrentRow = @vCurrentRow + 1;
            SELECT DISTINCT
                @vData = IsNull(RowData, '')
                , @vRowType = RowType
            FROM @pvRowData_Tab
            WHERE RowID = @vCurrentRow;
            PRINT @vData;
        END


		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END;
GO
