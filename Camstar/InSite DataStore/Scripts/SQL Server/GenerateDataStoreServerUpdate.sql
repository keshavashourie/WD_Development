--------------------------------------------------------------------------------
-- SCRIPT: GenerateDataStoreServerUpdate.sql
-- DESCR:  Generates the UPDATE statement for the setup parameter REMOTEDB
-- HISTORY:
--    5/25/2005 - Corrected bug with path string.
--   12/06/2006 - Updated copyright notice (SPR S9984) Bill Lippard.
--   04/23/2007 - Updated copyright notice(s) (SPR S9984) Bill Lippard.
--
-- Copyright Siemens 2023  
--
SET NOCOUNT ON
GO

SELECT 'SET NOCOUNT ON' UNION
SELECT 'UPDATE DATASTORESETUP SET VALUE=''['+ @@servername+'].['+db_name()+'].['+user_name()+']'' WHERE PARAMETER=''REMOTEDB'''
go
exit
