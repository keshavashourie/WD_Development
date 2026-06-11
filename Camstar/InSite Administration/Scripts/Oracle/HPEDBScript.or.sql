--------------------------------------------------------------------------------
-- SCRIPT: HPE_DB_Script.or.sql
-- DESCR:  These two new Global Temporary Tables are replacing Temp tables that were created in Designer
--
-- Copyright Siemens 2023
--------------------------------------------------------------------------------

DECLARE
   I INTEGER := 0;
  
BEGIN
	--
	SELECT COUNT(*)
	  INTO I
	  FROM USER_TABLES
	 WHERE Table_Name = 'ASSOCIATECONTAINERTEMP';

IF I > 0 THEN
EXECUTE IMMEDIATE 'DROP TABLE ASSOCIATECONTAINERTEMP';
END IF;


		EXECUTE IMMEDIATE 'CREATE GLOBAL TEMPORARY TABLE ASSOCIATECONTAINERTEMP (
                    ContainerId CHAR(16),
                    ContainerName VARCHAR2(40),
                    ParentContainerId CHAR(16),
                    CurrentStatusId CHAR(16),
                    NewCurrentStatusId CHAR(16),
                    WorkflowStepId CHAR(16),
                    CurrentCrossRefsId CHAR(16),
                    StepPassCountId CHAR(16),
                    AssociateHistoryId CHAR(16),
                    AssociateSeq NUMBER(10,0),
                    Sequence NUMBER(10,0))
                    ON COMMIT DELETE ROWS';
                    
       EXECUTE IMMEDIATE 'CREATE INDEX AssociateContainerTemp_NUI1 ON ASSOCIATECONTAINERTEMP( ContainerId )';
       EXECUTE IMMEDIATE 'CREATE INDEX AssociateContainerTemp_NUI2 ON ASSOCIATECONTAINERTEMP( ParentContainerId )';
       EXECUTE IMMEDIATE 'CREATE INDEX AssociateContainerTemp_NUI3 ON ASSOCIATECONTAINERTEMP( CurrentStatusId )';
                    

END;
/
DECLARE

   J INTEGER := 0;
BEGIN
	--
	SELECT COUNT(*)
	  INTO J
	  FROM USER_TABLES
	 WHERE Table_Name = 'ASSOCIATEFINDALLCHILDTEMP';

IF J > 0 THEN
EXECUTE IMMEDIATE 'DROP TABLE ASSOCIATEFINDALLCHILDTEMP';
END IF;



		EXECUTE IMMEDIATE 'CREATE GLOBAL TEMPORARY TABLE ASSOCIATEFINDALLCHILDTEMP (ContainerName VARCHAR2(40))ON COMMIT DELETE ROWS';
                    
        EXECUTE IMMEDIATE 'CREATE INDEX AssociateFindAllChildTemp_NUI1 ON ASSOCIATEFINDALLCHILDTEMP( ContainerName )';



END;
/
--------------------------------------------------------------------------------
-- Global Temporary Tables that utilise by scsDBMultiLotsModifyAttrs service
--------------------------------------------------------------------------------
DECLARE
   I INTEGER := 0;
  
BEGIN
	--
	SELECT COUNT(*)
	  INTO I
	  FROM USER_TABLES
	 WHERE Table_Name = 'MULTILOTSMODIFYATTRSTEMP';

IF I > 0 THEN
EXECUTE IMMEDIATE 'DROP TABLE MULTILOTSMODIFYATTRSTEMP';
END IF;

		EXECUTE IMMEDIATE 'CREATE GLOBAL TEMPORARY TABLE MULTILOTSMODIFYATTRSTEMP(
								ContainerName VARCHAR2(40), 
								AttributeName VARCHAR2(40),
								AttributeValue VARCHAR2(2000),
								AttributeRevision VARCHAR2(15),
								AttributeCDOName VARCHAR2(40),
								FieldType VARCHAR2(40),
								TableName VARCHAR2(40),
								ValidationFail NUMBER (1),
								CDOName VARCHAR2(40),
								CDODefId CHAR(16),
								PrecisionValue INTEGER,
								FieldName VARCHAR2(40),
								DBColumnName VARCHAR2(40),
								DOColumnName VARCHAR2(40),
								DBNameColumnName VARCHAR2(40),
								DBBaseColumnName VARCHAR2(40),
								DBRevisionColName VARCHAR2(40),
								AttributeTypeId CHAR(16),
								AccessLevel NUMBER(1),
								AlternateName1 VARCHAR2(40),
								AlternateName2 VARCHAR2(40),
								AttributeTypeName VARCHAR2(40),
								ObjectId CHAR(16),
								OldObjectId CHAR(16),
								OldRevision VARCHAR2(15),
								OldValue VARCHAR2(2000),
								ModifyAttrsHistoryDetailsId CHAR(16),
								ModifyAttributeSetupId CHAR(16),
								RequireModifyAttrsReason NUMBER(1),
								IsUpdated NUMBER(1),
								ApplyToChildLots NUMBER(1),
								LotTypeId CHAR(16)
								)
								ON COMMIT DELETE ROWS';
                    
       EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX MULTILOTSMODIFYATTRS_IDX1 ON MULTILOTSMODIFYATTRSTEMP (CONTAINERNAME ASC, ATTRIBUTENAME ASC)';  
	   EXECUTE IMMEDIATE 'CREATE INDEX MULTILOTSMODIFYATTRS_IDX2 ON MULTILOTSMODIFYATTRSTEMP (LotTypeId ASC)';     
END;
/
DECLARE
   I INTEGER := 0;
  
BEGIN
	--
	SELECT COUNT(*)
	  INTO I
	  FROM USER_TABLES
	 WHERE Table_Name = 'MULTILOTSMODIFYATTRSCHILDTEMP';

IF I > 0 THEN
EXECUTE IMMEDIATE 'DROP TABLE MULTILOTSMODIFYATTRSCHILDTEMP';
END IF;

		EXECUTE IMMEDIATE 'CREATE GLOBAL TEMPORARY TABLE MULTILOTSMODIFYATTRSCHILDTEMP(
								ContainerName VARCHAR2(40), 
								AttributeName VARCHAR2(40),
								AttributeValue VARCHAR2(2000),
								AttributeRevision VARCHAR2(15),
								AttributeCDOName VARCHAR2(40),
								FieldType VARCHAR2(40),
								TableName VARCHAR2(40),
								ValidationFail NUMBER (1),
								CDOName VARCHAR2(40),
								CDODefId CHAR(16),
								PrecisionValue INTEGER,
								FieldName VARCHAR2(40),
								DBColumnName VARCHAR2(40),
								DOColumnName VARCHAR2(40),
								DBNameColumnName VARCHAR2(40),
								DBBaseColumnName VARCHAR2(40),
								DBRevisionColName VARCHAR2(40),
								AttributeTypeId CHAR(16),
								AccessLevel NUMBER(1),
								AlternateName1 VARCHAR2(40),
								AlternateName2 VARCHAR2(40),
								AttributeTypeName VARCHAR2(40),
								ObjectId CHAR(16),
								OldObjectId CHAR(16),
								OldRevision VARCHAR2(15),
								OldValue VARCHAR2(2000),
								ModifyAttrsHistoryDetailsId CHAR(16),
								ModifyAttributeSetupId CHAR(16),
								RequireModifyAttrsReason NUMBER(1),
								IsUpdated NUMBER(1),
								LotTypeId CHAR(16)
								) 
								ON COMMIT DELETE ROWS';
                    
       EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX MULTILOTSMODIFYATTRSCHILDTEMP_IDX1 ON MULTILOTSMODIFYATTRSCHILDTEMP (CONTAINERNAME ASC, ATTRIBUTENAME ASC)'; 
	   EXECUTE IMMEDIATE 'CREATE INDEX MULTILOTSMODIFYATTRSCHILDTEMP_IDX2 ON MULTILOTSMODIFYATTRSCHILDTEMP (LotTypeId ASC)';  	   
END;
/

DECLARE
   I INTEGER := 0;
BEGIN
	SELECT COUNT(*) INTO I FROM USER_INDEXES WHERE INDEX_Name = 'CONTAINERBYNAMEUPPER';
IF I = 0 THEN
EXECUTE IMMEDIATE 'CREATE INDEX CONTAINERBYNAMEUPPER ON CONTAINER(UPPER(CONTAINERNAME))';
END IF;

END;
/
