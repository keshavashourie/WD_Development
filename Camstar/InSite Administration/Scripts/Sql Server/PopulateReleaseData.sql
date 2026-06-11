--------------------------------------------------------------------------------
-- SCRIPT:PopulateReleaseData.sql
-- DESCR: Creates stored procedures used to create MenuDefinitions and MenuItems
--        and then uses those stored procedures to populate the default data
--
-- Change History:
--	03/20/2007 - Task 2157: Added code to create default ExportImport menu in default installation.
--			Bill Lippard
--	03/28/2007 - SPR S11460: Added code to create default Modeling Audit Trail menu in default installation.
--			Bill Lippard
--
--	04/13/2007 - SPR S11720: Modified XMLConnectPropagatorConfig query to properly order rownum filter
--
--	04/10/2007 - SPR S11692 - Updated: Modeling Export/Import menu string incorrect. The menu item shall be labeled "Modeling Export / Import"
--			Harpreet Kalsi
--	04/10/2007 - SPR S11693 - Updated: Modeling Audit Trail menu structure incorrect. There should not be a side bar menu item....
--			Harpreet Kalsi
--  04/10/2007 - Added Code to create default Shopfloor Forms menu in default installation.
--			Harpreet Kalsi
--	04/23/2007 - SPR S9984  - Added copyright notice.
--			Bill Lippard
--  05/02/2007 - SPR S11846 - Updated: The Shopfloor tab should read Shop Floor instead of Shopfloor   
--			Harpreet Kalsi
--	05/02/2007 - Updated: Removed Feature forms from default Shop Floor Menu. Added to PopulateFeatureFormMenu Script File
--			Harpreet Kalsi 
--	05/04/2007 - SPR 11859 - Remove Modeling Audit Trail menu definition
--			Harpreet Kalsi 
--  06/19/2007 - SPR 12089 - Correct Shop Floor MenuItem CDOTypeId
--          Barry Etter
--  06/04/2008 - SPR 12089 - Add a new entry into IDControl table
--          Ramesh Nagamalli
--  07/23/2008 -  SPR 13800 - Added new Quality Modeling Menu and Security Menu 
--			   -  SPR 13567 - Removed Security Maintenance Menu	
--          Harpreet Kalsi
--  09/03/2008 - Removed ESig Role Group - 001450000000004a
--			Harpreet Kalsi	
--  09/25/2008 - SPR 14169 - Updated menu labels
--			Harpreet Kalsi	
--  10/15/2008 - SPR 14187 - Added Modeling ESig Menu
--			Harpreet Kalsi	
--  10/30/2008 - SPR 14482 - Added seed data for Regulatory Reporting Feature
--			Harpreet Kalsi	
--  02/11/2009 - SPR 14990 - Added Container Attribute menu
--				 SPR 14694 - Added Mfg Audit Trail menu	
--			Harpreet Kalsi	
--  03/19/2010 - SPR 17335 - Updated menu items
--				 Reworked script to use stored procedures
--			Oleg Kirasov
--  06/22/2010 - SPR 17837 - Updates NOTIFVAR records:
--				 The ASERole NotifVar record to have VariableExpression value of 'ApproverRole.Name'. 
--				 The ASEAssignee NotifVar record to have VariableExpression value of 'Approver.Name'.
--			Maksim Kutsak
--  09/21/2011   Separated the csiPRDGetNextInstanceId function from this script so that it could be used elsewhere as well.
--			Ramesh Nagamalli    
--  02/19/2014 - UserStory 9574 - Role Based Access Control Modifications
--				 Removed createMenuDefinition and createMenuItem procedures and all related code.
--  				 Removed inserting WebMenuDefinitionId for Employee.
--			Oleg Khlus
-- 
-- Copyright Siemens 2023  

--------------------------------------------------------------------------------
-- PROCEDURE: createClassification
-- DESCR: Helper function to create Classification record
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createClassification' 
	   AND 	  type = 'P')
    DROP PROCEDURE createClassification
GO
CREATE PROCEDURE createClassification(
	@CDOTypeId			int, 
	@Name				nvarchar(30),
	@Description		nvarchar(255),
	@Value				int)
AS
BEGIN
	DECLARE @InstanceId varchar(16)

	IF NOT EXISTS (SELECT * FROM Classification WHERE ClassificationName = @Name)
	BEGIN
		PRINT('Inserting Classification: ' + @Name);

		-- Get next InstanceID if one is not provided.
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT

		INSERT INTO Classification
			(ClassificationId
			,ClassificationName
			,CDOTypeId
			,ChangeCount
			,ChangeHistoryId
			,Description
			,EventClassification
			,IconId
			,IsFrozen)
			VALUES
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@CDOTypeId         -- int
			,1                  -- int
			,0                  -- char(16)
			,@Description       -- nvarchar(255)
			,@Value				-- number
			,0                  -- int
			,0);                -- bit

	END
	ELSE
		PRINT('Classification ' + @Name + ' already exists');

END
GO


--------------------------------------------------------------------------------
-- PROCEDURE: createSubclassification
-- DESCR: Helper function to create Subclassification record
--
-- Copyright Siemens 2023  

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'createSubclassification' 
	   AND 	  type = 'P')
    DROP PROCEDURE createSubclassification
GO
CREATE PROCEDURE createSubclassification(
	@CDOTypeId			int, 
	@Name				nvarchar(30),
	@Description		nvarchar(255),
	@Value				int)
AS
BEGIN
	DECLARE @InstanceId varchar(16)

	IF NOT EXISTS (SELECT * FROM Subclassification WHERE SubclassificationName = @Name)
	BEGIN
		PRINT('Inserting Subclassification: ' + @Name);

		-- Get next InstanceID if one is not provided.
		EXEC csiPRDGetNextInstanceId @CDOTypeId, @InstanceId OUTPUT

		INSERT INTO Subclassification
			(SubclassificationId
			,SubclassificationName
			,CDOTypeId
			,ChangeCount
			,ChangeHistoryId
			,Description
			,EventSubclassification
			,IconId
			,IsFrozen)
			VALUES
			(@InstanceId        -- char(16)
			,@Name				-- nvarchar(30)
			,@CDOTypeId         -- int
			,1                  -- int
			,0                  -- char(16)
			,@Description       -- nvarchar(255)
			,@Value				-- number
			,0                  -- int
			,0);                -- bit

	END
	ELSE
		PRINT('Subclassification ' + @Name + ' already exists');

END
GO


BEGIN
	SET NOCOUNT ON
	declare @mycount int;
  
	-- Create SessionValues Records
	select @mycount = count(*) from sessionvalues;
	if @mycount = 0
	begin
		INSERT INTO SESSIONVALUES (SESSIONVALUESID, EmployeeId,  FACTORYID, APPLICATION, 
			CLIENT, CHANGECOUNT, CDOTypeId)  
			VALUES ('00046a0000000001',  '0004740000000001', NULL, 0,  0, 1, 1130);
		INSERT INTO SESSIONVALUES (SESSIONVALUESID, EmployeeId,  FACTORYID, APPLICATION, 
			CLIENT, CHANGECOUNT, CDOTypeId)  
			VALUES ('00046a0000000002',  '0004740000000002', NULL, 0,  0, 1, 1130);
		INSERT INTO SESSIONVALUES (SESSIONVALUESID, EmployeeId,  FACTORYID, APPLICATION, 
			CLIENT, CHANGECOUNT, CDOTypeId)  
			VALUES ('00046a0000000003',  '0004740000000003', NULL, 0,  0, 1, 1130);
		UPDATE INSTANCEIDCOUNT SET CLIENTINSTANCEID =  '0000000000000003' WHERE CDODEFID = 1130;
    end

------------------------- SPR S14482 START ------------------------------------

----- Regulatory Report Type Start -----

	SELECT @mycount = COUNT(*) FROM REGULATORYREPORTTYPE WHERE REGULATORYREPORTTYPEID = '001ec60000000001';
	IF @myCount = 0
	BEGIN
		--
		PRINT('Inserting Regulatory Report Type: 5-Day MDR');

		INSERT INTO REGULATORYREPORTTYPE (REGULATORYREPORTTYPEID		-- CHAR(16)
											,CDOTYPEID					-- int	
											,CHANGECOUNT				-- int
											,NOTES						-- nvarchar(2000)	
											,CHANGEHISTORYID			-- char(16)
											,DESCRIPTION				-- nvarchar(255)	
											,ICONID						-- int
											,ISFROZEN					-- bit
											,REGULATORYREPORTTYPENAME )	-- nvarchar(30)	
									VALUES( '001ec60000000001'			--REEGULATORYREPORTTYPEID		-- CHAR(16)
											,7878						-- CDOTYPEID					-- int	
											,0							--CHANGECOUNT				-- int
											,NULL						--NOTES						-- nvarchar(2000)	
											,NULL						--CHANGEHISTORYID			-- char(16)
											,NULL						--DESCRIPTION				-- nvarchar(255)	
											,NULL						--ICONID						-- int
											,0							--ISFROZEN					-- bit
											,'5-Day MDR')				--REGULATORYREPORTTYPENAME -- nvarchar(30)	

	END
	--

	SELECT @mycount = COUNT(*) FROM REGULATORYREPORTTYPE WHERE REGULATORYREPORTTYPEID = '001ec60000000002';
	IF @myCount = 0
	BEGIN
		--
		PRINT('Inserting Regulatory Report Type: 30-Day MDR');

		INSERT INTO REGULATORYREPORTTYPE (REGULATORYREPORTTYPEID		-- CHAR(16)
											,CDOTYPEID					-- int	
											,CHANGECOUNT				-- int
											,NOTES						-- nvarchar(2000)	
											,CHANGEHISTORYID			-- char(16)
											,DESCRIPTION				-- nvarchar(255)	
											,ICONID						-- int
											,ISFROZEN					-- bit
											,REGULATORYREPORTTYPENAME )	-- nvarchar(30)	
									VALUES( '001ec60000000002'			--REEGULATORYREPORTTYPEID		-- CHAR(16)
											,7878						-- CDOTYPEID					-- int	
											,0							--CHANGECOUNT				-- int
											,NULL						--NOTES						-- nvarchar(2000)	
											,NULL						--CHANGEHISTORYID			-- char(16)
											,NULL						--DESCRIPTION				-- nvarchar(255)	
											,NULL						--ICONID						-- int
											,0							--ISFROZEN					-- bit
											,'30-Day MDR')				--REGULATORYREPORTTYPENAME -- nvarchar(30)	

	END
	--
----- Regulatory Report Type End -----

----- Report Mapping Config Start -----

	SELECT @mycount = COUNT(*) FROM REPORTMAPPINGCONFIG WHERE REPORTMAPPINGCONFIGID = '001ec30000000001';
	IF @myCount = 0
	BEGIN
		--
		PRINT('Inserting Report Mapping Config: FDAReportMapConfig');

		INSERT INTO REPORTMAPPINGCONFIG (REPORTMAPPINGCONFIGID			-- CHAR(16)
											,CDOTYPEID					-- int	
											,CHANGECOUNT				-- int
											,NOTES						-- nvarchar(2000)	
											,CHANGEHISTORYID			-- char(16)
											,DESCRIPTION				-- nvarchar(255)	
											,ICONID						-- int
											,ISFROZEN					-- bit
											,REPORTMAPPINGCONFIGNAME	-- nvarchar(30)	
											,REPORTMAPPINGCONFIGFILE	-- nvarchar(255)
											,REPORTTEMPLATE)			-- nvarchar(255)
									VALUES( '001ec30000000001'			--REPORTMAPPINGCONFIGID		-- CHAR(16)
											,7875						-- CDOTYPEID					-- int	
											,0							--CHANGECOUNT				-- int
											,NULL						--NOTES						-- nvarchar(2000)	
											,NULL						--CHANGEHISTORYID			-- char(16)
											,NULL						--DESCRIPTION				-- nvarchar(255)	
											,NULL						--ICONID						-- int
											,0							--ISFROZEN					-- bit
											,'FDAReportMapConfig'		--REPORTMAPPINGCONFIGNAME -- nvarchar(30)	
											,'RegulatoryReporting/Mappings/Medwatch.xml'				--REPORTMAPPINGCONFIGFILE	-- nvarchar(255)
											,'RegulatoryReporting/Reports/Medwatch.pdf')			--REPORTTEMPLATE	-- nvarchar(255)
	END
	--

----- Report Mapping Config End -----

----- Regulatory Agency Start -----

	SELECT @mycount = COUNT(*) FROM REGULATORYAGENCY WHERE REGULATORYAGENCYID = '001ec90000000001';
	IF @myCount = 0
	BEGIN
		--
		PRINT('Inserting Regulatory Agency: FDA');

		INSERT INTO REGULATORYAGENCY (REGULATORYAGENCYID			-- CHAR(16)
											,CDOTYPEID					-- int	
											,CHANGECOUNT				-- int
											,NOTES						-- nvarchar(2000)	
											,CHANGEHISTORYID			-- char(16)
											,DESCRIPTION				-- nvarchar(255)	
											,ICONID						-- int
											,ISFROZEN					-- bit
											,REGULATORYAGENCYNAME		-- nvarchar(30)	
											,DECISIONTREEPAGEFLOWID)	-- char(16)
									VALUES( '001ec90000000001'			--REGULATORYAGENCYID		-- CHAR(16)
											,7881						-- CDOTYPEID					-- int	
											,0							--CHANGECOUNT				-- int
											,NULL						--NOTES						-- nvarchar(2000)	
											,NULL						--CHANGEHISTORYID			-- char(16)
											,NULL						--DESCRIPTION				-- nvarchar(255)	
											,NULL						--ICONID						-- int
											,0							--ISFROZEN					-- bit
											,'FDA'						--REGULATORYAGENCYNAME -- nvarchar(30)	
											,'001e47000000000e')		--DECISIONTREEPAGEFLOWID	-- char(16)
	END
	--

----- Regulatory Agency End -----


----- Regulatory Agency - Regulatory Reports Mapping Start -----

	SELECT @mycount = COUNT(*) FROM REGULATORYREPORTMAP WHERE REGULATORYREPORTMAPID = '001ecc0000000001';
	IF @myCount = 0
	BEGIN
		--
		PRINT('Inserting Regulatory Agency - Regulatory Reports Mapping: 5-Day MDR');

		INSERT INTO REGULATORYREPORTMAP (REGULATORYREPORTMAPID			-- CHAR(16)
											,CDOTYPEID					-- int	
											,CHANGECOUNT				-- int
											,PARENTID					-- char(16)
											,ISFROZEN					-- bit
											,REGULATORYREPORTTYPEID		-- char(16)	
											,REPORTMAPPINGCONFIGID		-- char(16)
											,SUBMISSIONPERIOD)			-- int
									VALUES( '001ecc0000000001'			--REGULATORYREPORTMAPID		-- CHAR(16)
											,7884						-- CDOTYPEID					-- int	
											,0							--CHANGECOUNT				-- int
											,'001ec90000000001'						--PARENTID			-- char(16)
											,0							--ISFROZEN					-- bit
											,'001ec60000000001'			--REGULATORYREPORTTYPEID -- nvarchar(30)	
											,'001ec30000000001'			--REPORTMAPPINGCONFIGID	-- char(16)
											,5)							--SUBMISSIONPERIOD		-- int
	END
	--

	SELECT @mycount = COUNT(*) FROM REGULATORYREPORTMAP WHERE REGULATORYREPORTMAPID = '001ecc0000000002';
	IF @myCount = 0
	BEGIN
		--
		PRINT('Inserting Regulatory Agency - Regulatory Reports Mapping: 30-Day MDR');

		INSERT INTO REGULATORYREPORTMAP (REGULATORYREPORTMAPID			-- CHAR(16)
											,CDOTYPEID					-- int	
											,CHANGECOUNT				-- int
											,PARENTID					-- char(16)
											,ISFROZEN					-- bit
											,REGULATORYREPORTTYPEID		-- char(16)	
											,REPORTMAPPINGCONFIGID		-- char(16)
											,SUBMISSIONPERIOD)			-- int
									VALUES( '001ecc0000000002'			--REGULATORYREPORTMAPID		-- CHAR(16)
											,7884						-- CDOTYPEID					-- int	
											,0							--CHANGECOUNT				-- int
											,'001ec90000000001'						--PARENTID			-- char(16)
											,0							--ISFROZEN					-- bit
											,'001ec60000000002'			--REGULATORYREPORTTYPEID -- nvarchar(30)	
											,'001ec30000000001'			--REPORTMAPPINGCONFIGID	-- char(16)
											,30)							--SUBMISSIONPERIOD		-- int
	END
	--	

----- Regulatory Agency - Regulatory Reports Mapping End -----

------------------------- SPR S14482 END ------------------------------------

	declare @CDOTypeId int
  ----- Classifications Start
	SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'Classification'

	EXEC createClassification @CDOTypeId,'Product','Product',1
	EXEC createClassification @CDOTypeId,'Process','Process',2
	EXEC createClassification @CDOTypeId,'General','General',3
	EXEC createClassification @CDOTypeId,'Audit','Audit',4
	EXEC createClassification @CDOTypeId,'EHandS','Environment, Health & Safety',5
  ----- Classifications End

  ----- SubClassifications Start
	SELECT @CDOTypeId = CDODefId FROM CDODefinition WHERE CDOName = 'SubClassification'

	EXEC createSubClassification @CDOTypeId,'Acceptance','Acceptance',1
	EXEC createSubClassification @CDOTypeId,'Monitoring','Monitoring',2
	EXEC createSubClassification @CDOTypeId,'Calibration','Calibration',3
	EXEC createSubClassification @CDOTypeId,'FieldEvent','FieldEvent',4
	EXEC createSubClassification @CDOTypeId,'General','General',5
	EXEC createSubClassification @CDOTypeId,'Supplier','Supplier',6
	EXEC createSubClassification @CDOTypeId,'Incident','Incident',7
	EXEC createSubClassification @CDOTypeId,'Observation','Observation',8
	EXEC createSubClassification @CDOTypeId,'ManagementReview','Management Review',9
	EXEC createSubClassification @CDOTypeId,'Environment','Environment',10
	EXEC createSubClassification @CDOTypeId,'Health','Health',11
	EXEC createSubClassification @CDOTypeId,'Safety','Safety',12
  ----- SubClassifications End
	
  -- Create Employee Records
  select @mycount = count(*) from employee;
  if @mycount = 0 
    begin
     INSERT INTO EMPLOYEE (EMPLOYEEID,  EMPLOYEENAME, FULLNAME, SESSIONVALUESID, CANLOGIN, 
                           CHANGECOUNT, ModelerAccess, CDOTypeId) 
        VALUES ('0004740000000001', 'Administrator', 'Administrator', '00046a0000000001', 1, 1, 1, 1140);
     INSERT INTO EMPLOYEE (EMPLOYEEID,  EMPLOYEENAME, FULLNAME, SESSIONVALUESID, CANLOGIN, 
                           CHANGECOUNT, ModelerAccess, CDOTypeId) 
        VALUES ('0004740000000002', 'InSiteAdmin', 'InSite Administrator', '00046a0000000002', 1,   1, 1, 1140);
     INSERT INTO EMPLOYEE (EMPLOYEEID,  EMPLOYEENAME, FULLNAME, SESSIONVALUESID, CANLOGIN, 
                           CHANGECOUNT, ModelerAccess, CDOTypeId) 
        VALUES ('0004740000000003', 'System', 'System User for Automated History', '00046a0000000003', 0,   1, 0, 1140);
     UPDATE INSTANCEIDCOUNT SET CLIENTINSTANCEID =  '0000000000000003' WHERE CDODEFID = 1140;
    end

	-- Create NumberingRule Records
  select @mycount = count(*) from numberingrule;
  if @mycount = 0 
    begin
     INSERT INTO NUMBERINGRULE (NUMBERINGRULEID,  NUMBERINGRULENAME, NUMBERINGRULETYPE, PREFIX, SEQUENCELENGTH, USEALPHANUMBERICVALUE, USEHEXADECIMALVALUE,
                           USEPREFIXBASED, CHANGECOUNT, ISFROZEN, ISROLLOVER, EXCLUDEDVALUES, CDOTypeId) 
        VALUES ('001e2a8000000001', 'JobOrder_Name', 0, '"JobOrder-"', 6, 0, 0, 0, 1, 0, 0, 33636608, 7722);
     UPDATE INSTANCEIDCOUNT SET CLIENTINSTANCEID =  '0000000000000001' WHERE CDODEFID = 7722;
    end

  -- Create CommentType Records
  select @mycount = count(*) from CommentType;
  if @mycount = 0 
    begin
     INSERT INTO CommentType (CommentTypeID,  CommentTypeName, ChangeCount, IsFrozen, CDOTypeId) 
        VALUES ('0020de0000000001', 'Containment', 1, 0, 8414);
     INSERT INTO CommentType (CommentTypeID,  CommentTypeName, ChangeCount, IsFrozen, CDOTypeId) 
        VALUES ('0020de0000000002', 'Disposition', 1, 0, 8414);
     INSERT INTO CommentType (CommentTypeID,  CommentTypeName, ChangeCount, IsFrozen, CDOTypeId) 
        VALUES ('0020de0000000003', 'Investigation', 1, 0, 8414);
     UPDATE INSTANCEIDCOUNT SET CLIENTINSTANCEID =  '0000000000000003' WHERE CDODEFID = 8414;
    end

  -- Create ErrorMsg Records
  select @mycount = count(*) from errormsg;
  if @mycount = 0 
    begin
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000001', 0, 'FieldLabel', 'Local::FieldLabel', '<field label>', 1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000002', 0,  'FieldId',  'Local::FieldId',  '<field Id>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000003', 0,  'CDODefType',  'Local::CDOType',  '<CDO definition type>', 1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000004', 0,  'InstanceId',  'Local::CDOId',   '<instance Id>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000005', 0,  'Revision',  'Local::Revision',  '<revision>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000006', 0,  'ContainerLevel', 'Local::ContainerLevel', '<Container Level>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000007', 0,  'ComErrorMessage', 'Local::ComErrorMessage', '<Com Error Message>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000008', 0,  'ComErrorDescription', 'Local::ComErrorDescription', '<Com Error Description>', 1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000009', 0,  'CLSID',  'Local::CLSID',   '<GUID of a Com Object>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000000a', 0,  'StringExceptionText', 'Local::StringExceptionText',  '<String Exception Text>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000000b', 0,  'CDOID',  'Local::CDOID',   '<CDO Def ID not found>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000000c', 0,  'ListFieldName', 'Local::ListFieldName',  '<List Field Not Found>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000000d', 0,  'ObjectName',  'Local::ObjectName',   '<Invalid object>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000000e', 0,  'LabelKey',  'Local::LabelKey',   '<Invalid Label Key>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000000f', 0,  'OpKey',  'Local::OpKey',   '<Invalid Operation Key>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000010', 0,  'Type',   'Local::Type',    '<Invalid Object Type>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000011', 0,  'Name',   'Local::Name',    '<Object Name>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000012', 0,  'CDOTypeName',  'Local::CDOTypeName',   '<CDO Type Name>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000013', 0,  'Field1Name',  'Local::Field1Name',   '<Field 1 Name>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000014', 0,  'Field2Name',  'Local::Field2Name',   '<Field 2 Name>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000015', 0,  'DBErrorString', 'Local::DBErrorString',  '<Database Message>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000016', 0,  'ListType',  'Local::ListType',       '<List Type>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000017', 0,  'Expression',  'Local::Expression',      '<Field Expression>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000018', 0,  'Name2',  'Local::Name2',       '<Object Name>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000019', 0,  'Expression2',  'Local::Expression2',      '<Field Expression2>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000001a', 0,  'MapId',  'Local::MapId',       '<Map Id>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000001b', 0,  'MinValue',  'Local::MinValue',  '<Minimum Value>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000001c', 0,  'MaxValue',  'Local::MaxValue',       '<Maximum Value>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000001d', 0,  'FunctionName',  'Local::FunctionName',  '<FunctionName>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000001e', 0,  'ContainerName', 'Local::ContainerName',  '<ContainerName>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000001f', 0,  'ParameterName', 'Local::ParameterName',  '<Parameter Name>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000020', 0,  'Expected',  'Local::Expected',  '<Expected>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000021', 0,  'Received',  'Local::Received',   '<Received>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000022', 0,  'FieldName',  'Local::FieldName',   '<FieldName>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000023', 0,  'SystemMsg',  'Local::SystemMsg',   '<SystemMsg>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000024', 0,  'Value',  'Local::Value',   '<Value>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000025', 0,  'LabelName',  'Local::LabelName',   '<LabelName>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000026', 0,  'CategoryID',  'Local::CategoryID',   '<CategoryID>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000027', 0,  'QueryName',  'Local::QueryName',   '<QueryName>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000028', 0,  'ColumnName',  'Local::ColumnName',   '<ColumnName>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000029', 0,  'ObjectContext',  'Local::ObjectContext',   '<ObjectContext>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('001586000000002a', 0,  'Name3',   'Local::Name3',    '<Object Name3>',   1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000030', 0,  'MethodName',  'Local::MethodName',   '<MethodName>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000031', 0,  'ShiftStart',  'Local::ShiftStart',   '<ShiftStart>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000032', 0,  'ShiftStart2',  'Local::ShiftStart2',   '<ShiftStart2>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000033', 0,  'ShiftEnd',  'Local::ShiftEnd',   '<ShiftEnd>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000034', 0,  'ShiftEnd2',  'Local::ShiftEnd2',   '<ShiftEnd2>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000035', 0,  'MaxShiftDuration',  'Local::MaxShiftDuration',   '<MaxShiftDuration>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000036', 0,  'CalendarDate',  'Local::CalendarDate',   '<CalendarDate>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION,  
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000037', 0,  'Value2',  'Local::Value2',   '<value>',    1, 5510);                            
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION, 
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000038', 0,  'Parameter1Name',  'Local::Parameter1Name', '<Parameter 1 Name>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION, 
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000039', 0,  'Parameter2Name',  'Local::Parameter2Name', '<Parameter 2 Name>',    1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION, 
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000040', 0,  'Parameter1Value',  'Local::Parameter1Value', '<Parameter 1 Value>',  1, 5510);
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION, 
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000041', 0,  'Parameter2Value',  'Local::Parameter2Value', '<Parameter 2 Value>',   1, 5510);        
     INSERT INTO ERRORMSG (ERRORMSGID,  ISFROZEN, ERRORMSGNAME,  VARIABLEEXPRESSION, 
                           UNRESOLVEDVALUE,  CHANGECOUNT, CDOTypeId)
        VALUES ('0015860000000042', 0,  'LineNumber',  'Local::LineNumber', '<Line Number>',   1, 5510);        
	 UPDATE INSTANCEIDCOUNT SET CLIENTINSTANCEID =  '0000000000000042' WHERE CDODEFID = 5510;
    end

  -- Create HistInq Records
  select @mycount = count(*) from histinq;
  if @mycount = 0 
    begin
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000001', 'Date',     0, 'Transaction::InProcessHistoryCDO.TxnDate',       '<unknown date>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000002', 'GMTDate',    0, 'Transaction::InProcessHistoryCDO.TxnDateGMT',       '<unknown date>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000003', 'Resource',    0, 'Transaction::InProcessHistoryCDO.Resource.Name',      '<unknown resource>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000004', 'Operation',    0, 'Transaction::InProcessHistoryCDO.Operation.Name',     '<unknown operation>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000005', 'Factory',    0, 'Transaction::InProcessHistoryCDO.Factory.Name',      '<unknown factory>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000006', 'Container',    0, 'Transaction::InProcessHistoryCDO.Container.Name',      '<unknown container>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000007', 'Product',    0, 'Transaction::InProcessHistoryCDO.Product.Name',      '<unknown product>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000008', 'User',     0, 'Transaction::InProcessHistoryCDO.User.FullName',      '<unknown user>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000009', 'Employee',    0, 'Transaction::InProcessHistoryCDO.Employee.FullName',      '<unknown employee>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000000a', 'Login',    0, 'Transaction::InProcessHistoryCDO.Login.FullName',      '<unknown login>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000000b', 'Owner',    0, 'Transaction::InProcessHistoryCDO.Owner.Name',       '<unknown owner>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000000c', 'ResourceLocation',   0, 'Transaction::InProcessHistoryCDO.Resource.Location.Name',     '<unknown location>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000000d', 'Shift',    0, 'Transaction::InProcessHistoryCDO.Shift.Name',       '<unknown shift>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000000e', 'SysDate',    0, 'Transaction::InProcessHistoryCDO.SystemDate',       '<unknown date>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000000f', 'SysDateGMT',    0, 'Transaction::InProcessHistoryCDO.SystemDateGMT',      '<unknown date>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000010', 'ProductRev',    0, 'Transaction::InProcessHistoryCDO.Product.Revision',      '<unknown revision>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000011', 'Workflow',    0, 'Transaction::InProcessHistoryCDO.Product.Workflow.Name',     '<unknown workflow>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000012', 'WorkflowRev',    0, 'Transaction::InProcessHistoryCDO.Workflow.Revision',      '<unknown revision>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000013', 'StartStep',    0, 'Transaction::InProcessHistoryCDO.WorkflowStep.Name',      '<unknown workflowstep>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000014', 'MoveElapsedTime',   0, 'Transaction::InProcessHistoryCDO.HistoryDetails[0].ElapsedTime',    '<unknown time>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000015', 'AssociateParent',   0, 'Transaction::InProcessHistoryCDO.HistoryDetails[0].ParentContainer',    '<unknown container>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000016', 'DisassociateParent',   0, 'Transaction::InProcessHistoryCDO.HistoryDetails[0].ParentContainer',    '<unknown container>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000017', 'MoveFromStep',    0, 'Transaction::InProcessHistoryCDO.HistoryDetails[0].Step.Name',     '<unknown step>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000018', 'MoveToStep',    0, 'Transaction::InProcessHistoryCDO.HistoryDetails[0].ToStep.Name',    '<unknown step>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000019', 'Quantity',    0, 'Transaction::InProcessHistoryCDO.HistoryDetails[0].Qty',     '<unknown quantity>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
	VALUES ( '000dca000000001a', 'CDOLabel', 0, 'GetCDONameLabel(Transaction::InProcessHistoryCDO.TxnType)', '<Unknown CDO Name>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000001b', 'TxnTypeName',    0, 'Transaction::TxnTypeName',         '<unknown transaction type>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000001c', 'TxnName',    0, 'Transaction::InProcessTxnName',        '<unknown transaction>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000001d', 'BaseTxnName',    0, 'Transaction::InProcessBaseTxnName',        '<unknown base transaction>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000001e', 'CompoundTxnName',   0, 'Transaction::InProcessCompoundTxnName',       '<unknown compound transaction>',1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca000000001f', 'HistoryCDOName',   0, 'Transaction::InProcessHistoryCDOName',        '<unknown history CDO name>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000020', 'RevTxnName',    0, 'Transaction::InProcessRevTxnName',        '',    1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000021', 'WorkflowStep',    0, 'Transaction::InProcessHistoryCDO.WorkflowStep.Name',      '<unknown workflow step>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000022', 'Customer',    0, 'Transaction::InProcessHistoryCDO.Customer.Name',      '<unkown customer>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000023', 'DueDate',    0, 'Transaction::InProcessHistoryCDO.DueDate',       '<unknown due date>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000024', 'Qty',     0, 'Transaction::InProcessHistoryCDO.Qty',        '<unknown qty>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000025', 'UOM',     0, 'Transaction::InProcessHistoryCDO.UOM.Name',       '<unknown UOM>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000026', 'UnitCount',    0, 'Transaction::InProcessHistoryCDO.UnitCount',       '<unknown unit count>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000027', 'ChildCount',    0, 'Transaction::InProcessHistoryCDO.ChildCount',       '<unknown child count>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000028', 'ToStep',    0, 'Transaction::InProcessHistoryCDO.ToStep.Name',       '<unknown to step>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000029', 'Path',     0, 'Transaction::InProcessHistoryCDO.Path.Name',       '<unknown path>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000030', 'HoldReason',    0, 'Transaction::InProcessHistoryCDO.HoldReason.Name',      '<unknown reason>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000031', 'ReworkReason',    0, 'Transaction::InProcessHistoryCDO.ReworkReason.Name',      '<unknown reason>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000032', 'QtyInspected',    0, 'Transaction::InProcessHistoryCDO.QtyInspected',      '<unknown qty>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000033', 'ContainersInspected',   0, 'Transaction::InProcessHistoryCDO.ContainersInspected',      '<unknown number of containers>',1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000034', 'DefectCount',    0, 'Transaction::InProcessHistoryCDO.DefectCount',       '<unknown count>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000035', 'ReasonCode',    0, 'Transaction::InProcessHistoryCDO.ReasonCode.Name',      '<unknown reason code>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000036', 'ActualQtyIssued',   0, 'Transaction::InProcessHistoryCDO.ActualQtyIssued',      '<unknown qty issued>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000037', 'IssueDifferenceReason',  0, 'Transaction::InProcessHistoryCDO.IssueDifferenceReason.Name',     '<unknown reason>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000038', 'QtyRequired',    0, 'Transaction::InProcessHistoryCDO.QtyRequired',       '<unknown qty required>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000039', 'IssueReason',    0, 'Transaction::InProcessHistoryCDO.IssueReason.Name',      '<unknown reason>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000040', 'QtyIssued',    0, 'Transaction::InProcessHistoryCDO.QtyIssued',       '<unknown qty issued>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000041', 'SubstitutionReason',   0, 'Transaction::InProcessHistoryCDO.SubstitutionReason.Name',     '<unknown reason>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000042', 'VendorItem',    0, 'Transaction::InProcessHistoryCDO.VendorItem.ItemName',      '<unknown item>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000043', 'ToResource',    0, 'Transaction::InProcessHistoryCDO.ToResource.Name',      '<unknown resource>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000044', 'ToWorkflow',    0, 'Transaction::InProcessHistoryCDO.ToWorkflow.Name',      '<unknown workflow>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000045', 'ToLocation',    0, 'Transaction::InProcessHistoryCDO.ToLocation.Name',      '<unknown location>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000046', 'Step',     0, 'Transaction::InProcessHistoryCDO.WorkflowStep.Name',      '<unknown Step>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000047', 'ChangeQtyType',   0, 'Transaction::InProcessHistoryCDO.ChangeQtyType',      '<unknown qty type>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000049', 'HoldDuration',    0, 'Transaction::InProcessHistoryCDO.HoldDuration',      '<unknown time>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000050', 'QtyRemoved',    0, 'Transaction::InProcessHistoryCDO.QtyRemoved',       '<unknown qty>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000051', 'RemovalReason',   0, 'Transaction::InProcessHistoryCDO.RemovalReason.Name',      '<unknown reason>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000052', 'ToContainer',    0, 'Transaction::InProcessHistoryCDO.ToContainer.Name',      '<unknown container>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000053', 'ToStepWorkflow',   0, 'Transaction::InProcessHistoryCDO.ToStep.Workflow.DisplayName',     '<unknown workflow>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000054', 'FromContainer',   0, 'Transaction::InProcessHistoryCDO.FromContainer.Name',      '<unknown container>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000055', 'RollupReason',    0, 'Transaction::InProcessHistoryCDO.RollupReason.Name',      '<unknown rollup reason>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000056', 'ParentContainer',   0, 'Transaction::InProcessHistoryCDO.ParentContainer.Name',     '<unknown parent container>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000057', 'OldDisplayValue',   0, 'Transaction::InProcessHistoryCDO.OldDisplayValue',      '<unknown old value>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000058', 'NewDisplayValue',   0, 'Transaction::InProcessHistoryCDO.NewDisplayValue',      '<unknown new value>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000059', 'FromLot',    0, 'Transaction::InProcessHistoryCDO.FromLot',       '<unknown lot>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000060', 'FromStockPoint',   0, 'Transaction::InProcessHistoryCDO.FromStockPoint',      '<unknown stock point>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000061', 'DestinationContainer',   0, 'Transaction::InProcessHistoryCDO.DestinationContainer.Name',     '<unknown container>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000062', 'DestinationLot',   0, 'Transaction::InProcessHistoryCDO.DestinationLot',      '<unknown lot>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000063', 'DestinationStockPoint',  0, 'Transaction::InProcessHistoryCDO.DestinationStockPoint',     '<unknown stock point>', 1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000064', 'NetQtyRequired',   0, 'Transaction::InProcessHistoryCDO.NetQtyRequired',      '<unknown Qty>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000065', 'FieldName',    0, 'Transaction::InProcessHistoryCDO.FieldName',       '<unknown FieldName>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000066', 'Location',    0, 'Transaction::InProcessHistoryCDO.Location.Name',      '<unknown location>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000067', 'Spec',     0, 'Transaction::InProcessHistoryCDO.Spec.DisplayName',      '<unknown Spec>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000068', 'ToSpec',    0, 'Transaction::InProcessHistoryCDO.ToSpec.DisplayName',      '<unknown Spec>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000069', 'HMainlineContainer',   0, 'Transaction::InProcessHistoryCDO.Parent.Container.Name',     '<unknown Container>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000070', 'MoveOutToStep',    0, 'Transaction::InProcessHistoryCDO.HistoryDetails[2].ToStep.Name',    '<unknown step>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '000dca0000000071', 'CombineChildCount',    0, 'Transaction::InProcessHistoryCDO.HistoryDetails[0].ChildCount',    '<unknown child count>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000072', 'SignatureCount',    0, 'Transaction::InProcessHistoryCDO.SignatureCount',    '<unknown signature count>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000073', 'ESigReqName',    0, 'Transaction::InProcessHistoryCDO.ESigReqDetail.Parent.Name',    '<unknown ESig requirement>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000074', 'ESigReqDetailName',    0, 'Transaction::InProcessHistoryCDO.ESigReqDetail.Name',    '<unknown ESig detail>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000075', 'ESigMeaning',    0, 'Transaction::InProcessHistoryCDO.Meaning.Name',    '<unknown ESig meaning>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000076', 'ESigRole',    0, 'Transaction::InProcessHistoryCDO.ESigReqDetail.Role.Name',    '<unknown signer role>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000077', 'ESigCosignerRole',    0, 'Transaction::InProcessHistoryCDO.ESigReqDetail.CosignerRole.Name',    '<cosigner role>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000078', 'ESigSigner',    0, 'Transaction::InProcessHistoryCDO.SignerFullName',    '<none>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000079', 'ESigCosigner',    0, 'Transaction::InProcessHistoryCDO.CosignerFullName',    '<none>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000007a','ProcessObject',0,'Transaction::InProcessHistoryCDO.HistoryDetails[0].ProcessObject.Name','<Unknown ProcessObject>',1,3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000007b','ProcessObjectType',0,'GetCDONameLabel(GetCDOType(Transaction::InProcessHistoryCDO.HistoryDetails[0].ProcessObject,Transaction::CDOTypeName))','<Unknown ProcessObject type>',1,3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000007c','ProcessObjectAssignee',0,'Transaction::InProcessHistoryCDO.HistoryDetails[0].ProcessObject.Assignee.Name','<Unknown Assignee>',1,3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000007d','QualityObject',0,'Transaction::InProcessHistoryCDO.HistoryId.Name','<Unknown QualityObject>',1,3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000007e','QualityObjectType',0,'GetCDONameLabel(GetCDOType(Transaction::InProcessHistoryCDO.HistoryId,Transaction::CDOTypeName))','<Unknown QualityObject type>',1,3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000080', 'ESigCosignReason',    0, 'Transaction::InProcessHistoryCDO.CosignReasonName',    '<none>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000081', 'NCRName',    0, 'Transaction::InProcessHistoryCDO.HistoryId.Name',    '<Unknown Nonconformance>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000082', 'CreateEventName',    0, 'Transaction::InProcessHistoryCDO.HistoryId.Name',    '<Unknown Event>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ( '000dca0000000083', 'CAPAName',    0, 'Transaction::InProcessHistoryCDO.HistoryId.Name',    '<Unknown CAPA>',  1, 3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ('000dca0000000084','NCRFailureCode',0,'Transaction::InProcessHistoryCDO.NCRFailureCodes[0].Name','<Unknown NCR Failure Code>',1,3530);
	 INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ('000dca0000000085','CauseCode',0,'Transaction::InProcessHistoryCDO.PostNCRCauseCode.Name','<Unknown NCR cause code>',1,3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
         VALUES ('000dca0000000086','Resolution',0,'Transaction::InProcessHistoryCDO.PostNCRResolution.Name','<Unkonwn NCR Resolution>',1,3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca0000000087','DefectProduct',0,'Transaction::InProcessHistoryCDO.Product.Name','<Unknown NCR Defect Product>',1,3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca0000000088','DefectLot',0,'Transaction::InProcessHistoryCDO.Lot','<Unknown NCR Defect Lot>',1,3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca0000000089','ProcessObjectCollaborator',0,'Transaction::InProcessHistoryCDO.HistoryDetails[0].ProcessObject.Collaborator.Name','<Unknown User>',1,3530);
     INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008a','SummaryNCRNumber',0,'Transaction::InProcessHistoryCDO.HistoryDetails[0].NonconformanceNumber','<Unknown Nonconformance>',1,3530);
	 INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008b','JobOrderName',0,'JobOrderName','<Unknown Job Order>',1,3530);
	 INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008c','JobModelName',0,'JobModelName','<Unknown Job Model>',1,3530);
	 INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008d','ToStageName',0,'ToStageName','',1,3530);
	 INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008e','ToStageSequence',0,'ToStageSequence','',1,3530);
	 INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca000000008f','PartName',0,'PartDetails.Name','<Unknown Part Name>',1,3530);
	 INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca0000000090','RequestOrderName',0,'RequestOrderName','<Unknown Request Order Name>',1,3530);
	 INSERT INTO HISTINQ( HISTINQID, HISTINQNAME, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ('000dca0000000091','ReleaseReason',0,'Transaction::InProcessHistoryCDO.ReleaseReason.Name','<Unknown Release Reason Name>',1,3530);
   UPDATE INSTANCEIDCOUNT SET CLIENTINSTANCEID =  '0000000000000090' WHERE CDODEFID = 3530;
    end

  -- Create NotifVar Records
  select @mycount = count(*) from NotifVar;
  if @mycount = 0 
    begin
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000001', 'POName', 'Process Object Name', 0, 'Name', '<unknown POName>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000002', 'POQualityObject', 'Process Object Quality Object', 0, 'QualityObject.Name', '<unknown POQualityObject>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000003', 'POQualityCategory', 'Process Object Quality Category', 0, 'GetEnumLabel(Transaction::__Const.Category,QualityObject.Category)', '<unknown POQualityCategory>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000004', 'PODueDate', 'Process Object Due Date', 0, 'CompleteByGMT?GMTToLocal(CompleteByGMT):""', '<unknown PODueDate>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000005', 'POAssigneeOption', 'Process Object Assignee Option', 0, 'GetEnumLabel(Transaction::__Const.AssigneeOption,AssigneeOption)', '<unknown POAssigneeOption>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000006', 'POAssignee', 'Process Object Assignee', 0, 'Assignee.Name', '<unknown POAssignee>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000007', 'PORole', 'Process Object Role', 0, 'AssigneeRole.Name', '<unknown PORole>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000008', 'QOName', 'Quality Object Name', 0, 'Name', '<unknown QOName>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000009', 'QOCategory', 'Quality Object Category', 0, 'GetEnumLabel(Transaction::__Const.Category,Category)', '<unknown QOCategory>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa6000000000a', 'QOOwner', 'Quality Object Owner', 0, 'Owner.Name', '<unknown QOOwner>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa6000000000b', 'QORole', 'Quality Object Role', 0, 'Role.Name', '<unknown QORole>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa6000000000c', 'ASEParent', 'Approval Sheet Entry Parent', 0, 'Parent.Parent.Name', '<unknown ASEParent>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa6000000000d', 'ASEDueDate', 'Approval Sheet Entry Due Date', 0, 'CompleteByGMT?GMTToLocal(CompleteByGMT):""', '<unknown ASEDueDate>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa6000000000e', 'ASEAssignee', 'Approval Sheet Entry Assignee', 0, 'Approver.Name', '<unknown ASEAssignee>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa6000000000f', 'ASERole', 'Approval Sheet Entry Role', 0, 'ApproverRole.Name', '<unknown ASERole>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000010', 'POAFromStage', 'Process Object Approval From Stage', 0, 'GetEnumLabel(Transaction::__Const.Stage,Parent.FromStage)', '<unknown POAFromStage>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000011', 'POAToStage', 'Process Object Approval To Stage', 0, 'GetEnumLabel(Transaction::__Const.Stage,Parent.ToStage)', '<unknown POAToStage>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000012', 'POAQualityObject', 'Process Object Approval Quality Object', 0, 'Parent.Parent.QualityObject.Name', '<unknown POAQualityObject>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000013', 'POAQualityCategory', 'Process Object Approval Quality Category', 0, 'GetEnumLabel(Transaction::__Const.Category,Parent.Parent.QualityObject.Category)', '<unknown POAQualityCategory>',  1, 8102);
     INSERT INTO NotifVar( NOTIFVARID, NOTIFVARNAME, DESCRIPTION, ISFROZEN, VARIABLEEXPRESSION, UNRESOLVEDVALUE, CHANGECOUNT, CDOTypeId)
        VALUES ( '001fa60000000014', 'QOAQualityCategory', 'Quality Object Approval Quality Category', 0, 'GetEnumLabel(Transaction::__Const.Category,Parent.Parent.Category)', '<unknown QOAQualityCategory>',  1, 8102);
     UPDATE INSTANCEIDCOUNT SET CLIENTINSTANCEID =  '0000000000000014' WHERE CDODEFID = 8102;
    end

  -- Create IDControl Records
  select @mycount = count(*) from IDControl;
  if @mycount = 0 
    begin
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('UserLabel', 1083572224);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('NCR', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('ExportImportQueue', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('ExportImportHeader', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('ExportImportDetail', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('ExportImportLog', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('ImportContents', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('DocContents', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('ScheduledBizRuleExecHistory', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('Event', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('SecurityCacheRefreshRequest', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('EmailQueue', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('ExportImportTarget', 0);
     INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('CPFileSequence', 0);
	 INSERT INTO IDControl (IDType,  NextID) 
        VALUES ('001e2a8000000001', 0);
    end
    
	-- Create OutboundXMLDocStatusCode Records
	select @mycount = count(*) from OutboundXMLDocStatusCode;
	if @mycount = 0 
    begin  
		INSERT INTO OutboundXMLDocStatusCode 
				(StatusCodeId, StatusCodeName, Description, OkToProcess, OkToRemove)
			VALUES
				(0, 'NotProcessed', 'Not processed' , 1, 0); 
		INSERT INTO OutboundXMLDocStatusCode 
				(StatusCodeId, StatusCodeName, Description, OkToProcess, OkToRemove)
			VALUES
				(1, 'InProcess', 'Currently in processing', 0, 0); 
		INSERT INTO OutboundXMLDocStatusCode 
				(StatusCodeId, StatusCodeName, Description, OkToProcess, OkToRemove)
			VALUES
				(2, 'Failed', 'Transmission failed', 0, 0); 
		INSERT INTO OutboundXMLDocStatusCode 
				(StatusCodeId, StatusCodeName, Description, OkToProcess, OkToRemove)
			VALUES
				(3, 'Delivered', 'Document was delivered', 0, 1); 
		INSERT INTO OutboundXMLDocStatusCode 
				(StatusCodeId, StatusCodeName, Description, OkToProcess, OkToRemove)
			VALUES
				(4, 'InTransit', 'Delivered asynchronously', 0, 0); 
		INSERT INTO OutboundXMLDocStatusCode 
				(StatusCodeId, StatusCodeName, Description, OkToProcess, OkToRemove)
			VALUES
				(5, 'Canceled', 'Canceled', 0, 0); 
	end

	-- Create XMLConnectPropagatorConfig  Records
	select @mycount = count(*) from XMLConnectPropagatorConfig;
	if @mycount = 0 
    begin  
		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLDeleteDocs','DELETE OutboundXMLDoc FROM OutboundXMLDocStatus as S WITH(NOLOCK), OutboundXMLDocStatusCode as C WHERE OutboundXMLDoc.TxnId = S.TxnId AND OutboundXMLDoc.DocId = S.DocId AND S.StatusCodeId = C.StatusCodeId AND C.OkToRemove = 1;DELETE OutboundXMLDocStatus FROM OutboundXMLDocStatusCode as C WHERE OutboundXMLDocStatus.StatusCodeId = C.StatusCodeId AND C.OkToRemove = 1');

		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLFailureCheckTimeout','IF (SELECT count(DocID) FROM OutboundXMLDocProcessing WHERE DATEDIFF (second, StartTimestamp, CURRENT_TIMESTAMP) > <__timeout/>) > 0
						BEGIN
							BEGIN TRAN
							UPDATE OutboundXMLDocStatus SET StatusCodeId = C.StatusCodeId, LastActivityDate = CURRENT_TIMESTAMP, RetryCount = NULL FROM OutboundXMLDocStatusCode as C, OutboundXMLDocProcessing XP
							WHERE C.StatusCodeName = ''NotProcessed'' AND OutboundXMLDocStatus.TxnId = XP.TxnId AND OutboundXMLDocStatus.DocId = XP.DocId AND DATEDIFF (second, XP.StartTimestamp, CURRENT_TIMESTAMP) > <__timeout/>;
							DELETE FROM OutboundXMLDocProcessing WHERE DATEDIFF (second, StartTimestamp, CURRENT_TIMESTAMP) > <__timeout/>;
							COMMIT TRAN;
						END');

		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLGetDocInfo',
				'SELECT  DISTINCT TOP 1 S.TxnId, S.DocId, S.TransportId, S.CreationDate, S.LastActivityDate, S.StatusCodeId 
				FROM OutboundXMLDocStatus as S WITH(NOLOCK), DataTransport as T WITH(NOLOCK), OutboundXMLDocStatusCode as C WITH(NOLOCK) 
				WHERE S.StatusCodeId = C.StatusCodeId AND C.OkToProcess = 1 AND T.DataTransportId = S.TransportId AND T.DataTransportId NOT IN (
					SELECT DataTransportId FROM  OutboundXMLDocProcessing WITH(NOLOCK)							
				) <__dataTransportNamesInclude> AND T.DataTransportName IN (<__include/>) </__dataTransportNamesInclude> ORDER BY S.TxnId, S.DocId');
			
		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLGetDocOwnership','INSERT INTO OutboundXMLDocProcessing   (TxnId, DocId, DataTransportId, StartTimestamp, OwnedByUUID) VALUES   (''<__txnId/>'',<__docId/>,''<__dataTransportId/>'', CURRENT_TIMESTAMP, ''<__serviceUUID/>'')');
			
		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLGetDocument','SELECT D.XMLDocument  FROM OutboundXMLDoc as D,     OutboundXMLDocStatus as S,     DataTransport as T  WHERE D.TxnId = ''<__txnId/>''    AND D.DocId = <__docId/>    AND D.TxnId = S.TxnID    AND D.DocId = S.DocId    AND S.TransportId = T.DataTransportId  ORDER BY D.Sequence');
			
		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLGetDTChangeCount','SELECT ChangeCount FROM DataTransport WHERE DataTransportId = ''<__dataTransportId/>''');
			
		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLGetDTChangeCounts','SELECT DataTransportId,  ChangeCount FROM DataTransport WHERE DataTransportId IS NOT NULL
				<__dataTransportNamesInclude> AND DataTransportName IN (<__include/>) </__dataTransportNamesInclude>
				<__dataTransportNamesExclude> AND DataTransportName NOT IN (<__exclude/>) </__dataTransportNamesExclude>
				<__dataTransportTypesInclude> AND TransportType IN (<__include/>) </__dataTransportTypesInclude>
				<__dataTransportTypesExclude> AND TransportType NOT IN (<__exclude/>) </__dataTransportTypesExclude>');

		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLGetTransportInfo','SELECT TOP 1   DataTransportId,    ChangeCount,    DataTransportName,    TransportType,  IsSynchronous,  TransportAssembly,    ConnectionDocInit,  OkToTerminateIfFails FROM DataTransport WHERE DataTransportId = ''<__dataTransportId/>''');
			
		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLReleaseDocOwnership','DELETE FROM OutboundXMLDocProcessing   WHERE TxnId = ''<__txnId/>'' AND DocId = <__docId/>');
			
		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLSetDocCanceled','UPDATE OutboundXMLDocStatus
				SET StatusCodeId = C.StatusCodeId,
				LastActivityDate = CURRENT_TIMESTAMP,
				RetryCount = <__retryCount/>
				FROM OutboundXMLDocStatusCode as C
				WHERE TxnId = ''<__txnId/>'' AND DocId = <__docId/>
				AND C.StatusCodeName = ''Canceled''');

		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLSetDocDelivered','UPDATE OutboundXMLDocStatus
				SET StatusCodeId = C.StatusCodeId,
				LastActivityDate = CURRENT_TIMESTAMP,
				RetryCount = <__retryCount/>
				FROM OutboundXMLDocStatusCode as C
				WHERE TxnId = ''<__txnId/>'' AND DocId = <__docId/>
				AND C.StatusCodeName = ''Delivered''');

		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLSetDocFailed','UPDATE OutboundXMLDocStatus
				SET StatusCodeId = C.StatusCodeId,
				LastActivityDate = CURRENT_TIMESTAMP,
				RetryCount = <__retryCount/>
				FROM OutboundXMLDocStatusCode as C
				WHERE TxnId = ''<__txnId/>'' AND DocId = <__docId/>
				AND C.StatusCodeName = ''Failed''');

		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLSetDocInProcess',
				'DECLARE @codeidOk As INT; SELECT @codeidOk = StatusCodeId from OutboundXMLDocStatusCode WITH(NOLOCK) where OkToProcess = 1;			
				DECLARE @codeidInProcess As INT; SELECT @codeidInProcess = StatusCodeId from OutboundXMLDocStatusCode WITH(NOLOCK) where StatusCodeName = ''InProcess'';			
				UPDATE OutboundXMLDocStatus
				SET StatusCodeId = @codeidInProcess,
				LastActivityDate = CURRENT_TIMESTAMP
				WHERE TxnId = ''<__txnId/>'' AND DocId = <__docId/>
				AND StatusCodeId = @codeidOk;');

		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLSetDocInTransit','UPDATE OutboundXMLDocStatus
				SET StatusCodeId = C.StatusCodeId,
				LastActivityDate = CURRENT_TIMESTAMP,
				RetryCount = <__retryCount/>
				FROM OutboundXMLDocStatusCode as C
				WHERE TxnId = ''<__txnId/>'' AND DocId = <__docId/>
				AND C.StatusCodeName = ''InTransit''');

		INSERT INTO XMLConnectPropagatorConfig 
				(ConfigKey, ConfigValue)
			VALUES ('SQLSetDocNotProcessed','UPDATE OutboundXMLDocStatus
				SET StatusCodeId = C.StatusCodeId,
				LastActivityDate = CURRENT_TIMESTAMP
				FROM OutboundXMLDocStatusCode as C
				WHERE TxnId = ''<__txnId/>'' AND DocId = <__docId/>
				AND C.StatusCodeName = ''NotProcessed''');
	end

	-- Create Mfg Order Task Status Records
	select @mycount = count(*) from MfgOrderTaskStatus;
  	if @mycount = 0 
    begin
    	INSERT INTO MfgOrderTaskStatus (MfgOrderTaskStatusId,  MfgOrderTaskStatusName, TaskComplete, TaskFailed, CDOTypeId) 
        	VALUES ('0022d18000000001', 'Completed', 1, 0, 8913);
     	INSERT INTO MfgOrderTaskStatus (MfgOrderTaskStatusId,  MfgOrderTaskStatusName, TaskComplete, TaskFailed, CDOTypeId) 
        	VALUES ('0022d18000000002', 'In Progress', 0, 0, 8913);
     	INSERT INTO MfgOrderTaskStatus (MfgOrderTaskStatusId,  MfgOrderTaskStatusName, TaskComplete, TaskFailed, CDOTypeId) 
        	VALUES ('0022d18000000003', 'Failed', 0, 1, 8913);
     	UPDATE INSTANCEIDCOUNT SET CLIENTINSTANCEID =  '0000000000000003' WHERE CDODEFID = 8913;
    end
END
GO

--------------------------------------------------------------------------------------------------
-- This script performs the below actions:
--	- Updates UserLabel table with new records added in this release.
--  - Updates PortalMessageCategory table with new record added in this release.
--  - Updates CategoryNotificationMap table with new record added in this release.
--  - Updates CategoryNotificationMapTypes table with new record added in this release.
--
-- To clear data from tables added by this script run the following commands.
-- Keep in mind these commands will remove any other data you may have in these 
-- tables along with data created by these scripts.
--
-- truncate table UserLabel
-- truncate table PortalMessageCategory
-- truncate table CategoryNotificationMap
-- truncate table CategoryNotificationMapTypes
--
--  Modification History:
--  Name            	Date        	Action
--  --------------      ----------  	----------------
--  Maksim Kutsak		05/04/2010		Created
--
-- Copyright Siemens 2023  

--------------------------------------------------------------------------------------------------
CREATE PROCEDURE AddPortalMessageCenterData_Prc
AS
BEGIN

	DECLARE @n_ErrLocator				NUMERIC;
	DECLARE @v_ErrMsg					VARCHAR(1024);
	DECLARE @i_ErrorNumber				INTEGER;
	--
	DECLARE @MsgCategoryLabelCDODefId			       INT
	DECLARE @i_Seq 								       INTEGER;
	DECLARE @MyAssignmentsLabel					       VARCHAR(14)
	DECLARE @MyApprovalsLabel					       VARCHAR(12)
	DECLARE @MyPendingItemsLabel				       VARCHAR(16)
	DECLARE @MyAlertsLabel						       VARCHAR(9)
    DECLARE @PortalMessageCategoryName			       VARCHAR(23)
	DECLARE @RecordCount						       INT;
	DECLARE @InstanceId_CategoryNotificationMap        VARCHAR(16)
	DECLARE @CategoryNotificationMapCDODefId           INT
    DECLARE @ExistingCatMapInstanceID                  VARCHAR(16);
    DECLARE @ExistingLabelInstanceID                   VARCHAR(16);
    DECLARE @ExistingPortalMessageCategoryInstanceID   VARCHAR(16);
	DECLARE @PortalMessageCategoryCDODefId             INT
	DECLARE @InstanceId_PortalMessageCategory          VARCHAR(16)
	DECLARE @IDControlType                             VARCHAR(16)
	DECLARE @NotificationTypesFieldID                  INTEGER;

    SET     @MyAssignmentsLabel              = 'My Assignments';
    SET     @IDControlType                   = 'UserLabel';
    SET     @MyApprovalsLabel                = 'My Approvals';
    SET     @MyPendingItemsLabel             = 'My Pending Items';
    SET     @MyAlertsLabel                   = 'My Alerts';
    SET     @PortalMessageCategoryName       = 'Portal Message Category';
	SET     @MsgCategoryLabelCDODefId        = 8091
	SET     @PortalMessageCategoryCDODefId   = 8094
	SET     @CategoryNotificationMapCDODefId = 8098
    SET     @NotificationTypesFieldID        = 18429

	-- Create UserLabel Records --
	-- Add My Assignment --
	BEGIN TRY
		--
		PRINT('Adding instance data for Message Center ');
		PRINT('Adding Message Category Label ' + '"' + @MyAssignmentsLabel + '"');
		SET @n_ErrLocator = 110;
        IF NOT EXISTS (select UserLabelID from UserLabel where UserlabelName = @MyAssignmentsLabel)
  		  BEGIN
			DECLARE @nextid_MyAssignment int
			EXEC csiIDControl @IDControlType, @nextid_MyAssignment output
			DECLARE @InstanceId_MyAssignment varchar(16)
			EXEC csiPRDGetNextInstanceId @MsgCategoryLabelCDODefId,@InstanceId_MyAssignment OUTPUT

			INSERT INTO UserLabel (UserLabelId, LabelId, UserLabelName, ChangeCount, CategoryID, CDOTypeId, LabelValue) 
				VALUES (@InstanceId_MyAssignment, @nextid_MyAssignment, @MyAssignmentsLabel, 1, 75, @MsgCategoryLabelCDODefId, @MyAssignmentsLabel);

		  END;
        ELSE
          BEGIN
		    PRINT(' MessageCategoryLabel ' + '"' + @MyAssignmentsLabel + '"' + ' already exists skipping......');
			SELECT @ExistingLabelInstanceID = (select UserLabelID from UserLabel where UserlabelName = @MyAssignmentsLabel)
            SET @InstanceId_MyAssignment = @ExistingLabelInstanceID;
          END;

	-- Add My Approvals --
		SET @n_ErrLocator = 120;
		PRINT('Adding Message Category Label ' + '"' + @MyApprovalsLabel + '"');		
        IF NOT EXISTS (select UserLabelID from UserLabel where UserlabelName = @MyApprovalsLabel)
		 BEGIN
			DECLARE @nextid_MyApprovals int
			EXEC csiIDControl @IDControlType, @nextid_MyApprovals output
			DECLARE @InstanceId_MyApprovals varchar(16)
			EXEC csiPRDGetNextInstanceId @MsgCategoryLabelCDODefId,@InstanceId_MyApprovals OUTPUT

			INSERT INTO UserLabel (UserLabelId, LabelId, UserLabelName, ChangeCount, CategoryID, CDOTypeId, LabelValue) 
				VALUES (@InstanceId_MyApprovals, @nextid_MyApprovals, @MyApprovalsLabel, 1, 75, @MsgCategoryLabelCDODefId, @MyApprovalsLabel);
		 END;
        ELSE
          BEGIN
		    PRINT(' MessageCategoryLabel ' + '"' + @MyApprovalsLabel + '"' + ' already exists skipping......');
			SELECT @ExistingLabelInstanceID = (select UserLabelID from UserLabel where UserlabelName = @MyApprovalsLabel)
            SET @InstanceId_MyApprovals = @ExistingLabelInstanceID;
          END;

		--
		SET @n_ErrLocator = 130;
		--
		PRINT('Adding Message Category Label ' + '"' + @MyPendingItemsLabel + '"');				
        IF NOT EXISTS (select UserLabelID from UserLabel where UserlabelName = @MyPendingItemsLabel)
  		  BEGIN
			DECLARE @nextid_MyPendingItems int
			EXEC csiIDControl @IDControlType, @nextid_MyPendingItems output
			DECLARE @InstanceId_MyPendingItems varchar(16)
			EXEC csiPRDGetNextInstanceId @MsgCategoryLabelCDODefId,@InstanceId_MyPendingItems OUTPUT
			
			INSERT INTO UserLabel (UserLabelId, LabelId, UserLabelName, ChangeCount, CategoryID, CDOTypeId, LabelValue) 
				VALUES (@InstanceId_MyPendingItems, @nextid_MyPendingItems, @MyPendingItemsLabel, 1, 75, @MsgCategoryLabelCDODefId, @MyPendingItemsLabel);
		  END;
        ELSE
          BEGIN
		    PRINT(' MessageCategoryLabel ' + '"' + @MyPendingItemsLabel + '"' + ' already exists skipping......');
			SELECT @ExistingLabelInstanceID = (select UserLabelID from UserLabel where UserlabelName = @MyPendingItemsLabel)
            SET @InstanceId_MyPendingItems = @ExistingLabelInstanceID;
          END;


	-- Add My Alerts --

		SET @n_ErrLocator = 140;
		PRINT('Adding Message Category Label ' + '"' + @MyAlertsLabel + '"');				
        IF NOT EXISTS (select UserLabelID from UserLabel where UserlabelName = @MyAlertsLabel)
  		  BEGIN

			DECLARE @nextid_MyAlerts int
			EXEC csiIDControl @IDControlType, @nextid_MyAlerts output
			DECLARE @InstanceId_MyAlerts varchar(16)
			EXEC csiPRDGetNextInstanceId @MsgCategoryLabelCDODefId,@InstanceId_MyAlerts OUTPUT

			INSERT INTO UserLabel (UserLabelId, LabelId, UserLabelName, ChangeCount, CategoryID, CDOTypeId, LabelValue) 
				VALUES (@InstanceId_MyAlerts, @nextid_MyAlerts, @MyAlertsLabel, 1, 75, @MsgCategoryLabelCDODefId, @MyAlertsLabel);
		  END;
        ELSE
          BEGIN
		    PRINT(' MessageCategoryLabel ' + '"' + @MyAlertsLabel + '"' + ' already exists skipping......');
			SELECT @ExistingLabelInstanceID = (select UserLabelID from UserLabel where UserlabelName = @MyAlertsLabel)
            SET @InstanceId_MyAlerts = @ExistingLabelInstanceID;
		  END;

	END TRY
	--

	BEGIN CATCH
	--
		SELECT @v_ErrMsg = 'AddPortalMessageCenterData_Prc - ErrLoc: ' + CAST(@n_ErrLocator AS VARCHAR(8)) + ' ErrMsg: ' + ERROR_MESSAGE() + ' ErrNum: ' + CAST(ERROR_NUMBER() AS VARCHAR(8))
			  ,@i_ErrorNumber = ERROR_NUMBER();
		--
			PRINT 'Error Adding UserLabels ' + @v_ErrMsg;
		--
	END CATCH;
	
	-- Add PortalMessageCategory --	
	BEGIN TRY
	
		PRINT('Adding Portal Message Category ' + '"' + @PortalMessageCategoryName + '"');					
		SET @n_ErrLocator = 150;
        IF NOT EXISTS (select PortalMessageCategoryID from PortalMessageCategory where PortalMessageCategoryName = @PortalMessageCategoryName)
  		  BEGIN
			EXEC csiPRDGetNextInstanceId @PortalMessageCategoryCDODefId,@InstanceId_PortalMessageCategory OUTPUT
			INSERT INTO PortalMessageCategory (PortalMessageCategoryId, CDOTypeId, ChangeCount, PortalMessageCategoryName, YellowMaxRange, YellowMinRange ) 
					VALUES (@InstanceId_PortalMessageCategory, @PortalMessageCategoryCDODefId, 1, @PortalMessageCategoryName , 2, 0);

		  END;
        ELSE
          BEGIN
		   PRINT(' PortalMessageCategory ' + '"' + @PortalMessageCategoryName + '"' + ' already exists skipping......');           
		   SELECT @ExistingPortalMessageCategoryInstanceID = (Select PortalMessageCategoryID From PortalMessageCategory);
		   SET @InstanceId_PortalMessageCategory = @ExistingPortalMessageCategoryInstanceID;
		  END;

	END TRY

	BEGIN CATCH
			SELECT @v_ErrMsg = 'AddPortalMessageCenterData_Prc - ErrLoc: ' + CAST(@n_ErrLocator AS VARCHAR(8)) + ' ErrMsg: ' + ERROR_MESSAGE() + ' ErrNum: ' + CAST(ERROR_NUMBER() AS VARCHAR(8))
			  ,@i_ErrorNumber = ERROR_NUMBER();
		   	  PRINT @v_ErrMsg;
	END CATCH;

	--	
	--------------------------------------------------
	-- Add My Assignment to CategoryNotificationMap -- 
	--------------------------------------------------
	BEGIN TRY
		--
        SET @ExistingCatMapInstanceID           = NULL;
        SET @i_ErrorNumber                      = 0;
		SET @n_ErrLocator                       = 170;
        SET @InstanceId_CategoryNotificationMap = NULL;
        SET @ExistingLabelInstanceID            = NULL;
		--
		SET @n_ErrLocator = 160;
	    SELECT @ExistingCatMapInstanceID = (Select CategoryNotificationMapID 
                              From CategoryNotificationMap,
                                   UserLabel 
                             Where CategoryNotificationMap.MessageCategoryLabelId = UserLabel.UserLabelId
                               and UserLabelName = @MyAssignmentsLabel);

		SET @n_ErrLocator = 170;
		PRINT('Adding Category Notification Map ' + '"' + @MyAssignmentsLabel + '"' + ' and associated Notification Types');							
		  If(@ExistingCatMapInstanceID is Null)
            BEGIN
				EXEC csiPRDGetNextInstanceId @CategoryNotificationMapCDODefId,@InstanceId_CategoryNotificationMap OUTPUT
				INSERT INTO CategoryNotificationMap (CategoryNotificationMapId, CDOTypeId, MessageCategoryLabelId, ChangeCount, DisplayInConcierge, DisplayInMessageCenter, ParentId, Sequence, Icon) 
					VALUES (@InstanceId_CategoryNotificationMap, @CategoryNotificationMapCDODefId, @InstanceId_MyAssignment, 1, 1, 1, @InstanceId_PortalMessageCategory, 1, 'mailbox_fav_32.gif');
            END;
		   ELSE
            BEGIN
			  PRINT(' CategoryNotificationMap for ' + '"' + @MyAssignmentsLabel + '"' + ' already exists skipping......');
              SET @InstanceId_CategoryNotificationMap = @ExistingCatMapInstanceID 
            END;
			--
		SET @n_ErrLocator = 160;
		select @RecordCount = count(*) from CategoryNotificationMapTypes where CategoryNotificationMapId = @InstanceId_CategoryNotificationMap;
		IF (@RecordCount = 0)
		   BEGIN
			--
			-- Add Quality Record Ownership Assignment to My Assignments --
			--
				SET @i_Seq = 1;
				SET @n_ErrLocator = 210;
				--
				INSERT INTO CategoryNotificationMapTypes (CategoryNotificationMapId, FieldId, NotificationType, Sequence) 
					VALUES (@InstanceId_CategoryNotificationMap, @NotificationTypesFieldID, 1, @i_Seq);
		  END;
		ELSE
		  PRINT ' Notification types for Map ' + '"' + @MyAssignmentsLabel + '"' + ' already exist skipping......';
	      
		END TRY
		--
		BEGIN CATCH
		--
			SELECT @v_ErrMsg = 'AddPortalMessageCenterData_Prc - ErrLoc: ' + CAST(@n_ErrLocator AS VARCHAR(8)) + ' ErrMsg: ' + ERROR_MESSAGE() + ' ErrNum: ' + CAST(ERROR_NUMBER() AS VARCHAR(8))
				  ,@i_ErrorNumber = ERROR_NUMBER();

			PRINT @v_ErrMsg;
		--
		END CATCH;

	-------------------------------------------------
	-- Add My Approvals to CategoryNotificationMap --
	-------------------------------------------------
	BEGIN TRY
		--
		SET @n_ErrLocator                       = 171;
        SET @ExistingCatMapInstanceID           = NULL;
        SET @InstanceId_CategoryNotificationMap = NULL;

		SET @n_ErrLocator = 220;
		SELECT @ExistingCatMapInstanceID = (Select CategoryNotificationMapID 
								  From CategoryNotificationMap,
									   UserLabel 
								 Where CategoryNotificationMap.MessageCategoryLabelId = UserLabel.UserLabelId
								   and UserLabelName = @MyApprovalsLabel);

		SET @n_ErrLocator = 230;
		PRINT('Adding Category Notification Map ' + '"' + @MyApprovalsLabel + '"' + ' and associated Notification Types');									
		  If(@ExistingCatMapInstanceID is Null)
            BEGIN
				EXEC csiPRDGetNextInstanceId @CategoryNotificationMapCDODefId,@InstanceId_CategoryNotificationMap OUTPUT
				INSERT INTO CategoryNotificationMap (CategoryNotificationMapId, CDOTypeId, MessageCategoryLabelId, ChangeCount, DisplayInConcierge, DisplayInMessageCenter, ParentId, Sequence, Icon) 
				VALUES (@InstanceId_CategoryNotificationMap, @CategoryNotificationMapCDODefId, @InstanceId_MyApprovals, 1, 1, 1, @InstanceId_PortalMessageCategory, 2,'mailbox_write_32.gif');
            END;
		   ELSE
            BEGIN
			  PRINT(' CategoryNotificationMap for ' + '"' + @MyApprovalsLabel + '"' + ' already exists skipping......');
              SET @InstanceId_CategoryNotificationMap = @ExistingCatMapInstanceID 
            END;

		SET @n_ErrLocator = 240;
		Select @RecordCount = count(*) from CategoryNotificationMapTypes where CategoryNotificationMapId = @InstanceId_CategoryNotificationMap;
		IF (@RecordCount = 0)
		   BEGIN

		--
		-- Add Quality Record Approval Assignment to My Approvals
		--
			SET @i_Seq = 1;
			SET @n_ErrLocator = 260;
			--
			INSERT INTO CategoryNotificationMapTypes (CategoryNotificationMapId, FieldId, NotificationType, Sequence) 
				VALUES (@InstanceId_CategoryNotificationMap, @NotificationTypesFieldID, 6, @i_Seq);

		  END;
		ELSE
		  PRINT ' Notification types for Map ' + '"' + @MyApprovalsLabel + '"' + ' already exist skipping......';
      
	END TRY
	--
	BEGIN CATCH
	--
		SELECT @v_ErrMsg = 'AddPortalMessageCenterData_Prc - ErrLoc: ' + CAST(@n_ErrLocator AS VARCHAR(8)) + ' ErrMsg: ' + ERROR_MESSAGE() + ' ErrNum: ' + CAST(ERROR_NUMBER() AS VARCHAR(8))
			  ,@i_ErrorNumber = ERROR_NUMBER();

		PRINT @v_ErrMsg;
	--
	END CATCH;

	-----------------------------------------------------
	-- Add My Pending Items to CategoryNotificationMap -- 
	-----------------------------------------------------
	BEGIN TRY
		--
		SET @n_ErrLocator                       = 172;
        SET @ExistingCatMapInstanceID           = NULL;
        SET @InstanceId_CategoryNotificationMap = NULL;
		--
		SET @n_ErrLocator = 270;
		SELECT @ExistingCatMapInstanceID = (Select CategoryNotificationMapID 
								  From CategoryNotificationMap,
									   UserLabel 
								 Where CategoryNotificationMap.MessageCategoryLabelId = UserLabel.UserLabelId
								   and UserLabelName = @MyPendingItemsLabel);

		SET @n_ErrLocator = 280;
		PRINT('Adding Category Notification Map ' + '"' + @MyPendingItemsLabel + '"' + ' and associated Notification Types');											
		If(@ExistingCatMapInstanceID is Null)
           BEGIN
			EXEC csiPRDGetNextInstanceId @CategoryNotificationMapCDODefId,@InstanceId_CategoryNotificationMap OUTPUT
			INSERT INTO CategoryNotificationMap (CategoryNotificationMapId, CDOTypeId, MessageCategoryLabelId, ChangeCount, DisplayInConcierge, DisplayInMessageCenter, ParentId, Sequence, Icon) 
				VALUES (@InstanceId_CategoryNotificationMap, @CategoryNotificationMapCDODefId, @InstanceId_MyPendingItems, 1, 1, 1, @InstanceId_PortalMessageCategory, 3, 'mailbox_clock_32.gif');
           END;
		ELSE
           BEGIN
		    PRINT(' CategoryNotificationMap for ' + '"' + @MyPendingItemsLabel + '"' + ' already exists skipping......');
            SET @InstanceId_CategoryNotificationMap = @ExistingCatMapInstanceID 
           END;
			--
		SET @n_ErrLocator = 290;
		Select @RecordCount = count(*) from CategoryNotificationMapTypes where CategoryNotificationMapId = @InstanceId_CategoryNotificationMap;
		IF (@RecordCount = 0)
		   BEGIN
			--
			-- Add Quality Record Pending Assignment to My Pending Items
			--
				SET @i_Seq = 1;
				SET @n_ErrLocator = 300;
				--
				INSERT INTO CategoryNotificationMapTypes (CategoryNotificationMapId, FieldId, NotificationType, Sequence) 
					VALUES (@InstanceId_CategoryNotificationMap, @NotificationTypesFieldID, 34, @i_Seq);
		END;
	  ELSE
	    PRINT ' Notification types for Map ' + '"' + @MyPendingItemsLabel + '"' + ' already exist skipping......';      
	END TRY
	--
	BEGIN CATCH
	--
		SELECT @v_ErrMsg = 'AddPortalMessageCenterData_Prc - ErrLoc: ' + CAST(@n_ErrLocator AS VARCHAR(8)) + ' ErrMsg: ' + ERROR_MESSAGE() + ' ErrNum: ' + CAST(ERROR_NUMBER() AS VARCHAR(8))
			  ,@i_ErrorNumber = ERROR_NUMBER();

		PRINT @v_ErrMsg;
	--
	END CATCH;

	----------------------------------------------
	-- Add My Alerts to CategoryNotificationMap -- 
	----------------------------------------------
	BEGIN TRY
		--
		SET @n_ErrLocator                       = 174;
        SET @ExistingCatMapInstanceID           = NULL;
        SET @InstanceId_CategoryNotificationMap = NULL;
		--

		SET @n_ErrLocator = 310;
		SELECT @ExistingCatMapInstanceID = (Select CategoryNotificationMapID 
								  From CategoryNotificationMap,
									   UserLabel 
								 Where CategoryNotificationMap.MessageCategoryLabelId = UserLabel.UserLabelId
								   and UserLabelName = @MyAlertsLabel);

		SET @n_ErrLocator = 320;
		PRINT('Adding Category Notification Map ' + '"' + @MyAlertsLabel + '"' + ' and associated Notification Types');													
		If(@ExistingCatMapInstanceID is Null)
           BEGIN
			 EXEC csiPRDGetNextInstanceId @CategoryNotificationMapCDODefId,@InstanceId_CategoryNotificationMap OUTPUT
		  	 INSERT INTO CategoryNotificationMap (CategoryNotificationMapId, CDOTypeId, MessageCategoryLabelId, ChangeCount, DisplayInConcierge, DisplayInMessageCenter, ParentId, Sequence, Icon) 
			   	  VALUES (@InstanceId_CategoryNotificationMap, @CategoryNotificationMapCDODefId, @InstanceId_MyAlerts, 1, 1, 1, @InstanceId_PortalMessageCategory, 4, 'mailbox_info_32.gif');
           END;
		ELSE
           BEGIN
		    PRINT(' CategoryNotificationMap for ' + '"' + @MyAlertsLabel + '"' + ' already exists skipping......');
            SET @InstanceId_CategoryNotificationMap = @ExistingCatMapInstanceID 
           END;
			--
		SET @n_ErrLocator = 330;
		Select @RecordCount = count(*) from CategoryNotificationMapTypes where CategoryNotificationMapId = @InstanceId_CategoryNotificationMap;
		IF (@RecordCount = 0)
		   BEGIN
			--
			-- Add Quality Record Resolution Approved to My Alerts
			--
				SET @i_Seq = 1;
				SET @n_ErrLocator = 510;
				INSERT INTO CategoryNotificationMapTypes (CategoryNotificationMapId, FieldId, NotificationType, Sequence) 
					VALUES (@InstanceId_CategoryNotificationMap, @NotificationTypesFieldID, 9, @i_Seq);
			--
			-- Add Quality Record Resolution Rejected to My Alerts
			--
				SET @i_Seq = @i_Seq + 1;
				SET @n_ErrLocator = 520;
				INSERT INTO CategoryNotificationMapTypes (CategoryNotificationMapId, FieldId, NotificationType, Sequence) 
					VALUES (@InstanceId_CategoryNotificationMap, @NotificationTypesFieldID, 10, @i_Seq);
			END;
		  ELSE
			PRINT ' Notification types for Map ' + '"' + @MyAlertsLabel + '"' + ' already exist skipping......';      

	PRINT 'Finished loading instance data for Message Center ';														

	END TRY
	

	
	BEGIN CATCH
		--
		SELECT @v_ErrMsg = 'AddPortalMessageCenterData_Prc - ErrLoc: ' + CAST(@n_ErrLocator AS VARCHAR(8)) + ' ErrMsg: ' + ERROR_MESSAGE() + ' ErrNum: ' + CAST(ERROR_NUMBER() AS VARCHAR(8))
		      ,@i_ErrorNumber = ERROR_NUMBER();

		PRINT 'Error loading instance data for Message Center ' + @v_ErrMsg;
	END CATCH;

END
GO

EXEC AddPortalMessageCenterData_Prc
GO
DROP PROCEDURE AddPortalMessageCenterData_Prc
GO


