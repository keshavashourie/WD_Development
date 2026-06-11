ALTER PROCEDURE CSI_PurgeUtil_Execute_PurgeBySingleHierarchy
    ( @pvSetupName                      NVARCHAR(40)
	, @pvInstanceId                     NVARCHAR(16)
	, @pvArchiveSQL						VARCHAR(MAX)
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

		SET @vProgID = @vObject_Name + '.Validate Archive SQL By Setup';
         IF @pvArchiveSQL IS NULL 
			RAISERROR ('Archive SQL must not be blank', 16, 1);
		SET @vProgID = @vObject_Name + '.Validate Setup Status';
        IF @vSetupStatus IS NULL OR @vSetupStatus <> 'ACTIVE'
			RAISERROR ('Setup Status is not set to ACTIVE', 16, 1);
		---------------------------------------------------------------------------
		SET @vSQLStatement = @vSQLStatement + N'DECLARE @vCSI_PurgeUtil_Instance_Tab CSI_PurgeUtil_Instance_Tab;' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'DECLARE @vContainerCursor CURSOR;' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'DECLARE @vInstanceId as NVARCHAR(32);' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'SET @vContainerCursor = CURSOR FOR ' + @pvArchiveSQL + ' ' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'OPEN @vContainerCursor;' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'FETCH NEXT FROM @vContainerCursor INTO @vInstanceId;' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'WHILE @@FETCH_STATUS = 0' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'BEGIN' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'INSERT INTO @vCSI_PurgeUtil_Instance_Tab (InstanceID) VALUES(@vInstanceId)' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'FETCH NEXT FROM @vContainerCursor INTO @vInstanceId;' + char(13) + char(10);
		SET @vSQLStatement = @vSQLStatement + N'END' + char(13) + char(10);


		--EXEC sp_executesql @vInstanceIdSQLStatement;

		--OPEN @vContainerCursor;
		--	FETCH NEXT FROM @vContainerCursor INTO @vInstanceId;
		--	WHILE @@FETCH_STATUS = 0
		--		BEGIN
		--		SET @vSQLStatement = @vSQLStatement + 'INSERT INTO @vCSI_PurgeUtil_Instance_Tab (InstanceID) VALUES(' + @vInstanceId + '); ' + char(13) + char(10);
		--		FETCH NEXT FROM @vContainerCursor INTO @vInstanceId;
		--END

        --SET @vSQLStatement = @vSQLStatement + 'EXECUTE CSI_PurgeUtil_DeleByTable_' + @pvSetupName + ' @pvSetupName = ''' + @pvSetupName + ''', ' + char(13) + char(10);
        --SET @vSQLStatement = @vSQLStatement + ' @pvCSI_PurgeUtil_Instance_Tab = @vCSI_PurgeUtil_Instance_Tab;';
        SET @vSQLStatement = @vSQLStatement + 'EXECUTE CSI_PurgeUtil_DeleByTable_' + @pvSetupName + ' @pvSetupName = ''' + @pvSetupName + ''', ' + char(13) + char(10);
        SET @vSQLStatement = @vSQLStatement + ' @pvPurgeType = ''PurgeBySetup'', ' + char(13) + char(10);
        SET @vSQLStatement = @vSQLStatement + ' @pvCSI_PurgeUtil_Instance_Tab = @vCSI_PurgeUtil_Instance_Tab;';
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
