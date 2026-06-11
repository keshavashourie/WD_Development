/* ********************************************************************************
csiGenStageDDL
  Procedure used to generate the DDL for all LiveSync Stage tables
  Parameters:
       owner - owner of the stage tables
  Output:
       Prints the formated DDL returned by csigentableddl
  Usage: exec csiGenStageDDL <owner>
  History:
    Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
    Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
********************************************************************************** */

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiGenStageDDL' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiGenStageDDL
GO


CREATE PROCEDURE csiGenStageDDL 
  @owner  varchar(128)

AS
 --
 -- Copyright Siemens 2023  
 --
 declare @table_name varchar(128)
 declare @tab_ddl varchar(8000)
 declare @table_cursor CURSOR 

 BEGIN
    set nocount on
    set @table_cursor = CURSOR FOR 
        select name from sysobjects 
        where name like 'LS#%' and type='U' and uid=user_id(@owner)
    open @table_cursor
    fetch next from @table_cursor into @table_name
    WHILE (@@fetch_status = 0)  
      BEGIN
        exec csigentableddl @owner, @table_name, @tab_ddl output
        print @tab_ddl
        fetch next from @table_cursor into @table_name
      END
    CLOSE @table_cursor
    deallocate @table_cursor
 END
GO

