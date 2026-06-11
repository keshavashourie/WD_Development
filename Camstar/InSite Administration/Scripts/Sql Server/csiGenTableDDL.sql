/*
CsiGenTableDDL
    Generate the DDL for creating a specified table
Parameters:
    Owner - Table Owner
    Table_Name - Name of table to get PK info for
    TabDDL - OUTPUT Formated DDL string
    
Usage: exec CsiGenTableDDL <owner>, <table_name>, <@var1> output

History:
    Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
    Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
*/

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'CsiGenTableDDL' 
	   AND 	  type = 'P')
    DROP PROCEDURE CsiGenTableDDL
GO

CREATE PROCEDURE CsiGenTableDDL 
  @owner varchar(128),
  @table_name varchar(128),
  @tab_ddl varchar(8000) OUTPUT
AS
 --
 -- Copyright Siemens 2023  
 --
 declare @column_name varchar(128)
 declare @column_type varchar(128)
 declare @column_precision varchar(30)
 declare @nullable varchar(128)
 declare @ddl_cursor CURSOR 
 BEGIN
    set nocount on
    set @tab_ddl = 'Create table ' + @owner +'.'+ @table_name + '(' + char(13)
    set @ddl_cursor = CURSOR FOR 
        select  left(a.name,30) Name, left(b.name,15) Type, cast(a.prec as varchar), 
        case isnullable when 1 then 'NULL'
                        when 0 then 'NOT NULL'
        end
        from syscolumns a join systypes b on a.xusertype=b.xusertype 
        where id = object_id(@owner +'.'+ @table_name) order by colorder 
    open @ddl_cursor 
    fetch next from @ddl_cursor into @column_name, @column_type, @column_precision, @nullable 
    WHILE (@@fetch_status = 0)  
    BEGIN
      set @tab_ddl = @tab_ddl + '   ' + @column_name + ' ' + @column_type 
      if @column_type in ('nchar', 'char', 'nvarchar', 'varchar')
         set @tab_ddl = @tab_ddl + '(' + @column_precision + ')' + ' ' + @nullable
      else
         set @tab_ddl = @tab_ddl + ' ' + @nullable
      set @tab_ddl = @tab_ddl + ',' + char(13)
      fetch next from @ddl_cursor into @column_name, @column_type, @column_precision, @nullable 
    END
    CLOSE @ddl_cursor
    deallocate @ddl_cursor
    set @tab_ddl = @tab_ddl + ');' + char(13) + char(13)
end 
go

-- TEST 
-- declare @mytab varchar(8000)
-- exec CsiGenTableDDL 'insiteadmin', 'historymainline', @mytab output
-- print @mytab
-- go

