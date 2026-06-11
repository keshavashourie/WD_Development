--------------------------------------------------------------------------------
-- SCRIPT:PopulateSPCData.sql
-- DESCR: Adds data for SPC tester as well as modeling data for SPCCharts.
--        
--
-- Change History:
--	03/29/2012 - Initial Creation.
-- 
-- Copyright Siemens 2023  

--------------------------------------------------------------------------------

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiPopulateTesterData')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiPopulateTesterData;
 	END IF;
END $$;
CREATE PROCEDURE csiPopulateTesterData()
LANGUAGE plpgsql
AS $$
DECLARE
    vInsertRowCount    INTEGER := 0;	
    vErrMsg            VARCHAR(1024);
    vErrLocator        INTEGER := 0;
	vExpErrLocator	   VARCHAR;
BEGIN
    PERFORM 1 FROM information_schema.tables WHERE lower(table_name) = 'spctesttable' LIMIT 1;
    IF NOT FOUND THEN
        CREATE TABLE SPCTestTable (
            CollectionSeq   CHAR(5) NOT NULL,
            Length          DOUBLE PRECISION,
            Thickness       DOUBLE PRECISION,
            Width           DOUBLE PRECISION,
            CONSTRAINT SPCTestTable_PK PRIMARY KEY (CollectionSeq)
        );
        
        BEGIN
			--
			RAISE NOTICE '------------------ Loading SPC test data -------------------';
			-- Load SPC data            
            --			
			vErrLocator := 10;
            INSERT INTO SPCTESTTABLE (Length, Thickness, Width, CollectionSeq) 
			VALUES (1, 0.96, 1, '00001');            
            vInsertRowCount := vInsertRowCount + 1;
            --			
			vErrLocator := 15;
            INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq ) 
			VALUES ( 1,0.96,1.01,'00002' );
			vInsertRowCount := vInsertRowCount + 1;		
            --
			vErrLocator := 20;	
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1,0.96,0.99,'00003' );			
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 25;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1,0.96,0.98,'00004' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 30;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1,0.96,1.01,'00005' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 35;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1,0.96,1.01,'00006' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 40;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.025,0.98,0.955,'00007' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 45;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.05,1.05,0.965,'00008' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 50;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.05,1.07,0.96,'00009' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 55;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.05,0.99,0.98,'00010' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 60;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.05,0.99,0.985,'00011' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 65;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.05,0.99,0.975,'00012' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 70;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.05,0.89,0.99,'00013' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 75;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.05,0.89,1.01,'00014' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 80;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.05,0.99,1,'00015' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 85;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.02,1,1.02,'00016' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 90;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.02,1,1.025,'00017' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 95;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.02,1,1.015,'00018' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 100;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.02,0.94,1.04,'00019' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 105;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 1.02,0.94,1.045,'00020' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			vErrLocator := 110;
			INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
			VALUES ( 0.95,0.94,1.035,'00021' );
			vInsertRowCount := vInsertRowCount + 1;
			--
			RAISE NOTICE 'SPC test data successfully inserted % row(s)', vInsertRowCount;
        EXCEPTION
            WHEN OTHERS THEN
                GET STACKED DIAGNOSTICS vErrMsg = MESSAGE_TEXT, vExpErrLocator = PG_EXCEPTION_CONTEXT;
                RAISE NOTICE 'Error loading SPC data - ErrLoc: % ErrMsg: % ErrNum: %', vExpErrLocator, vErrMsg, SQLSTATE;
        END;
                        
    END IF;
end $$;

do $$ 
begin	
 CALL csiPopulateTesterData();
 DROP PROCEDURE IF EXISTS csiPopulateTesterData;
end $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiAddSPCUserDataCollection')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiAddSPCUserDataCollection;
 	END IF;
END $$;
CREATE PROCEDURE csiAddSPCUserDataCollection()
LANGUAGE plpgsql
AS $$
DECLARE	
	DECLARE pInsertRowCount	INTEGER;
	--
	DECLARE pCountRS            INTEGER;
	DECLARE pCDOTypeBase        INTEGER;
	DECLARE pCDOTypeRev         INTEGER;	
	DECLARE pCDOTypeDataPoint   INTEGER;		
	DECLARE pBaseInstanceId     VARCHAR(16);
	DECLARE pRevInstanceId      VARCHAR(16);
	DECLARE pDPInstanceId       VARCHAR(16);	
BEGIN
			
	pInsertRowCount   := 0;		
	pCDOTypeBase      := 7266;		
	pCDOTypeRev       := 7265;		
	pCDOTypeDataPoint := 7271;		
	
	--
	RAISE NOTICE '------------------ Add a Data Collection with a data point -------------------';
	--

    SELECT COUNT(*) into pCountRS FROM DataCollectionDefBase WHERE DataCollectionDefName = 'SPCTesterPageUDC';   
    IF pCountRS = 0 THEN
	  BEGIN	  
		CALL csiPRDGetNextInstanceId(pCDOTypeBase, pBaseInstanceId);
		CALL csiPRDGetNextInstanceId(pCDOTypeRev, pRevInstanceId);
		CALL csiPRDGetNextInstanceId(pCDOTypeDataPoint, pDPInstanceId);						
        
        INSERT INTO DataCollectionDefBase
 				     (CDOTypeId
					 ,ChangeCount
					 ,DataCollectionDefBaseId
					 ,DataCollectionDefName
					 ,IconId
					 ,RevOfRcdId)
			VALUES
                    (pCDOTypeBase,
					 1,
					 pBaseInstanceId,
					 'SPCTesterPageUDC',
					 Null,
					 pRevInstanceId);
					 
					pInsertRowCount := pInsertRowCount + 1;
           
           			  
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
				   (pCDOTypeRev
					,1
				   ,pBaseInstanceId
				   ,pRevInstanceId
				   ,'1'
				   ,2
				   ,'OOB Data collection used to test SPC'
				   ,1
				   ,'Please enter a thickness.'
				   ,0
				   ,7273
				   ,1);
				  
				  pInsertRowCount := pInsertRowCount + 1;
				  


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
				   (pCDOTypeDataPoint
				   ,1
				   ,1
				   ,pRevInstanceId
				   ,pDPInstanceId
				   ,'Thickness'
				   ,3
				   ,0
				   ,0
				   ,1
				   ,0
				   ,1);
				   
			  pInsertRowCount := pInsertRowCount + 1;								   

	  END;
	ELSE
	   RAISE NOTICE 'Data Collection SPCTesterPageUDC already exists';
	END IF;
	
	RAISE NOTICE 'SPC modeling data for Data Collection successfully inserted % row(s)', pInsertRowCount;
end $$;

do $$ 
begin	
 CALL csiAddSPCUserDataCollection();
 DROP PROCEDURE IF EXISTS csiAddSPCUserDataCollection;
end $$;

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiAddSMTPTransport')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiAddSMTPTransport;
 	END IF;
END $$;
CREATE PROCEDURE csiAddSMTPTransport()
LANGUAGE plpgsql
AS $$
DECLARE
	pInsertRowCount	INTEGER;
	--
	pCountRS            INTEGER;
	pCDOTypeId          INTEGER;
	pInstanceId         VARCHAR(16);
BEGIN	
	
	pInsertRowCount := 0;
	pCDOTypeId := 7019;		

	--
	RAISE NOTICE '------------------ Adding SMTPTransport -------------------';
	--

    SELECT COUNT(*) INTO pCountRS FROM DataTransport WHERE DataTransportName = 'Email Transport';    
    IF pCountRS = 0 THEN
	  BEGIN	  
		CALL csiPRDGetNextInstanceId(pCDOTypeId, pInstanceId);
			INSERT INTO DataTransport
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
						,pCDOTypeId
						,1
						,pInstanceId
						,'Email Transport'
						,0
						,0
						,0	        
						,'SMTP'
						,'<SMTPTransport><Name><![CDATA[Email Transport]]></Name><Description __empty="yes"/><TransportType><![CDATA[SMTP]]></TransportType><URL __empty="yes"/><UserName __empty="yes"/><Password __empty="yes"/><IsSynchronous>false</IsSynchronous><TransportAssembly __empty="yes"/><OkToTerminateIfFails>false</OkToTerminateIfFails><Notes __empty="yes"/><ESigHistoryDetails/><UseSSL>false</UseSSL></SMTPTransport>'
						,0);
				   
		  pInsertRowCount := pInsertRowCount + 1;
	  END;
	ELSE
	   RAISE NOTICE 'DataTransport Email Transport already exists';
	END IF;
	
	RAISE NOTICE 'SPC modeling data for DataTransport successfully inserted % row(s)', pInsertRowCount;

end $$;

do $$ 
begin	
 CALL csiAddSMTPTransport();	
 DROP PROCEDURE IF EXISTS csiAddSMTPTransport;
end $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiAddEMailMessage')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiAddEMailMessage;
 	END IF;
END $$;
CREATE PROCEDURE csiAddEMailMessage()
LANGUAGE plpgsql
AS $$
DECLARE
	pInsertRowCount	INTEGER;
	--
	pCountRS            INTEGER;
	pCDOTypeId          INTEGER;
	pInstanceId         VARCHAR(16);
BEGIN	
	
	pInsertRowCount := 0;
	pCDOTypeId := 7546;

	--
	RAISE NOTICE '------------------ Adding EMailMessage -------------------';
	--

    SELECT COUNT(*) INTO pCountRS FROM EMailMessage WHERE EMailMessageName = 'SPC Email Message';    
    IF pCountRS = 0 THEN
	  BEGIN	  
		CALL csiPRDGetNextInstanceId(pCDOTypeId, pInstanceId);
		INSERT INTO EMailMessage
					(CDOTypeId
					 ,ChangeCount
					 ,EMailMessageId
					 ,EMailMessageName
					 ,IsFrozen
					 ,MessageFormat
					 ,Sender)
			 VALUES
				   (pCDOTypeId
					,1
					,pInstanceId	        
					,'SPC Email Message'	        
					,0
					,0
					,'CamstarAdmin@camstar.com');
				   
		  pInsertRowCount := pInsertRowCount + 1;	
	  END;
	ELSE
	   RAISE NOTICE 'EMailMessage SPC Email Message already exists';
	END IF;

	RAISE NOTICE 'SPC modeling data for EMailMessage successfully inserted % row(s)', pInsertRowCount;

end $$;

do $$ 
begin	
 CALL csiAddEMailMessage();
 DROP PROCEDURE IF EXISTS csiAddEMailMessage;
end $$;