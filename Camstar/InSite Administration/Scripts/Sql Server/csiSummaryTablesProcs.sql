-- Copyright Siemens 2025 
DECLARE @tmp Integer;
DECLARE @sql nvarchar(512);
DECLARE @roleName nvarchar(50);
DECLARE @dbName nvarchar(50);
DECLARE @msg nvarchar(512);
BEGIN
   -- Verify that the database user is a member of the necessary role.
   -- Because the role is in another database (msdb), and we have to use the 
   -- "use <database>" statement, we have to put the query into a dynamic
   -- SQL statement so our current active database target is retained. Otherwise,
   -- the rest of the statements in this file would be executed against msdb.
   SET @dbName = N'msdb';
   SET @roleName = N'SQLAgentUserRole';
   --
   SET @sql = N'USE ' + @dbName + N'; SELECT @tmp = ISNULL(IS_MEMBER(@roleName),0);'
   EXEC sp_executesql @sql, N'@roleName nvarchar(50), @tmp Integer OUTPUT', @roleName, @tmp OUTPUT
   IF (@tmp = NULL OR @tmp = 0)
   BEGIN
      SET @msg = N'Database user is not a member of "' + @dbName + N'\' + @roleName + N'", which is required for Summary Tables. Please refer to the install guide for prerequisite setup.'
      RAISERROR (@msg,18,1);
   END
END
GO
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSummaryTables_GetNextRunDate' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSummaryTables_GetNextRunDate
GO
--------------------------------------------------------------------
-- Name: csiSummaryTables_GetNextRunDate
-- Params: 
--
-- Descr: For the given SummaryTableDef item and a given date/time,
--        find the next date in which this job should run.
--
-- History:
-- 
CREATE PROCEDURE csiSummaryTables_GetNextRunDate
				@pId char(16),
				@pCurrentDate datetime,
				@pNextDate datetime output
AS
   DECLARE @v_CurrDate     datetime
   DECLARE @v_Hours        nvarchar(255)
   DECLARE @v_DaysOfWeek   nvarchar(255)
   DECLARE @v_DaysOfMonth  nvarchar(255)
   DECLARE @v_Months       nvarchar(255)
   DECLARE @v_NextDate     datetime
   DECLARE @v_ContinueLoop bit
   DECLARE @v_SQL          nvarchar(512)
   DECLARE @FetchStatus    int
   DECLARE @ErrorStatus    int
   DECLARE @RowCount       int
   DECLARE @Dummy          nvarchar(1)
   DECLARE @cnt int
BEGIN
   -- We only go as granular as hours, so take the input date, strip off the minutes and seconds
   SET @v_CurrDate = @pCurrentDate
   SET @v_NextDate = CONVERT(datetime, CAST(DATEPART(YEAR,@v_CurrDate) AS nvarchar)+N'-'+CAST(DATEPART(MONTH, @v_CurrDate) AS nvarchar) + N'-' + CAST(DATEPART(DAY, @v_CurrDate) AS nvarchar) + N' ' +CAST(DATEPART(HOUR, @v_CurrDate) AS nvarchar) + N':00:00', 20)
      
   -- The configuration contains comma-delimited lists of selected hours, days, months, etc.
   -- If any of these are null, it means run for every item (e.g. every hour)
   -- Hours: 0-23
   -- DaysOfWeek: 1-7 (1=Sunday)
   -- DaysOfMonth: 1-31 (depending on month)
   -- Months: 1-12
   SELECT @v_Hours=ScheduleHours,
          @v_DaysOfWeek=ScheduleDaysOfWeek,
          @v_DaysOfMonth=ScheduleDaysOfMonth,
          @v_Months=ScheduleMonths
   FROM SummaryTableDef
   WHERE SummaryTableDefId=@pId;
   ----
   -- Trim of the trailing comma
   IF (SUBSTRING(@v_Hours,LEN(@v_Hours),1)=',') SET @v_Hours = SUBSTRING(@v_Hours,1,LEN(@v_Hours)-1)
   IF (SUBSTRING(@v_DaysOfWeek,LEN(@v_DaysOfWeek),1)=',') SET @v_DaysOfWeek = SUBSTRING(@v_DaysOfWeek,1,LEN(@v_DaysOfWeek)-1)
   IF (SUBSTRING(@v_DaysOfMonth,LEN(@v_DaysOfMonth),1)=',') SET @v_DaysOfMonth = SUBSTRING(@v_DaysOfMonth,1,LEN(@v_DaysOfMonth)-1)
   IF (SUBSTRING(@v_Months,LEN(@v_Months),1)=',') SET @v_Months = SUBSTRING(@v_Months,1,LEN(@v_Months)-1)
   
   ---- Find the next run date based on the configuration.
   ---- The SummaryTableDef allows each summary to be executed based on certain:
   ----   HoursOfDay
   ----   DaysOfWeek
   ----   DaysOfMonth
   ----   Months
   ---- Each of these columns contains a comma-delimted list of numbers. e.g. '1,3,5,7,9'.
   ---- If any of these columns is NULL, it means that the summary should run for EVERY (e.g. every day of the week).
   ---- There may be a more efficient algorithm, but the below logic simply steps ahead from the current
   ---- date by 1-hour increments and checks to see if that date/time matches the configured hours, days, months
   SET @v_NextDate = DATEADD(hour, 1, @v_NextDate); -- The shortest update time is 1 hour, so start there and then find the next run time
   SET @v_ContinueLoop = 1
   
   WHILE (@v_ContinueLoop = 1)
   BEGIN
   	  SET @v_SQL = N'SELECT @Dummy=''X'' WHERE 1=1 ';
      IF (LEN(@v_Hours) > 0) 
         SET @v_SQL = @v_SQL+N' AND '+CAST(DATEPART(HOUR, @v_NextDate) AS nvarchar)+N' IN ('+@v_Hours+N')';
      IF (LEN(@v_DaysOfWeek) > 0) 
         SET @v_SQL = @v_SQL+N' AND '+CAST(DATEPART(WEEKDAY, @v_NextDate) AS nvarchar)+N' IN ('+@v_DaysOfWeek+N')';
      IF (LEN(@v_DaysOfMonth) > 0) 
         SET @v_SQL = @v_SQL+N' AND '+CAST(DATEPART(DAY, @v_NextDate) AS nvarchar)+N' IN ('+@v_DaysOfMonth+N')';
      IF (LEN(@v_Months) > 0) 
         SET @v_SQL = @v_SQL+N' AND '+CAST(DATEPART(MONTH, @v_NextDate) AS nvarchar)+N' IN ('+@v_Months+N')';

      EXEC sp_executesql @v_SQL, N'@Dummy nvarchar(1)',@Dummy
      SELECT @FetchStatus=@@fetch_status, @ErrorStatus = @@Error, @RowCount=@@ROWCOUNT
      IF (@RowCount>0)
         SET @v_ContinueLoop=0
      ELSE      
         SET @v_NextDate = DATEADD(hour, 1, @v_NextDate);
   END -- WHILE LOOP
   
   SET @pNextDate=@v_NextDate;
END
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSummaryTables_RunSingle' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSummaryTables_RunSingle
GO
--------------------------------------------------------------------
-- Name: csiSummaryTables_RunSingle
-- Params: 
--
-- Descr: Processes a single summary job regardless of whether or
--        not it is enabled.
--
-- History:
-- 
CREATE PROCEDURE csiSummaryTables_RunSingle 
				@pId char(16)
AS
	DECLARE @v_CurrDate datetime
	DECLARE @v_LastRunDate datetime
	DECLARE @v_AppendToTable bit
	DECLARE @v_TargetTableName nvarchar(255)
    DECLARE @v_SummarySQL nvarchar(max)
    DECLARE @v_TempTableName nvarchar(255)
    DECLARE @v_Cnt integer
    DECLARE @v_TblSQL nvarchar(max)
    DECLARE @v_NextDate datetime
    DECLARE @v_IsView bit
    DECLARE @v_ForceRefresh bit
    DECLARE @v_Message nvarchar(512)
    DECLARE @v_Type  nvarchar(5)
BEGIN
    SET NOCOUNT ON  
	--
	BEGIN TRY
	
	SELECT @v_TargetTableName=case IsView when N'0' then N'csiTbl_'+TargetTableName else N'csiView_'+TargetTableName end,
	       @v_SummarySQL=SummarySQL,
	       @v_LastRunDate=LastRunDate,
	       @v_AppendToTable=AppendToTable,
	       @v_IsView=IsView,
	       @v_ForceRefresh=ForceRefresh
	FROM SummaryTableDef
	WHERE SummaryTableDefId = @pId;
	--
	IF (LEN(@v_SummarySQL) = 0 )
		RETURN;
	IF (@v_ForceRefresh = 1)
	BEGIN
		-- Drop the view/table and reset the LastRunDate=NULL
		SELECT @v_Cnt=COUNT(*), @v_Type=MAX(Type)
		FROM sysobjects
		WHERE UPPER(Name)=UPPER(@v_TargetTableName);
		
		IF (@v_Cnt>0)
		BEGIN
			IF (@v_Type='U') -- Table 
				SET @v_TblSQL = N'DROP TABLE ' + @v_TargetTableName;
			IF (@v_Type='V') -- View
				SET @v_TblSQL = N'DROP VIEW ' + @v_TargetTableName;
			
			IF (LEN(@v_TblSQL)>0) 
				EXEC (@v_TblSQL);
		END
		SET @v_LastRunDate=NULL
	END
	SET @v_CurrDate = GETDATE();
	IF (@v_IsView = 1)
	BEGIN		
	    -- Create a view instead of a table
	    SELECT @v_Cnt=COUNT(*)
		FROM sysobjects
		WHERE UPPER(Name)=UPPER(@v_TargetTableName)
		AND Type='V';
		
		IF (@v_Cnt=0)
		BEGIN
		   SET @v_TblSQL = N'CREATE VIEW ' + @v_TargetTableName + N' AS ' + @v_SummarySQL
		   EXEC (@v_TblSQL);
		END
	END
	ELSE
	BEGIN
		-- Create an actual table rather than a view
		SELECT @v_Cnt=COUNT(*)
			FROM sysobjects
			WHERE UPPER(Name)=UPPER(@v_TargetTableName)
			AND Type='U';
			
	    IF (@v_AppendToTable=1 AND @v_Cnt>0) -- If we are appending AND the table already exists
		BEGIN	

			IF lower(@v_SummarySQL) like '%with %'
			BEGIN
				-- when recursive query is used, create a temporary view and then append data to the table using the view.
				-- as INSERT INTO will not work along with the recursive query.
				SET @v_TblSQL = N'CREATE VIEW vw_' + @v_TargetTableName + N' AS ' + @v_SummarySQL
				EXEC (@v_TblSQL); 
			
				SET @v_TblSQL = N'INSERT INTO ' + @v_TargetTableName + N' SELECT * FROM vw_' + @v_TargetTableName
				EXECUTE sp_executesql @v_TblSQL, N'@LastRunDate datetime', @LastRunDate = @v_LastRunDate
			
				SET @v_TblSQL = N'DROP VIEW vw_' + @v_TargetTableName
				EXEC (@v_TblSQL); 
			END
			ELSE
			BEGIN
				-- The SQL Query may have a parameter named :LastRunDate.
				-- Usage: INSERT INTO ExistingTable (SQL Query)
				SET @v_TblSQL = N'INSERT INTO ' + @v_TargetTableName + N' ' + @v_SummarySQL-- + N') a WHERE a.' + @v_AppendTimestampColumn + N'>=''' + CONVERT(nvarchar(125), @v_LastRunDate, 121) + N''''
				EXECUTE sp_executesql @v_TblSQL, N'@LastRunDate datetime', @LastRunDate = @v_LastRunDate		
			END
		END
		ELSE
		BEGIN
		    SET @v_TempTableName = N'tmp_'+@v_TargetTableName;
			SELECT @v_Cnt=COUNT(*)
			FROM sysobjects
			WHERE UPPER(Name)=UPPER(@v_TempTableName)
			AND Type='U';
		   
			IF (@v_Cnt>0)
			  EXEC (N'DROP TABLE '+@v_TempTableName);
		   
			SET @v_CurrDate = GETDATE();

			IF lower(@v_SummarySQL) like '%with %'
			BEGIN
			SET @v_TblSQL = N'CREATE VIEW vw_' + @v_TargetTableName + N' AS ' + @v_SummarySQL
		    EXEC (@v_TblSQL); 
			
			SET @v_TblSQL = N'SELECT * INTO '+@v_TempTableName+N' FROM vw_' + @v_TargetTableName
			EXECUTE sp_executesql @v_TblSQL, N'@LastRunDate datetime', @LastRunDate = @v_LastRunDate

			SET @v_TblSQL = N'DROP VIEW vw_' + @v_TargetTableName
			EXEC (@v_TblSQL); 
			END

			ELSE
			BEGIN
			-- The SQL Query may have a parameter named :LastRunDate.
			-- Usage: SELECT * INTO NewTable FROM (SQL Query) a    
			SET @v_TblSQL = N'SELECT * INTO '+@v_TempTableName+N' FROM ('
			SET @v_TblSQL = @v_TblSQL+@v_SummarySQL
			SET @v_TblSQL = @v_TblSQL+N') a'

			EXECUTE sp_executesql @v_TblSQL, N'@LastRunDate datetime', @LastRunDate = @v_LastRunDate
			END

			--
			SELECT @v_Cnt=COUNT(*)
			FROM sysobjects
			WHERE UPPER(Name)=UPPER(@v_TargetTableName)
			AND Type='U';
		    
			IF (@v_Cnt>0)
			  EXEC (N'DROP TABLE '+@v_TargetTableName);
		    
			EXEC sp_rename @v_TempTableName, @v_TargetTableName
		END
    END -- END ELSE (if view)

    EXEC csiSummaryTables_GetNextRunDate @pId, @v_CurrDate, @v_NextDate OUTPUT

    SET @v_Cnt = DATEDIFF(SECOND, @v_CurrDate, GETDATE())

    UPDATE SummaryTableDef
    SET LastRunDate = @v_CurrDate,
       LastRunMessage = NULL,
       LastRunSuccess = 1,
       NextRunDate = @v_NextDate,
       LastRunElapsedSeconds = @v_Cnt,
       ForceRefresh=0,
       ForceExecute=0
    WHERE SummaryTableDefId=@pId;

    END TRY 
    BEGIN CATCH
		SELECT @v_Message=ERROR_MESSAGE()
		UPDATE SummaryTableDef
			SET LastRunDate = @v_CurrDate,
			LastRunMessage = @v_Message,
			LastRunSuccess = 0,
			NextRunDate = @v_NextDate,
			LastRunElapsedSeconds = @v_Cnt,
			ForceRefresh=0,
            ForceExecute=0
			WHERE SummaryTableDefId=@pId;
    END CATCH
END
GO
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSummaryTables_RunSingleByName' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSummaryTables_RunSingleByName
GO
--------------------------------------------------------------------
-- Name: csiSummaryTables_RunSingleByName
-- Params: 
--
-- Descr: Processes a single summary job by name regardless of whether or
--        not it is enabled.
--
-- History:
-- 
CREATE PROCEDURE csiSummaryTables_RunSingleByName 
				@pName nvarchar(255)
AS
	DECLARE @v_ID char(16)	
BEGIN
    SET NOCOUNT ON
	SELECT @v_ID=SummaryTableDefId
	FROM SummaryTableDef
	WHERE SummaryTableDefName = @pName;
	--
	IF (@@rowcount > 0 )
		EXEC csiSummaryTables_RunSingle @v_ID
	--	
END
GO
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSummaryTables_RunScheduled' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSummaryTables_RunScheduled
GO
--------------------------------------------------------------------
-- Name: csiSummaryTables_RunScheduled
-- Params: 
--
-- Descr: Processes any waiting summary jobs that are enabled and not
--        marked with IsManuallyExecuted
--
-- History:
--
CREATE PROCEDURE csiSummaryTables_RunScheduled 
AS
   DECLARE @cJob   cursor
   DECLARE @v_ID   char(16)
   DECLARE @v_Name nvarchar(255)
     
BEGIN
   -- Find any summary jobs that are waiting to be processed
   SET @cJob = CURSOR FOR
      SELECT SummaryTableDefId, SummaryTableDefName
        FROM SummaryTableDef 
       WHERE (IsEnabled=1 OR IsView=1)
         AND (IsManuallyExecuted=0 OR IsView=1)
		 AND TargetTableName IS NOT NULL
         AND (NextRunDate<GETDATE() OR NextRunDate IS NULL)
         AND (EndDate>GETDATE() or EndDate IS NULL)
         AND (StartDate<=GETDATE() or StartDate IS NULL)
       UNION
       SELECT SummaryTableDefId, SummaryTableDefName
        FROM SummaryTableDef
		WHERE (ForceExecute=1 OR ForceRefresh=1)
		AND TargetTableName IS NOT NULL;
   OPEN @cJob
   FETCH NEXT FROM @cJob INTO @v_ID, @v_Name
   WHILE (@@fetch_status = 0)
   BEGIN
      PRINT 'Summary waiting: '+@v_Name
      EXEC csiSummaryTables_RunSingle @v_ID
      FETCH NEXT FROM @cJob INTO @v_ID, @v_Name    
   END

   CLOSE @cJob
   DEALLOCATE @cJob
   
END;
GO
--------------------------------------------------------------------------------
-- SCRIPT: csiSummaryTables_CreateJob
-- DESCR: Verifies and/or creates jobs in SQL Server Agent
-- HISTORY:
--
-- @ 2016  
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiSummaryTables_CreateJob' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiSummaryTables_CreateJob
GO

CREATE PROCEDURE csiSummaryTables_CreateJob
        @JobName varchar(255),
        @Description varchar(255),
        @Command varchar(255),
        @CommandType varchar(255)
AS
--
-- @ 2016  
--
DECLARE @JobID BINARY(16)  
DECLARE @ReturnCode INT    
DECLARE @DbName varchar(100)
BEGIN
	SET NOCOUNT ON

	SET @DbName=db_name()
	IF @CommandType IS NULL
	  SET @CommandType='TSQL'         
	BEGIN TRY   -- Wrap this because the job may already exist in which case an exception is thrown.        
		EXECUTE @ReturnCode = msdb.dbo.sp_add_job 
		 @job_id = @JobID OUTPUT , 
		 @job_name = @JobName, 
		 @description = @Description 
		IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
    END TRY 
    BEGIN CATCH
		RETURN
    END CATCH
    
	EXECUTE @ReturnCode = msdb.dbo.sp_add_jobstep 
	 @job_id = @JobID, 
	 @step_id = 1, 
	 @step_name = N'Step 1', 
	 @command = @Command, 
	 @database_name=@DbName,
	 @server = N'',
	 @on_fail_action = 2,
	 @subsystem = @CommandType
	 --@proxy_name=N'InSite Proxy'
	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

	EXECUTE @ReturnCode = msdb.dbo.sp_update_job 
	 @job_id = @JobID, 
	 @start_step_id = 1 
	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
	
	EXECUTE @ReturnCode = msdb.dbo.sp_add_jobschedule 
	 @job_id = @JobID, 
	 @name = N'Every Minute', 
	 @freq_type = 4, 
	 @freq_interval = 1, 
	 @freq_subday_type = 4, 
	 @freq_subday_interval = 1
	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback

	EXECUTE @ReturnCode = msdb.dbo.sp_add_jobserver 
	 @job_id = @JobID, 
	 @server_name = N'(LOCAL)'

QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

END
GO
EXEC	csiSummaryTables_CreateJob
		@JobName = N'Camstar Summary Tables (:DBNAME)',
		@Description = N'Camstar Summary Tables refresh job.',
		@Command = N'EXEC csiSummaryTables_RunScheduled',
		@CommandType = NULL
		
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
