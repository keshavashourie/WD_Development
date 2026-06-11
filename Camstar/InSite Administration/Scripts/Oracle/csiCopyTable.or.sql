-- Copyright Siemens 2023  
/* csiCopyTable
--     Simple proceure for copying a table
-- Parameters:
--     Source_Owner - Owner of the source table
--     Source_Name -  Name of the source table
--     Target_Owner - Owner of the target table
--     Target_Table - name of the target table
-- Usage: exec csiCopyTable (<Source_Owner>, <Source_Table>,<Target_Owner>,
--                             <Target_Table>,['S'/'T])
-- Notes: 
--      Requires "create any table" priv
--      ex: grant create any table to insiteadmin;
--
-- History:
--
-- Bill Lippard      12/04/2006  Added copyright notice (SPR S9984).
-- Bill Lippard      04/23/2007  Updated copyright notice (SPR S9984).
--
*/

create or replace procedure csicopytable
 (source_owner in  varchar2,
  source_name  in  varchar2,
  target_owner in  varchar2,
  target_name  in  varchar2,
  gen_columns  in  varchar2 := 'N')   /* specify which table to use to generate column list
                                         'S' = Source table 'T' = Target table */
IS
--
-- Copyright Siemens 2023  
--
 sqlstmt varchar2(8000);
 column_list    varchar2(8000);
 exists_check number;

BEGIN
    sqlstmt := 'select count(*) from dba_tables where table_name = upper('''
               || target_name || ''') and owner = upper(''' || target_owner ||''')';
    execute immediate sqlstmt into exists_check;
    IF exists_check > 0 then
      IF UPPER(gen_columns) = 'T' THEN
         /* GENERATE COLUMN LIST FOR THE TARGET TABLE 
            ADDED FOR LIVESYNC "SYNC" PROCESSING */
         csigetcolumnlist (target_owner, target_name, column_list);
         sqlstmt := 'truncate table ' || target_owner || '.' || target_name;
         execute immediate sqlstmt;
         sqlstmt := 'insert into ' || target_owner || '.' || target_name || 
                             '(' || column_list ||')' ||
                             ' select ' || column_list || ' from ' || source_owner || '.' || 
                               source_name;
         execute immediate sqlstmt;
      ELSIF UPPER(gen_columns) = 'S' THEN
          /* GENERATE COLUMN LIST FOR THE SOURCE TABLE
             ADDED FOR LIVESYNC "RESTORE" PROCESSING WHERE TARGET TABLE SCHEMA 
             MAY HAVE BEEN MODIFIED */
          csigetcolumnlist (source_owner, source_name, column_list);
          sqlstmt := 'truncate table ' || target_owner || '.' || target_name;
          execute immediate sqlstmt;
          sqlstmt := 'insert into '|| target_owner || '.' || target_name || 
                              '(' || column_list || ')' ||
                              ' select ' || column_list || ' from ' || source_owner || '.' || 
                                source_name;
          execute immediate sqlstmt;
      ELSE
          dbms_output.put_line('Target Table exists and no column list specified');
      END IF;
    ELSE /* TARGET TABLE DOES NOT EXIST PERFORM CREATE */
      sqlstmt := 'create table '|| target_owner || '.' || target_name ||
                 ' as select * from ' || source_owner || '.' || source_name;
      execute immediate sqlstmt;
    END IF;
--  Allow SQL errors codes to propagate to the calling program
--  exception
--    when others then
--      DBMS_OUTPUT.put_line(SQLERRM(SQLCODE) ||chr(13)|| sqlstmt);
--      rollback;
END;
/

