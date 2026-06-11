--------------------------------------------------------------------------------
-- SCRIPT:ContainerUpdateCount.or.sql
-- DESCR: This script is run when we upgrade from V7 to V7SU1. For the preexisting containers the InqualityControl
--			field will be updated and set to the total of events that container was asiigned to.
--			This script doesnot need any parameters. 
--  		it should be run after doing the following
--				 upgrade  v7 to v7SU1, run the csiEnableMillisecondPrecision script and do DBupdate with upgraded mdb.
--  Copyright Siemens 2023  
--------------------------------------------------------------------------------
-------------------------------------------------------------------------
-- PROCEDURE: ContainerUpdateCount
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE ContainerUpdateCount
AS
    vSQLString VARCHAR2(4000);
    c1 SYS_REFCURSOR;
    vContName VARCHAR2(30);
BEGIN
    vSQLString := 'Select cont.containername ' ||
			'From	container cont ';

   OPEN c1 FOR vSQLString;
   FETCH c1 INTO vContName;
   WHILE (c1%FOUND) LOOP
       UPDATE Container
        SET Container.InQualityControl = (SELECT COUNT(Event.EventId)
            FROM Event
              LEFT JOIN EventData ON Event.EventDataId = EventData.EventDataId
              LEFT JOIN EventLot ON EventData.EventDataId = EventLot.EventDataId
              WHERE EventLot.Lot = vContName)
      where Container.ContainerName = vContName;
      FETCH c1 INTO vContName;
   END LOOP;
   
     
   
   CLOSE c1;
END;
/


BEGIN
  ContainerUpdateCount;
  COMMIT;
END;
/

