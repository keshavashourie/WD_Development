ALTER PROCEDURE CSI_PurgeUtil_Execute_ActivateSetup 
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_Execute_ActivateSetup
                     Note : Only support scheduling up to hour interval.  
                            Does not support minute interval.
  Author           : Benny.Chia 
  Date             : 25 Sep 2015
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvSetupName                  NVARCHAR(40)
    , @pvJobStartDate               Datetime = NULL
	, @pvJobEndDate					Datetime = NULL
    , @pvJobInterval                INT = 1
    , @pvIntervalType               NVARCHAR(10) = 'DAILY' -- MINUTE, HOURLY, DAILY, WEEKLY, MONTHLY
    ) 
AS
DECLARE @vMessage                   NVARCHAR(4000)=''; -- Message text.
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               SYSNAME = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE @vJobName                   SYSNAME;
DECLARE @vJobStartDate              Datetime = @pvJobStartDate;
DECLARE @vJobEndDate				Datetime = @pvJobEndDate;
DECLARE @vJobNo                     BINARY(16);
DECLARE @vJobStartDateNumber        INT;
DECLARE @vJobStartTimeNumber        INT;
DECLARE @vJobEndDateNumber          INT;
DECLARE @vJobEndTimeNumber          INT;
--
DECLARE @vCommand                   NVARCHAR(MAX);
DECLARE @vDbName                    SYSNAME;
DECLARE @vReturnCode                INTEGER;
--
-- Frequency types (from msdb.dbo.sysschedules)
DECLARE @vFreqType_Daily            INT = 4;
DECLARE @vFreqType_Weekly           INT = 8;
DECLARE @vFreqType_Monthly          INT = 16;
--
-- Subday types
DECLARE @vFreqSubDayType_Minute             INT = 4;
DECLARE @vFreqSubDayType_Hour				INT = 8;
DECLARE @vFreqSubDayType_AtSpecifiedTime	INT = 1;
--
-- For frequency
DECLARE @vDayOfWeek                 INT; -- Bitmask for days (1=Sun, 2=Mon, 4=Tue, etc.)
DECLARE @vDayOfMonth                INT; -- Day of month to run
--
DECLARE @vArchiveRequired           INT = 1;
DECLARE @vStep_id                   INT = 0;
--
BEGIN 
    SET NOCOUNT ON;
    SET @vProgID = @vObject_Name + '.START'; 
    --
    BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
        ---------------------------------------------------------------------------
        -- Validations
        ---------------------------------------------------------------------------
        SET @vProgID = @vObject_Name + '.Validate parameters';
        IF @pvSetupName IS NULL 
            RAISERROR ('Setup Name must not be blank', 16, 1);
            
        -- Validate interval type
        IF UPPER(@pvIntervalType) NOT IN ('MINUTE', 'HOURLY', 'DAILY', 'WEEKLY', 'MONTHLY')
            RAISERROR ('Invalid interval type. Must be MINUTE, HOURLY, DAILY, WEEKLY, or MONTHLY', 16, 1);
            
        -- Validate interval value based on type
        IF UPPER(@pvIntervalType) = 'MINUTE' AND (@pvJobInterval < 1 OR @pvJobInterval > 60)
            RAISERROR ('Minute interval must be between 1 and 60', 16, 1);

        IF UPPER(@pvIntervalType) = 'HOURLY' AND (@pvJobInterval < 1 OR @pvJobInterval > 23)
            RAISERROR ('Hourly interval must be between 1 and 23', 16, 1);
            
        IF UPPER(@pvIntervalType) = 'DAILY' AND (@pvJobInterval < 1 OR @pvJobInterval > 365)
            RAISERROR ('Daily interval must be between 1 and 365', 16, 1);
            
        IF UPPER(@pvIntervalType) = 'WEEKLY' AND (@pvJobInterval < 1 OR @pvJobInterval > 52)
            RAISERROR ('Weekly interval must be between 1 and 52', 16, 1);
            
        IF UPPER(@pvIntervalType) = 'MONTHLY' AND (@pvJobInterval < 1 OR @pvJobInterval > 12)
            RAISERROR ('Monthly interval must be between 1 and 12', 16, 1);
            
        ---------------------------------------------------------------------------
        -- Validate if the setup exist.
        ---------------------------------------------------------------------------
        SET @vProgID = @vObject_Name + '.Validate Setup record for existence';
        IF (SELECT COUNT(1) 
            FROM CSI_PURGEUTIL_SETUP ps
            WHERE UPPER(ps.setupname) = UPPER(@pvSetupName)) = 0  
           RAISERROR ('Setup record is not found in the CSI_PURGEUTIL_SETUP table', 16, 1);
        --
        IF @vJobStartDate IS NULL 
            SET @vJobStartDate = GETDATE();
            
        -- Get the year, month and day part
        SET @vJobStartDateNumber = CONVERT(INT, 
            CONVERT(NVARCHAR, DATEPART(YEAR, @vJobStartDate)) + 
            RIGHT('0' + CONVERT(NVARCHAR, DATEPART(MONTH, @vJobStartDate)), 2) + 
            RIGHT('0' + CONVERT(NVARCHAR, DATEPART(DAY, @vJobStartDate)), 2));
                                   
        -- Get the hour, minute and second part
        SET @vJobStartTimeNumber = CONVERT(INT,
            RIGHT('0' + CONVERT(NVARCHAR, DATEPART(HOUR, @vJobStartDate)), 2) + 
            RIGHT('0' + CONVERT(NVARCHAR, DATEPART(MINUTE, @vJobStartDate)), 2) + 
            RIGHT('0' + CONVERT(NVARCHAR, DATEPART(SECOND, @vJobStartDate)), 2));

		IF @vJobEndDate IS NULL 
		BEGIN
			-- No End date
			SET @vJobEndDateNumber = 99991231;
			SET @vJobEndTimeNumber = 235959;
		END
		ELSE
		BEGIN
			-- Get the year, month and day part
			SET @vJobEndDateNumber = CONVERT(INT, 
				CONVERT(NVARCHAR, DATEPART(YEAR, @vJobEndDate)) + 
				RIGHT('0' + CONVERT(NVARCHAR, DATEPART(MONTH, @vJobEndDate)), 2) + 
				RIGHT('0' + CONVERT(NVARCHAR, DATEPART(DAY, @vJobEndDate)), 2));
                                   
			-- Get the hour, minute and second part
			SET @vJobEndTimeNumber = CONVERT(INT,
				RIGHT('0' + CONVERT(NVARCHAR, DATEPART(HOUR, @vJobEndDate)), 2) + 
				RIGHT('0' + CONVERT(NVARCHAR, DATEPART(MINUTE, @vJobEndDate)), 2) + 
				RIGHT('0' + CONVERT(NVARCHAR, DATEPART(SECOND, @vJobEndDate)), 2));
		END

        ---------------------------------------------------------------------------
        -- Log a start message 
        SET @vMessage = 'Activating Job Scheduling for ''' + @pvSetupName + ''' purging started. ';
        SET @vMessage = @vMessage + 'Job Started on ''' + CAST(@vJobStartDate AS nvarchar) + '''';
        SET @vMessage = @vMessage + ' with a job interval of ' + CAST(@pvJobInterval AS nvarchar) + ' ' + @pvIntervalType;
        EXECUTE CSI_PurgeUtil_Global_LogMessage @vMessage;
        
        BEGIN TRANSACTION;
        SET @vJobName = 'CSI_PurgeUtil_Execute_PurgeBySetup @pvSetupName = ''' + @pvSetupName + '''';
        --------------------------------------------------------------------------------------------------
        -- Delete the job if exist
        --------------------------------------------------------------------------------------------------
        IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE (name = @vJobName))
        BEGIN TRY 
            SET @vProgID = @vObject_Name + '.Deleting Job';
            EXECUTE msdb.dbo.sp_delete_job
                @job_name = @vJobName;
            --
        END TRY
        BEGIN CATCH
            SET @ErrorMessage = ERROR_MESSAGE(); 
            SET @vProgID = @vObject_Name + '.' + 'Fail to delete the job for setup ' + @pvSetupName; 
            RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --------------------------------------------------------------------------------------------------
        -- Create the job
        --------------------------------------------------------------------------------------------------
        BEGIN TRY 
            SET @vProgID = @vObject_Name + '.Creating Job';
            EXECUTE @vReturnCode = msdb.dbo.sp_add_job 
                @job_id = @vJobNo OUTPUT, 
                @job_name = @vJobName,
                @description = @vJobName;
            --
        END TRY
        BEGIN CATCH
            SET @ErrorMessage = ERROR_MESSAGE(); 
            SET @vProgID = @vObject_Name + '.' + 'Fail to create the job for setup ' + @pvSetupName; 
            RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --------------------------------------------------------------------------------------------------
        -- Create the job step
        --------------------------------------------------------------------------------------------------
        BEGIN TRY 
            SET @vProgID = @vObject_Name + '.Creating Job Step';
            SET @vCommand = 'EXEC CSI_PurgeUtil_Execute_PurgeBySetupJob ''' + @pvSetupName + '''';
            SET @vDBName = DB_NAME();
            SET @vStep_id = @vStep_id + 1;
            EXECUTE @vReturnCode = msdb.dbo.sp_add_jobstep 
                @job_name = @vJobName,
                @step_id = @vStep_id,
                @step_name = N'PurgeBySetup',
                @command = @vCommand, 
                @database_name = @vDbName,
                @server = N'',
                @on_success_action = 1,  -- Quit with success
                @on_fail_action = 2,  -- Quit with failure
                @subsystem = 'TSQL',
				--@proxy_name=N'InSite Proxy' -- A job step must specify a proxy unless the creator of the job step is a member of the sysadmin fixed security role.
                @proxy_name = NULL;
            --
        END TRY
        BEGIN CATCH
            SET @ErrorMessage = ERROR_MESSAGE(); 
            SET @vProgID = @vObject_Name + '.' + 'Fail to create the job step for setup ' + @pvSetupName; 
            RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --------------------------------------------------------------------------------------------------
        -- Create the job schedule based on interval type
        --------------------------------------------------------------------------------------------------
        BEGIN TRY 
            SET @vProgID = @vObject_Name + '.Creating Job Schedule';
            
            IF UPPER(@pvIntervalType) = 'MINUTE'
            BEGIN
                -- Minute schedule
                EXECUTE @vReturnCode = msdb.dbo.sp_add_jobschedule 
                    @job_name = @vJobName,
                    @name = N'Minute Schedule', 
                    @freq_type = @vFreqType_Daily, 
                    @freq_interval = 1, -- Every day
                    @freq_subday_type = @vFreqSubDayType_Minute, 
                    @freq_subday_interval = @pvJobInterval,
                    @active_start_date = @vJobStartDateNumber,
                    @active_start_time = @vJobStartTimeNumber,
                    @active_end_date = @vJobEndDateNumber, -- Same day
                    @active_end_time = @vJobEndTimeNumber;
            END
            ELSE IF UPPER(@pvIntervalType) = 'HOURLY'
            BEGIN
                -- Hourly schedule
                EXECUTE @vReturnCode = msdb.dbo.sp_add_jobschedule 
                    @job_name = @vJobName,
                    @name = N'Hourly Schedule', 
                    @freq_type = @vFreqType_Daily, 
                    @freq_interval = 1, -- Every day
                    @freq_subday_type = @vFreqSubDayType_Hour, 
                    @freq_subday_interval = @pvJobInterval,
                    @active_start_date = @vJobStartDateNumber,
                    @active_start_time = @vJobStartTimeNumber,
                    @active_end_date = @vJobEndDateNumber, -- Same day
                    @active_end_time = @vJobEndTimeNumber;
            END
            ELSE IF UPPER(@pvIntervalType) = 'DAILY'
            BEGIN
                -- Daily schedule
                EXECUTE @vReturnCode = msdb.dbo.sp_add_jobschedule 
                    @job_name = @vJobName,
                    @name = N'Daily Schedule', 
                    @freq_type = @vFreqType_Daily, 
                    @freq_interval = @pvJobInterval, -- Every X days
                    @freq_subday_type = @vFreqSubDayType_AtSpecifiedTime, 
                    @freq_subday_interval = 1, -- Once per day at the specified time
                    @active_start_date = @vJobStartDateNumber,
                    @active_start_time = @vJobStartTimeNumber,
                    @active_end_date = @vJobEndDateNumber,
                    @active_end_time = @vJobStartTimeNumber;
            END
            ELSE IF UPPER(@pvIntervalType) = 'WEEKLY'
            BEGIN
                -- Weekly schedule (runs on the same day of week as start date)
                SET @vDayOfWeek = POWER(2, DATEPART(WEEKDAY, @vJobStartDate) - 1);
                
                EXECUTE @vReturnCode = msdb.dbo.sp_add_jobschedule 
                    @job_name = @vJobName,
                    @name = N'Weekly Schedule', 
                    @freq_type = @vFreqType_Weekly, 
                    @freq_interval = @vDayOfWeek, -- 1-Sun, 2-Mon, 4-Tue, 8-Wed...
                    @freq_recurrence_factor = @pvJobInterval,  -- Reoccur Every X week
                    @freq_subday_type = @vFreqSubDayType_AtSpecifiedTime, 
                    @freq_subday_interval = 1, -- Once per week at the specified time
                    @active_start_date = @vJobStartDateNumber,
                    @active_start_time = @vJobStartTimeNumber,
                    @active_end_date = @vJobEndDateNumber,
                    @active_end_time = @vJobStartTimeNumber
            END
            ELSE IF UPPER(@pvIntervalType) = 'MONTHLY'
            BEGIN
                -- Monthly schedule (runs on the same day of month as start date)
                SET @vDayOfMonth = DATEPART(DAY, @vJobStartDate);
                
                EXECUTE @vReturnCode = msdb.dbo.sp_add_jobschedule 
                    @job_name = @vJobName,
                    @name = N'Monthly Schedule', 
                    @freq_type = @vFreqType_Monthly, 
                    @freq_interval = @vDayOfMonth,
                    @freq_recurrence_factor = @pvJobInterval, -- Every X months
                    @freq_subday_type = @vFreqSubDayType_AtSpecifiedTime, 
                    @freq_subday_interval = 1, -- Once per month at the specified time
                    @active_start_date = @vJobStartDateNumber,
                    @active_start_time = @vJobStartTimeNumber,
                    @active_end_date = @vJobEndDateNumber,
                    @active_end_time = @vJobStartTimeNumber
            END
            --
        END TRY
        BEGIN CATCH
            SET @ErrorMessage = ERROR_MESSAGE(); 
            SET @vProgID = @vObject_Name + '.' + 'Fail to create the job schedule for setup ' + @pvSetupName; 
            RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --------------------------------------------------------------------------------------------------
        -- Add job to the job server
        --------------------------------------------------------------------------------------------------
        BEGIN TRY 
            SET @vProgID = @vObject_Name + '.Adding Job to the Job Server';
            EXECUTE @vReturnCode = msdb.dbo.sp_add_jobserver 
                @job_name = @vJobName,
                @server_name = N'(LOCAL)';
            --
        END TRY
        BEGIN CATCH
            SET @ErrorMessage = ERROR_MESSAGE(); 
            SET @vProgID = @vObject_Name + '.' + 'Fail to add the job ' + @vJobName + 'to job server'; 
            RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        ---------------------------------------------------------------------------
        -- Activate the setup
        ---------------------------------------------------------------------------
        BEGIN TRY 
            UPDATE CSI_PURGEUTIL_SETUP
            SET JobNo = @vJobNo,
                JobStartDate = @vJobStartDate,
                JobEndDate = @pvJobEndDate,
                JobInterval = @pvJobInterval,
                JobIntervalType = @pvIntervalType
            WHERE SetupId = (SELECT SetupId FROM CSI_PURGEUTIL_SETUP WHERE SetupName = @pvSetupName);
            --
            SET @vProgID = @vObject_Name + '.SetupStatus updated to ACTIVE'; 
        END TRY
        BEGIN CATCH
            SET @ErrorMessage = ERROR_MESSAGE(); 
            SET @vProgID = @vObject_Name + '.' + 'Fail to update the setup ' + @pvSetupName + ' to ACTIVE status'; 
            RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --------------------------------------------------------------------------
        IF @@TRANCOUNT > 0 
            COMMIT TRANSACTION;
            
        -- Log a completion message 
        SET @vMessage = 'Activating Job Scheduling for ''' + @pvSetupName + ''' purging completed. ';
        EXECUTE CSI_PurgeUtil_Global_LogMessage @vMessage;
        --
        SET @vProgID = @vObject_Name + '.SUCCESSFUL'; 
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            ROLLBACK TRANSACTION;
        IF @ErrorMessage IS NULL
            SET @ErrorMessage = ERROR_MESSAGE();
        IF @ErrorSeverity IS NULL
            SET @ErrorSeverity = ERROR_SEVERITY(); 
        IF @ErrorState IS NULL
            SET @ErrorState = ERROR_STATE();
        IF @vProgID IS NULL
            SET @vProgID = @vObject_Name + '.OTHER ERROR'; 
        EXECUTE CSI_PurgeUtil_Global_Log @pvProgID = @vProgID, @pvErrMsg = @ErrorMessage; 
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO