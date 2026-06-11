ALTER   PROCEDURE CSI_PurgeUtil_CheckInstanceExist
    ( @pvSetupName                      NVARCHAR(40)
	, @pvInstanceId                     NVARCHAR(16)
	) 
AS
	DECLARE @vMainTable VARCHAR(300)
	DECLARE @vMainTableInstanceColName VARCHAR(300)
	DECLARE @vSQLStatement NVARCHAR(4000) = '';
	DECLARE @InstanceCount INT;
	DECLARE @vMessage                   NVARCHAR(4000)=''; -- Message text.
	DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
	DECLARE @ErrorSeverity              INT;            -- Severity.
	DECLARE @ErrorState                 INT;            -- State.
	DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
	DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;


	BEGIN
		SET NOCOUNT ON;
		SET @vProgID = @vObject_Name + '.START'; 
		BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
			---------------------------------------------------------------------------
			-- Validations
			---------------------------------------------------------------------------
			SET @vProgID = @vObject_Name + '.Validate parameters';
			IF @pvSetupName IS Null 
				RAISERROR ('Setup Name must not be blank', 16, 1);
			IF @pvInstanceId IS Null 
				RAISERROR ('InstanceId must not be blank', 16, 1);
			---------------------------------------------------------------------------
			-- Validate if the setup exist.
			---------------------------------------------------------------------------
			SET @vProgID = @vObject_Name + '.Validate Setup record for existence';
			IF (SELECT COUNT(1) 
				FROM CSI_PURGEUTIL_SETUP ps
				WHERE UPPER(ps.setupname) = UPPER(@pvSetupName)) = 0  
				RAISERROR ('Setup record is not found in the CSI_PURGEUTIL_SETUP table', 16, 1);
			--
			---------------------------------------------------------------------------
			SELECT @vMainTable = MainTableName,
			@vMainTableInstanceColName = MainTableInstanceCol
			FROM CSI_PURGEUTIL_SETUP ps WHERE UPPER(ps.setupname) = UPPER(@pvSetupName)
			---------------------------------------------------------------------------
			SET @vSQLStatement = 'SELECT @Count = COUNT(*) FROM ' + @vMainTable + ' WHERE ' + @vMainTableInstanceColName + ' = ''' + @pvInstanceId + ''''
			EXECUTE sp_executesql @vSQLStatement, N'@Count INT OUTPUT', @Count = @InstanceCount OUTPUT;

			--------------------------------------------------------------------------
			--
			SET @vProgID = @vObject_Name + '.SUCCESSFUL'; 
			RETURN @InstanceCount;
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


