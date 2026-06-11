ALTER PROCEDURE CSI_PurgeUtil_Execute_PurgeByInstance
    ( @pvSetupName                      NVARCHAR(40)
	, @pvInstanceValues					NVARCHAR(MAX)
	) 
AS
DECLARE @vMessage                   NVARCHAR(4000)=''; -- Message text. 
DECLARE @ErrorMessage               NVARCHAR(4000); -- Error Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE @vSQLStatement              NVARCHAR(MAX) = '';
DECLARE @vSetupStatus               NVARCHAR(10);    
DECLARE @pvBatchExecutionId         NVARCHAR(16);   

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
		SET @vProgID = @vObject_Name + '.Validate Archive SQL By Setup';
         IF @pvInstanceValues IS NULL 
			RAISERROR ('Instance Value(s) must not be blank', 16, 1);
		--
		---------------------------------------------------------------------------
        -- Validate if the setup exist.
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate Setup record for existence';
        IF (SELECT COUNT(1) 
            FROM CSI_PURGEUTIL_SETUP ps
            WHERE UPPER(ps.setupname) = UPPER(@pvSetupName)) = 0  
           RAISERROR ('Setup record is not found in the CSI_PURGEUTIL_SETUP table', 16, 1);
		---------------------------------------------------------------------------
		SELECT @vSetupStatus = su.SetupStatus
        FROM CSI_PURGEUTIL_SETUP su 
        WHERE UPPER(su.SetupName) = UPPER(@pvSetupName);   

		SET @vProgID = @vObject_Name + '.Validate Setup Status';
        IF @vSetupStatus IS NULL OR @vSetupStatus <> 'ACTIVE'
			RAISERROR ('Setup Status is not set to ACTIVE', 16, 1);
		---------------------------------------------------------------------------
		SET @vSQLStatement = @vSQLStatement + N'DECLARE @vCSI_PurgeUtil_Instance_Tab CSI_PurgeUtil_Instance_Tab;' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'DECLARE @vCursor CURSOR;' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'DECLARE @vInstanceId as NVARCHAR(32);' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'SET @vCursor = CURSOR FOR SELECT value FROM STRING_SPLIT(''' + @pvInstanceValues + ''', '','')' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'OPEN @vCursor;' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'FETCH NEXT FROM @vCursor INTO @vInstanceId;' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'WHILE @@FETCH_STATUS = 0' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'BEGIN' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'INSERT INTO @vCSI_PurgeUtil_Instance_Tab (InstanceID) VALUES(@vInstanceId)' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'FETCH NEXT FROM @vCursor INTO @vInstanceId;' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'END' + char(13) + char(10);

        SET @vSQLStatement = @vSQLStatement + N'EXECUTE CSI_PurgeUtil_DeleByTable_' + @pvSetupName + ' @pvSetupName = ''' + @pvSetupName + ''', ' + char(13) + char(10);
        SET @vSQLStatement = @vSQLStatement + N' @pvPurgeType = ''PurgeByInstance'', ' + char(13) + char(10);
        SET @vSQLStatement = @vSQLStatement + N' @pvCSI_PurgeUtil_Instance_Tab = @vCSI_PurgeUtil_Instance_Tab;';
        --PRINT @vSQLStatement
        EXECUTE sp_executesql @vSQLStatement;
        --------------------------------------------------------------------------
        --
	    SET @vProgID = @vObject_Name + '.SUCCESSFUL'; 
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
