--
-- © 2018 Siemens Product Lifecycle Management Software Inc.
--
-------------------------------------------------------------------------------
-- R_Errorlog
-------------------------------------------------------------------------------

BEGIN
EXEC csiIncreaseStringColMaxLength 'ISOEERESOURCEDETAILSBYSHIFT','RESOURCEFAMILY', 255
END
GO
IF EXISTS (SELECT name FROM sysobjects 
         WHERE name = 'isOEEErrorlog' AND type = 'U')
   DROP TABLE isOEEErrorlog
GO
create table isOEEErrorlog 
    (
	CREATION_DATETIME          DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL         -- 'Record creation datetime.'
	, PROGID                   NVARCHAR(255) NULL                                  -- 'Program that initiate the error.'
	, ERRMSG                   NVARCHAR(4000) NULL                                 -- 'Error description.'
    ); 
GO
create index isOEEErrorlog_IX1 on isOEEErrorlog (Creation_Datetime);
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE UPPER(name) = UPPER('isOEEErrorLog_Record') AND type = 'P')
    EXEC ('CREATE PROCEDURE isOEEErrorLog_Record AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE isOEEErrorLog_Record 
    ( @pvProgId              NVARCHAR(255)
	, @pvErrMsg              NVARCHAR(4000) = Null
	) 
AS
DECLARE @iDurationOfLogData          INT=30;  -- in terms of days
DECLARE @iNoOfHistoricalRecs         INT=0;
--
DECLARE @pvNextInstanceID           NVARCHAR(16);
BEGIN 
	SET NOCOUNT ON;
    --PRINT '@@TRANCOUNT(isOEEErrorLog_Record):' + STR(@@TRANCOUNT);
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
        --
		---------------------------------------------------------------------------
	    BEGIN TRY 
			--
			BEGIN TRANSACTION
            SAVE TRANSACTION SavePoint1;
			INSERT INTO isOEEERRORLOG (
				progId
				, ErrMsg
				) 
            SELECT 
                @pvProgId
                , @pvErrMsg
            --
		    IF @@TRANCOUNT > 0
			    COMMIT TRANSACTION;
 	    END TRY
		BEGIN CATCH
            THROW;
		END CATCH
        BEGIN TRY
		    ----------------------------------------------------------------------
		    -- Housekeep this table based on date of historical records (keep for <iDurationOfLogData> days)
		    ----------------------------------------------------------------------
			BEGIN TRANSACTION
            SAVE TRANSACTION SavePoint1;
		    SELECT @iNoOfHistoricalRecs = COUNT(*) FROM isOEEERRORLOG
		    WHERE CREATION_DATETIME < (GetDate() - @iDurationOfLogData);    
		    IF @iNoOfHistoricalRecs > 0  -- found
			    DELETE FROM isOEEERRORLOG 
			    WHERE CREATION_DATETIME < (GetDate() - @iDurationOfLogData)
			    ;
			--
			IF @@TRANCOUNT > 0
				COMMIT TRANSACTION;
        END TRY    
		BEGIN CATCH
            THROW;
		END CATCH
        --------------------------------------------------------------------------
		RETURN 0;                   
	END TRY
	BEGIN CATCH
        IF XACT_STATE() = -1 -- Transaction is doomed, Rollback everything.
            ROLLBACK TRANSACTION;
        IF XACT_STATE() = 1 --Transaction is commitable, we can rollback to a save point
            ROLLBACK TRANSACTION SavePoint1 ; 
        THROW;
	END CATCH
END;
GO
IF NOT EXISTS (SELECT name FROM sysobjects 
               WHERE UPPER(name) = UPPER('isOEEGetEquipmentStatusChange') AND type = 'IF')
    EXEC ('CREATE FUNCTION isOEEGetEquipmentStatusChange (@pFromDate DATETIME, @pToDate DATETIME, @pRunDate  DATETIME) RETURNS TABLE AS RETURN (SELECT * FROM ResourceDef)')
GO
--
ALTER FUNCTION isOEEGetEquipmentStatusChange (
    @pFromDate   DATETIME
    , @pToDate   DATETIME
    , @pRunDate  DATETIME 
    )
RETURNS TABLE
AS
RETURN
(
    -- Get the Equipment uptime/downtime within the reporting period 
    -- where the run date must be scheduled to run after the reporting period (@pFromDate to @pToDate).
    WITH
    vtResourceHistory AS  -- From Equipment Histories : the resource may be up/down one or more times within the reporting period
    (
        SELECT
            R.ResourceId                                            EquipmentId
            , RSH.isOldOEELossCategory                              FromOEELossCategory
            , RSH.OldResourceState                                  FromResourceState
            , oldrsc.ResourceStatusCodeName                         FromStatus
            , oldrsr.ResourceStatusReasonName FromReason
            , RSH.OldAvailability                                   FromAvailability
            , ISNULL(rsh.oldlaststatuschangedate, @pFromDate)       FromStatusChangedate
            , RSH.isOEELossCategory                                 ToOEELossCategory
            , RSH.ResourceState                                     ToResourceState
            , newrsc.ResourceStatusCodeName                         ToStatus
            , newrsr.ResourceStatusReasonName                       ToReason
		    , rsh.availability                                      ToAvailability
		    , rsh.laststatuschangedate                              ToStatusChangedate
 		    --------------------------------------------------------------
		    -- Note : OldLastStatusChangeDate is defined as FromStatusChangedate
		    --        LastStatusChangeDate is defined as ToStatusChangedate
		    --        If the Equipment has a status change for the first time,  
		    --        the OldLastStatusChangeDate is stored as Null.
		    --------------------------------------------------------------
            , CASE 
                --------------------------------------------------------------
                -- Case 110 : 
                --------------------------------------------------------------
                -- 1. Resource OldLastStatusChangeDate < Reporting Start Date
			    -- 2. Resource LastStatusChangeDate > Reporting End Date
			    -- StatusChangeDuration = Reporting End Date - Reporting Start Date
                --------------------------------------------------------------
			    WHEN ISNULL(rsh.oldlaststatuschangedate, cs.lastchangedate) < @pFromDate AND RSH.LastStatusChangeDate > @pToDate
                    THEN 
                    --DateDiff(minute, @pFromDate, @pToDate) 
                    110
                --------------------------------------------------------------
                -- Case 120 : 
                --------------------------------------------------------------
                -- 1. Resource OldLastStatusChangeDate < Reporting Start Date 
			    -- 2. Resource LastStatusChangeDate within the Reporting Period
			    -- StatusChangeDuration = Resource LastStatusChangeDate - Reporting Start Date
                --------------------------------------------------------------
			    WHEN RSH.LastStatusChangeDate >= @pFromDate AND RSH.LastStatusChangeDate < @pToDate
                    AND ISNULL(rsh.oldlaststatuschangedate, cs.lastchangedate) < @pFromDate
                    THEN 
                    --DateDiff(minute, @pFromDate, RSH.LastStatusChangeDate) 
                    120
 			    --------------------------------------------------------------
                -- Case 130 :  
                --------------------------------------------------------------
                -- 1. Resource OldLastStatusChangeDate within the Reporting Period 
			    -- 2. Resource LastStatusChangeDate > Reporting End Date
			    -- StatusChangeDuration = Reporting End Date - Resource OldLastStatusChangeDate
                --------------------------------------------------------------
			    WHEN ISNULL(rsh.oldlaststatuschangedate, cs.lastchangedate) >= @pFromDate AND ISNULL(rsh.oldlaststatuschangedate, cs.lastchangedate) < @pToDate
                    AND RSH.LastStatusChangeDate > @pToDate
                    THEN 
                    --DateDiff(minute, ISNULL(rsh.oldlaststatuschangedate, cs.lastchangedate), @pToDate) 
                    130
                --------------------------------------------------------------
                -- Case 140 : 
                --------------------------------------------------------------
                -- 1. Resource OldLastStatusChangeDate within the Reporting Period 
			    -- 2. Reporting LastStatusChangeDate < Reporting End Date
			    -- StatusChangeDuration = Resource OldLastStatusChangeDate - Resource LastStatusChangeDate
                --------------------------------------------------------------
                WHEN ISNULL(rsh.oldlaststatuschangedate, cs.lastchangedate) >= @pFromDate AND ISNULL(rsh.oldlaststatuschangedate, cs.lastchangedate) < @pToDate
                    AND RSH.LastStatusChangeDate <= @pToDate
                    THEN 
                    --DateDiff(minute, ISNULL(rsh.oldlaststatuschangedate, cs.lastchangedate), RSH.LastStatusChangeDate) 
                    140
                --------------------------------------------------------------
                -- Case 150 : 
                --------------------------------------------------------------
                -- 1. Reporting OldLastStatusChangeDate < Reporting Start Date 
			    -- 2. Resource LastStatusChangeDate within the Reporting Period
			    -- StatusChangeDuration = Resource LastStatusChangeDate - Reporting Start Date
                --------------------------------------------------------------
                WHEN RSH.LastStatusChangeDate >= @pFromDate AND RSH.LastStatusChangeDate < @pToDate
                    AND ISNULL(rsh.oldlaststatuschangedate, cs.lastchangedate) <= @pFromDate
                    THEN 
                    --DateDiff(minute, @pFromDate, RSH.LastStatusChangeDate) 
                    150
                /* 
                --------------------------------------------------------------
                -- Case 160 : Not interested to get this piece of data as the equipment status change are not within the reporting period
                --------------------------------------------------------------
                -- 1. Reporting OldLastStatusChangeDate < Reporting Start Date 
			    -- 2. Resource LastStatusChangeDate < Reporting Start Date
			    -- StatusChangeDuration = @pToDate - @pFromDate
                --------------------------------------------------------------
                WHEN RSH.LastStatusChangeDate <= @pFromDate
                    AND ISNULL(rsh.oldlaststatuschangedate, cs.lastchangedate) <= @pFromDate
                    THEN 
                    --DateDiff(minute, @pFromDate, @pToDate) 
                    160
                */
                --------------------------------------------------------------
                -- Case 200 : 
                --------------------------------------------------------------
			    -- WhichCase = -1 denotes that this record should be excluded from this query
                ELSE -- 
                    -1 
            END WhichCase
        FROM ResourceDef R
        INNER JOIN ChangeStatus CS ON R.ResourceId = CS.ParentId
        INNER JOIN ResourceStatusHistory RSH ON R.ResourceId = RSH.HistoryId
        -- Old
		LEFT OUTER JOIN ResourceStatusCode oldrsc ON oldrsc.ResourceStatusCodeId = rsh.OldResourceStatusCodeId
		LEFT OUTER JOIN ResourceStatusReason oldrsr ON oldrsr.ResourceStatusReasonId = rsh.OldResourceStatusReasonCodeId
        -- New
		LEFT OUTER JOIN ResourceStatusCode newrsc ON newrsc.ResourceStatusCodeId = rsh.ResourceStatusCodeId
		LEFT OUTER JOIN ResourceStatusReason newrsr ON newrsr.ResourceStatusReasonId = rsh.ResourceStatusReasonCodeId
        WHERE r.isIncludeInOEE = 1 
    ),
    vtResourceCurrent AS  -- From Equipment Up/Down Current Status : the resource that are up/down at this point of time within the reporting period
    (
        SELECT
            R.ResourceId                                            EquipmentId
            , ps.isOEELossCategory                                  FromOEELossCategory
            , ps.ResourceState                                      FromResourceState
		    , rsc.ResourceStatusCodeName                            FromStatus
		    , rsr.ResourceStatusReasonName                          FromReason
            , ps.Availability                                       FromAvailability
            , CASE WHEN ps.LastStatusChangeDate IS NULL THEN @pFromDate 
              ELSE ps.LastStatusChangeDate 
              END                                                   FromStatusChangedate
            , NULL                                                  ToOEELossCategory
            , NULL                                                  ToResourceState
		    , NULL                                                  ToStatus
		    , NULL                                                  ToReason
		    , NULL                                                  ToAvailability
		    , NULL                                                  ToStatusChangedate
            , CASE 
                    --------------------------------------------------------------
                    -- Case 210 : The process is ran now but the reporting period is in the future
                    --------------------------------------------------------------
                    -- 1. Reporting Start Date > Run Date 
				    -- StatusChangeDuration = 0
				    -- Note : this case will never be true as the Run Date must always be scheduled
				    --        to run after the reporting period.
                    --------------------------------------------------------------
			        WHEN @pFromDate >= @pRunDate 
				        THEN 
                        --0 
                        210
                    --------------------------------------------------------------
                    -- Case 220 : 
                    --------------------------------------------------------------
                    -- 1. Resource LastStatusChangeDate < Reporting Start Date
				    -- 2. Reporting End Date > Run Date
				    -- StatusChangeDuration = Run Date - Reporting Start Date
				    -- Note 1 : Return False if the Run Date is scheduled to run after the reporting period, this is case will never be true.
				    -- Note 2 : Return False if the Run Date is scheduled to run within the reporting period.
                    --------------------------------------------------------------
                    WHEN (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) < @pFromDate 
                        AND @pToDate > @pRunDate
                        THEN 
                        --DateDiff(minute, @pFromDate, @pRunDate) 
                        220   
                    --------------------------------------------------------------
                    -- Case 230 : 
                    --------------------------------------------------------------
                    -- 1. Resource LastStatusChangeDate < Reporting Start Date
				    -- 2. Reporting End Date <= Run Date
				    -- StatusChangeDuration = Run Date - Reporting Start Date
                    --------------------------------------------------------------
                    WHEN (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) < @pFromDate 
                        AND @pToDate <= @pRunDate
                        THEN 
                        --DateDiff(minute, @pFromDate, @pToDate)  
                        230
 				    --------------------------------------------------------------
                    -- Case 240 : 
                    --------------------------------------------------------------
                    -- 1. Resource LastStatusChangeDate within the Reporting Period 
				    -- 2. Reporting End Date < Run Date
				    -- StatusChangeDuration = Reporting End Date - Resource LastStatusChangeDate
                    --------------------------------------------------------------
				    WHEN (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) >= @pFromDate 
                        AND (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) < @pToDate
                        AND @pToDate < @pRunDate
                        THEN 
                        --DateDiff(minute, PS.LastStatusChangeDate, @pToDate) 
                        240
                    --------------------------------------------------------------
                    -- Case 250 : Resource Status is changed to Unavailable within the Reporting Period
				    --            AND Reporting End Date >= Run Date
				    --            Note : this case will never be true as the Run Date must always be scheduled
				    --                   to run after the reporting period.
                    --------------------------------------------------------------
                    -- 1. Resource LastStatusChangeDate within the Reporting Period 
				    -- 2. Reporting End Date >= Run Date
				    -- StatusChangeDuration = Run Date - Resource LastStatusChangeDate
                    --------------------------------------------------------------
                    WHEN (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) >= @pFromDate 
                        AND (CASE WHEN ps.LastStatusChangeDate IS NULL THEN cs.lastchangedate ELSE ps.LastStatusChangeDate END) < @pToDate
                        AND @pToDate >= @pRunDate
                        THEN 
                        --DateDiff(minute, PS.LastStatusChangeDate, @pRunDate) 
                        250
                    --------------------------------------------------------------
                    -- Case 300 : 
                    --------------------------------------------------------------
			        -- WhichCase = -1 denotes that this record should be excluded from this query
                    ELSE -- 
                        -1 
            END WhichCase 
        FROM
            ResourceDef R
            LEFT OUTER JOIN ProductionStatus PS ON R.ProductionStatusId = PS.ProductionStatusId
            INNER JOIN ChangeStatus CS ON R.ResourceId = CS.ParentId
		    LEFT OUTER JOIN ResourceStatusCode rsc ON rsc.ResourceStatusCodeId = ps.statusid
		    LEFT OUTER JOIN ResourceStatusReason rsr ON rsr.ResourceStatusReasonId = ps.ReasonId
        WHERE r.isIncludeInOEE = 1 
    )
    SELECT * 
    FROM (
	    SELECT EquipmentId, FromOEELossCategory, FromResourceState, FromStatus, FromReason, FromAvailability, FromStatusChangedate, ToStatus, ToReason, ToAvailability, ToStatusChangedate, WhichCase 
            , CASE 
              WHEN WhichCase = 110 THEN DateDiff(minute, @pFromDate, @pToDate) 
              WHEN WhichCase = 120 THEN DateDiff(minute, @pFromDate, t1.ToStatusChangedate) 
              WHEN WhichCase = 130 THEN DateDiff(minute, t1.FromStatusChangedate, @pToDate)  
              WHEN WhichCase = 140 THEN DateDiff(minute, t1.FromStatusChangedate, t1.ToStatusChangedate)  
              WHEN WhichCase = 150 THEN DateDiff(minute, @pFromDate, t1.ToStatusChangedate)  
              WHEN WhichCase = 160 THEN DateDiff(minute, @pFromDate, @pToDate)  
              END StatusChangeDuration
	    FROM vtResourceHistory t1
	    UNION ALL
	    SELECT EquipmentId, FromOEELossCategory, FromResourceState, FromStatus, FromReason, FromAvailability, FromStatusChangedate, ToStatus, ToReason, ToAvailability, ToStatusChangedate, WhichCase 
            , CASE 
              WHEN WhichCase = 210 THEN 0 
              WHEN WhichCase = 220 THEN DateDiff(minute, @pFromDate, @pRunDate) 
              WHEN WhichCase = 230 THEN DateDiff(minute, @pFromDate, @pToDate)  
              WHEN WhichCase = 240 THEN DateDiff(minute, t2.FromStatusChangedate, @pToDate)  
              WHEN WhichCase = 250 THEN DateDiff(minute, t2.FromStatusChangedate, @pRunDate)  
              END StatusChangeDuration
	    FROM vtResourceCurrent t2
    ) t
    WHERE t.WhichCase <> -1 
        AND FromStatus IS NOT NULL -- Note : Equipment which is newly created will not have any Equipment State and therefore should be excluded from the query.
)
GO
IF NOT EXISTS (SELECT name FROM sysobjects WHERE UPPER(name) = UPPER('isOEEUpdateEquipmentStatusByShift') AND type = 'P') 
    EXEC ('CREATE PROCEDURE isOEEUpdateEquipmentStatusByShift AS BEGIN SET NOCOUNT ON; END');
GO
ALTER PROCEDURE isOEEUpdateEquipmentStatusByShift (
    @pRunDate DATETIME = NULL
    )
AS
DECLARE @ErrorMessage               NVARCHAR(4000)=''; -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @xstate                     INT;
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE @vRunDate                   DATETIME;
DECLARE @vRunFromDate               DATETIME;
DECLARE @vRunToDate                 DATETIME;
DECLARE @vCurrentStoredCalendarShiftId  CHAR(16) = Null;  -- The CalendarShiftId that was stored in the isOEECurrentShift table
DECLARE @vCurrentRunCalendarShiftId     CHAR(16) = Null;  -- The CalendarShiftId of the current run of this stored procedure
--
DECLARE @vLastShiftDate             DATETIME; -- Any DATETIME value that falls within the Last Shift Period
DECLARE @vLastShiftFromDate         DATETIME; -- Last Shift Period's From Date
DECLARE @vLastShiftToDate           DATETIME; -- Last Shift Period's To Date
DECLARE @vShiftName                 NVARCHAR(30);
DECLARE @vCalendarDate              DATETIME;
BEGIN
    --
    SET NOCOUNT ON;
    SET @vProgID = @vObject_Name + '.START'; 
	BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
        BEGIN TRANSACTION;
		---------------------------------------------------------------------------
		-- Validations
		---------------------------------------------------------------------------
		--IF @pFromDate > @pToDate 
		--	RAISERROR ('The From Date must not be greater than the To Date', 16, 1);
		---------------------------------------------------------------------------
		-- Default values
		---------------------------------------------------------------------------
        IF @pRunDate IS NULL 
            SET @vRunDate = GetDate();
        ELSE
            SET @vRunDate = @pRunDate;
		---------------------------------------------------------------------------
		-- Get the CurrentStoredCalendarShiftId.
		---------------------------------------------------------------------------
	    BEGIN TRY
            SELECT @vCurrentStoredCalendarShiftId = oeeCS.CurrentCalendarShiftId
            FROM isOEECurrentShift oeeCS
            ;
            --PRINT '@vCurrentStoredCalendarShiftId = ' + ISNULL(@vCurrentStoredCalendarShiftId, 'NULL');
        END TRY
        BEGIN CATCH
	        SET @ErrorMessage = ERROR_MESSAGE(); 
	        SET @vProgID = @vObject_Name + '.' + 'Failed to get the CurrentStoredCalendarShiftId from isOEECurrentShift table'; 
	        RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --
		---------------------------------------------------------------------------
		-- Get the CurrentRunCalendarShiftId.
		---------------------------------------------------------------------------
	    BEGIN TRY
            SELECT 
                @vCurrentRunCalendarShiftId = cs.CalendarShiftId 
                , @vRunFromDate = cs.ShiftStart
                , @vRunToDate = cs.ShiftEnd
            FROM CalendarShift cs 
            WHERE @vRunDate BETWEEN cs.ShiftStart AND cs.ShiftEnd
            ;
            --PRINT '@vCurrentRunCalendarShiftId    = ' + ISNULL(@vCurrentRunCalendarShiftId, 'NULL');
            --PRINT '@vRunDate = '; PRINT @vRunDate;
            --PRINT '@vRunFromDate = '; PRINT @vRunFromDate;
            --PRINT '@vRunToDate = '; PRINT @vRunToDate;
        END TRY
        BEGIN CATCH
	        SET @ErrorMessage = ERROR_MESSAGE(); 
	        SET @vProgID = @vObject_Name + '.' + 'Failed to get the CurrentRunCalendarShiftId from CalendarShift table'; 
	        RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --
        ---------------------------------------------------------------------------
		-- For each shift run, get the Equipment StatusChange Duration of the last shift 
        --     and store last shift information into reporting buckets 
		---------------------------------------------------------------------------
	    BEGIN TRY
            IF @vCurrentStoredCalendarShiftId IS NULL OR 
               @vCurrentRunCalendarShiftId <> @vCurrentStoredCalendarShiftId BEGIN
                PRINT 'Computing the Equipment StatusChange information of the last shift...';
                ---------------------------------------------------------------------------
		        -- Get the last shift period based on the @vLastShiftDate
		        ---------------------------------------------------------------------------
                -- Minus 1 hour from the @vRunFromDate to derive the @vLastShiftDate
                SET @vLastShiftDate = DATEADD(hour, -1, @vRunFromDate);
                --
                SELECT 
                    @vLastShiftFromDate = cs.ShiftStart
                    , @vLastShiftToDate = cs.ShiftEnd
                    , @vShiftName = s.ShiftName
                    , @vCalendarDate = cs.CalendarDate
                FROM CalendarShift cs 
                INNER JOIN Shift s ON s.shiftid = cs.shiftid
                WHERE @vLastShiftDate BETWEEN cs.ShiftStart AND cs.ShiftEnd
                ;
                --PRINT '@vLastShiftDate = '; PRINT @vLastShiftDate;
                --PRINT '@vLastShiftFromDate = '; PRINT @vLastShiftFromDate;
                --PRINT '@vLastShiftToDate = '; PRINT @vLastShiftToDate;
                --
                INSERT INTO isOEEResourceDetailsByShift (
                    CalendarDate
                    , CalendarShiftId
                    , OEELossCategory
                    , OEELossCategoryName 
                    , ResourceFamily
                    , ResourceId
                    , ResourceName
                    , ResourceState
                    , ResourceStateName
                    , ResourceStatusCode
                    , ResourceStatusReason
                    , Shift
                    , TimeInStatus
                    )
                SELECT 
                    @vCalendarDate
                    , @vCurrentRunCalendarShiftId
                    , esc.FromOEELossCategory
                    , CASE 
                      WHEN FromOEELossCategory = 1 THEN 'Availability Loss'
                      WHEN FromOEELossCategory = 2 THEN 'Performance Loss'
                      WHEN FromOEELossCategory = 3 THEN 'Schedule Loss'
                      ELSE 'No Such Loss Category'
                      END FromOEELossCategoryName 
                    , rf.ResourceFamilyName
                    , esc.EquipmentId
                    , rd.ResourceName
                    , esc.FromResourceState
                    , CASE 
                      WHEN FromResourceState = 1 THEN 'NonScheduled'
                      WHEN FromResourceState = 2 THEN 'UnscheduleDown'
                      WHEN FromResourceState = 3 THEN 'ScheduleDown'
                      WHEN FromResourceState = 4 THEN 'Engineering'
                      WHEN FromResourceState = 5 THEN 'Productive'
                      WHEN FromResourceState = 6 THEN 'Standby'
                      ELSE 'No Such Resource State'
                      END FromResourceStateName 
                    , esc.FromStatus
                    , esc.FromReason
                    , @vShiftName
                    , esc.StatusChangeDuration
                FROM isOEEGetEquipmentStatusChange (@vLastShiftFromDate, @vLastShiftToDate, @vRunDate) esc 
                INNER JOIN ResourceDef rd ON rd.ResourceId = esc.EquipmentId
                LEFT OUTER JOIN ResourceFamily rf ON rf.resourcefamilyid = rd.resourcefamilyid
                ORDER BY rd.ResourceName, esc.FromStatusChangedate
                ;
                --
                END
            ELSE
                PRINT 'There is no need to recompute the Equipment StatusChange information of the last shift.';
            --
        END TRY
        BEGIN CATCH
	        SET @ErrorMessage = ERROR_MESSAGE(); 
	        SET @vProgID = @vObject_Name + '.' + 'Failed to insert record(s) into isOEEResourceDetailsByShift table'; 
	        RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --
		---------------------------------------------------------------------------
		-- Update isOEECurrentShift table with the @vCurrentRunCalendarShiftId.
        -- Note : isOEECurrentShift is a one record table.
		---------------------------------------------------------------------------
	    BEGIN TRY
            IF EXISTS (SELECT oeeCS.CurrentCalendarShiftId 
                       FROM isOEECurrentShift oeeCS) 
                UPDATE isOEECurrentShift
                SET CurrentCalendarShiftId = @vCurrentRunCalendarShiftId
                ;
            ELSE
                INSERT INTO isOEECurrentShift (CurrentCalendarShiftId)
                VALUES (@vCurrentRunCalendarShiftId)
                ;
        END TRY
        BEGIN CATCH
	        SET @ErrorMessage = ERROR_MESSAGE(); 
	        SET @vProgID = @vObject_Name + '.' + 'Failed to INSERT/UPDATE CurrentCalendarShiftId into the isOEECurrentShift table'; 
	        RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --
        ---------------------------------------------------------------------------
		IF @@TRANCOUNT > 0
			COMMIT TRANSACTION;
        --
        --------------------------------------------------------------------------
        SET @vProgID = @vObject_Name + '.SUCCESSFULL'; 
		RETURN 0;
	END TRY
    --
	BEGIN CATCH
		IF @ErrorMessage IS NULL
		    SET @ErrorMessage  = ERROR_MESSAGE();
		IF @ErrorSeverity IS NULL
		    SET @ErrorSeverity = ERROR_SEVERITY(); 
		IF @ErrorState IS NULL
		    SET @ErrorState = ERROR_STATE();
 		IF @vProgID IS NULL
		    SET @vProgID = @vObject_Name + '.OTHER ERROR'; 
       IF @@TRANCOUNT > 0 
            ROLLBACK TRANSACTION;
        EXECUTE isOEEErrorLog_Record @pvProgID=@vProgID, @pvErrMsg=@ErrorMessage; 
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END
GO