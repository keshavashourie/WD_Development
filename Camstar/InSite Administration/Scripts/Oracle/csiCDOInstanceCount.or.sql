--------------------------------------------------------------------------------
-- SCRIPT: csiCDOInstanceCount.sql
--         Function to count existing CDO instances.
--
-- Parameters:
--      pTableName - The name of table stored instances.
--
-- Return:
--      Count of existing CDOs.
--
-- HISTORY:
--
-- Copyright Siemens 2023  
--
CREATE OR REPLACE FUNCTION csiCDOInstanceCount ( pTableName IN VARCHAR2 ) RETURN NUMBER 
--
-- Copyright Siemens 2023  
--
IS 
  v_count NUMBER :=0;
  table_does_not_exists EXCEPTION;
  invalid_table_name EXCEPTION;
  PRAGMA EXCEPTION_INIT(table_does_not_exists, -942);
  PRAGMA EXCEPTION_INIT(invalid_table_name  , -903);
BEGIN
  EXECUTE IMMEDIATE 'select count(*) from ' || pTableName  INTO v_count;  
  RETURN v_count;
  EXCEPTION 
    WHEN table_does_not_exists OR invalid_table_name THEN  RETURN NULL;
END;
/
