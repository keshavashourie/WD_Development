/* ********************************************************************************
csiBCPStageTables
  Generate and Print BCP commands required for the export and import of LiveSync
  Stage Tables
  Output can be redirected to create command files.

  NOTE: BCP Commands can be run from within the database using the following setup
    However, this requires the datafiles to be "local"to the database server.
      example: 
         connect as SA
         use master
         exec sp_grantdbaccess 'insiteadmin'
         grant execute on xp_cmdshell to insiteadmin
         exec xp_sqlagent_proxy_account N'SET', N'servername', N'administrator', N'admin'
         EXEC master..xp_cmdshell @bcpcmd , no_output

  History:
    Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
    Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
********************************************************************************** */

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiBCPStageTables' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiBCPStageTables
GO

CREATE PROCEDURE csiBCPStageTables 
  @action varchar(10),
  @owner varchar(128),
  @dbname varchar(128),
  @file_path varchar(512),
  @servername varchar(128)
AS
--
-- Copyright Siemens 2023  
--
 declare @bcpcmd varchar(500)
 declare @table_name varchar(128)
 declare @table_cursor CURSOR 

 BEGIN
    set nocount on
    set @table_cursor = CURSOR FOR 
        select name from sysobjects where name like 'LS#%' and type='U' and uid=user_id(@owner)
           union
        select 'CONTROLDETAILS'
           union
        select 'UPDATECONTROL'
    open @table_cursor
    fetch next from @table_cursor into @table_name
    WHILE (@@fetch_status = 0)  
    BEGIN
      IF @action = 'EXPORT'    /* GENERATE BCP OUT COMMANDS FOR ALL LS# TABLES */
      BEGIN
         set @bcpcmd =  'bcp '
                        + @dbname + '.'
                        + @owner  + '.' 
                        + @table_name + ' OUT ' 
                        + @file_path +'\' 
                        + @table_name + '.txt -n -U %3 -P %4 -S %7'
      END
      IF @action = 'IMPORT'    /* GENERATE BCP IN COMMANDS FOR ALL LS# TABLES */
      BEGIN
         set @bcpcmd =  'bcp '
                        + '%2' + '.'
                        + '%5'  + '.' 
                        + @table_name + ' IN ' 
                        + '%6' +'\' 
                        + @table_name + '.txt -n -U %3 -P %4 -S %7'
      END
      print @bcpcmd
      fetch next from @table_cursor into @table_name
    END
    CLOSE @table_cursor
    deallocate @table_cursor
 END
GO


