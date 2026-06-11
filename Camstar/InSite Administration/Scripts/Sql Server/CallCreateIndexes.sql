-- SQL Server version
-- SCRIPT:CallCreateIndexes.sql
-- DESCR: Calls the stored procedure that creates indexes
--
-- Copyright Siemens 2023  


EXECUTE csiCreateIndexes 'NEW','EXECUTE',':DBType'
GO
