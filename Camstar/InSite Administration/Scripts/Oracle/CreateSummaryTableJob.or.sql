--------------------------------------------------------------------------------
-- SCRIPT: CreateSummaryTableJob.sql
-- DESCR: Creates the Summary Table job
--
-- Copyright Siemens 2023  
DECLARE
   CURSOR c1 IS
   SELECT *
   FROM USER_JOBS
   WHERE WHAT LIKE 'csiSummaryTablePackage%';
   --
   v_JobNum NUMBER;
BEGIN
   FOR crec IN c1 LOOP
      DBMS_JOB.remove(CREC.JOB);
   END LOOP;
   --
   DBMS_JOB.submit(v_JobNum,'csiSummaryTablePackage.RunScheduled;',SYSDATE,'sysdate+1/(24*60)');
   
   COMMIT;
END;
/


