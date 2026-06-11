--------------------------------------------------------------------------------
-- SCRIPT:scsUpdateColumnLength.sql
-- DESCR: Executes stored procedures to update column lengths to Semiconductor definition
--
--  Copyright Siemens 2025

--------------------------------------------------------------------------------
DECLARE
    v_table_exists NUMBER := 0;
    v_column_exists NUMBER := 0;
    v_column_datatype VARCHAR2(30);
    v_char_length NUMBER; -- To store the character length of the column
BEGIN
	csiIncreaseStringColMaxLength('ResourceDef','ss_ChildStatusCombination',4000);
    csiIncreaseStringColMaxLength('A_SPCSetupDetailsInlineParam','ParamValue',255);
    csiIncreaseStringColMaxLength('ContainerRenameHistDetail','Name',40);
    csiIncreaseStringColMaxLength('csiTbl_FactoryPerformanceSum','ProductName',100);
    csiIncreaseStringColMaxLength('csiTbl_ProductionDashboard','ProductName',100);
    csiIncreaseStringColMaxLength('csiTbl_Yield','ProductName',100);
    csiIncreaseStringColMaxLength('DocumentEntry','DocumentEntryName',255);
    csiIncreaseStringColMaxLength('DynamicNumberingRule','DynamicNumberingRuleName',100);
    csiIncreaseStringColMaxLength('HistoryMainline','ContainerName',100);
    csiIncreaseStringColMaxLength('MasterDataCatalogDtl','CDODisplayName',255);
    csiIncreaseStringColMaxLength('ProductBase','ProductName',100);
    csiIncreaseStringColMaxLength('SPCViolationHistoryDetail','ViolationName',2000);
    csiIncreaseStringColMaxLength('UserQueryParameter','DynamicValue',255);

    -- --- Start of conditional block for ss_SlotWaferInstruction.ss_SlotWaferNumber ---

    -- 1. Check if the table 'ss_SlotWaferInstruction' exists
    SELECT COUNT(*)
    INTO v_table_exists
    FROM USER_TABLES
    WHERE TABLE_NAME = UPPER('ss_SlotWaferInstruction');

    IF v_table_exists > 0 THEN
        -- Table exists, now check if the column 'ss_SlotWaferNumber' exists
        BEGIN
            SELECT COUNT(*)
            INTO v_column_exists
            FROM USER_TAB_COLUMNS
            WHERE TABLE_NAME = UPPER('ss_SlotWaferInstruction')
            AND COLUMN_NAME = UPPER('ss_SlotWaferNumber');

            IF v_column_exists > 0 THEN
                -- Column exists, retrieve its data type and character length
                SELECT DATA_TYPE, CHAR_LENGTH
                INTO v_column_datatype, v_char_length
                FROM USER_TAB_COLUMNS
                WHERE TABLE_NAME = UPPER('ss_SlotWaferInstruction')
                AND COLUMN_NAME = UPPER('ss_SlotWaferNumber');

                -- 2. Check if the column is already VARCHAR2(40) or longer
                -- We check for >= 40 to account for cases where it might already be VARCHAR2(50) for instance.
                IF v_column_datatype = 'VARCHAR2' AND v_char_length >= 40 THEN
                    DBMS_OUTPUT.PUT_LINE('Column ss_SlotWaferInstruction.ss_SlotWaferNumber is already VARCHAR2 with sufficient length (' || v_char_length || '). Skipping modification.');
                ELSE
                    -- Column is not VARCHAR2(40) or longer, proceed with the multi-step modification
                    DBMS_OUTPUT.PUT_LINE('Modifying column ss_SlotWaferInstruction.ss_SlotWaferNumber from ' || v_column_datatype || '(' || v_char_length || ') to VARCHAR2(40).');
                    BEGIN
                        EXECUTE IMMEDIATE 'ALTER TABLE ss_SlotWaferInstruction ADD ss_SlotWaferNumber_NEW VARCHAR2(40)';
                        EXECUTE IMMEDIATE 'UPDATE ss_SlotWaferInstruction SET ss_SlotWaferNumber_NEW = TO_NCHAR(ss_SlotWaferNumber)';
                        EXECUTE IMMEDIATE 'ALTER TABLE ss_SlotWaferInstruction DROP COLUMN ss_SlotWaferNumber';
                        EXECUTE IMMEDIATE 'ALTER TABLE ss_SlotWaferInstruction RENAME COLUMN ss_SlotWaferNumber_NEW TO ss_SlotWaferNumber';
                        DBMS_OUTPUT.PUT_LINE('ss_SlotWaferInstruction.ss_SlotWaferNumber successfully modified to VARCHAR2(40).');
                    EXCEPTION
                        WHEN OTHERS THEN
                            -- Catch any errors during the modification sequence itself
                            DBMS_OUTPUT.PUT_LINE('Error during modification of ss_SlotWaferInstruction.ss_SlotWaferNumber: ' || SQLERRM);
                            -- If this is a critical operation, you might want to re-raise the error: RAISE;
                    END;
                END IF;
            ELSE
                DBMS_OUTPUT.PUT_LINE('Column ss_SlotWaferNumber not found in table ss_SlotWaferInstruction. Skipping modification for this column.');
            END IF;
        EXCEPTION
            WHEN OTHERS THEN
                -- Catch any errors during column metadata retrieval (e.g., if USER_TAB_COLUMNS query fails)
                DBMS_OUTPUT.PUT_LINE('Error checking metadata for ss_SlotWaferInstruction.ss_SlotWaferNumber: ' || SQLERRM);
        END;
    ELSE
        DBMS_OUTPUT.PUT_LINE('Table ss_SlotWaferInstruction does not exist. Skipping modification for ss_SlotWaferNumber column.');
    END IF;

    -- --- End of conditional block for ss_SlotWaferInstruction.ss_SlotWaferNumber ---

END;
/