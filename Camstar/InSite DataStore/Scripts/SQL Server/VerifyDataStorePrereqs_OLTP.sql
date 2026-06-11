--------------------------------------------------------------------------------
-- SCRIPT: VerifyDataStorePrereqs_OLTP.sql
-- DESCR: Verifies OLTP prereqs for DataStore
-- To  create a linked server, execute the following as sa: 
/*
EXEC sp_addlinkedserver @server = N'CSILOOPBACK',@srvproduct = N' ',@provider = N'SQLNCLI', @datasrc = @@SERVERNAME
EXEC master.dbo.sp_serveroption @server=N'CSILOOPBACK', @optname=N'rpc', @optvalue=N'true'
EXEC master.dbo.sp_serveroption @server=N'CSILOOPBACK', @optname=N'rpc out', @optvalue=N'true'
EXEC master.dbo.sp_serveroption @server=N'CSILOOPBACK', @optname=N'remote proc transaction promotion', @optvalue=N'false'

*/
--
-- Copyright Siemens 2023  
IF NOT EXISTS (SELECT 'X' FROM sys.servers WHERE Name = 'CSILOOPBACK')
BEGIN
    RaisError('OLTP server loop back server CSILOOPBACK does not exist.  Please complete prerequisites',12,1)
END
GO

