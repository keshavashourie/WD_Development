/*
CsiGetColumnList
     Return list of columns associated with a given table
Parameters:
    Owner - Owner of the table
    Table_Name - Name of the table
    column_list - OUTPUT comma seperated list of all columns for the table
Usage: exec CsiGetColumnList <Owner>, <Table>, <@var> output
History:
    Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
    Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
*/


IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'CsiGetColumnList' 
	   AND 	  type = 'P')
    DROP PROCEDURE CsiGetColumnList
GO

CREATE PROCEDURE CsiGetColumnList 
  @owner varchar(128),
  @table_name varchar(128),
  @column_list varchar(8000) OUTPUT
AS
 --
 -- Copyright Siemens 2023  
 --
 declare @column_name varchar(128)
 declare @column_cursor CURSOR 
 BEGIN
    set nocount on
    set @column_list = ' '
    set @column_cursor = CURSOR FOR 
        select column_name from information_schema.columns 
        where table_schema = @owner and table_name = @table_name
    open @column_cursor
    fetch next from @column_cursor into @column_name
    WHILE (@@fetch_status = 0) 
    BEGIN
      set @column_list = @column_list + @column_name + ','
      fetch next from @column_cursor into @column_name
    END
    CLOSE @column_cursor
    deallocate @column_cursor
    set @column_list = substring(@column_list,1,len(@column_list)-1) 
end 
go


