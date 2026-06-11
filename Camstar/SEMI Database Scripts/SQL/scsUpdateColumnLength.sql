-- SCRIPT:scsUpdateColumnLength.sql
-- DESCR: Executes stored procedures to update column lengths to Semiconductor definition
--
--  Copyright Siemens 2025

--------------------------------------------------------------------------------
SET NOCOUNT ON; -- Prevents "X rows affected" messages for cleaner output

DECLARE
    @v_table_exists INT = 0,
    @v_column_exists INT = 0,
    @v_column_datatype NVARCHAR(128),
    @v_char_length INT,
    @sql_statement NVARCHAR(MAX);

BEGIN
     EXEC csiIncreaseStringColMaxLength 'ResourceDef','ss_ChildStatusCombination',4000;
     EXEC csiIncreaseStringColMaxLength 'A_SPCSetupDetailsInlineParam','ParamValue',255;
     EXEC csiIncreaseStringColMaxLength 'ContainerRenameHistDetail','Name',40;
     EXEC csiIncreaseStringColMaxLength 'csiTbl_FactoryPerformanceSum','ProductName',100;
     EXEC csiIncreaseStringColMaxLength 'csiTbl_ProductionDashboard','ProductName',100;
     EXEC csiIncreaseStringColMaxLength 'csiTbl_Yield','ProductName',100;
     EXEC csiIncreaseStringColMaxLength 'DocumentEntry','DocumentEntryName',255;
     EXEC csiIncreaseStringColMaxLength 'DynamicNumberingRule','DynamicNumberingRuleName',100;
     EXEC csiIncreaseStringColMaxLength 'HistoryMainline','ContainerName',100;
     EXEC csiIncreaseStringColMaxLength 'MasterDataCatalogDtl','CDODisplayName',255;
     EXEC csiIncreaseStringColMaxLength 'ProductBase','ProductName',100;
     EXEC csiIncreaseStringColMaxLength 'SPCViolationHistoryDetail','ViolationName',2000;
     EXEC csiIncreaseStringColMaxLength 'UserQueryParameter','DynamicValue',255;


    -- --- Start of conditional block for ss_SlotWaferInstruction.ss_SlotWaferNumber ---
    PRINT '--- Starting conditional block for ss_SlotWaferInstruction.ss_SlotWaferNumber ---';

    -- 1. Check if the table ss_SlotWaferInstruction exists
    SELECT @v_table_exists = COUNT(*)
    FROM sys.tables
    WHERE name = 'ss_SlotWaferInstruction';

    IF @v_table_exists > 0
    BEGIN
        -- Table exists, now check if the column exists
        SELECT @v_column_exists = COUNT(*)
        FROM sys.columns sc
        JOIN sys.tables st ON sc.object_id = st.object_id
        WHERE st.name = 'ss_SlotWaferInstruction'
        AND sc.name = 'ss_SlotWaferNumber';

        IF @v_column_exists > 0
        BEGIN
            -- Column exists, retrieve its data type and character length
            SELECT
                @v_column_datatype = t.name,
                @v_char_length = CASE
                                    WHEN t.name IN ('nchar', 'nvarchar') THEN sc.max_length / 2
                                    WHEN t.name IN ('char', 'varchar') THEN sc.max_length
                                    ELSE sc.max_length
                                END
            FROM sys.columns sc
            JOIN sys.types t ON sc.system_type_id = t.system_type_id
            WHERE sc.object_id = OBJECT_ID('ss_SlotWaferInstruction')
            AND sc.name = 'ss_SlotWaferNumber';

            -- 2. Check if the column is already NVARCHAR(40) or longer
            IF @v_column_datatype = 'nvarchar' AND @v_char_length >= 40
            BEGIN
                PRINT 'Column ss_SlotWaferInstruction.ss_SlotWaferNumber is already NVARCHAR with sufficient length (40). Skipping modification.';
            END
            ELSE
            BEGIN
                PRINT 'Modifying column ss_SlotWaferInstruction.ss_SlotWaferNumber from ' + @v_column_datatype + '(' + CAST(@v_char_length AS NVARCHAR(10)) + ') to NVARCHAR(40).';
                BEGIN TRY
                    SET @sql_statement = 'ALTER TABLE ' + QUOTENAME('ss_SlotWaferInstruction') + ' ALTER COLUMN ' + QUOTENAME('ss_SlotWaferNumber') + ' NVARCHAR(40)';
                    EXEC sp_executesql @sql_statement;
                    PRINT 'ss_SlotWaferInstruction.ss_SlotWaferNumber successfully modified to NVARCHAR(40).';
                END TRY
                BEGIN CATCH
                    PRINT 'Error during modification of ss_SlotWaferInstruction.ss_SlotWaferNumber' + ': ' + ERROR_MESSAGE();
                END CATCH
            END
        END
        ELSE
        BEGIN
            PRINT 'Column ss_SlotWaferNumber not found in table ss_SlotWaferInstruction. Skipping modification for this column.';
        END
    END
    ELSE
    BEGIN
        PRINT 'Table ss_SlotWaferInstruction does not exist. Skipping modification for ss_SlotWaferNumber column.';
    END
    PRINT '--- End of conditional block for ss_SlotWaferInstruction.ss_SlotWaferNumber ---';

END;
GO