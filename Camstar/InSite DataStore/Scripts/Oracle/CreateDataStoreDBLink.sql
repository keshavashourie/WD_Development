--------------------------------------------------------------------------------
-- SCRIPT: CreateDataStoreDBLink.sql
-- DESCR: Creates the Datastore-to-OLTP database link
--
-- Copyright Siemens 2024
DECLARE
  I NUMBER;
  v_sql VARCHAR2(500);
BEGIN
   SELECT COUNT(*)
   INTO I
   FROM USER_DB_LINKS
   WHERE DB_LINK LIKE 'TO_INSITE_PRODUCTION%';
   --
   IF (I=0) THEN
      v_sql:='CREATE DATABASE LINK TO_INSITE_PRODUCTION CONNECT TO :DBUserName IDENTIFIED BY :DBPassword USING '':OLTPProtocol://:OLTPDBHOST::OLTPPORT/:DBName'' ';
      EXECUTE IMMEDIATE v_sql;
   END IF;
END;
/

