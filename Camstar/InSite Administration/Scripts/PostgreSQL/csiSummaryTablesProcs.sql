-- Copyright Siemens 2023  
DO $$
DECLARE
	tmp integer;
    roleName varchar(50);
    dbName varchar(50);
    msg varchar(512);
BEGIN
	-- Verify that the database user is a member of the necessary role
    dbName := current_database();
    roleName := current_user;
    
    -- Perform the membership check using the pg_roles system catalog
    SELECT 1 INTO tmp
    FROM pg_roles
    WHERE rolname = roleName;
    
    IF tmp IS NULL OR tmp = 0 THEN
        msg := 'Database user is not a member of "' || dbName || '\\' || roleName || '", which is required for Summary Tables. Please refer to the install guide for prerequisite setup.';
        RAISE EXCEPTION '%', msg;
    END IF;
END;
$$;

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSummaryTables_GetNextRunDate')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSummaryTables_GetNextRunDate;
 	END IF;
END $$;
--------------------------------------------------------------------
-- Name: csiSummaryTables_GetNextRunDate
-- Params: 
--
-- Descr: For the given SummaryTableDef item and a given date/time,
--        find the next date in which this job should run.
--
-- History:
-- 
CREATE PROCEDURE csiSummaryTables_GetNextRunDate(
	pId char(16),
	pCurrentDate timestamp,
	out pNextDate timestamp
)
LANGUAGE plpgsql
AS $$
DECLARE
	v_CurrDate     timestamp;
    v_Hours        varchar(255);
    v_DaysOfWeek   varchar(255);
    v_DaysOfMonth  varchar(255);
    v_Months       varchar(255);
    v_NextDate     timestamp;
    v_ContinueLoop boolean;
    v_SQL          varchar(512);
    FetchStatus    integer;
    ErrorStatus    integer;
    RowCount       integer;
    Dummy          varchar(1);
    cnt integer;
BEGIN
	-- We only go as granular as hours, so take the input date, strip off the minutes and seconds
	v_CurrDate := pCurrentDate;
	v_NextDate := TO_TIMESTAMP(TO_CHAR(v_CurrDate, 'YYYY-MM-DD HH24:00:00'), 'YYYY-MM-DD HH24:MI:SS');

	-- The configuration contains comma-delimited lists of selected hours, days, months, etc.
	-- If any of these are null, it means run for every item (e.g. every hour)
	-- Hours: 0-23
	-- DaysOfWeek: 1-7 (1=Sunday)
	-- DaysOfMonth: 1-31 (depending on month)
	-- Months: 1-12
	SELECT ScheduleHours, ScheduleDaysOfWeek, ScheduleDaysOfMonth, ScheduleMonths INTO v_Hours, v_DaysOfWeek, v_DaysOfMonth, v_Months
	FROM SummaryTableDef
	WHERE SummaryTableDefId = pId;

	-- Trim of the trailing comma
	IF SUBSTRING(v_Hours FROM LENGTH(v_Hours) FOR 1) = ',' THEN
	    v_Hours := SUBSTRING(v_Hours FROM 1 FOR LENGTH(v_Hours) - 1);
	END IF;
	
	IF SUBSTRING(v_DaysOfWeek FROM LENGTH(v_DaysOfWeek) FOR 1) = ',' THEN
	    v_DaysOfWeek := SUBSTRING(v_DaysOfWeek FROM 1 FOR LENGTH(v_DaysOfWeek) - 1);
	END IF;
	
	IF SUBSTRING(v_DaysOfMonth FROM LENGTH(v_DaysOfMonth) FOR 1) = ',' THEN
	    v_DaysOfMonth := SUBSTRING(v_DaysOfMonth FROM 1 FOR LENGTH(v_DaysOfMonth) - 1);
	END IF;
	
	IF SUBSTRING(v_Months FROM LENGTH(v_Months) FOR 1) = ',' THEN
	    v_Months := SUBSTRING(v_Months FROM 1 FOR LENGTH(v_Months) - 1);
	END IF;
	
	--
	-- Find the next run date based on the configuration.
	-- The SummaryTableDef allows each summary to be executed based on certain:
	--   HoursOfDay
	--   DaysOfWeek
	--   DaysOfMonth
	--   Months
	-- Each of these columns contains a comma-delimted list of numbers. e.g. '1,3,5,7,9'.
	-- If any of these columns is NULL, it means that the summary should run for EVERY (e.g. every day of the week).
	-- There may be a more efficient algorithm, but the below logic simply steps ahead from the current
	-- date by 1-hour increments and checks to see if that date/time matches the configured hours, days, months
	v_NextDate := v_NextDate + INTERVAL '1' HOUR; -- The shortest update time is 1 hour, so start there and then find the next run time
	v_ContinueLoop:= TRUE;
    WHILE (v_ContinueLoop) LOOP
		--v_SQL := 'SELECT TO_TIMESTAMP(''' || TO_CHAR(v_NextDate,'MM/DD/YYYY HH24:MI:SS') || ''',''MM/DD/YYYY HH24:MI:SS'') WHERE 1=1';
        v_SQL := 'SELECT ''X'' AS Dummy WHERE 1=1';
	    IF (v_Hours IS NOT NULL) THEN
			v_SQL := v_SQL || ' AND EXTRACT(HOUR FROM TIMESTAMP ' || quote_literal(v_NextDate) || ') IN (' || v_Hours || ')';
        END IF;
		IF (v_DaysOfWeek IS NOT NULL) THEN
			v_SQL := v_SQL || ' AND EXTRACT(DOW FROM TIMESTAMP ' || quote_literal(v_NextDate) || ') IN (' || v_DaysOfWeek || ')';
		END IF;
		IF (v_DaysOfMonth IS NOT NULL) THEN
			v_SQL := v_SQL || ' AND EXTRACT(DAY FROM TIMESTAMP ' || quote_literal(v_NextDate) || ') IN (' || v_DaysOfMonth || ')';
		END IF;
		IF (v_Months IS NOT NULL) THEN
			v_SQL := v_SQL || ' AND EXTRACT(MONTH FROM TIMESTAMP ' || quote_literal(v_NextDate) || ') IN (' || v_Months || ')';
		END IF;
	
		-- Save query results into Dummy
		execute v_SQL into Dummy;

		-- if query returns NULL, add 1 hour, else return current v_NextDate
		if Dummy IS NULL THEN
			v_NextDate := v_NextDate + INTERVAL '1' HOUR;
		else
			v_ContinueLoop := FALSE;
		end if;
   END LOOP;
   pNextDate := v_NextDate;
END;
$$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSummaryTables_RunSingle')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSummaryTables_RunSingle;
 	END IF;
END $$;
--------------------------------------------------------------------
-- Name: csiSummaryTables_RunSingle
-- Params: 
--
-- Descr: Processes a single summary job regardless of whether or
--        not it is enabled.
--
-- History:
-- 
CREATE PROCEDURE csiSummaryTables_RunSingle(
	pId char(16)
)
LANGUAGE plpgsql
as $$
DECLARE
	v_CurrDate TIMESTAMP;
    v_LastRunDate TIMESTAMP;
    v_AppendToTable INTEGER;
    v_TargetTableName VARCHAR(255);
    v_SummarySQL TEXT;
    v_TempTableName VARCHAR(255);
    v_Cnt INTEGER;
    v_TblSQL TEXT;
    v_NextDate TIMESTAMP;
    v_IsView INTEGER;
    v_ForceRefresh INTEGER;
    v_Message TEXT;
    v_Type VARCHAR(5);
BEGIN
	SELECT
        CASE WHEN IsView = 0 THEN 'csiTbl_' || TargetTableName ELSE 'csiView_' || TargetTableName END,
        SummarySQL,
        LastRunDate,
        AppendToTable,
        IsView,
        ForceRefresh
    INTO
        v_TargetTableName,
        v_SummarySQL,
        v_LastRunDate,
        v_AppendToTable,
        v_IsView,
        v_ForceRefresh
    FROM SummaryTableDef
    WHERE SummaryTableDefId = pId;
	--
	IF (LENGTH(v_SummarySQL) = 0) THEN
		RETURN;
	END IF;
	
	IF (v_ForceRefresh = 1) THEN
		-- Drop the view/table and reset the LastRunDate=NULL
        SELECT COUNT(*), MAX(relkind) -- change 'type' to 'relkind'
        INTO v_Cnt, v_Type
        FROM pg_catalog.pg_class c
        JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
        WHERE c.relname = LOWER(v_TargetTableName);
            
        IF v_Cnt > 0 THEN
            IF v_Type = 'r' THEN -- Table (u in SQL SERVER)
                EXECUTE 'DROP TABLE ' || v_TargetTableName || ' CASCADE';
            ELSIF v_Type = 'v' THEN -- View
                EXECUTE 'DROP VIEW ' || v_TargetTableName || ' CASCADE';
            END IF;
        END IF;
            
        v_LastRunDate := NULL;
	END IF;
	
	v_CurrDate := CLOCK_TIMESTAMP();
	
	IF (v_IsView = 1) THEN
		-- Create a view instead of a table
        SELECT COUNT(*)
        INTO v_Cnt
        FROM pg_catalog.pg_class c
        JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
        WHERE c.relname = LOWER(v_TargetTableName)
        AND c.relkind = 'v';
        
        IF v_Cnt = 0 THEN
            EXECUTE 'CREATE VIEW ' || v_TargetTableName || ' AS ' || v_SummarySQL;
        END IF;
	ELSE
		-- Create an actual table rather than a view
        SELECT COUNT(*)
        INTO v_Cnt
        FROM pg_catalog.pg_class c
        JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
        WHERE c.relname = LOWER(v_TargetTableName)
        AND c.relkind = 'r';
		
		IF v_AppendToTable = 1 AND v_Cnt > 0 THEN -- If we are appending AND the table already exists
            -- The SQL Query may have a parameter named :LastRunDate.
            -- Usage: INSERT INTO ExistingTable (SQL Query)
			EXECUTE 'INSERT INTO '||v_TargetTableName||' '||v_SummarySQL USING v_LastRunDate;
		ELSE
			v_TempTableName := 'tmp_' || v_TargetTableName;
            SELECT COUNT(*)
            INTO v_Cnt
            FROM pg_catalog.pg_class c
            JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
            WHERE c.relname = LOWER(v_TempTableName)
            AND c.relkind = 'r';
			
			IF v_Cnt > 0 THEN
                EXECUTE 'DROP TABLE ' || v_TempTableName || ' CASCADE';
            END IF;
			
			v_CurrDate := CLOCK_TIMESTAMP();
			
			/* IF POSITION('WITH ' IN v_SummarySQL) > 0 THEN
                EXECUTE 'CREATE OR REPLACE VIEW vw_' || v_TargetTableName || ' AS ' || v_SummarySQL;
                EXECUTE 'INSERT INTO ' || v_TempTableName || ' SELECT * FROM vw_' || v_TargetTableName;
                EXECUTE 'DROP VIEW vw_' || v_TargetTableName;
            ELSE
                -- The SQL Query may have a parameter named :LastRunDate.
                -- Usage: SELECT * INTO NewTable FROM (SQL Query) a    
                EXECUTE 'INSERT INTO ' || v_TempTableName || ' SELECT * FROM (' || v_SummarySQL || ') a';
            END IF; */
			
			EXECUTE 'CREATE TABLE ' || v_TempTableName || ' AS ' || v_SummarySQL;	-- refer Oracle v_ErrLoc 110
			
			SELECT COUNT(*)
            INTO v_Cnt
            FROM pg_catalog.pg_class c
            JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace
            WHERE c.relname = LOWER(v_TargetTableName)
            AND c.relkind = 'r';
                
            IF v_Cnt > 0 THEN
                EXECUTE 'DROP TABLE ' || v_TargetTableName || ' CASCADE';
            END IF;
                
            EXECUTE 'ALTER TABLE ' || v_TempTableName || ' RENAME TO ' || v_TargetTableName;
		END IF;
	END IF;	-- END ELSE (if view)
	
	CALL csiSummaryTables_GetNextRunDate(pId, v_CurrDate, v_NextDate);
	
	v_Cnt := EXTRACT(EPOCH FROM (v_CurrDate - CLOCK_TIMESTAMP()));
	
	UPDATE SummaryTableDef
    SET LastRunDate = v_CurrDate,
        LastRunMessage = NULL,
        LastRunSuccess = 1,
        NextRunDate = v_NextDate,
        LastRunElapsedSeconds = v_Cnt,
        ForceRefresh = 0,
        ForceExecute = 0
    WHERE SummaryTableDefId = pId;
	EXCEPTION
        WHEN OTHERS then
            GET STACKED DIAGNOSTICS v_Message = MESSAGE_TEXT;
            UPDATE SummaryTableDef
            SET LastRunDate = v_CurrDate,
                LastRunMessage = v_Message,
                LastRunSuccess = 0,
                NextRunDate = v_NextDate,
                LastRunElapsedSeconds = v_Cnt,
                ForceRefresh = 0,
                ForceExecute = 0
            WHERE SummaryTableDefId = pId;
END;
$$;

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSummaryTables_RunSingleByName')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSummaryTables_RunSingleByName;
 	END IF;
END $$;
--------------------------------------------------------------------
-- Name: csiSummaryTables_RunSingleByName
-- Params: 
--
-- Descr: Processes a single summary job by name regardless of whether or
--        not it is enabled.
--
-- History:
-- 
CREATE PROCEDURE csiSummaryTables_RunSingleByName(
	pName varchar(255)
)
LANGUAGE plpgsql
as $$
DECLARE
	v_ID char(16);
BEGIN
	SELECT SummaryTableDefId
	INTO v_ID
	FROM SummaryTableDef
	WHERE SummaryTableDefName = pName;
	--
	IF FOUND THEN
		CALL csiSummaryTables_RunSingle(v_ID);
	END IF;
	--
END;
$$;

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSummaryTables_RunScheduled')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSummaryTables_RunScheduled;
 	END IF;
END $$;
--------------------------------------------------------------------
-- Name: csiSummaryTables_RunScheduled
-- Params: 
--
-- Descr: Processes any waiting summary jobs that are enabled and not
--        marked with IsManuallyExecuted
--
-- History:
--
CREATE PROCEDURE csiSummaryTables_RunScheduled()
LANGUAGE plpgsql
as $$
DECLARE
	v_ID char(16);
	v_Name varchar(255);
	cJob cursor FOR 
		SELECT SummaryTableDefId, SummaryTableDefName
        FROM SummaryTableDef 
		WHERE (IsEnabled=1 OR IsView=1)
         AND (IsManuallyExecuted=0 OR IsView=1)
		 AND TargetTableName IS NOT NULL
         AND (NextRunDate<CLOCK_TIMESTAMP() OR NextRunDate IS NULL)
         AND (EndDate>CLOCK_TIMESTAMP() or EndDate IS NULL)
         AND (StartDate<=CLOCK_TIMESTAMP() or StartDate IS NULL)
		UNION
		SELECT SummaryTableDefId, SummaryTableDefName
        FROM SummaryTableDef
		WHERE (ForceExecute=1 OR ForceRefresh=1)
		AND TargetTableName IS NOT NULL;
BEGIN
	OPEN cJob;
	
	LOOP
		FETCH NEXT FROM cJob INTO v_ID, v_Name;
		EXIT WHEN NOT FOUND;
		
		RAISE NOTICE 'Summary waiting: %', v_Name;
		CALL csiSummaryTables_RunSingle(v_ID);
	END LOOP;
	CLOSE cJob;
END;
$$;

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiSummaryTables_CreateJob')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiSummaryTables_CreateJob;
 	END IF;
END $$;
--------------------------------------------------------------------------------
-- SCRIPT: csiSummaryTables_CreateJob
-- DESCR: Verifies and/or creates jobs in PostgreSQL pgAgent Jobs
--
-- HISTORY:
--
CREATE PROCEDURE csiSummaryTables_CreateJob(JobName varchar(255), Description varchar(255), Command varchar(255), SchemaName varchar(255))
 LANGUAGE plpgsql
AS $procedure$
DECLARE
	--JobID     bytea;
    --v_Hours        varchar(255);
    DbName          varchar(100);
    jid integer;
    scid integer;
begin
	 DbName = current_database();
	-- DbName = current_schema;

	-- Creating a new job
	INSERT INTO pgagent.pga_job(
		jobjclid, 
		jobname, 
		jobdesc, 
		jobhostagent, 
		jobenabled
	) VALUES (
		1::integer, 
		JobName::text, 
		Description::text, 
		''::text, 
		true
	) RETURNING jobid INTO jid;

	-- Steps
	-- Inserting a step (jobid: NULL)
	INSERT INTO pgagent.pga_jobstep (
		jstjobid, 
		jstname, 
		jstenabled, 
		jstkind,
		jstconnstr,
		jstdbname, 
		jstonerror,
		jstcode, 
		jstdesc
	) VALUES (
		jid, 
		'Step 1'::text, 
		true, 
		's'::character(1),
		''::text, 
		DbName::name, 
		'f'::character(1),
		'SET SEARCH_PATH = ' || SchemaName || '; ' || COMMAND || ';'::text, 
		'Step 1 failed.'::text
	) ;

	-- Schedules
	-- Inserting a schedule
	INSERT INTO pgagent.pga_schedule(
		jscjobid,
		jscname, 
		jscdesc,
		jscenabled,
		jscstart,     
		jscminutes, 
		jschours, 
		jscweekdays, 
		jscmonthdays, 
		jscmonths
	) VALUES (
		jid, 
		'''Every Minute'''::text, 
		''::text, 
		true,
		clock_timestamp()::timestamp with time zone, 
		-- Minutes
		'{t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t}'::bool[]::boolean[],
		-- Hours
		'{t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t,t}'::bool[]::boolean[],
		-- Week days
		'{t,t,t,t,t,t,t}'::bool[]::boolean[],
		-- Month days
		'{f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f,f}'::bool[]::boolean[],
		-- Months
		'{f,f,f,f,f,f,f,f,f,f,f,f}'::bool[]::boolean[]
	) RETURNING jscid INTO scid;

END 
$procedure$;

-- Parameterized Create pgAgent Jobs Procedure Call
CALL csisummarytables_createjob(
    'Camstar Summary Tables ('||current_database()||')',
    'Camstar Summary Tables refresh job.',
    'CALL csisummarytables_runscheduled()',
    current_schema::varchar
);
