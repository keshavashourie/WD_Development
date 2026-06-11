-- SQL Server version
-- SCRIPT:CallCreateIndexes.sql
-- DESCR: Calls the stored procedure that creates indexes
--
-- Copyright Siemens 2023  


CALL csiCreateIndexes ('NEW','EXECUTE',':DBType');
