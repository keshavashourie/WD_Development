-- Copyright Siemens 2023  
IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS 
 	    WHERE Name = 'DimTime' 
	      AND Type = 'U')
	--
	DROP TABLE DimTime
	--
GO
CREATE TABLE DimTime(
                FullDateKey int NOT NULL,
                isWeekday bit NOT NULL,
                DayOfWeek int NOT NULL,
                DayOfMonth int NOT NULL,
                DayOfQuarter int NOT NULL,
                DayOfYear int NOT NULL,
                DayName varchar(25) NOT NULL,
                WeekOfYear int NOT NULL,
                WeekName varchar(25) NOT NULL,
                MonthOfYear int NOT NULL,
                MonthName varchar(25) NOT NULL,
                CalendarQuarter int NOT NULL,
                CalendarQuarterName char(25) NOT NULL,
                CalendarYear int NOT NULL,
				CONSTRAINT PK_DimTime PRIMARY KEY CLUSTERED 
(
                FullDateKey ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
)
GO

/*
05/10/2006 Created to load time dimension

Sample Exec:

      Exec dbo.wspGenerateTimeDimension
            @StartDate = '1/1/1983',
            @EndDate = '12/31/2020',
            @DisplayOnlyFlag = 0

select * from DimTime

*/
IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS 
 	    WHERE Name = 'wspGenerateTimeDimension' 
	      AND Type = 'p')
	--
	DROP PROCEDURE wspGenerateTimeDimension
	--
GO
CREATE PROCEDURE wspGenerateTimeDimension
(
@StartDate datetime,
@EndDate datetime,
@DisplayOnlyFlag bit = 0
)
AS
SET NOCOUNT ON
SET DATEFIRST 1

DECLARE @DimTime TABLE (
      FullDateKey int NOT NULL,
      isWeekday bit NOT NULL,
      DayOfWeek int NOT NULL,
      DayOfMonth int NOT NULL,
      DayOfQuarter int NOT NULL,
      DayOfYear int NOT NULL,
      DayName varchar(25)NOT NULL,
      WeekOfYear int NOT NULL,
      WeekName varchar(25) NOT NULL,
      MonthOfYear int NOT NULL,
      MonthName varchar(25) NOT NULL,
      CalendarQuarter int NOT NULL,
      CalendarQuarterName char(25) NOT NULL,
      CalendarYear int NOT NULL
)

DECLARE @CurrentDate int
DECLARE @Date datetime
DECLARE @ID int

SET @ID = 0
SET @Date = DATEADD(dd, @ID, @StartDate)

WHILE (@Date <= @EndDate)
BEGIN
                  SELECT @CurrentDate = 
                                                CONVERT(int,
                                                (
                                                CAST(YEAR(@Date) AS VARCHAR(4)) 
                                                + RIGHT(('00' + CAST(MONTH(@Date) AS Varchar)),2)
                                                + RIGHT(('00' + CAST(DAY(@Date) AS Varchar)),2)
                                                )
                                                )

      INSERT INTO @DimTime
      (
      FullDateKey,isWeekDay,DayOfWeek,DayOfMonth,
      DayOfQuarter,DayOfYear,DayName,
      WeekOfYear,WeekName,MonthOfYear,
      MonthName,CalendarQuarter,CalendarQuarterName,CalendarYear
      )
      VALUES
      (
      @CurrentDate,
      CASE 
            WHEN DATEPART(dw,@Date) IN(6,7) THEN 0
            ELSE 1
      END,
      DATEPART(dw,@Date),
      DATEPART(dd,@Date),
      (DATEDIFF(DAY,DATEADD(qq,DATEDIFF(qq,0,@Date),0),@Date) + 1),
      DATEPART(dy,@Date),
      CASE DATEPART(dw,@Date)
            WHEN 1      THEN 'Monday'
            WHEN 2      THEN 'Tuesday'
            WHEN 3      THEN 'Wednesday'
            WHEN 4      THEN 'Thursday'
            WHEN 5      THEN 'Friday'
            WHEN 6      THEN 'Saturday'
            WHEN 7      THEN 'Sunday'           
      END,
      DATEPART(ww,@Date),
      'Week ' + RIGHT('0' + DATENAME(ww,@Date),2) + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date)),
      DATEPART(mm,@Date),
      CASE DATEPART(mm,@Date)
            WHEN 1      THEN  'January' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 2      THEN  'February' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 3      THEN  'March' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 4      THEN  'April' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 5      THEN  'May' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 6      THEN  'June' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 7      THEN  'July' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 8      THEN  'August' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 9      THEN  'September' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 10     THEN  'October' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 11     THEN  'November' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
            WHEN 12     THEN  'December' + ' ' + CONVERT(varchar(4),DATEPART(yy,@Date))
      END,
      DATEPART(qq,@Date),
      'Q' + DATENAME(qq,@Date) + ' ' + DATENAME(yy,@Date),
      DATEPART(yy,@Date)
      )
      
      SET @ID = @ID + 1
      SET @Date = DATEADD(dd,@ID,@StartDate)
END

IF (@DisplayOnlyFlag = 0)
BEGIN
      DELETE DimTime

      INSERT INTO DimTime
      (
      FullDateKey,isWeekDay,DayOfWeek,DayOfMonth,
      DayOfQuarter,DayOfYear,DayName,
      WeekOfYear,WeekName,MonthOfYear,
      MonthName,CalendarQuarter,CalendarQuarterName,CalendarYear
      )
      SELECT
      FullDateKey,isWeekDay,DayOfWeek,DayOfMonth,
      DayOfQuarter,DayOfYear,DayName,
      WeekOfYear,WeekName,MonthOfYear,
      MonthName,CalendarQuarter,CalendarQuarterName,CalendarYear
      FROM @DimTime
END
ELSE
BEGIN
      SELECT
      FullDateKey,isWeekDay,DayOfWeek,DayOfMonth,
      DayOfQuarter,DayOfYear,DayName,
      WeekOfYear,WeekName,MonthOfYear,
      MonthName,CalendarQuarter,CalendarQuarterName,CalendarYear
      FROM @DimTime
END

RETURN 0
GO
/*
   Call the stored procedure, giving it +/- 20 years (making sure to start on Jan. 1 and end on Dec. 31).
*/
DECLARE @StartDate datetime
DECLARE @EndDate datetime
BEGIN
   SET @StartDate = CONVERT(datetime,N'1/1/' + CAST(YEAR(GETDATE())-20 AS nvarchar), 101);
   SET @EndDate = CONVERT(datetime,N'12/31/' + CAST(YEAR(GETDATE())+20 AS nvarchar), 101);
   EXEC wspGenerateTimeDimension @StartDate, @EndDate, 0
END
GO
-- The procedure is not needed anymore, so drop it
IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS 
 	    WHERE Name = 'wspGenerateTimeDimension' 
	      AND Type = 'p')
	--
	DROP PROCEDURE wspGenerateTimeDimension
	--
GO

