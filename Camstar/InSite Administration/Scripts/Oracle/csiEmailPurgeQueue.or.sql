--------------------------------------------------------------------------------
-- SCRIPT: csiEmailPurgeQueue.or.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
CREATE OR REPLACE PROCEDURE csiEmailPurgeQueue
AS
--
-- Copyright Siemens 2023  
--      
   v_RetentionPeriod         NUMBER;
   v_FailureRetentionPeriod  NUMBER;
BEGIN
   SELECT TO_NUMBER(TValue)
   INTO v_RetentionPeriod
   FROM InSiteSiteInfo
   WHERE TName='EmailNotificationRetentionPeriod';

   SELECT TO_NUMBER(TValue)
   INTO v_FailureRetentionPeriod
   FROM InSiteSiteInfo
   WHERE TName='EmailNotificationFailureRetentionPeriod';
   
   -- Successful E-mails
   DELETE FROM EmailQueue
   WHERE ProcessingStatus=1
   AND CreatedDate < SYSDATE - v_RetentionPeriod/24; -- RetentionPeriod is in hours

   -- Failed E-mails
   DELETE FROM EmailQueue
   WHERE ProcessingStatus=2
   AND CreatedDate < SYSDATE - v_FailureRetentionPeriod/24;

   COMMIT;
END;
/

