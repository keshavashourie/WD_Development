--------------------------------------------------------------------------------
-- SCRIPT: StopDataStore.sql
-- DESCR: Stops (disables) the data store
-- HISTORY:
--   12/06/2006  Updated copyright notice (SPR S9984). Bill Lippard
--   04/23/2007  Updated copyright notice (SPR S9984). Bill Lippard
--
-- Copyright Siemens 2023  
BEGIN
   UPDATE DATASTORESETUP SET VALUE='Y' WHERE PARAMETER='DATASTORE_TERMINATE';
   COMMIT;
END;
/
