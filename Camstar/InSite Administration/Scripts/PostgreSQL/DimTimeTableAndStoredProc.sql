DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.tables 
		WHERE lower(table_name) = lower('DimTime')
	) THEN
		DROP TABLE IF EXISTS DimTime;
	END IF;
END $$;

CREATE TABLE DimTime(
	FullDateKey integer NOT NULL,
    isWeekday integer NOT NULL,
    DayOfWeek integer NOT NULL,
    DayOfMonth integer NOT NULL,
    DayOfQuarter integer NOT NULL,
    DayOfYear integer NOT NULL,
    DayName varchar(25) NOT NULL,
    WeekOfYear integer NOT NULL,
    WeekName varchar(25) NOT NULL,
    MonthOfYear integer NOT NULL,
    MonthName varchar(25) NOT NULL,
    CalendarQuarter integer NOT NULL,
    CalendarQuarterName char(25) NOT NULL,
    CalendarYear integer NOT NULL,
    CONSTRAINT PK_DimTime PRIMARY KEY (FullDateKey)
);

/*
05/10/2006 Created to load time dimension

Sample CALL:

      CALL wspGenerateTimeDimension
            ('1/1/1983',
            '12/31/2020',
            FALSE);

select * from DimTime

*/
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('wspGenerateTimeDimension')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS wspGenerateTimeDimension;
 	END IF;
END $$;
CREATE PROCEDURE wspGenerateTimeDimension(
	StartDate TIMESTAMP,
	EndDate TIMESTAMP,
	DisplayOnlyFlag BOOLEAN = FALSE
)
LANGUAGE plpgsql
as $$
DECLARE
	CurrentDate integer;
	Date timestamp;
	ID integer;
BEGIN
	-- Create temp_table
	CREATE TEMPORARY TABLE temp_table (
		FullDateKey integer NOT NULL,
		isWeekday integer NOT NULL,
		DayOfWeek integer NOT NULL,
		DayOfMonth integer NOT NULL,
		DayOfQuarter integer NOT NULL,
		DayOfYear integer NOT NULL,
		DayName varchar(25) NOT NULL,
		WeekOfYear integer NOT NULL,
		WeekName varchar(25) NOT NULL,
		MonthOfYear integer NOT NULL,
		MonthName varchar(25) NOT NULL,
		CalendarQuarter integer NOT NULL,
		CalendarQuarterName char(25) NOT NULL,
		CalendarYear integer NOT NULL
	) on commit drop;

	ID := 0;
	Date = StartDate + ID * interval '1 day';
	
	WHILE Date <= EndDate LOOP
		CurrentDate := (TO_CHAR(Date, 'YYYY') || TO_CHAR(Date, 'MM') || TO_CHAR(Date, 'DD'))::integer;
		
		INSERT INTO temp_table (
			FullDateKey,isWeekDay,DayOfWeek,DayOfMonth,
			DayOfQuarter,DayOfYear,DayName,
			WeekOfYear,WeekName,MonthOfYear,
			MonthName,CalendarQuarter,CalendarQuarterName,CalendarYear
		) VALUES
		(
			CurrentDate,
			CASE
				WHEN EXTRACT(ISODOW FROM Date) IN (6, 7) THEN 0
				ELSE 1
			END,
			EXTRACT(ISODOW FROM Date),
			EXTRACT(DAY FROM Date),
			(EXTRACT(DAY FROM Date - DATE_TRUNC('quarter', Date)) + 1),
			EXTRACT(DOY FROM Date),
			TO_CHAR(Date, 'Day'),
			EXTRACT(WEEK FROM Date),
			-- 'Week ' || TO_CHAR(Date, 'WW YYYY'),
			'Week ' || TO_CHAR(EXTRACT(WEEK FROM Date)::integer, '00') || ' ' || TO_CHAR(Date, 'YYYY'),
			EXTRACT(MONTH FROM Date),
			TO_CHAR(Date, 'Month YYYY'),
			EXTRACT(QUARTER FROM Date),
			'Q' || EXTRACT(QUARTER FROM Date) || ' ' || TO_CHAR(Date, 'YYYY'),
			EXTRACT(YEAR FROM Date)
		);
		
		ID := ID + 1;
		Date = StartDate + ID * interval '1 day';
	END LOOP;
	
	IF DisplayOnlyFlag = FALSE THEN
		TRUNCATE DimTime;
		
		INSERT INTO DimTime
		(
		  FullDateKey,isWeekDay,DayOfWeek,DayOfMonth,
		  DayOfQuarter,DayOfYear,DayName,
		  WeekOfYear,WeekName,MonthOfYear,
		  MonthName,CalendarQuarter,CalendarQuarterName,CalendarYear
		)
		SELECT
			tt.FullDateKey, tt.isWeekDay, tt.DayOfWeek, tt.DayOfMonth,
			tt.DayOfQuarter, tt.DayOfYear, tt.DayName,
			tt.WeekOfYear, tt.WeekName, tt.MonthOfYear,
			tt.MonthName, tt.CalendarQuarter, tt.CalendarQuarterName, tt.CalendarYear
		FROM temp_table tt;
	ELSE
		PERFORM -- SELECT
			tt.FullDateKey, tt.isWeekDay, tt.DayOfWeek, tt.DayOfMonth,
			tt.DayOfQuarter, tt.DayOfYear, tt.DayName,
			tt.WeekOfYear, tt.WeekName, tt.MonthOfYear,
			tt.MonthName, tt.CalendarQuarter, tt.CalendarQuarterName, tt.CalendarYear
		FROM temp_table tt;
	END IF;
END;
$$;

/*
   Call the stored procedure, giving it +/- 20 years (making sure to start on Jan. 1 and end on Dec. 31).
*/
DO $$
DECLARE
	StartDate timestamp;
	EndDate timestamp;
BEGIN
	StartDate := TO_DATE('1/1/' || EXTRACT(YEAR FROM CURRENT_DATE) - 20, 'MM/DD/YYYY');
	EndDate := TO_DATE('12/31/' || EXTRACT(YEAR FROM CURRENT_DATE) + 20, 'MM/DD/YYYY');
	CALL wspGenerateTimeDimension(StartDate, EndDate, FALSE);
END;
$$;

-- The procedure is not needed anymore, so drop it
DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('wspGenerateTimeDimension')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS wspGenerateTimeDimension;
 	END IF;
END $$;