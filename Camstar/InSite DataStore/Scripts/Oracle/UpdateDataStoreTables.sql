--------------------------------------------------------------------------------
-- SCRIPT: UpdateDataStoreTables.sql
-- DESCR: Updates the Datastore tables in the datastore database
-- HISTORY:
--   	11/22/2005 			Initial build. Using HotFix 34088 as the baseline.
--   	12/06/2006 			Updated copyright notice (SPR S9984). Bill Lippard
--   	04/23/2007 			Updated copyright notice (SPR S9984). Bill Lippard
--   	03/24/2008 			Updated the version number(SPR S12904). Purushotham Neelakantachar
--   	04/19/2016			Removed the direct update of DATASTORESET.VERSION 
--	04/19/2016	Dan Maloney	csiDataStorepackage.sql has been modified with PL/SQL code that reads the cVersion 
--					CONSTANT from csiDataStorepackage to set the DATASTORESETUP.VERSION to match the 
--					constant csiDataStorepackage.cVersion
--
--Copyright Siemens 2023  
DECLARE
  	I NUMBER;
  	v_TblNum NUMBER;
  	v_sql VARCHAR2(512);
	v_Version VARCHAR2(10);
BEGIN
   	-- TODO: Add new HotFix code here.   
  	v_Version := NULL;
  	v_sql := 'UPDATE DATASTORESETUP SET VALUE = ''' || v_Version || ''' WHERE PARAMETER = ''VERSION''';
	EXECUTE IMMEDIATE v_sql;
  	COMMIT;
EXCEPTION
WHEN OTHERS THEN
	ROLLBACK;
	RAISE;
END;
/



