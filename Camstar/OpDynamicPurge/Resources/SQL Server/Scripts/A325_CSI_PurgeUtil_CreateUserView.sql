ALTER PROCEDURE CSI_PurgeUtil_CreateUserView
    @pvTemplateViewName	NVARCHAR(255),
	@pvUserViewName		NVARCHAR(255),
    @pvSchemaName		NVARCHAR(40),
	@pvSetupName		NVARCHAR(40),
	@pvSetupBatchSize	INTEGER
AS
BEGIN

	DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
	DECLARE @ErrorSeverity              INT;            -- Severity.
	DECLARE @ErrorState                 INT;            -- State.
	DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
	DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;

    DECLARE @vTemplateViewDefinition	NVARCHAR(MAX);
    DECLARE @vSQL						NVARCHAR(MAX);
    
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START';

    BEGIN TRY
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		IF @pvTemplateViewName IS Null 
			RAISERROR ('TemplateViewName must not be blank', 16, 1);
		IF @pvUserViewName IS Null 
			RAISERROR ('UserViewName must not be blank', 16, 1);
		IF @pvSchemaName IS Null 
			RAISERROR ('SchemaName must not be blank', 16, 1);
		IF @pvSetupName IS Null 
			RAISERROR ('SetupName must not be blank', 16, 1);
		IF @pvTemplateViewName = @pvUserViewName 
			RAISERROR ('Template view [%s] and user view [%s] name are the same', 16, 1, @pvTemplateViewName, @pvUserViewName);
		IF @pvSetupBatchSize <= 0
			RAISERROR ('SetupBatchSize must be more than 0', 16, 1);
		---------------------------------------------------------------------------

        -- Check if template view exists
        IF NOT EXISTS (
            SELECT 1 
            FROM INFORMATION_SCHEMA.VIEWS 
            WHERE TABLE_NAME = @pvTemplateViewName
            AND TABLE_SCHEMA = @pvSchemaName
        )
        BEGIN
			SET @vProgID = @vObject_Name + '.' + 'Validate template view exist'; 
            RAISERROR('Template view "%s" does not exist.', 16, 1, @pvTemplateViewName);
        END

        -- Check if user view exists and drop it if it does
        IF EXISTS (
            SELECT 1 
            FROM INFORMATION_SCHEMA.VIEWS 
            WHERE TABLE_NAME = @pvUserViewName
            AND TABLE_SCHEMA = @pvSchemaName
        )
        BEGIN TRY
            SET @vSQL = 'DROP VIEW [' + @pvSchemaName + '].[' + @pvUserViewName + ']';
            EXEC sp_executesql @vSQL;
        END TRY
		BEGIN CATCH
			BEGIN
				SET @ErrorMessage = ERROR_MESSAGE(); 
				SET @vProgID = @vObject_Name + '.' + 'Dropping existing user view [' + @pvUserViewName + ']'; 
				RAISERROR (@ErrorMessage, 16, 1);
			END
		END CATCH
        
        -- Get the definition of the template view
        SET @vTemplateViewDefinition = OBJECT_DEFINITION(OBJECT_ID(@pvTemplateViewName));
        
        -- If the definition is NULL, try another method
        IF @vTemplateViewDefinition IS NULL
        BEGIN
            SELECT @vTemplateViewDefinition = definition
            FROM sys.sql_modules
            WHERE object_id = OBJECT_ID(@pvTemplateViewName);
        END

		-- After both attempts, check if still NULL
		IF @vTemplateViewDefinition IS NULL
		BEGIN
			SET @vProgID = @vObject_Name + '.' + 'Retrive template view definition'; 
			RAISERROR('Could not retrieve definition for view "%s". It may be encrypted or you may lack permissions.', 16, 1, @pvTemplateViewName);
		END
        
        -- Replace the original view name with the new view name in the definition
		SET @vSQL = @vTemplateViewDefinition;
		SET @vSQL = REPLACE(@vSQL, @pvTemplateViewName, @pvUserViewName);
		SET @vSQL = REPLACE(@vSQL, ':vSetupName', @pvSetupName);
		SET @vSQL = REPLACE(@vSQL, '-- DO NOT MODIFY TOP 50 (Placeholder value - will be replaced with user-defined batch size)', ''); -- REMOVE COMMENT
		SET @vSQL = REPLACE(@vSQL, 'TOP 50', 'TOP ' + CAST(@pvSetupBatchSize AS NVARCHAR(10)));

		SET @vProgID = @vObject_Name + '.Create user view ' + @pvUserViewName;
        EXEC sp_executesql @vSQL;

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