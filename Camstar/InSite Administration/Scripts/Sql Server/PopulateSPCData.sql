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

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiPopulateTesterData' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiPopulateTesterData
GO

CREATE PROCEDURE csiPopulateTesterData AS
BEGIN

	IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[SPCTestTable]') AND type in (N'U'))
	BEGIN

	CREATE TABLE [SPCTestTable](
					[CollectionSeq] [nchar](5) NOT NULL,
					[Length] [float] NULL,
					[Thickness] [float] NULL,
					[Width] [float] NULL,
	CONSTRAINT [SPCTestTable671088643] PRIMARY KEY NONCLUSTERED 
		(
										[CollectionSeq] ASC
		) 
		) ON [PRIMARY]


			DECLARE @n_ErrLocator		NUMERIC;
			DECLARE @n_InsertRowCount	NUMERIC;
			DECLARE @n_UpdateRowCount	NUMERIC;
			--
			DECLARE @v_ErrMsg			VARCHAR(1024);
			DECLARE @i_ErrorNumber		INTEGER;
			DECLARE @CDOTypeId          INTEGER;
			DECLARE @InstanceId         VARCHAR(16);

			--
			PRINT ('------------------ Loading SPC test data -------------------');
			--
			BEGIN TRY
				--
				SET @n_InsertRowCount = 0;
				SET @v_ErrMsg = NULL;
				--
				-- Load SPC data....
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 10;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1,0.96,1,00001 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 15;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1,0.96,1.01,00002 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 20;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1,0.96,0.99,00003 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 25;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1,0.96,0.98,00004 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 30;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1,0.96,1.01,00005 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 35;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1,0.96,1.01,00006 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 40;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.025,0.98,0.955,00007 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 45;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.05,1.05,0.965,00008 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 50;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.05,1.07,0.96,00009 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 55;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.05,0.99,0.98,00010 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 60;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.05,0.99,0.985,00011 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 65;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.05,0.99,0.975,00012 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 70;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.05,0.89,0.99,00013 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 75;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.05,0.89,1.01,00014 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 80;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.05,0.99,1,00015 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 85;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.02,1,1.02,00016 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 90;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.02,1,1.025,00017 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 95;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.02,1,1.015,00018 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 100;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.02,0.94,1.04,00019 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 105;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 1.02,0.94,1.045,00020 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
					SET @n_ErrLocator = 110;
					--
					INSERT INTO SPCTESTTABLE ( Length,Thickness,Width,CollectionSeq )
					VALUES ( 0.95,0.94,1.035,00021 );
					--
					SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;
					--
				--
				--
				PRINT 'SPC test data successfully inserted ' + CAST(@n_InsertRowCount AS VARCHAR(8)) + ' row(s)';				   						   				   				   
				--
				--
					
			END TRY
			--	
			--
			BEGIN CATCH
				--
				SELECT @v_ErrMsg = 'Error loading SPC data - ErrLoc: ' + CAST(@n_ErrLocator AS VARCHAR(8)) + ' ErrMsg: ' + ERROR_MESSAGE() + ' ErrNum: ' + CAST(ERROR_NUMBER() AS VARCHAR(8))
					  ,@i_ErrorNumber = ERROR_NUMBER();

				PRINT @v_ErrMsg;
  				--
			END CATCH;
			--	
			--
	END
END			
GO

EXEC csiPopulateTesterData
GO
DROP PROCEDURE csiPopulateTesterData
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiAddSPCUserDataCollection' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiAddSPCUserDataCollection
GO

CREATE PROCEDURE csiAddSPCUserDataCollection AS

BEGIN
	DECLARE @n_ErrLocator		NUMERIC;
	DECLARE @n_InsertRowCount	NUMERIC;
	DECLARE @n_UpdateRowCount	NUMERIC;
	--
	DECLARE @v_ErrMsg			VARCHAR(1024);
	DECLARE @i_ErrorNumber		INTEGER;
	DECLARE @mycount            INTEGER;
	DECLARE @CDOTypeBase        INTEGER;
	DECLARE @CDOTypeRev         INTEGER;	
	DECLARE @CDOTypeDataPoint   INTEGER;		
	DECLARE @BaseInstanceId     VARCHAR(16);
	DECLARE @RevInstanceId      VARCHAR(16);
	DECLARE @DPInstanceId       VARCHAR(16);		
	
	SET @n_InsertRowCount = 0;		
	SET @CDOTypeBase      = 7266;		
	SET @CDOTypeRev       = 7265;		
	SET @CDOTypeDataPoint = 7271;		
	
	--
	PRINT ('------------------ Add a Data Collection with a data point -------------------');
	--

    SELECT @mycount = COUNT(*) FROM DataCollectionDefBase WHERE DataCollectionDefName = 'SPCTesterPageUDC';    
    if @mycount = 0 
	  BEGIN	  
		EXEC csiPRDGetNextInstanceId @CDOTypeBase,      @BaseInstanceId OUTPUT		
		EXEC csiPRDGetNextInstanceId @CDOTypeRev,       @RevInstanceId  OUTPUT		
		EXEC csiPRDGetNextInstanceId @CDOTypeDataPoint, @DPInstanceId   OUTPUT						
        
         INSERT INTO DataCollectionDefBase
 				     (CDOTypeId
					 ,ChangeCount
					 ,DataCollectionDefBaseId
					 ,DataCollectionDefName
					 ,IconId
					 ,RevOfRcdId)
           VALUES
                    (@CDOTypeBase,
					 1,
					 @BaseInstanceId,
					 'SPCTesterPageUDC',
					 Null,
					 @RevInstanceId);
					 
					  SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;							 
           
           			  
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
					   (@CDOTypeRev
					    ,1
					   ,@BaseInstanceId
					   ,@RevInstanceId
					   ,'1'
					   ,2
					   ,'OOB Data collection used to test SPC'
					   ,1
					   ,'Please enter a thickness.'
					   ,0
					   ,7273
					   ,1);
					  
					  SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;		
					  


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
						   (@CDOTypeDataPoint
						   ,1
						   ,1
						   ,@RevInstanceId
						   ,@DPInstanceId
						   ,'Thickness'
						   ,3
						   ,0
						   ,0
						   ,1
						   ,0
						   ,1);
						   
					  SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;								   
					  
	  END
	ELSE
	   PRINT('Data Collection SPCTesterPageUDC already exists');							      				    	   
		--
		--
		PRINT 'SPC modeling data for Data Collection successfully inserted ' + CAST(@n_InsertRowCount AS VARCHAR(8)) + ' row(s)';				   						   				   				   
		--
		--
END
GO

EXEC csiAddSPCUserDataCollection
GO
DROP PROCEDURE csiAddSPCUserDataCollection
GO

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiAddSMTPTransport' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiAddSMTPTransport
GO

CREATE PROCEDURE csiAddSMTPTransport AS

BEGIN
	DECLARE @n_ErrLocator		NUMERIC;
	DECLARE @n_InsertRowCount	NUMERIC;
	DECLARE @n_UpdateRowCount	NUMERIC;
	--
	DECLARE @v_ErrMsg			VARCHAR(1024);
	DECLARE @i_ErrorNumber		INTEGER;
	DECLARE @mycount            INTEGER;
	DECLARE @CDOTypeId          INTEGER;
	DECLARE @InstanceId         VARCHAR(16);
	
	SET @n_InsertRowCount = 0;		
	SET @CDOTypeId = 7019;		

	--
	PRINT ('------------------ Adding SMTPTransport -------------------');
	--

    SELECT @mycount = COUNT(*) FROM DataTransport WHERE DataTransportName = 'Email Transport';    
    if @mycount = 0 
	  BEGIN	  
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT			  
			Insert into [DataTransport]
						([ByteOrderMark]
						,[CDOTypeId]
						,[ChangeCount]
						,[DataTransportId]
						,[DataTransportName]
						,[IsFrozen]
						,[IsSynchronous]
						,[OkToTerminateIfFails]
						,[TransportType]
						,[ConnectionDocInit]
						,[UseSSL])
				 VALUES
					   (0
						,@CDOTypeId
						,1
						,@InstanceId
						,'Email Transport'
						,0
						,0
						,0	        
						,'SMTP'
						,'<SMTPTransport><Name><![CDATA[Email Transport]]></Name><Description __empty="yes"/><TransportType><![CDATA[SMTP]]></TransportType><URL __empty="yes"/><UserName __empty="yes"/><Password __empty="yes"/><IsSynchronous>false</IsSynchronous><TransportAssembly __empty="yes"/><OkToTerminateIfFails>false</OkToTerminateIfFails><Notes __empty="yes"/><ESigHistoryDetails/><UseSSL>false</UseSSL></SMTPTransport>'
						,0);
				   
		  SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;		
	  END
	ELSE
	   PRINT('DataTransport ' + 'Email Transport' + ' already exists');							      				    	   
		--
		--
		PRINT 'SPC modeling data for DataTransport successfully inserted ' + CAST(@n_InsertRowCount AS VARCHAR(8)) + ' row(s)';				   						   				   				   
		--
		--
END
GO

EXEC csiAddSMTPTransport
GO
DROP PROCEDURE csiAddSMTPTransport
GO


IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiAddEMailMessage' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiAddEMailMessage
GO

CREATE PROCEDURE csiAddEMailMessage AS

BEGIN
	DECLARE @n_ErrLocator		NUMERIC;
	DECLARE @n_InsertRowCount	NUMERIC;
	DECLARE @n_UpdateRowCount	NUMERIC;
	--
	DECLARE @v_ErrMsg			VARCHAR(1024);
	DECLARE @i_ErrorNumber		INTEGER;
	DECLARE @mycount            INTEGER;
	DECLARE @CDOTypeId          INTEGER;
	DECLARE @InstanceId         VARCHAR(16);
	
	SET @n_InsertRowCount = 0;		
	SET @CDOTypeId = 7546;		

	--
	PRINT ('------------------ Adding EMailMessage -------------------');
	--

    SELECT @mycount = COUNT(*) FROM EMailMessage WHERE EMailMessageName = 'SPC Email Message';    
    if @mycount = 0 
	  BEGIN	  
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT			  
		Insert into [EMailMessage]
					([CDOTypeId]
					 ,[ChangeCount]
					 ,[EMailMessageId]
					 ,[EMailMessageName]
					 ,[IsFrozen]
					 ,[MessageFormat]
					 ,[Sender])
			 VALUES
				   (@CDOTypeId
					,1
					,@InstanceId	        
					,'SPC Email Message'	        
					,0
					,0
					,'CamstarAdmin@camstar.com');
				   
		  SET @n_InsertRowCount = @n_InsertRowCount + @@ROWCOUNT;		
	  END
	ELSE
	   PRINT('EMailMessage ' + 'SPC Email Message' + ' already exists');		
	   					      				    	   
		--
		--
		PRINT 'SPC modeling data for EMailMessage successfully inserted ' + CAST(@n_InsertRowCount AS VARCHAR(8)) + ' row(s)';				   						   				   				   
		--
		--
END
GO

EXEC csiAddEMailMessage
GO
DROP PROCEDURE csiAddEMailMessage
GO



