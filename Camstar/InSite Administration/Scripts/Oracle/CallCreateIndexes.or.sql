-- Oracle version
-- SCRIPT:CallCreateIndexes.sql
-- DESCR: Calls the stored procedure that creates indexes
--
-- 
-- Copyright Siemens 2023  
BEGIN
	csiCreateIndexes('NEW','EXECUTE',':DBType');
END;
/
