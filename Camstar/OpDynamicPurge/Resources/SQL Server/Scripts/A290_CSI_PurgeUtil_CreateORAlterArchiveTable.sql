ALTER PROCEDURE CSI_PurgeUtil_CreateORAlterArchiveTable 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_CreateORAlterArchiveTable
                     Create and/or alter the database table structure to be 
					 in sync with that of the source database table structure.
                     Note : Since CREATE/ALTER TABLE, CREATE/ALTER COLUMN are 
                            DDL statement, there must not be any BEGIN TRAN,
                            COMMIT TRAN and ROLLBACK TRAN statement in here.
  Author           : Benny.Chia 
  Date             : 08 Apr 2014
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvSourceDBName               NVARCHAR(40)
	, @pvArchiveDBName              NVARCHAR(40)
	, @pvSourceSchemaName    		NVARCHAR(40)
	, @pvArchiveSchemaName			NVARCHAR(40)
	, @pvTableName            		NVARCHAR(30)
	, @pvParentTable         		NVARCHAR(30)
	, @pvTableAlteredFlag           INTEGER OUTPUT -- return 1 if the source table has alter/add columns else return 0.
	) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
DECLARE	@vTablename					  NVARCHAR(30) = UPPER(@pvTableName); 
DECLARE  @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
DECLARE @vCreateTableStmt             NVARCHAR(max);
DECLARE @vCreateIndexStmt             NVARCHAR(max);
DECLARE @vIndex_Name                  NVARCHAR(32);
--
DECLARE @vDataType					  NVARCHAR(32);;
DECLARE @vLength					  int;    
DECLARE @vPrecision					  int;
DECLARE @vScale  					  int;
DECLARE @vIs_nullable                 int;
--
DECLARE	@vTableDictStmt				  nvarchar(max);
DECLARE @vColumnName				  NVARCHAR(30);
DECLARE @vPrimaryKeyColumnName	      NVARCHAR(30);
DECLARE @vAlterTableStmt              NVARCHAR(max);
BEGIN 
	SET NOCOUNT ON;
	SET @vProgID = @vObject_Name + '.START';
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		SET @vProgID = @vObject_Name + '.Validate parameters';
		IF @pvSourceDBName IS Null 
			RAISERROR ('Source Database Name must not be blank', 16, 1);
		IF @pvArchiveDBName IS Null 
			RAISERROR ('Archive Database Name must not be blank', 16, 1);
		IF @pvSourceSchemaName IS Null 
			RAISERROR ('SourceSchemaName must not be blank', 16, 1);
		IF @pvArchiveSchemaName IS Null 
			RAISERROR ('ArchiveSchemaName must not be blank', 16, 1);
		IF @vTableName IS Null 
			RAISERROR ('TableName must not be blank', 16, 1);
		--
		---------------------------------------------------------------------------
	    BEGIN TRY 
		    BEGIN TRY 
			    SET @vCreateTableStmt =  N'CREATE TABLE ' + @pvArchiveDBName + '.' + @pvArchiveSchemaName + '.' + @vTableName + ' ';
			    SET @vCreateTableStmt += '(';
			    SET @vCreateTableStmt += 'BATCHEXECUTIONID NVARCHAR(16) NOT NULL';
			    SET @vCreateTableStmt += ', PURGEID NVARCHAR(16) NOT NULL';
			    SET @vCreateTableStmt += ', PURGE_DATETIME DATETIME DEFAULT CURRENT_TIMESTAMP not null';
			    SET @vCreateTableStmt += ', MAINTABLEINSTANCEVALUE NVARCHAR(16) NULL';
				--IF UPPER(@vTableName) IN ('CONTAINER', 'RESOURCEDEF', 'EVENT', 'MFGORDER') -- Root level table
				--BEGIN
			    --    SET @vCreateTableStmt += ', INSTANCEEXECUTIONTIME FLOAT';
			    --    SET @vCreateTableStmt += ', INSTANCEHISTORYMAINLINECOUNT INT';
				--END;
				--ELSE IF @pvParentTable IS NULL OR @pvParentTable = ''  -- Root level table
			    --    SET @vCreateTableStmt += ', INSTANCEEXECUTIONTIME FLOAT';
			    SET @vCreateTableStmt += ')';
				EXECUTE sp_executesql @vCreateTableStmt;
	        END TRY
			BEGIN CATCH
				IF ERROR_NUMBER() = 2714 -- 2714 = There is already an object named <TableName> in the database.
					SET @ErrorMessage = '';  -- Ignore this error 
				ELSE
				BEGIN
					SET @ErrorMessage = ERROR_MESSAGE(); 
					SET @vProgID = @vObject_Name + '.' + 'Create table ' + @vTableName; 
            		RAISERROR (@ErrorMessage, 16, 1);
				END;
			END CATCH
			-----------------------------------------------------------------------
			-- Create index on BatchExecutionId
			-----------------------------------------------------------------------
		    BEGIN TRY 
				SET @vIndex_Name = RTRIM(SUBSTRING(@vTableName, 1, 29)) + '1';
				SET @vCreateIndexStmt = N'CREATE INDEX ' + @vIndex_Name + ' ON ' + @pvArchiveDBName + '.' + @pvArchiveSchemaName + '.' + @vTableName + ' (BATCHEXECUTIONID) ';
				EXECUTE sp_executesql @vCreateIndexStmt;
	        END TRY
			BEGIN CATCH
				IF ERROR_NUMBER() = 1913  -- 1913 : The operation failed because an index or statistics with name '<IndexName>' already exists on table '<TableName>'.
					SET @ErrorMessage = '';  -- Ignore this error 
				ELSE
				BEGIN
					SET @ErrorMessage = ERROR_MESSAGE(); 
					SET @vProgID = @vObject_Name + '.' + 'Create index ' + @vIndex_Name;
            		RAISERROR (@ErrorMessage, 16, 1);
				END;
			END CATCH
			-----------------------------------------------------------------------
			-- Create index on purgeid
			-----------------------------------------------------------------------
		    BEGIN TRY 
				SET @vIndex_Name = RTRIM(SUBSTRING(@vTableName, 1, 29)) + '2';
				SET @vCreateIndexStmt = N'CREATE INDEX ' + @vIndex_Name + ' ON ' + @pvArchiveDBName + '.' + @pvArchiveSchemaName + '.' + @vTableName + ' (PURGEID) ';
				EXECUTE sp_executesql @vCreateIndexStmt;
	        END TRY
			BEGIN CATCH
				IF ERROR_NUMBER() = 1913  -- 1913 : The operation failed because an index or statistics with name '<IndexName>' already exists on table '<TableName>'.
					SET @ErrorMessage = '';  -- Ignore this error 
				ELSE
				BEGIN
					SET @ErrorMessage = ERROR_MESSAGE(); 
					SET @vProgID = @vObject_Name + '.' + 'Create index ' + @vIndex_Name; 
            		RAISERROR (@ErrorMessage, 16, 1);
				END;
			END CATCH
			-----------------------------------------------------------------------
			-- Create index on PURGE_DATETIME
			-----------------------------------------------------------------------
		    BEGIN TRY 
				SET @vIndex_Name = RTRIM(SUBSTRING(@vTableName, 1, 29)) + '3';
				SET @vCreateIndexStmt = N'CREATE INDEX ' + @vIndex_Name + ' ON ' + @pvArchiveDBName + '.' + @pvArchiveSchemaName + '.' + @vTableName + ' (PURGE_DATETIME) ';
				EXECUTE sp_executesql @vCreateIndexStmt;
	        END TRY
			BEGIN CATCH
				IF ERROR_NUMBER() = 1913  -- 1913 : The operation failed because an index or statistics with name '<IndexName>' already exists on table '<TableName>'.
					SET @ErrorMessage = '';  -- Ignore this error 
				ELSE
				BEGIN
					SET @ErrorMessage = ERROR_MESSAGE(); 
					SET @vProgID = @vObject_Name + '.' + 'Create index ' + @vIndex_Name; 
            		RAISERROR (@ErrorMessage, 16, 1);
				END;
			END CATCH
			-----------------------------------------------------------------------
			-- Create index on MAINTABLEINSTANCEVALUE
			-----------------------------------------------------------------------
		    BEGIN TRY 
				SET @vIndex_Name = RTRIM(SUBSTRING(@vTableName, 1, 29)) + '4';
				SET @vCreateIndexStmt = N'CREATE INDEX ' + @vIndex_Name + ' ON ' + @pvArchiveDBName + '.' + @pvArchiveSchemaName + '.' + @vTableName + ' (MAINTABLEINSTANCEVALUE) ';
				EXECUTE sp_executesql @vCreateIndexStmt;
	        END TRY
			BEGIN CATCH
				IF ERROR_NUMBER() = 1913  -- 1913 : The operation failed because an index or statistics with name '<IndexName>' already exists on table '<TableName>'.
					SET @ErrorMessage = '';  -- Ignore this error 
				ELSE
				BEGIN
					SET @ErrorMessage = ERROR_MESSAGE(); 
					SET @vProgID = @vObject_Name + '.' + 'Create index ' + @vIndex_Name; 
            		RAISERROR (@ErrorMessage, 16, 1);
				END;
			END CATCH
			-----------------------------------------------------------------------
			-- Verify if there is any table structure differences, if yes then syncronised
			-- the target table structure with that of the source table structure.
			-----------------------------------------------------------------------
			BEGIN TRY 
		        DECLARE @vParmDefinition nvarchar(500);
		        DECLARE @paramTablename NVARCHAR(30);
                --SET @pvColNames = '';
				SET @vTableDictStmt = 
				   --N'DECLARE curSyncColumn CURSOR FOR 
				   N'DECLARE curSyncColumn CURSOR FOR 
				   SELECT Tablename   = so.name  
						, Columnname  = sc.name 
						, DataType    = st.name 
						, Length      = sc.max_length 
						, precision   = sc.precision 
						, scale       = sc.scale
						, is_nullable = sc.is_nullable
					FROM   '+ @pvSourceDBName + '.SYS.objects So 
					INNER JOIN '+ @pvSourceDBName + '.SYS.columns Sc ON So.object_id = Sc.object_id 
					INNER JOIN '+ @pvSourceDBName + '.SYS.types St ON Sc.system_type_id = St.system_type_id 
						AND Sc.user_type_id   = St.user_type_id 
					WHERE SO.TYPE =''U'' 
						AND UPPER(SO.Name) = @paramTablename 
					EXCEPT 
					SELECT Tablename  = so.name  
						, Columnname = sc.name 
						, DataType   = St.name 
						, Length     = Sc.max_length 
						, precision  = Sc.precision 
						, scale       = sc.scale
						, is_nullable = sc.is_nullable
					FROM ' + @pvArchiveDBName + '.SYS.objects So 
					INNER JOIN ' + @pvArchiveDBName + '.SYS.columns Sc ON So.object_id = Sc.object_id 
					INNER JOIN ' + @pvArchiveDBName + '.SYS.types St ON Sc.system_type_id = St.system_type_id 
						AND Sc.user_type_id   = St.user_type_id 
					WHERE SO.TYPE =''U'' 
						AND UPPER(SO.Name) = @paramTablename 
					';
				--
				SET @vParmDefinition = N'@paramTablename NVARCHAR(30)';
				EXECUTE sp_executesql @vTableDictStmt, @vParmDefinition, @paramTablename=@vTablename;
	        END TRY
			BEGIN CATCH
				BEGIN
					SET @ErrorMessage = ERROR_MESSAGE(); 
					SET @vProgID = @vObject_Name + '.' + 'Check table dictionary.'; 
            		RAISERROR (@ErrorMessage, 16, 1);
				END;
			END CATCH
			--
			OPEN curSyncColumn;
			FETCH NEXT FROM curSyncColumn
			INTO @vTableName, @vColumnName, @vDataType, @vLength, @vPrecision, @vScale, @vIs_nullable;
			--
		    SET @pvTableAlteredFlag = 0;
			WHILE @@FETCH_STATUS = 0
			BEGIN
				BEGIN TRY
				    SET @pvTableAlteredFlag = 1;
					SET @vDataType = UPPER(@vDataType);
					SET @vAlterTableStmt =  N'ALTER TABLE ' + @pvArchiveDBName + '.' + @pvArchiveSchemaName  + '.' + @vTableName + ' ';
					-----------------------------------------------------------
					-- Attempt to ALTER COLUMN to an existing column to the archive table in the archive database first.
                    -- If failed, then ADD a new column to the archive table in the archive database.
					-----------------------------------------------------------
					SET @vAlterTableStmt += 'ALTER COLUMN ';
					SET @vAlterTableStmt += @vColumnName + ' ' + @vDataType;
					--
					IF @vDataType IN ('INT', 'FLOAT', 'DATETIME')
						SET @vAlterTableStmt += ' ';
					ELSE IF @vDataType IN ('CHAR', 'VARCHAR', 'BINARY','VARBINARY')
					BEGIN
						IF @vLength = -1 
							SET @vAlterTableStmt += ' (MAX)';
						ELSE
							SET @vAlterTableStmt += ' (' + CAST(@vLength AS NVARCHAR(MAX)) + ')';
					END;
					ELSE IF @vDataType IN ('NCHAR', 'NVARCHAR')
					BEGIN
						IF @vLength = -1 
							SET @vAlterTableStmt += ' (MAX)';
						ELSE
						BEGIN
							SET @vLength = @vLength/2;
							SET @vAlterTableStmt += ' (' + CAST(@vLength AS NVARCHAR(MAX)) + ')';
						END;
					END;
					ELSE IF @vDataType IN ('DECIMAL')
						SET @vAlterTableStmt += ' (' + CAST(@vPrecision AS NVARCHAR(MAX)) + ',' + CAST(@vScale AS NVARCHAR(MAX)) + ')';
					--
					IF @vIs_nullable = 1
						SET @vAlterTableStmt +=  ' NULL ';
					ELSE
						SET @vAlterTableStmt +=  ' NOT NULL ';
					--
					EXECUTE sp_executesql @vAlterTableStmt;
				END TRY
				BEGIN CATCH
				    BEGIN TRY
                        -- Change ALTER COLUMN to ADD
                        SET @vAlterTableStmt = REPLACE(@vAlterTableStmt, 'ALTER COLUMN ', 'ADD ');
					    EXECUTE sp_executesql @vAlterTableStmt;
				    END TRY
				    BEGIN CATCH
					    SET @ErrorMessage = ERROR_MESSAGE();
					    SET @vProgID = @vObject_Name + '.' + '@vAlterTableStmt=' + @vAlterTableStmt;
                		RAISERROR (@ErrorMessage, 16, 1);
    				END CATCH;
				END CATCH;
				---------------------------------------------------------------
				-- Get the next table column
				---------------------------------------------------------------
				FETCH NEXT FROM curSyncColumn
				INTO @vTableName, @vColumnName, @vDataType, @vLength, @vPrecision, @vScale, @vIs_nullable;
			END;  -- Column loop
			CLOSE curSyncColumn;
			DEALLOCATE curSyncColumn;
			--
		    BEGIN TRY 
			    -- Restrict the length of the index to the permissible 30 characters.
				SELECT @vPrimaryKeyColumnName = column_name
				FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
				INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KU ON TC.CONSTRAINT_TYPE = 'PRIMARY KEY' 
				    AND TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME
				    AND UPPER(ku.table_name)= @vTableName
				ORDER BY KU.TABLE_NAME, KU.ORDINAL_POSITION;
				--
				SET @vIndex_Name = SUBSTRING(@vTableName, 1, 29)  + '0' ;
				SET @vCreateIndexStmt = N'CREATE INDEX ' + @vIndex_Name + ' ON ' + @pvArchiveDBName + '.' + @pvArchiveSchemaName + '.' + @vTableName + ' (' + @vPrimaryKeyColumnName +  ') ';
				EXECUTE sp_executesql @vCreateIndexStmt;
	        END TRY
			BEGIN CATCH
				IF ERROR_NUMBER() = 1913  -- 1913 : The operation failed because an index or statistics with name '<IndexName>' already exists on table '<TableName>'.
					SET @ErrorMessage = '';  -- Ignore this error 
				ELSE
				BEGIN
					--PRINT 'ERROR NUMBER ' + CAST(ERROR_NUMBER() AS NVARCHAR(10)) + ' : Create index. ' + ERROR_MESSAGE();
					SET @ErrorMessage = ERROR_MESSAGE(); 
					SET @vProgID = @vObject_Name + '.' + 'Create index ' + @vIndex_Name;
            		RAISERROR (@ErrorMessage, 16, 1);
				END;
			END CATCH
		END TRY
		BEGIN CATCH
			SET @ErrorMessage = ERROR_MESSAGE();
			SET @vProgID = @vObject_Name + '.' + 'CreateORAlterArchiveTable ' + @vTableName; 
    		RAISERROR (@ErrorMessage, 16, 1);
		END CATCH
	    SET @vProgID = @vObject_Name + '.SUCCESSFULL';  
		RETURN 0;
	END TRY
	BEGIN CATCH
		--SET @ErrorMessage = ' TableDictStmt=' + IsNull(@vTableDictStmt, ''); 
		IF @ErrorMessage IS NULL 
		    SET @ErrorMessage  = ERROR_MESSAGE();
		--ELSE
    	--SET @ErrorMessage = IsNull(@ErrorMessage, '') + ' TableDictStmt=' + @vTableDictStmt; 
		IF @ErrorSeverity IS NULL
		    SET @ErrorSeverity = ERROR_SEVERITY(); 
		IF @ErrorState IS NULL
		    SET @ErrorState = ERROR_STATE();
		IF @vProgID IS NULL
		    SET @vProgID = @vObject_Name + '.OTHER ERROR'; 
		EXECUTE CSI_PurgeUtil_Global_Log @pvProgID=@vProgID, @pvErrMsg=@ErrorMessage; 
		IF (SELECT CURSOR_STATUS('local','curSyncColumn')) >= -1
		BEGIN
            IF (SELECT CURSOR_STATUS('local','curSyncColumn')) > -1
            BEGIN
                CLOSE curSyncColumn;
            END
			DEALLOCATE curSyncColumn;
		END;
		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END;
GO
