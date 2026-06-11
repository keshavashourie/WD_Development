ALTER PROCEDURE CSI_PurgeUtil_Global_TableColumnExists 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_Global_TableColumnExists
                     Check if the physical table column exists in the table.
  Author           : Benny.Chia 
  Date             : 01 Apr 2014
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvFound         INT OUTPUT 
	, @pvTableName     NVARCHAR(40)
	, @pvColumnName    NVARCHAR(40)
	, @pvSourceDatabase	NVARCHAR(128)
	, @pvSourceSchema	NVARCHAR(128)
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
DECLARE @vSqlStatement				NVARCHAR(1000);
--
DECLARE  @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START';  
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		IF @pvTableName IS NULL
		BEGIN
			SET @ErrorMessage = 'Table must not be blank' 
			RAISERROR (@ErrorMessage, 16, 1);
		END
		IF @pvColumnName IS NULL 
		BEGIN
			SET @ErrorMessage = 'Column must not be blank';
			RAISERROR (@ErrorMessage, 16, 1);
		END
		---------------------------------------------------------------------------
	    BEGIN TRY 
		SET @vSqlStatement = N'
			SELECT @pvFound = COUNT(1) FROM ' + @pvSourceDatabase + '.sys.columns T
			WHERE UPPER(t.name) = UPPER(''' + @pvColumnName + ''') 
			    AND UPPER(t.object_id) = UPPER(OBJECT_ID('''+@pvSourceDatabase + '.' + @pvSourceSchema + '.' + @pvTableName +'''));'
			EXEC sp_executesql @vSqlStatement, N'@pvFound INT OUTPUT', @pvFound OUTPUT;
	    END TRY
		BEGIN CATCH
			SET @ErrorMessage = ERROR_MESSAGE(); 
			SET @vProgID = @vObject_Name + '.' + 'Existence check on column ' + @pvColumnName + ' of table ' + @pvTableName;  
    		RAISERROR (@ErrorMessage, 16, 1);
		END CATCH
        --------------------------------------------------------------------------
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
		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END;
GO
