------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:  CreateDataStoreInsertUpdateTables.sql
-- DESCR:   Creates the Datastore tables in the OLTP database.  Executes on the transaction database (OLTP)
-- HISTORY:
--      05/04/2005 			    Added ID column.
--    	05/20/2005 			    Added index.
--    	06/23/2005 			    Added master tables, triggers, etc. to support new master/detail architecture.
--    	08/11/2006 			    Changed nvarchar columns to MAX
--   	12/06/2006 			    Updated/added copyright notice(s) (SPR S9984) Bill Lippard
--   	04/23/2007 			    Updated copyright notice(s) (SPR S9984) Bill Lippard
--	    06/13/2016	Dan Maloney	Changed variable naming to prefix with data type.  Example @sql Varchar(512) has become @nv_Sql NVARCHAR(4000)
--	    06/13/2016	Dan Maloney	Added error handling throughout with BEGIN TRY/END TRY and BECGIN CATCH/END CATCH
--	    06/13/2016	Dan Maloney	Added explicit transactions handling for DML
--      10/23/2019  Dan maloney Updated to use CREATE OR REPLACE to avoid dropping existing triggers or stored procedures (CPR 1260)
--		01/19/2022  v8 Alex Lind	remove terminating character from inbound message to prevent invalid string. 
-- Copyright Siemens 2023
------------------------------------------------------------------------------------------------------------------------------------------------------



DECLARE @i_ErrorStatus 		INT
DECLARE @i_NumTables 		INT
DECLARE @i_TblNum 			INT
DECLARE @f_Version 			FLOAT
DECLARE @v_MastTblName 		VARCHAR(50)
DECLARE @v_TblName 			VARCHAR(50)
DECLARE @v_TblTrgName 		VARCHAR(50)
DECLARE @v_MastTblTrgName 	VARCHAR(50)
DECLARE @v_Action 			VARCHAR(8)
DECLARE @nv_Sql 			NVARCHAR(4000)
DECLARE @nv_Msg				NVARCHAR(2048)
BEGIN
	SET NOCOUNT ON
   	SET @i_ErrorStatus = 0 
   	SELECT @f_Version = CONVERT(FLOAT,LEFT(CONVERT(NVARCHAR,SERVERPROPERTY('productversion')),CHARINDEX('.',CONVERT(NVARCHAR,SERVERPROPERTY('productversion')))))

	BEGIN TRY
		SET @nv_Sql = N'UPDATE INSITESITEINFO SET TValue=''Y'' WHERE TName = ''DataStorePresent'''
		EXEC(@nv_Sql)
		PRINT 'Updated InsiteSiteInfo to set DataStorePresent to ''Y'''
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRAN
			SET @nv_Msg = N'Unable to update InsiteSiteInfo to set DataStorePresent = ''Y''.  Script CreateDataStoreInsertUpdateTables.sql exiting'
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
	END CATCH

   	IF NOT EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiLog' AND Type='U')
    BEGIN
		BEGIN TRY
            SET @nv_Sql = N'CREATE TABLE csiLog 
			(Id BIGINT NOT NULL IDENTITY (1, 1) NOT FOR REPLICATION PRIMARY KEY, 
			RecordDate DateTime DEFAULT GetDate(), 
			Source NVARCHAR(50), 
			Message NVARCHAR(MAX) )'

			EXEC (@nv_Sql)
			PRINT 'Create csiLog table'
		END TRY
		BEGIN CATCH
			-- Unable to create table
			SET @nv_Msg =  N'Unable to create csiLog table.  Script CreateDataStoreInsertUpdateTables.sql exiting'
			PRINT @nv_Msg
			PRINT ERROR_MESSAGE();
			THROW 51000, @nv_Msg, 1
		END CATCH
    END


    SELECT @i_NumTables = CONVERT(INT,TValue)
    FROM InsiteSiteInfo
    WHERE TName='DataStoreInsertTables'     

     SET @i_TblNum = 0
     WHILE @i_TblNum <= @i_NumTables
     BEGIN
       	IF @i_TblNum=0
       	BEGIN
       		SET @v_MastTblName='DataStoreUpdatesMaster'
       		SET @v_TblName='DataStoreUpdates'
       		SET @v_TblTrgName='DataStoreUpdTrg'
       		SET @v_MastTblTrgName='DataStoreUpdMTrg'
       	END
       	ELSE
       	BEGIN
       		SET @v_MastTblName = 'DataStoreInserts' + CONVERT(NVARCHAR, @i_TblNum) + 'Master'
       		SET @v_TblName = 'DataStoreInserts' + CONVERT(NVARCHAR, @i_TblNum)
       		SET @v_TblTrgName = 'DataStoreIns' + CONVERT(NVARCHAR, @i_TblNum) + 'Trg'
       		SET @v_MastTblTrgName = 'DataStoreIns' + CONVERT(NVARCHAR, @i_TblNum) + 'MTrg'
       	END

      	-- Create master (control) table
       	IF NOT EXISTS (SELECT Name FROM sysobjects WHERE Name = @v_MastTblName AND Type='U')
       	BEGIN
			BEGIN TRY
				SET @nv_Sql = N'CREATE TABLE ' + @v_MastTblName + 
				N' (Id BIGINT NOT NULL IDENTITY (1,1) NOT FOR REPLICATION PRIMARY KEY,
				TxnId CHAR(16),
				TxnType CHAR(1),
				Status CHAR(1) DEFAULT ''U'',
				RecordDate DateTime DEFAULT GetDate(),
				Server NVARCHAR(50),
				CDOID Int,
				ERROR NVARCHAR(MAX))'

            	EXEC (@nv_Sql)
				PRINT 'Created ' + @v_MastTblName + ' table'
			END TRY
			BEGIN CATCH
				-- Unable to create table
				SET @nv_Msg =  N'Unable to create ' + @v_MastTblName + N' table.  Script CreateDataStoreInsertUpdateTables.sql exiting'
				PRINT @nv_Msg
				PRINT ERROR_MESSAGE();
				THROW 51000, @nv_Msg, 1
			END CATCH
       	END
		
		-- Create the delete trigger
		BEGIN
			BEGIN TRY
       			SET @nv_Sql =  N'CREATE OR ALTER TRIGGER ' + @v_MastTblTrgName + N' ON ' + @v_MastTblName + CHAR(13) + CHAR(10) +
				N'FOR DELETE AS' + CHAR(13) + CHAR(10) +
				N'BEGIN' + CHAR(13) + CHAR(10) +
				N'	DELETE FROM ' + @v_tblName + N' WHERE TxnId IN (SELECT DEL.TxnId FROM DELETED DEL)' + CHAR(13) + CHAR(10) + N'END'
			
				PRINT 'Created ' + @v_MastTblTrgName + ' trigger'
				EXEC(@nv_Sql)
			END TRY
			BEGIN CATCH
				-- Unable to create trigger
				SET @nv_Msg =  N'Unable to create ' + @v_MastTblTrgName + N' trigger.  Script CreateDataStoreInsertUpdateTables.sql exiting'
				PRINT @nv_Msg
				PRINT ERROR_MESSAGE();
				THROW 51000, @nv_Msg, 1
			END CATCH
		END


    

       	 -- Create the data table
       	IF NOT EXISTS (SELECT Name FROM sysobjects WHERE Name = @v_TblName AND Type='U')
       	BEGIN
			BEGIN TRY
       			SET @nv_Sql= N'CREATE TABLE ' + @v_TblName +
				N' (TxnId CHAR(16) NOT NULL, 
				Sequence INT NOT NULL, 
				TxnType CHAR(1), 
				SqlStmt NVARCHAR(MAX),
				CONSTRAINT PK_' + @v_TblName + N' PRIMARY KEY CLUSTERED (TxnId, Sequence))'
				            
				EXEC (@nv_Sql)
				PRINT 'Created ' + @v_TblName + ' table'
			END TRY
			BEGIN CATCH
				-- Unable to create table
				SET @nv_Msg = N'Unable to create ' + @v_TblName + N' table.  Script CreateDataStoreInsertUpdateTables.sql exiting'
				PRINT @nv_Msg
				PRINT ERROR_MESSAGE();
				THROW 51000, @nv_Msg, 1
			END CATCH
		END
		
		-- Create the insert trigger
		BEGIN
			BEGIN TRY
       			SET @nv_Sql = N'CREATE OR ALTER TRIGGER ' + @v_TblTrgName + N' ON ' + @v_TblName + CHAR(13) + CHAR(10) +
				N'FOR INSERT AS' + CHAR(13) + CHAR(10) +
				N'DECLARE @i_Seq INT' + CHAR(13) + CHAR(10) +
				N'DECLARE @c_TxnId CHAR(16)' + CHAR(13) + CHAR(10) +
				N'DECLARE @c_TxnType CHAR(1)' + CHAR(13) + CHAR(10) +
				N'DECLARE @v_Host VARCHAR(50)' + CHAR(13) + CHAR(10) +
				N'BEGIN' + CHAR(13) + CHAR(10) +
				N'	SELECT @i_Seq = INS.Sequence, @c_TxnId = INS.TxnId, @c_TxnType = INS.TxnType FROM INSERTED INS' + CHAR(13) + CHAR(10) + 
				N'	IF @i_Seq = 1' + CHAR(13) + CHAR(10) +
				N'		SELECT @v_Host = HOST_NAME()' + CHAR(13) + CHAR(10) 

				IF @f_Version > 9 
					-- Call via linked server loopback to make autonomous transaction
					SET @nv_Sql = @nv_Sql + N'	EXEC [CSILOOPBACK].[' + DB_NAME() + N'].[' + SCHEMA_NAME() + N'].csiInsertDSMastRec ''' + @v_MastTblName + N''', @c_TxnId, @c_TxnType, @v_Host' + CHAR(13) + CHAR(10) + N'END'
				ELSE
					SET @nv_Sql = @nv_Sql + N'	INSERT INTO ' + @v_MastTblName + N' (TxnId, TxnType) SELECT INS.TxnId, INS.TxnType FROM INSERTED INS' + 	CHAR(13) + CHAR(10) + N'END'
		
				PRINT 'Created ' + @v_TblTrgName + ' trigger'
				EXEC(@nv_Sql)
			END TRY
			BEGIN CATCH
				-- Unable to create trigger
				SET @nv_Msg =  N'Unable to create ' + @v_TblTrgName + N' trigger.  Script CreateDataStoreInsertUpdateTables.sql exiting'
				PRINT @nv_Msg
				PRINT ERROR_MESSAGE();
				THROW 51000, @nv_Msg, 1
			END CATCH
		END
 	         
         SET @i_TblNum=@i_TblNum+1
   END
END
GO







CREATE OR ALTER PROCEDURE csiUpdateDSMastRec
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name: 	csiUpdateDSMastRec
-- Params: 	<in> 	@pc_TxnId			CHAR(16)	
--		<in>	@pc_Status			CHAR(1)
--		<in>	@pi_CDOID	 		INT
--		<in>	@pnv_Msg 			NVARCHAR(MAX)
--
-- Descr: 	Updates the status of records into the remote master queue tables for a given transaction id
--
-- HISTORY:
--	06/13/2016	Dan Maloney	Changed variable naming to prefix with data type.  Example @sql Varchar(512) has become @nv_Sql NVARCHAR(4000)
--	06/13/2016	Dan Maloney	Added error handling throughout with BEGIN TRY/END TRY and BECGIN CATCH/END CATCH
--	06/13/2016	Dan Maloney	Added explicit transactions handling for DML
--	06/15/2016	Dan Maloney	Added RAISEERROR in CATCH blocks to raise error back to calling entity
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------
(
@pc_Txnid   	CHAR(16),  
@pc_Status   	CHAR(1),
@pi_CDOID    	INT = 0,
@pnv_Msg      	NVARCHAR(MAX) = 'NA'
)
AS
DECLARE @i_ErrorSeverity 	INT  
DECLARE @i_ErrorState 		INT 
DECLARE @i_NumTables 		INT
DECLARE @i_TblNum 		INT
DECLARE @v_Table 		VARCHAR(50)
DECLARE @nv_ErrorMessage 	NVARCHAR(4000) 
DECLARE @nv_Sql 		NVARCHAR(4000)
DECLARE @nv_Sql2 		NVARCHAR(4000)
BEGIN
	SELECT @i_NumTables=Convert(INT,TValue)
	FROM InsiteSiteInfo
	WHERE TName = 'DataStoreInsertTables' 
	
	SET @nv_Sql = N'Master SET Status = ''' + @pc_Status + N'''  , RecordDate = GETDATE()'
	SET @pnv_Msg = RTRIM(@pnv_Msg)
	IF LEN(@pnv_Msg) > 0
		SET @nv_Sql= @nv_Sql + N', ERROR= ''' + @pnv_Msg + N''''
	IF @pi_CDOID > 0
		SET @nv_Sql = @nv_Sql + N', CDOID = ' + CONVERT(NVARCHAR,@pi_CDOID) 

	SET @nv_Sql = @nv_Sql + N' WHERE TxnId = ''' + @pc_Txnid + N''''    
     	
	BEGIN TRY
		BEGIN TRAN
			IF @pc_Status = 'R'
				INSERT INTO csiLog(Source,Message) VALUES ('DataStore','A transaction was rolled back at the server. TxnID:'+ @pc_TxnID + ' CDOID:' + CONVERT(NVARCHAR,@pi_CDOID)+ ' Message:' + @pnv_Msg)

    			SET @i_TblNum = 0
			WHILE @i_TblNum <= @i_NumTables
			BEGIN
				IF @i_TblNum = 0
					SET @v_Table= 'Updates'
				ELSE
					SET @v_Table= 'Inserts' + CONVERT(NVARCHAR, @i_TblNum)	
		
				SET @nv_Sql2 = N'UPDATE Datastore' +  @v_Table + @nv_Sql 
		
				EXEC(@nv_Sql2)
				SET @i_TblNum = @i_TblNum + 1
			END 
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		SELECT
        	@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - ' + ERROR_MESSAGE(),  
        	@i_ErrorSeverity = ERROR_SEVERITY(),  
        	@i_ErrorState = ERROR_STATE()

		IF @@TRANCOUNT > 0
			ROLLBACK TRAN

    		RAISERROR (@nv_ErrorMessage,  
               	@i_ErrorSeverity,   
               	@i_ErrorState   
               	);  
	END CATCH
END
GO





CREATE OR ALTER PROCEDURE csiSetTxnStatus
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name: 	csiSetTxnStatus
-- Params: 	<in> 	@TxnId	 			NVARCHAR(16)
--		<in>	@Status				NVARCHAR(1)
--		<in>	@CDOID	 			INT
--		<in>	@HMsg	 			NVARCHAR(MAX)
--
-- Descr: 	Calls csiIpdateDSMastRec via CSILOOPBACK linked server to make autonomous transaction
--
-- HISTORY:
--	06/13/2016	Dan Maloney	Changed variable naming to prefix with data type.  Example @sql Varchar(512) has become @nv_Sql NVARCHAR(4000)
--	06/13/2016	Dan Maloney	Added error handling throughout with BEGIN TRY/END TRY and BECGIN CATCH/END CATCH
--	06/15/2016	Dan Maloney	Added autonomous transaction using csiloopback link server call in csiSetTxnStatus and insert triggers on detal queue tables
--					(i.e. DataStoreIns1Trg, DataStoreIns2Trg, DataStoreUpdTrg)
--	06/15/2016	Dan Maloney	Added RAISEERROR in CATCH blocks to raise error back to calling entity
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------
(
@Txnid   	NVARCHAR(16),  
@Status   	NVARCHAR(1),
@CDOID    	INT = 0,
@Msg      	NVARCHAR(MAX) = 'NA'
)
AS
DECLARE @i_ErrorSeverity 	INT
DECLARE @i_ErrorState 		INT 
DECLARE @nv_ErrorMessage 	NVARCHAR(4000) 
DECLARE @nv_Sql 		NVARCHAR(4000)
BEGIN
	BEGIN TRY
 		-- Call via linked server loopback to make autonomous transaction
		SET @msg = Replace(@msg, '''', '') --remove string breaking character FF9831376
		SET @nv_Sql = N'EXEC [CSILOOPBACK].[' + DB_NAME() + N'].[' + SCHEMA_NAME() + N'].csiUpdateDSMastRec ''' +  @TxnId + N''',''' + @Status + N''',' + CONVERT(NVARCHAR, @CDOID) +  N',''' + @Msg + N''''
		EXEC(@nv_Sql)	
	END TRY
	BEGIN CATCH
		SELECT
        	@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - ' + ERROR_MESSAGE(),  
        	@i_ErrorSeverity = ERROR_SEVERITY(),  
        	@i_ErrorState = ERROR_STATE()

		IF @@TRANCOUNT > 0
			ROLLBACK TRAN

    		RAISERROR (@nv_ErrorMessage,  
               	@i_ErrorSeverity,   
               	@i_ErrorState   
               	);  
	END CATCH
END
GO





--Create the Inserted Stored Proc
CREATE OR ALTER PROCEDURE csiInsertDSMastRec 
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name: 	csiInsertDSMasctRec
-- Params: 	<in> 	@pv_TableName 			VARCHAR(50)	(DATASTOREUPDATES, DATASTOREINSERTS1,2,3,4...) 
--		<in>	@pc_TxnId 			CHAR(16)
--		<in>	@pc_TxnType	 		CHAR(1)
--		<in>	@pv_Host	 		VARCHAR(50)
--
-- Descr: 	Inserts rcord into one of the detail remote queue tables
--
-- HISTORY:
--	06/13/2016	Dan Maloney	Changed variable naming to prefix with data type.  Example @sql Varchar(512) has become @nv_Sql NVARCHAR(4000)
--	06/13/2016	Dan Maloney	Added error handling throughout with BEGIN TRY/END TRY and BECGIN CATCH/END CATCH
--	06/13/2016	Dan Maloney	Added explicit transactions handling for DML
--	06/15/2016	Dan Maloney	Added RAISEERROR in CATCH blocks to raise error back to calling entity
--
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------
(
@pv_TableName 		VARCHAR(50), 
@pc_TxnId 		CHAR(16), 
@pc_TxnType 		CHAR(1), 
@pv_Host 		VARCHAR(50)
)
AS
DECLARE @i_ErrorSeverity 	INT  
DECLARE @i_ErrorState 		INT 
DECLARE @nv_ErrorMessage 	NVARCHAR(4000)
DECLARE @nv_Sql 		NVARCHAR(4000) 
BEGIN
	BEGIN TRY
		BEGIN TRAN
         		SET @nv_Sql = N'INSERT INTO ' + @pv_TableName + N' (TxnId, TxnType, Server) VALUES (''' + @pc_TxnID + N''', ''' + @pc_TxnType + N''',''' + @pv_Host + N''')'
	 		EXEC (@nv_Sql)
		COMMIT TRAN
	END TRY
	BEGIN CATCH
    		SELECT   
        	@nv_ErrorMessage = CONVERT(NVARCHAR(10), ERROR_NUMBER()) + N' - ' + ERROR_MESSAGE(),  
        	@i_ErrorSeverity = ERROR_SEVERITY(),  
        	@i_ErrorState = ERROR_STATE()

		IF @@TRANCOUNT > 0
			ROLLBACK TRAN

    		RAISERROR (@nv_ErrorMessage,  
               	@i_ErrorSeverity,   
               	@i_ErrorState   
               	);  
	END CATCH
END
GO

