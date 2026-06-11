/*
csiGetPKColumns
    Return a comma seperated list of columns that constitute the PK for a given table
Parameters:
    TabOwner - Table Owner
    TabName - Name of table to get PK info for
    Constraintname - OUTPUT name of PK constraint
    col_list - OUTPUT comma seperated list of columns in the PK
History:
    Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
    Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
*/

CREATE OR REPLACE PROCEDURE csiGetPKColumns (
      tabowner varchar2,
      tabname varchar2,
      constraintname out varchar2,
      col_list out varchar2)    
IS
--
-- Copyright Siemens 2023  
--
 colname varchar2(30);
 TYPE ColCurTyp is REF CURSOR;
 constraint_curs ColCurTyp;

BEGIN
  col_list := ' ';
  open constraint_curs for 
          'select con.CONSTRAINT_NAME, col.column_name
           from dba_constraints con, dba_cons_columns col
           where con.constraint_name = col.constraint_name
             and con.table_name = upper(:1)
             and con.owner = upper(:2)
             and con.constraint_type = ''P''
           order by position' 
       using tabname, tabowner;
  LOOP
    fetch constraint_curs into constraintname, colname;
    exit when constraint_curs%NOTFOUND;
    col_list := col_list || colname ||',';
  END LOOP; 
  if length(col_list) > 1 then
    col_list := substr(col_list,1,length(col_list)-1);
  end if;
  close constraint_curs;
--  Allow SQL errors codes to propagate to the calling program
--  exception
--    when others then
--      DBMS_OUTPUT.put_line(SQLERRM(SQLCODE));
--      rollback;
end;
/


