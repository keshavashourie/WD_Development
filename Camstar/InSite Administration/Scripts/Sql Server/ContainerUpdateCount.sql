--------------------------------------------------------------------------------
-- SCRIPT:ContainerUpdateCount.sql
-- DESCR: This script is run when we upgrade from V7 to V7SU1. For the preexisting containers the InqualityControl
--			field will be updated and set to the total of events that container was asiigned to.
--			This script doesnot need any parameters. 
--  		it should be run after doing the following
--				 upgrade  v7 to v7SU1, run the csiEnableMillisecondPrecision script and do DBupdate with upgraded mdb.
-- Copyright Siemens 2023  



IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'ContainerUpdateCount' 
	   AND 	  type = 'P')
    DROP PROCEDURE ContainerUpdateCount
GO
CREATE PROCEDURE ContainerUpdateCount
AS
   DECLARE @SQLString NVARCHAR(MAX)
    DECLARE @c1 CURSOR
	DECLARE @ContName VARCHAR(30)
BEGIN
  SET NOCOUNT ON;
		
   SET @SQLString = N'SET @c1 = CURSOR FAST_FORWARD FOR ' + --@sqlQuery
						 'Select cont.containername ' +
						  'From	container cont ' +
				          ' FOR READ ONLY; OPEN @c1'
   EXEC sp_executesql @SQLString, N'@c1 CURSOR OUTPUT', @c1 OUTPUT
   FETCH NEXT FROM @c1 INTO @ContName
   WHILE(@@fetch_status = 0)
   BEGIN
      UPDATE Container
	SET Container.InQualityControl = (SELECT COUNT(Event.EventId)
	FROM Event
		LEFT JOIN EventData ON Event.EventDataId = EventData.EventDataId
		LEFT JOIN EventLot ON EventData.EventDataId = EventLot.EventDataId
	WHERE EventLot.Lot = @ContName)
	where Container.ContainerName = @ContName
      FETCH NEXT FROM @c1 INTO @ContName 
   END
   CLOSE @c1
   DEALLOCATE @c1

   PRINT('Container InQualityControl Count Updated.');
   PRINT('Successfully Completed');

END
GO


EXEC ContainerUpdateCount
GO
--DROP PROCEDURE ContainerUpdateCount
--GO
