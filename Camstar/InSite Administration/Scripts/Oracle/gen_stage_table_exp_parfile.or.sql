-- Generates a parameter file for use by the export utility to export all LS# tables
-- for a given schema/owner
-- Requires two input parms
--  1 - schema owner for the stage tables
--  2 - spool filename
--
-- Copyright Siemens 2023  
--

SET TERMOUT OFF
SET VERIFY OFF
SET FEEDBACK OFF;
SET ECHO OFF;
SET HEAD OFF;
set linesize 255;
spool &2
set serveroutput on size 300000;
declare 
   tab_owner varchar2(50);
   table_name varchar2(50);
   cursor c1 is
     select table_name from dba_tables where table_name like 'LS#%' and owner = upper('&1')
       union 
     select 'CONTROLDETAILS' from dual
       union
     select 'UPDATECONTROL' from dual;
begin
   tab_owner := '&1';
   DBMS_OUTPUT.put_line('tables=(');
   open c1;
   loop
      fetch c1 into table_name;
      if (c1%notfound) then
        exit;
      end if;
      DBMS_OUTPUT.put_line(''''||tab_owner||'.'||table_name||''',');
   end loop;
   DBMS_OUTPUT.put_line(')');
end;
/
spool off
exit;
