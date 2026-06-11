ALTER PROCEDURE CSI_PurgeUtil_Global_TableExists 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_Global_TableExists
                     Check if the physical table exists in the database.
  Author           : Benny.Chia 
  Date             : 01 Apr 2014
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvFound        INT OUTPUT 
	, @pvTableName     NVARCHAR(40)
	, @pvSourceDatabase		NVARCHAR(128)
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE  @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
DECLARE @vSqlStatement		NVARCHAR(1000)
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START'; 
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		IF @pvTableName IS NULL 
			RAISERROR ('TableName must not be blank', 16, 1);
		---------------------------------------------------------------------------
	    BEGIN TRY 
			SET @vSqlStatement = N'
			SELECT @pvFound = COUNT(1)
			FROM ' + @pvSourceDatabase + '.dbo.SYSOBJECTS t
			WHERE t.type = ''U''
			    AND UPPER(t.name) = UPPER(''' + @pvTableName + ''');'
			EXEC sp_executesql @vSqlStatement, N'@pvFound INT OUTPUT', @pvFound OUTPUT;
	    END TRY
		BEGIN CATCH
			SET @ErrorMessage = ERROR_MESSAGE(); 
			SET @vProgID = @vObject_Name + '.' + 'Existence check on table' + @pvTableName; 
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
