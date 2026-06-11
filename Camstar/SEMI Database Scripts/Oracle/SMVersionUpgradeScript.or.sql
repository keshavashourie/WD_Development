-- Copyright Siemens 2025 --
DECLARE
    v_isStatusModelUpdated NUMBER(1) := 0;
    v_isStatusModelDetailUpdated NUMBER(1) := 0;
    v_isStatusModelDetailToReasonUpdated NUMBER(1) := 0;
    v_isResourceLayoutUpdated NUMBER(1) := 0;
    v_isResourceLayoutDetailsUpdated NUMBER(1) := 0;
    v_table_exists NUMBER;
    v_count NUMBER;
    v_sql VARCHAR2(4000);
BEGIN
    -- StatusModel Table
    v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''SCSSTATUSMODEL''';
    EXECUTE IMMEDIATE v_sql INTO v_table_exists;
    
    IF v_table_exists > 0 THEN
        v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''A_STATUSMODEL''';
        EXECUTE IMMEDIATE v_sql INTO v_table_exists;
        
        IF v_table_exists > 0 THEN
            v_sql := 'SELECT COUNT(*) FROM A_StatusModel WHERE StatusModelId LIKE ''48806e%''';
            EXECUTE IMMEDIATE v_sql INTO v_count;
            
            IF v_count > 0 THEN
                -- Create temporary table
                EXECUTE IMMEDIATE 
                'CREATE GLOBAL TEMPORARY TABLE temp_status_model AS 
                SELECT * FROM scsStatusModel WHERE StatusModelId LIKE ''48806e%''';
                
                -- Delete new values
                v_sql := 'DELETE FROM scsStatusModel WHERE StatusModelId LIKE ''48806e%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Insert previous records
                v_sql := 'INSERT INTO scsStatusModel(CDOTypeId, ChangeCount, ChangeHistoryId, Description, 
                    FilterTags, IconId, isFrozen, Notes, SetupAccessId, StatusModelId, StatusModelName)
                    SELECT CDOTypeId, ChangeCount, ChangeHistoryId, Description, FilterTags, IconId, 
                    isFrozen, Notes, SetupAccessId, StatusModelId, StatusModelName 
                    FROM A_StatusModel WHERE StatusModelId LIKE ''48806e%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Delete previous records
                v_sql := 'DELETE FROM A_StatusModel WHERE StatusModelId LIKE ''48806e%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Insert current records
                v_sql := 'INSERT INTO scsStatusModel SELECT * FROM temp_status_model';
                EXECUTE IMMEDIATE v_sql;
                
                -- Drop temporary table
                EXECUTE IMMEDIATE 'DROP TABLE temp_status_model';
                
                -- Drop index
                BEGIN
                    EXECUTE IMMEDIATE 'DROP INDEX A_StatusModel1';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;
            END IF;
        END IF;
    END IF;

    -- StatusModelDetail Table
    v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''SCSSTATUSMODELDETAIL''';
    EXECUTE IMMEDIATE v_sql INTO v_table_exists;
    
    IF v_table_exists > 0 THEN
        v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''A_STATUSMODELDETAIL''';
        EXECUTE IMMEDIATE v_sql INTO v_table_exists;
        
        IF v_table_exists > 0 THEN
            v_sql := 'SELECT COUNT(*) FROM A_StatusModelDetail WHERE StatusModelDetailId LIKE ''48822e%''';
            EXECUTE IMMEDIATE v_sql INTO v_count;
            
            IF v_count > 0 THEN
                -- Create temporary table
                EXECUTE IMMEDIATE 
                'CREATE GLOBAL TEMPORARY TABLE temp_status_model_detail AS 
                SELECT * FROM scsStatusModelDetail WHERE StatusModelDetailId LIKE ''48822e%''';
                
                -- Delete new values
                v_sql := 'DELETE FROM scsStatusModelDetail WHERE StatusModelDetailId LIKE ''48822e%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Insert previous records
                v_sql := 'INSERT INTO scsStatusModelDetail(AutoSetPrecondition, AutoSetPreconditionForChild, 
                    AutoSetPreconditionForParent, AutoSetReasonId, CDOTypeId, ChangeCount, ExportImportKey, 
                    IsFrozen, StatusId, StatusModelDetailId, StatusModelId, StatusSequence, ToStatusId)
                    SELECT AutoSetPrecondition, AutoSetPreconditionForChild, AutoSetPreconditionForParent, 
                    AutoSetReasonId, CDOTypeId, ChangeCount, ExportImportKey, IsFrozen, StatusId, 
                    StatusModelDetailId, StatusModelId, StatusSequence, ToStatusId 
                    FROM A_StatusModelDetail WHERE StatusModelDetailId LIKE ''48822e%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Delete previous records
                v_sql := 'DELETE FROM A_StatusModelDetail WHERE StatusModelDetailId LIKE ''48822e%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Insert current records
                v_sql := 'INSERT INTO scsStatusModelDetail SELECT * FROM temp_status_model_detail';
                EXECUTE IMMEDIATE v_sql;
                
                -- Drop temporary table
                EXECUTE IMMEDIATE 'DROP TABLE temp_status_model_detail';
                
                -- Drop indexes
                BEGIN
                    EXECUTE IMMEDIATE 'DROP INDEX A_StatusModelDetail1';
                    EXECUTE IMMEDIATE 'DROP INDEX A_StatusModelDetail2';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;
            END IF;
        END IF;
    END IF;

    -- StatusModelDetailToReason Table
    v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''SCSSTATUSMODELDETAILTOREASON''';
    EXECUTE IMMEDIATE v_sql INTO v_table_exists;
    
    IF v_table_exists > 0 THEN
        v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''A_STATUSMODELDETAILTOREASON''';
        EXECUTE IMMEDIATE v_sql INTO v_table_exists;
        
        IF v_table_exists > 0 THEN
            v_sql := 'SELECT COUNT(*) FROM A_StatusModelDetailToReason WHERE StatusModelDetailToReasonId LIKE ''48822f%''';
            EXECUTE IMMEDIATE v_sql INTO v_count;
            
            IF v_count > 0 THEN
                -- Create temporary table
                EXECUTE IMMEDIATE 
                'CREATE GLOBAL TEMPORARY TABLE temp_status_model_detail_reason AS 
                SELECT * FROM scsStatusModelDetailToReason WHERE StatusModelDetailToReasonId LIKE ''48822f%''';
                
                -- Delete new values
                v_sql := 'DELETE FROM scsStatusModelDetailToReason WHERE StatusModelDetailToReasonId LIKE ''48822f%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Insert previous records
                v_sql := 'INSERT INTO scsStatusModelDetailToReason(AutoJobCreateJobModelId, AutoJobCreatePrecondition, 
                    CDOTypeId, ChangeCount, ExportImportKey, IsFrozen, ReasonId, StatusModelDetailId, 
                    StatusModelDetailToReasonId)
                    SELECT AutoJobCreateJobModelId, AutoJobCreatePrecondition, CDOTypeId, ChangeCount, 
                    ExportImportKey, IsFrozen, ReasonId, StatusModelDetailId, StatusModelDetailToReasonId 
                    FROM A_StatusModelDetailToReason WHERE StatusModelDetailToReasonId LIKE ''48822f%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Delete previous records
                v_sql := 'DELETE FROM A_StatusModelDetailToReason WHERE StatusModelDetailToReasonId LIKE ''48822f%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Insert current records
                v_sql := 'INSERT INTO scsStatusModelDetailToReason SELECT * FROM temp_status_model_detail_reason';
                EXECUTE IMMEDIATE v_sql;
                
                -- Drop temporary table
                EXECUTE IMMEDIATE 'DROP TABLE temp_status_model_detail_reason';
                
                -- Drop index
                BEGIN
                    EXECUTE IMMEDIATE 'DROP INDEX A_StatusModelDetailToReason1';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;
            END IF;
        END IF;
    END IF;

    -- ResourceLayout Table
    v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''RESOURCELAYOUT''';
    EXECUTE IMMEDIATE v_sql INTO v_table_exists;
    
    IF v_table_exists > 0 THEN
        v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''A_RESOURCELAYOUT''';
        EXECUTE IMMEDIATE v_sql INTO v_table_exists;
        
        IF v_table_exists > 0 THEN
            v_sql := 'SELECT COUNT(*) FROM A_ResourceLayout WHERE ResourceLayoutId LIKE ''488067%''';
            EXECUTE IMMEDIATE v_sql INTO v_count;
            
            IF v_count > 0 THEN
                -- Create temporary table
                EXECUTE IMMEDIATE 
                'CREATE GLOBAL TEMPORARY TABLE temp_resource_layout AS 
                SELECT * FROM ResourceLayout WHERE ResourceLayoutId LIKE ''488067%''';
                
                -- Delete new values
                v_sql := 'DELETE FROM ResourceLayout WHERE ResourceLayoutId LIKE ''488067%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Insert previous records
                v_sql := 'INSERT INTO ResourceLayout(BackgroundFileName, CDOTypeId, ChangeCount, ChangeHistoryId, 
                    Description, FilterTags, IconId, IsFrozen, LayoutHeight, LayoutWidth, Notes, 
                    ResourceLayoutId, ResourceLayoutName, SetupAccessId)
                    SELECT BackgroundFileName, CDOTypeId, ChangeCount, ChangeHistoryId, Description, 
                    FilterTags, IconId, IsFrozen, LayoutHeight, LayoutWidth, Notes, ResourceLayoutId, 
                    ResourceLayoutName, SetupAccessId 
                    FROM A_ResourceLayout WHERE ResourceLayoutId LIKE ''488067%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Delete previous records
                v_sql := 'DELETE FROM A_ResourceLayout WHERE ResourceLayoutId LIKE ''488067%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Insert current records
                v_sql := 'INSERT INTO ResourceLayout SELECT * FROM temp_resource_layout';
                EXECUTE IMMEDIATE v_sql;
                
                -- Drop temporary table
                EXECUTE IMMEDIATE 'DROP TABLE temp_resource_layout';
                
                -- Drop index
                BEGIN
                    EXECUTE IMMEDIATE 'DROP INDEX A_ResourceLayout1';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;
            END IF;
        END IF;
    END IF;

    -- ResourceLayoutDetails Table
    v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''RESOURCELAYOUTDETAILS''';
    EXECUTE IMMEDIATE v_sql INTO v_table_exists;
    
    IF v_table_exists > 0 THEN
        v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''A_RESOURCELAYOUTDETAILS''';
        EXECUTE IMMEDIATE v_sql INTO v_table_exists;
        
        IF v_table_exists > 0 THEN
            v_sql := 'SELECT COUNT(*) FROM A_ResourceLayoutDetails WHERE ResourceLayoutDetailsId LIKE ''488223%''';
            EXECUTE IMMEDIATE v_sql INTO v_count;
            
            IF v_count > 0 THEN
                -- Create temporary table
                EXECUTE IMMEDIATE 
                'CREATE GLOBAL TEMPORARY TABLE temp_resource_layout_details AS 
                SELECT * FROM ResourceLayoutDetails WHERE ResourceLayoutDetailsId LIKE ''488223%''';
                
                -- Delete new values
                v_sql := 'DELETE FROM ResourceLayoutDetails WHERE ResourceLayoutDetailsId LIKE ''488223%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Insert previous records
                v_sql := 'INSERT INTO ResourceLayoutDetails(CDOTypeId, ChangeCount, ExportImportKey, IsFrozen, 
                    ResourceId, ResourceLayoutDetailsId, ResourceLayoutId, XLocation, YLocation)
                    SELECT CDOTypeId, ChangeCount, ExportImportKey, IsFrozen, ResourceId, 
                    ResourceLayoutDetailsId, ResourceLayoutId, XLocation, YLocation 
                    FROM A_ResourceLayoutDetails WHERE ResourceLayoutDetailsId LIKE ''488223%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Delete previous records
                v_sql := 'DELETE FROM A_ResourceLayoutDetails WHERE ResourceLayoutDetailsId LIKE ''488223%''';
                EXECUTE IMMEDIATE v_sql;
                
                -- Insert current records
                v_sql := 'INSERT INTO ResourceLayoutDetails SELECT * FROM temp_resource_layout_details';
                EXECUTE IMMEDIATE v_sql;
                
                -- Drop temporary table
                EXECUTE IMMEDIATE 'DROP TABLE temp_resource_layout_details';
                
                -- Drop indexes
                BEGIN
                    EXECUTE IMMEDIATE 'DROP INDEX A_ResourceLayoutDetails1';
                    EXECUTE IMMEDIATE 'DROP INDEX A_ResourceLayoutDetails2';
                EXCEPTION
                    WHEN OTHERS THEN NULL;
                END;
            END IF;
        END IF;
    END IF;

    -- Handle table renames if not updated
    IF v_isStatusModelUpdated = 0 THEN
        v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''SCSSTATUSMODEL''';
        EXECUTE IMMEDIATE v_sql INTO v_table_exists;
        
        IF v_table_exists = 0 THEN
            EXECUTE IMMEDIATE 'RENAME A_StatusModel TO scsStatusModel';
            EXECUTE IMMEDIATE 'ALTER INDEX A_StatusMode679477466 RENAME TO scsStatusMode679477466';
            EXECUTE IMMEDIATE 'ALTER INDEX A_StatusModel1 RENAME TO scsStatusModel1';
        END IF;
    END IF;

	IF v_isStatusModelDetailUpdated = 0 THEN
        v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''SCSSTATUSMODELDETAIL''';
        EXECUTE IMMEDIATE v_sql INTO v_table_exists;
        
        IF v_table_exists = 0 THEN
            EXECUTE IMMEDIATE 'RENAME A_StatusModelDetail TO scsStatusModelDetail';
            EXECUTE IMMEDIATE 'ALTER INDEX A_StatusMode679477467 RENAME TO scsStatusMode679477467';
            EXECUTE IMMEDIATE 'ALTER INDEX A_StatusModelDetail1 RENAME TO scsStatusModelDetail1';
            EXECUTE IMMEDIATE 'ALTER INDEX A_StatusModelDetail2 RENAME TO scsStatusModelDetail2';
        END IF;
    END IF;
	
	IF v_isStatusModelDetailToReasonUpdated = 0 THEN
        v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''SCSSTATUSMODELDETAILTOREASON''';
        EXECUTE IMMEDIATE v_sql INTO v_table_exists;
        
        IF v_table_exists = 0 THEN
            EXECUTE IMMEDIATE 'RENAME A_StatusModelDetailToReason TO scsStatusModelDetailToReason';
            EXECUTE IMMEDIATE 'ALTER INDEX A_StatusMode679477468 RENAME TO scsStatusMode679477468';
            EXECUTE IMMEDIATE 'ALTER INDEX A_StatusModelDetailToReason1 RENAME TO scsStatusModelDetailToReason1';
        END IF;
    END IF;
	
	IF v_isResourceLayoutUpdated = 0 THEN
        v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''RESOURCELAYOUT''';
        EXECUTE IMMEDIATE v_sql INTO v_table_exists;
        
        IF v_table_exists = 0 THEN
            EXECUTE IMMEDIATE 'RENAME A_ResourceLayout TO ResourceLayout';
            EXECUTE IMMEDIATE 'ALTER INDEX A_ResourceLa679477421 RENAME TO ResourceLayo679477421';
            EXECUTE IMMEDIATE 'ALTER INDEX A_ResourceLayout1 RENAME TO ResourceLayout1';
        END IF;
    END IF;
	
	IF v_isResourceLayoutDetailsUpdated = 0 THEN
        v_sql := 'SELECT COUNT(*) FROM user_tables WHERE table_name = ''RESOURCELAYOUTDETAILS''';
        EXECUTE IMMEDIATE v_sql INTO v_table_exists;
        
        IF v_table_exists = 0 THEN
            EXECUTE IMMEDIATE 'RENAME A_ResourceLayoutDetails TO ResourceLayoutDetails';
            EXECUTE IMMEDIATE 'ALTER INDEX A_ResourceLa679477423 RENAME TO ResourceLayo679477423';
            EXECUTE IMMEDIATE 'ALTER INDEX A_ResourceLayoutDetails1 RENAME TO ResourceLayoutDetails1';
            EXECUTE IMMEDIATE 'ALTER INDEX A_ResourceLayoutDetails2 RENAME TO ResourceLayoutDetails2';
        END IF;
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
        RAISE;
END;
/