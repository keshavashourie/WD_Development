--------------------------------------------------------------------------------
-- SCRIPT: VerifyDataStorePrerequisites.sql
-- DESCR: Verifies the major installation prerequisites.
-- HISTORY:
--
--     v8.4 (3)       Dan Maloney     10/05/2020 replace DBMS_LOCK.SLEEP with DBMS_SESSION.SLEEP as DBMS_LOCK.SLEEP is depreciated.  US 109349
-- Copyright Siemens 2023  
DECLARE
   v_sql varchar2(155);
BEGIN
   v_sql:='BEGIN DBMS_SESSION.SLEEP(0); END;';
   EXECUTE IMMEDIATE v_sql;
EXCEPTION
   WHEN OTHERS THEN
   RAISE_APPLICATION_ERROR(-20001,'The required system package "DBMS_SESSION" has not been installed or configured correctly!');
END;
/
DECLARE
   v_sql varchar2(255);
BEGIN
   v_sql:='DECLARE I NUMBER; BEGIN SELECT COUNT(*) INTO I FROM INSITESITEINFO@TO_INSITE_PRODUCTION; END;';
   EXECUTE IMMEDIATE v_sql;
EXCEPTION
   WHEN OTHERS THEN
   RAISE_APPLICATION_ERROR(-20001,'Unable to communicate to the OLTP database using the database link TO_INSITE_PRODUCTION!');  
END;
/
