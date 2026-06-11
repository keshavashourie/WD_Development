-- Copyright Siemens 2023  
DECLARE
	--
	v_TableFound		VARCHAR2(30) := 'FALSE';
	--
BEGIN
	--
	SELECT 'TRUE'
	  INTO v_TableFound
	  FROM USER_TABLES
	 WHERE Table_Name = 'DIMTIME';
	--
EXCEPTION
	WHEN NO_DATA_FOUND THEN
		--
		EXECUTE IMMEDIATE 'CREATE TABLE DimTime(
                FullDateKey NUMBER NOT NULL,
                isWeekday NUMBER NOT NULL,
                DayOfWeek NUMBER NOT NULL,
                DayOfMonth NUMBER NOT NULL,
                DayOfQuarter NUMBER NOT NULL,
                DayOfYear NUMBER NOT NULL,
                DayName VARCHAR2(25) NOT NULL,
                WeekOfYear NUMBER NOT NULL,
                WeekName VARCHAR2(25) NOT NULL,
                MonthOfYear NUMBER NOT NULL,
                MonthName VARCHAR2(25) NOT NULL,
                CalendarQuarter NUMBER NOT NULL,
                CalendarQuarterName VARCHAR2(25) NOT NULL,
                CalendarYear NUMBER NOT NULL
                )';
		--
	WHEN OTHERS THEN
		--
		DBMS_OUTPUT.PUT_LINE('Error creating DimTime table: '||SQLERRM);
		--
		RAISE_APPLICATION_ERROR(-20001,'Error retrieving DimTime table information: '||SQLERRM);
		--
END;
/

/*
05/10/2006 Created to load time dimension

Sample Exec:

      Exec dbo.wspGenerateTimeDimension
            @StartDate = '1/1/1983',
            @EndDate = '12/31/2020',
            @DisplayOnlyFlag = 0

select * from DimTime

*/
CREATE OR REPLACE PROCEDURE wspGenerateTimeDimension(StartDate DATE, EndDate DATE, DisplayOnlyFlag NUMBER DEFAULT 0)
AS
  vDate        DATE;
  vID          NUMBER;
BEGIN
    EXECUTE IMMEDIATE 'TRUNCATE TABLE DIMTIME';
    
    vID := 0;
    vDate := StartDate + vID;

   WHILE (vDate <= EndDate) LOOP
     
      INSERT INTO DIMTIME
      (
      FullDateKey,isWeekDay,DayOfWeek,DayOfMonth,
      DayOfQuarter,DayOfYear,DayName,
      WeekOfYear,WeekName,MonthOfYear,
      MonthName,CalendarQuarter,CalendarQuarterName,CalendarYear
      )
      VALUES
      (
      TO_NUMBER(TO_CHAR(vDate,'YYYYMMDD')),
      CASE 
            WHEN TO_CHAR(vDate,'D') IN('1','7') THEN 0
            ELSE 1
      END,
      TO_CHAR(vDate,'D'),
      TO_CHAR(vDate,'DD'),
      TRUNC(vDate - TRUNC(vDate, 'Q')) + 1,
      TO_CHAR(vDate,'DDD'),
      RTRIM(TO_CHAR(vDate,'Day')),
      TO_CHAR(vDate,'WW'),
      'Week '||TO_CHAR(vDate, 'WW')||' '||TO_CHAR(vDate,'YYYY'),
      TO_CHAR(vDate,'MM'),
      RTRIM(TO_CHAR(vDate,'Month'))||' '||TO_CHAR(vDate,'YYYY'),
      TO_CHAR(vDate, 'Q'),
      'Q'||TO_CHAR(vDate, 'Q')||' '||TO_CHAR(vDate, 'YYYY'),
      TO_CHAR(vDate, 'YYYY')
      );
      
      vID := vID + 1;
      vDate := Startdate + vID;
   END LOOP;
   
   COMMIT;
END;
/
/*
   Call the stored procedure, giving it +/- 20 years (making sure to start on Jan. 1 and end on Dec. 31).
*/
BEGIN
   wspGenerateTimeDimension (TRUNC(ADD_MONTHS(SYSDATE,-240),'YYYY'),TRUNC(ADD_MONTHS(SYSDATE,252),'YYYY')-1);
END;
/
DROP PROCEDURE wspGenerateTimeDimension
/

