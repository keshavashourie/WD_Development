ALTER PROCEDURE CSI_PurgeUtil_TableSetup_UpdateSetupTable 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_TableSetup_UpdateSetupTable
                     Insert/Update a purging setup table in the CSI_PURGEUTIL_SETUPTABLES table.
  Author           : Benny.Chia 
  Date             : 03 Apr 2014
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvSetupName					NVARCHAR(40)
	, @pvTableName					NVARCHAR(30)
	, @pvInstanceCol1A				NVARCHAR(30)
	, @pvInstanceCol1B				NVARCHAR(30)
	, @pvInstanceCol1C				NVARCHAR(30)
	, @pvInstanceCol2				NVARCHAR(30)
	, @pvInstanceParentCol			NVARCHAR(30) 
	, @pvParentTable				NVARCHAR(30) 
	, @pvParentTableLinkCol         NVARCHAR(30) 
	, @pvCommitBatchSize            INTEGER = 1000
    , @pvSQLQueryPredicate          NVARCHAR(40)
	, @pvSourceDatabase				NVARCHAR(128)
	, @pvSourceSchema				NVARCHAR(128)
	, @pvTableLevel					INTEGER
	, @pvRemarks                    NVARCHAR(1000)
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
DECLARE @vFound                     INT=0;
--
DECLARE  @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
DECLARE @pvNextInstanceID           NVARCHAR(16);
--
DECLARE @vSetupId					NVARCHAR(16);
DECLARE @vSetupTablesId				NVARCHAR(16);
DECLARE @vRootTable                 NVARCHAR(30);
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START';
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		IF @pvSetupName IS Null 
			RAISERROR ('SetupName must not be blank', 16, 1);
		--
		SET @vFound = 0;
        EXEC CSI_PurgeUtil_TableSetup_ValidateSetupTable @vFound OUTPUT, @pvSetupName, @pvTableName, @pvTableLevel, @pvParentTable;
        IF @vFound = 1 
        BEGIN
            SET @ErrorMessage = 'Duplicate found for table: ' + @pvTableName + ', with table level ' + CAST(@pvTableLevel AS NVARCHAR) + '.';
            RAISERROR (@ErrorMessage, 16, 1);
        END;
        IF @vFound = 2
        BEGIN
            SET @ErrorMessage = 'Recursive relationship not supported.';
            RAISERROR (@ErrorMessage, 16, 1);
        END;
		--
        SET @vFound = 0; 
		EXEC CSI_PurgeUtil_Global_TableExists @vFound OUTPUT, @pvTableName, @pvSourceDatabase;
		IF @vFound = 0 
		BEGIN
		    SET @vProgID = @vObject_Name + '.Check Table ' + @pvTableName + ' for existence. ';
			SET @ErrorMessage = 'Table ' + @pvTableName + ' does not exist';
			RAISERROR (@ErrorMessage, 16, 1);
		END
		IF LEN(@pvInstanceCol2) = 0
		    SET @pvInstanceCol2 = Null;
		--
        SET @vFound = 0; 
	    EXEC CSI_PurgeUtil_Global_TableColumnExists @vFound OUTPUT, @pvTableName, @pvInstanceCol1A, @pvSourceDatabase, @pvSourceSchema;
	    IF @vFound = 0 
	    BEGIN
	        SET @vProgID = @vObject_Name + '.Check Instance Column1A ' + @pvInstanceCol1A + ' for existence in table ' + @pvTableName + '. ';
		    SET @ErrorMessage = 'Instance Column 1A' + @pvInstanceCol1A + ' does not exist in table ' + @pvTableName;
		    RAISERROR (@ErrorMessage, 16, 1);
	    END;
	    IF @pvInstanceCol1B IS NOT NULL
	    BEGIN
            SET @vFound = 0; 
	        EXEC CSI_PurgeUtil_Global_TableColumnExists @vFound OUTPUT, @pvTableName, @pvInstanceCol1B, @pvSourceDatabase, @pvSourceSchema;
	        IF @vFound = 0 
	        BEGIN
	            SET @vProgID = @vObject_Name + '.Check Instance Column1B ' + @pvInstanceCol1B + ' for existence in table ' + @pvTableName + '. ';
		        SET @ErrorMessage = 'Instance Column1B ' + @pvInstanceCol1B + ' does not exist in table ' + @pvTableName;
		        RAISERROR (@ErrorMessage, 16, 1);
	        END;
        END
	    IF @pvInstanceCol1C IS NOT NULL
	    BEGIN
            SET @vFound = 0; 
	        EXEC CSI_PurgeUtil_Global_TableColumnExists @vFound OUTPUT, @pvTableName, @pvInstanceCol1C, @pvSourceDatabase, @pvSourceSchema;
	        IF @vFound = 0 
	        BEGIN
	            SET @vProgID = @vObject_Name + '.Check Instance Column1C ' + @pvInstanceCol1C + ' for existence in table ' + @pvTableName + '. ';
		        SET @ErrorMessage = 'Instance Column1C ' + @pvInstanceCol1C + ' does not exist in table ' + @pvTableName;
		        RAISERROR (@ErrorMessage, 16, 1);
	        END;
        END
	    IF @pvInstanceCol2 IS NOT NULL
	    BEGIN
            SET @vFound = 0; 
	        EXEC CSI_PurgeUtil_Global_TableColumnExists @vFound OUTPUT, @pvTableName, @pvInstanceCol2, @pvSourceDatabase, @pvSourceSchema;
	        IF @vFound = 0 
	        BEGIN
	            SET @vProgID = @vObject_Name + '.Check Instance Column2 ' + @pvInstanceCol2 + ' for existence in table ' + @pvTableName + '. ';
		        SET @ErrorMessage = 'Instance Column2 ' + @pvInstanceCol2 + ' does not exist in table ' + @pvTableName;
		        RAISERROR (@ErrorMessage, 16, 1);
	        END;
        END
		--
		-- If this is the root level table, Instance Parent Column and Parent Table are not needed and therefore, can be updated as Null.
        SELECT @vRootTable = t.MainTableName 
        FROM CSI_PURGEUTIL_SETUP t 
        WHERE UPPER(t.SetupName) = UPPER(@pvSetupName);
        --
        --PRINT @vRootTable;
        IF UPPER(@pvTableName) = UPPER(@vRootTable) 
		BEGIN
			IF @pvInstanceParentCol IS NOT NULL OR @pvParentTable IS NOT NULL
			BEGIN
		        SET @vProgID = @vObject_Name + '.Verify both Instance Parent Column and Parent Table must be blank for root table ' + @pvTableName + '. ';
				SET @ErrorMessage = 'Both Instance Parent Column and Parent Table must be blank for root table ' + @pvTableName ;
				RAISERROR (@ErrorMessage, 16, 1);
			END;
		END;
		ELSE IF @pvInstanceParentCol IS NULL -- 2nd level and greater needs linkage using Instance Parent Column and Parent Table
		BEGIN
		    SET @vProgID = @vObject_Name + '.Verify Instance Parent Column is required for table ' + @pvTableName + '. ';
			SET @ErrorMessage = 'Instance Parent Column is required for this table (2nd level or greater table) ' + @pvTableName + '. ';
			RAISERROR (@ErrorMessage, 16, 1);
		END;
		ELSE 
		BEGIN
			--PRINT 'SUBSTRING(@pvInstanceParentCol';
			--IF SUBSTRING(@pvInstanceParentCol,1,1) = '('  -- If the first character is '(', then do not validate Instance Parent Column.
				SELECT Null; -- This is a sql statement eg: (SELECT containerid FROM Container WHERE containername = LotNo) AND ROWNUM=1)
			--ELSE
			--BEGIN
				SET @vFound = 0; 
				EXEC CSI_PurgeUtil_Global_TableColumnExists @vFound OUTPUT, @pvTableName, @pvInstanceParentCol, @pvSourceDatabase, @pvSourceSchema;
				IF @vFound = 0 
				BEGIN
				    SET @vProgID = @vObject_Name + '.Check Instance Parent Column ' + @pvInstanceParentCol + ' for existence for table ' + @pvTableName + '. ';
					SET @ErrorMessage = 'Instance Parent Column ' + @pvInstanceParentCol + ' does not exist in table ' + @pvTableName;
					RAISERROR (@ErrorMessage, 16, 1);
				END;
			--END;
			SET @vFound = 0; 
			EXEC CSI_PurgeUtil_Global_TableExists @vFound OUTPUT, @pvParentTable, @pvSourceDatabase;
			IF @vFound = 0 
			BEGIN
			    SET @vProgID = @vObject_Name + '.Check Parent Table ' + IsNull(@pvParentTable, '') + ' for existence. ';
				SET @ErrorMessage = 'Parent Table ' + @pvParentTable + ' does not exist';
				RAISERROR (@ErrorMessage, 16, 1);
			END;
		END;
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Check Setup ' + @pvSetupName + ' for existence. ';
        SET @vFound = 0; 
		EXEC CSI_PurgeUtil_TableSetup_SetupExists @vFound OUTPUT, @pvSetupName;
       	IF @vFound = 0 
		BEGIN
			SET @ErrorMessage = 'Setup ' + @pvSetupName + ' does not exist';
			RAISERROR (@ErrorMessage, 16, 1);
		END;
		---------------------------------------------------------------------------
		-- Check whether the setuptable already exist
		---------------------------------------------------------------------------
	    BEGIN TRY 
			BEGIN TRANSACTION;
			EXECUTE CSI_PurgeUtil_GetInstance @pvNextInstanceID OUTPUT, 'CSI_PURGEUTIL_SEQ';
			INSERT INTO CSI_PurgeUtil_SetupTables
			(
				SetupTablesId
				, SetupId
				, TableName
				, InstanceCol1A
				, InstanceCol1B
				, InstanceCol1C
				, InstanceCol2
				, InstanceParentCol
				, ParentTable
				, ParentTableLinkCol
				, TxnDate
				, CommitBatchSize
				, SQLQueryPredicate
				, TableLevel
			)
			VALUES
			(
				@pvNextInstanceID
				, (SELECT SetupId FROM CSI_PURGEUTIL_SETUP su WHERE UPPER(su.SetupName) = UPPER(@pvSetupName))
				, @pvTableName
				, @pvInstanceCol1A
				, @pvInstanceCol1B
				, @pvInstanceCol1C
				, @pvInstanceCol2
				, @pvInstanceParentCol
				, @pvParentTable
				, @pvParentTableLinkCol
				, GetDate()
				, @pvCommitBatchSize
				, @pvSQLQueryPredicate
				, @pvTableLevel
			);
			--
			---------------------------------------------------------------------------
			-- 	Set the Setup Status to SETUP		
			---------------------------------------------------------------------------
			UPDATE
				CSI_PurgeUtil_Setup
			SET
				SetupStatus = 'SETUP'
			WHERE
				UPPER(SetupName) = UPPER(@pvSetupName);
			--
			IF @@TRANCOUNT > 0	
				COMMIT TRANSACTION;
	    END TRY
		BEGIN CATCH
			SET @ErrorMessage = ERROR_MESSAGE(); 
			SET @vProgID = @vObject_Name + '.' + 'Insert/Update a purging setup ' + @pvSetupName;  
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
