--------------------------------------------------------------------------------
-- SCRIPT: StartDataStore.sql
-- DESCR: Starts (enables) the data store
-- HISTORY:
--   12/06/2006  Updated copyright notice (SPR S9984). Bill Lippard
--   04/23/2007  Updated copyright notice (SPR S9984). Bill Lippard
--
-- Copyright Siemens 2023  
BEGIN
   UPDATE DATASTORESETUP SET VALUE='N' WHERE PARAMETER='DATASTORE_TERMINATE';
   COMMIT;
END;
/
