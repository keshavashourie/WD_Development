--------------------------------------------------------------------------------
-- SCRIPT:PopulateSPCData.or.sql
-- DESCR: Adds data collection for SPC tester as well as modeling data for SPCCharts.
--        
--
-- Change History:
--	03/29/2012 - Initial Creation.
-- 
--  Copyright Siemens 2023  
--------------------------------------------------------------------------------
CREATE OR REPLACE PROCEDURE csiPopulateTesterData 
AS
    vTableCount integer;
BEGIN

   select COUNT(*)
     into vTableCount
     from user_objects
    where object_name = 'SPCTESTTABLE'  
     and object_type = 'TABLE';
    
    IF (vTableCount = 0) THEN
          EXECUTE IMMEDIATE 'CREATE TABLE SPCTestTable(CollectionSeq varchar2(5), Length float, Thickness float, Width float) '; 
         
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1,0.96,1,00001 )';
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq ) VALUES ( 1,0.96,1.01,00002 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1,0.96,0.99,00003 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq ) VALUES ( 1,0.96,0.98,00004 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1,0.96,1.01,00005 )';
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1,0.96,1.01,00006 )';
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq ) VALUES ( 1.025,0.98,0.955,00007 )';
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.05,1.05,0.965,00008 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.05,1.07,0.96,00009 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.05,0.99,0.98,00010 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.05,0.99,0.985,00011 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.05,0.99,0.975,00012 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.05,0.89,0.99,00013 )';
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.05,0.89,1.01,00014 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.05,0.99,1,00015 )';
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.02,1,1.02,00016 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.02,1,1.025,00017 )';  
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.02,1,1.015,00018 )';
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.02,0.94,1.04,00019 )';
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 1.02,0.94,1.045,00020 )';
          EXECUTE IMMEDIATE 'INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )	VALUES ( 0.95,0.94,1.035,00021 )';           
    END IF;
    
END;
/
BEGIN
  csiPopulateTesterData;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'DROP PROCEDURE csiPopulateTesterData';
END;
/

CREATE OR REPLACE PROCEDURE csiAddSPCUserDataCollection
AS
   CDOTypeBase NUMBER := 7266;   
   BaseInstanceId VARCHAR2(16);
   
   CDOTypeRev NUMBER := 7265;
   RevInstanceId VARCHAR2(16);
   
   CDOTypeDataPoint NUMBER := 7271;
   DPInstanceId VARCHAR2(16);    
   
BEGIN

    BEGIN
      SELECT DataCollectionDefBaseId INTO BaseInstanceId FROM DataCollectionDefBase WHERE DataCollectionDefName = 'SPCTesterPageUDC'; 
      EXCEPTION  WHEN NO_DATA_FOUND THEN BaseInstanceId := NULL; 
    END;
  
    IF(BaseInstanceId is Null) then   
      BEGIN
        csiPRDGetNextInstanceId(CDOTypeBase,BaseInstanceId);            
        csiPRDGetNextInstanceId(CDOTypeRev,RevInstanceId);            
        csiPRDGetNextInstanceId(CDOTypeDataPoint,DPInstanceId);            
        
               INSERT INTO DataCollectionDefBase
                   (CDOTypeId
                 ,ChangeCount
                 ,DataCollectionDefBaseId
                 ,DataCollectionDefName
                 ,IconId
                 ,RevOfRcdId)
                 VALUES
                          (CDOTypeBase,
                 1,
                 BaseInstanceId,
                 'SPCTesterPageUDC',
                 Null,
                 RevInstanceId);
                 
            INSERT INTO DataCollectionDef
                   (CDOTypeId
                   ,ChangeCount
                   ,DataCollectionDefBaseId
                   ,DataCollectionDefId
                   ,DataCollectionDefRevision
                   ,DataPointLayout
                   ,Description
                   ,DisplayLimits
                   ,Instructions
                   ,IsFrozen
                   ,ParametricDataDefType
                   ,Status)
               VALUES
                   (CDOTypeRev
                    ,1
                   ,BaseInstanceId
                   ,RevInstanceId
                   ,'1'
                   ,2
                   ,'OOB Data collection used to test SPC'
                   ,1
                   ,'Please enter a thickness.'
                   ,0
                   ,7273
                   ,1);
                   
              INSERT INTO DataPoint
                     (CDOTypeId
                     ,ChangeCount
                     ,ColumnPosition
                     ,DataCollectionDefId
                     ,DataPointId
                     ,DataPointName
                     ,DataType
                     ,DisplayLimits
                     ,IsFrozen
                     ,IsLimitOverrideAllowed
                     ,IsRequired
                     ,RowPosition)
                 VALUES
                     (CDOTypeDataPoint
                     ,1
                     ,1
                     ,RevInstanceId
                     ,DPInstanceId
                     ,'Thickness'
                     ,3
                     ,0
                     ,0
                     ,1
                     ,0
                     ,1);             


      END;
    ELSE
       DBMS_OUTPUT.PUT_LINE('Data Collection SPCTesterPageUDC already exists');
    END IF;
END;
/

BEGIN
  csiAddSPCUserDataCollection;
END;
/

BEGIN
   EXECUTE IMMEDIATE 'DROP PROCEDURE csiAddSPCUserDataCollection';  
END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiAddSMTPTransport
-- DESCR: Creates SMTPTransport
--
-- Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiAddSMTPTransport
AS
   vCDODefId NUMBER := 7019;
   pInstanceId VARCHAR2(16);
BEGIN

    BEGIN
      SELECT DataTransportId INTO pInstanceId FROM DataTransport WHERE DataTransportName = 'Email Transport'; 
      EXCEPTION  WHEN NO_DATA_FOUND THEN pInstanceId := NULL; 
    END;
  
    IF(pInstanceId is Null) then   
      BEGIN
        csiPRDGetNextInstanceId(vCDODefId,pInstanceId);            
        Insert into DataTransport
              (ByteOrderMark
              ,CDOTypeId
              ,ChangeCount
              ,DataTransportId
              ,DataTransportName
              ,IsFrozen
              ,IsSynchronous
              ,OkToTerminateIfFails
              ,TransportType
              ,ConnectionDocInit
              ,UseSSL)
           VALUES
               (0
              ,vCDODefId
              ,1
              ,pInstanceId
              ,'Email Transport'
              ,0
              ,0
              ,0	        
              ,'SMTP'
              ,'<SMTPTransport><Name><![CDATA[Email Transport]]></Name><Description __empty="yes"/><TransportType><![CDATA[SMTP]]></TransportType><URL __empty="yes"/><UserName __empty="yes"/><Password __empty="yes"/><IsSynchronous>false</IsSynchronous><TransportAssembly __empty="yes"/><OkToTerminateIfFails>false</OkToTerminateIfFails><Notes __empty="yes"/><ESigHistoryDetails/><UseSSL>false</UseSSL></SMTPTransport>'
              ,0);
 
        END;
      ELSE
         DBMS_OUTPUT.PUT_LINE('SMTPTransport ' || 'Email Transport' || ' already exists');
      END IF;
    

END;
/

--------------------------------------------------------------------------------
-- PROCEDURE: csiAddEMailMessage
-- DESCR: Creates vCDODefId
--
--  Copyright Siemens 2023  
CREATE OR REPLACE PROCEDURE csiAddEMailMessage
AS
   vCDODefId NUMBER := 7546;
   pInstanceId VARCHAR2(16);
BEGIN

    BEGIN
      SELECT EMailMessageId INTO pInstanceId FROM EMailMessage WHERE EMailMessageName = 'SPC Email Message'; 
      EXCEPTION  WHEN NO_DATA_FOUND THEN pInstanceId := NULL; 
    END;
  
    IF(pInstanceId is Null) then   
      BEGIN
        csiPRDGetNextInstanceId(vCDODefId,pInstanceId);            
        Insert into EMailMessage
              (CDOTypeId
               ,ChangeCount
               ,EMailMessageId
               ,EMailMessageName
               ,IsFrozen
               ,MessageFormat
               ,Sender)
           VALUES
               (vCDODefId
              ,1
              ,pInstanceId	        
              ,'SPC Email Message'	        
              ,0
              ,0
              ,'CamstarAdmin@camstar.com');
      END;
    ELSE
       DBMS_OUTPUT.PUT_LINE('EMailMessage ' || 'SPC Email Message' || ' already exists');
    END IF;
END;
/

BEGIN
  csiAddSMTPTransport;
  csiAddEMailMessage;
END;
/

BEGIN
  EXECUTE IMMEDIATE 'DROP PROCEDURE csiAddSMTPTransport';
  EXECUTE IMMEDIATE 'DROP PROCEDURE csiAddEMailMessage';  
END;
/

