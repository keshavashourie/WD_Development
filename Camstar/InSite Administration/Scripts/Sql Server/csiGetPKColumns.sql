/*
csiGetPKColumns
    Return a comma seperated list of columns that constitute the PK for a given table
Parameters:
    TabOwner - Table Owner
    TabName - Name of table to get PK info for
    Constraintname - OUTPUT name of PK constraint
    col_list - OUTPUT comma seperated list of columns in the PK
    
Usage: exec csiGetPKColumns <owner>, <table>, <@var1> output,  <@var2> output,

History:
    Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
    Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
*/

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetPKColumns' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiGetPKColumns
GO

CREATE PROCEDURE csiGetPKColumns 
  @tabowner varchar(128),
  @tabname varchar(128),
  @constraintname varchar(128) OUTPUT,
  @col_list varchar(2000) OUTPUT

AS
 --
 -- Copyright Siemens 2023  
 --
 declare @colname varchar(128)
 declare @constraint_curs CURSOR

BEGIN
  set @col_list = ' '
  set @constraint_curs = CURSOR FOR 
          select constraint_name, column_name
          from information_schema.key_column_usage 
          where table_name = @tabname
            and table_schema = @tabowner
          order by ordinal_position
  open @constraint_curs
  fetch next from @constraint_curs into @constraintname, @colname
  WHILE (@@fetch_status = 0)  
  BEGIN
    set @col_list = @col_list + @colname + ','
    fetch next from @constraint_curs into @constraintname, @colname    
  END 
  if len(@col_list) > 1
    set @col_list = substring(@col_list,1,len(@col_list)-1);
  close @constraint_curs
end
go



--Test
--declare @mycols varchar(2000)
--declare @mycons varchar(128)
--begin
--exec csiGetPKColumns 'insiteadmin','WorkflowStepPathSelectors', @mycons output, @mycols output
--print @mycons + ' ' + @mycols
--end
--go
