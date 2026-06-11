/*
CsiGetColumnList
     Return list of columns associated with a given table
Parameters:
    Owner - Owner of the table
    Table_Name - Name of the table
    column_list - OUTPUT comma seperated list of all columns for the table
History:
    Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
    Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
*/

create or replace procedure csigetcolumnlist
    (tableowner in varchar2, 
     tablename in varchar2, 
     col_list out varchar2) 
is 
--
-- Copyright Siemens 2023  
--
  TYPE ColCurTyp IS REF CURSOR;
  Col_cv ColCurTyp;
  col_name varchar2(30);
 
begin
   col_list := ' ';
   open col_cv for 'Select column_name from dba_tab_columns 
                    where owner = upper(:1)
                      and table_name = upper(:2)'
         using tableowner, tablename;
   LOOP
     fetch col_cv into col_name; 
     exit when col_cv%NOTFOUND;
     col_list := col_list || col_name ||',';
   END LOOP;
  col_list := substr(col_list,1,length(col_list)-1);
--  Allow SQL errors codes to propagate to the calling program
--  exception
--    when others then
--      DBMS_OUTPUT.put_line(SQLERRM(SQLCODE));
--      rollback;
END csigetcolumnlist; 
/

