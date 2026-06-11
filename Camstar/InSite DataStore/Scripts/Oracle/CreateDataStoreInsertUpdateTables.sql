--------------------------------------------------------------------------------
-- SCRIPT: CreateDataStoreInsertUpdateTables.sql
-- DESCR: Creates the Datastore tables in the OLTP database
-- HISTORY:
--   05/03/2005 - Added ID column, Sequence and Trigger
--   05/27/2005 - Added index
--   06/23/2005 - Added master tables, triggers, etc. to support new
--                master/detail architecture.
--   08/09/2005 - Modified SEQUENCEs for ORDER and CACHE
--   12/20/2005 - Change to drop/recreate triggers.
--   12/06/2006 - Updated copyright notice (SPR S9984). Bill Lippard
--   04/23/2007 - Updated copyright notice (SPR S9984). Bill Lippard
--   10/25/2019 - Modified CREATE TRIGGER to CREATE OR REPLACE TRIGGER (CPR 1260)
--
-- Copyright Siemens 2023  
DECLARE
  I             NUMBER;
  v_TabNum      NUMBER;
  v_sql         VARCHAR2(600);  
  
  v_TblName     VARCHAR2(50);
  v_TblDef      VARCHAR2(255):=' (TXNID CHAR(16), SEQUENCE NUMBER, TXNTYPE CHAR(1), SQLSTMT CLOB)';
  v_TblPKDef    VARCHAR2(255):=' PRIMARY KEY (TXNID,SEQUENCE)';
  v_TblTrgName  VARCHAR2(50);
  
  v_MastTblName VARCHAR2(50);
  v_MastTblDef  VARCHAR2(255):='ID NUMBER, TXNID CHAR(16), TXNTYPE CHAR(1)';     
  v_MastTblNewCol VARCHAR2(255):= 'STATUS CHAR(1), RECORDDATE Date DEFAULT SYSDATE, SERVER VARCHAR2(50), CDOID NUMBER, 
ERROR VARCHAR2(1000)';--JAL Add to track process status of Txns
  v_MastPKDef   VARCHAR2(255):=' PRIMARY KEY (ID)';  
  v_SeqName     VARCHAR2(50);
  v_SeqOpts     VARCHAR2(255):=' ORDER NOCACHE';
  v_MastTblTrgName VARCHAR2(50);   
  
   v_NumTables   NUMBER;

   Column_Exists EXCEPTION;
   PRAGMA EXCEPTION_INIT(Column_Exists, -01430);

BEGIN
   SELECT TO_NUMBER(TVALUE)
   INTO v_NumTables
   FROM INSITESITEINFO
   WHERE TNAME='DataStoreInsertTables';
   
   IF v_NumTables=0 THEN
      RAISE_APPLICATION_ERROR(-20001,'DataStoreInsertTables is "0", should be >0.');
   END IF;  
 
   FOR v_TabNum IN 0..v_NumTables LOOP
      IF (v_TabNum=0) THEN
         v_MastTblName:='DATASTOREUPDATESMASTER';
         v_TblName:='DATASTOREUPDATES';
         v_TblTrgName:='DATASTOREUPDTRG';
         v_SeqName:='DATASTOREUPDSEQ';       
         v_MastTblTrgName:='DATASTOREUPDMTRG';  
      ELSE
         v_MastTblName:='DATASTOREINSERTS'||v_TabNum||'MASTER';
         v_TblName:='DATASTOREINSERTS'||v_TabNum;
         v_TblTrgName:='DATASTOREINS'||v_TabNum||'TRG';
         v_SeqName:='DATASTOREINSSEQ'||v_TabNum;         
         v_MastTblTrgName:='DATASTOREINS'||v_TabNum||'MTRG';
      END IF;
     
      -- Create Master Table sequence
      SELECT COUNT(*) INTO I FROM USER_SEQUENCES WHERE SEQUENCE_NAME = v_SeqName;      
      IF I=0 THEN
         v_sql:='CREATE SEQUENCE '||v_SeqName||v_SeqOpts;
         EXECUTE IMMEDIATE v_sql;
      END IF;
      
      -- Create master (control) table
      SELECT COUNT(*) INTO I FROM USER_TABLES WHERE TABLE_NAME = v_MastTblName;
      IF I=0 THEN
         v_sql:='CREATE TABLE '||v_MastTblName||'('||v_MastTblDef||', '||v_MastTblNewCol||')';
         EXECUTE IMMEDIATE v_sql;
         v_sql:='ALTER TABLE '||v_MastTblName||' ADD CONSTRAINT PK_'||v_MastTblName||v_MastPKDef;
         EXECUTE IMMEDIATE v_sql;
      ELSE
	
	 v_sql:='ALTER TABLE '||v_MastTblName||' ADD('||v_MastTblNewCol||')';

	 BEGIN
 	 	EXECUTE IMMEDIATE v_sql;
	 EXCEPTION 
             WHEN Column_Exists THEN
               DBMS_Output.Put_Line ('column exists');
         END;
         
      END IF;   
      
    
      -- Create the data table
      SELECT COUNT(*) INTO I FROM USER_TABLES WHERE TABLE_NAME = v_TblName;
      IF I=0 THEN
         v_sql:='CREATE TABLE '||v_TblName||v_TblDef;
         EXECUTE IMMEDIATE v_sql;
         v_sql:='ALTER TABLE '||v_TblName||' ADD CONSTRAINT PK_'||v_TblName||v_TblPKDef;
         EXECUTE IMMEDIATE v_sql;
      END IF; 
     
      -- Create the insert trigger
      v_sql:='CREATE OR REPLACE TRIGGER '||v_TblTrgName||' BEFORE INSERT ON '||v_TblName||' FOR EACH ROW ';
      v_sql:=v_sql||' DECLARE ';
      v_sql:=v_sql||'    PRAGMA AUTONOMOUS_TRANSACTION; ';
      v_sql:=v_sql||' BEGIN ';
      v_sql:=v_sql||'    IF (:NEW.SEQUENCE=1) THEN ';
      v_sql:=v_sql||'       INSERT INTO '||v_MastTblName||'(TXNID,TXNTYPE,STATUS,SERVER) ';
      v_sql:=v_sql||'       	VALUES (:NEW.TXNID,:NEW.TXNTYPE,''U'', SYS_CONTEXT(''USERENV'',''HOST'')); ';
      v_sql:=v_sql||'    END IF; ';
      v_sql:=v_sql||' COMMIT; ';
      v_sql:=v_sql||' END; ';
      EXECUTE IMMEDIATE v_sql;
       
      -- Create the delete trigger
      v_sql:='CREATE OR REPLACE TRIGGER '||v_MastTblTrgName||' BEFORE INSERT OR DELETE ON '||v_MastTblName||' FOR EACH ROW ';
      v_sql:=v_sql||' BEGIN ';
      v_sql:=v_sql||'    IF DELETING THEN DELETE FROM '||v_TblName||' WHERE TXNID=:OLD.TXNID; END IF;';
      v_sql:=v_sql||'    IF INSERTING THEN SELECT '||v_SeqName||'.NEXTVAL INTO :NEW.ID FROM DUAL; END IF;';
      v_sql:=v_sql||' END; ';
      EXECUTE IMMEDIATE v_sql;
    
   END LOOP;
END;
/
create or replace
PROCEDURE CSISETTXNSTATUS(
  v_Txnid    IN VARCHAR2,  
  v_Status   IN VARCHAR2,
  v_CDOID    IN NUMBER   DEFAULT 0,
  v_Msg      IN VARCHAR2 DEFAULT 'NA'
  )
IS
-------------------------------------------------------------------
-- csiHandleDataStoreRecord updates the status of Master record depending on 
-- success or failure of the commit of details records.
-- 
-- Copyright Siemens 2023  ------------------------------------------------------------------------------------------
-------------------------------------------------------------------
PRAGMA AUTONOMOUS_TRANSACTION; 
  v_NumOfTables NUMBER;
  v_TabNumber NUMBER;
  v_SQL varchar2(255);
BEGIN
  v_SQL:='UPDATE DatastoreUpdatesMaster set Status = '''||v_Status||'''  , recorddate = sysdate, ERROR= '''||v_Msg||''' 
, CDOID = '''||v_CDOID||'''  where txnid = '''||v_Txnid||'''';
  EXECUTE IMMEDIATE v_SQL; 
  SELECT TO_NUMBER(TVALUE)
    INTO v_NumOfTables
    FROM INSITESITEINFO
    WHERE TNAME='DataStoreInsertTables';

  FOR v_TabNumber in 1..v_NumOfTables LOOP
     v_SQL:='UPDATE DatastoreInserts'||v_TabNumber||'Master set Status = '''||v_Status||'''  , recorddate = sysdate, 
ERROR= '''||v_Msg||''' , CDOID = '''||v_CDOID||''' where txnid = '''||v_Txnid||'''';
     EXECUTE IMMEDIATE v_SQL;
  END LOOP;  
  COMMIT;
END;

/

