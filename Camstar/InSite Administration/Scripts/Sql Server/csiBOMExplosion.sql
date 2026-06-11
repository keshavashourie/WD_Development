
IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS 
 	    WHERE Name = 'csiBOMExplosion' 
	      AND Type = 'P')
	--
	DROP PROCEDURE csiBOMExplosion
	--
GO


CREATE PROCEDURE  [csiBOMExplosion] @p_MasterProductName VARCHAR(50), 
				@p_MasterProductRev VARCHAR(50), 
				@p_ComponentLevel INT = '0', 
                                @p_ContainerId VARCHAR(50) = '' 
AS 
-----------------------------------------------------------------------------------------------
-- This procedure will explode the BOM Components and stores it in EXPLODEDBOM table. Reports 
-- can be generated using the EXPLODEDBOM table. The EXPLODEDBOM table will be deleted for each
-- run of this stored procedure. At any given time, this table will have data from the last run.
--
--  Modification History:
--  Name                Date            Action
--  --------------      ----------      ----------------
--  Rami Lokas      	N/A      	Initial Creation
--  Purushotham N.	12/18/2005	Modified the CURSOR SQL Statement to retrieve Component
--					Revisions for all BOM levels
--					- Formatted the code for more readability.
--					- Implemented T-SQL Coding standards
--  Barry E.            12/20/2005      Added p_ContainerId parameter and column to resultset.
--  Bill Lippard        12/04/2006      Updated copyright notices (SPR S9984).
--  Bill Lippard        04/23/2007      Updated copyright notice (SPR S9984).
--  Dan Maloney         12/06/2017      Update first record insrt into #ExplodeBOM so that the following fields will have a NULL value
--                                      ComponentQty,  
--                                      OverConsumption, 
--                                      UnderConsumption, 
--                                      Substitution, 
--                                      EffectiveFrom, 
--                                      EffectiveThru, 
--                                      ComponentUOM, 
--                                      Phantom, 
--                                      IssueType
--  Dan Maloney         12/06/2017      Added 11 columns to select clause for BOMComponents Cursor to match the Oracle csiBOMExplosiong stored proc   
--
-- Copyright Siemens 2023  
-----------------------------------------------------------------------------------------------
	--
    DECLARE @v_ProductRev		VARCHAR(50)  
	DECLARE @v_ProductName		VARCHAR(100)  
	DECLARE @v_ComponentName 	VARCHAR(50)  
	DECLARE @v_ComponentRevision	VARCHAR(50)  
	DECLARE @v_ComponentQty 	VARCHAR(50)  
	DECLARE @v_ComponentUOM 	VARCHAR(50)  
	DECLARE @v_LevelString 		VARCHAR(50)  
	DECLARE @i_LoopIndex 		INT  
	DECLARE @v_OverConsumption  VARCHAR(3) 
	DECLARE @v_UnderConsumption  VARCHAR(3) 
	DECLARE @v_Substitution VARCHAR(3) 
	DECLARE @v_EffectiveFrom datetime   
	DECLARE @v_EffectiveThru datetime   
    DECLARE @v_Phantom VARCHAR(3) 
    DECLARE @v_IssueType VARCHAR(30) 
    DECLARE @v_MasterProductId VARCHAR(16) 
    DECLARE @v_MasterProductBaseId VARCHAR(16) 
    DECLARE @v_CompProductId VARCHAR(16) 
    DECLARE @v_CompProductBaseId VARCHAR(16) 
    DECLARE @v_SubCount INT 

 
    DECLARE @v_BOMBaseId1 VARCHAR(16) 
    DECLARE @v_BOMBaseId2 VARCHAR(16) 
	DECLARE @v_PMLIProdBaseId1 VARCHAR(16)
    DECLARE @v_PRD2ProdBaseId2 VARCHAR(16)
    DECLARE @v_PMLIProdId1 VARCHAR(16)
    DECLARE @v_PRD1ProdId VARCHAR(16)
    DECLARE @v_PRD2ProdId VARCHAR(16)
    DECLARE @v_PRDBS3RevOfRcdId VARCHAR(16)
    DECLARE @v_BOMBASERevOfRcdId VARCHAR(16) 
	--
	SET @v_LevelString = ''  
	SET @i_LoopIndex = 0  
	--
	DECLARE BOMComponents CURSOR LOCAL  
	FOR  
	SELECT  
	PRDBASE1.ProductBaseId As MasterProductBaseId
    ,PRD1.ProductId As MasterProductId
	,PRDBASE3.ProductBaseId As CompProductBaseId
	,PRD2.ProductId As CompProductId 
	,PRD1.ProductRevision        ProductRev 	 
	,PRDBASE1.ProductName        ProductName  
	,PRDBASE3.ProductName AS ComponentName
	,PRD2.ProductRevision AS ComponentRevision
	,UOM.UOMName                 ComponentUOM  
	,PMLI1.QtyRequired AS ComponentQty
	,case PMLI1.AllowOverConsumption when 1 then 'Yes' else 'No' End AS OverConsumption
	,case PMLI1.AllowUnderConsumption when 1 then 'Yes' else 'No' End AS UnderConsumtion
	,PMLI1.EffectiveFromDate AS EffectiveFrom
	,PMLI1.EffectiveThruDate AS EffectiveThru
    ,case PRD2.IsPhantom when 1 then 'Yes' else 'No' End  as Phantom
    ,Case PMLI1.MaterialTxnLogic  
		WHEN 1 THEN 'Serialized'		 
		WHEN 2 THEN 'Bulk'		 
		WHEN 3 THEN 'Lot and Stock Point' 
		WHEN 4 THEN 'StockPoint Only'		 
		WHEN 5 THEN 'No Tracking'		 
		Else 'Comment Only'	 
	    END  as IssueType 
	,PRD1.BOMBaseId               BOMBaseId1  
    ,PRD2.BOMBaseId               BOMBaseId2  
    ,PMLI1.ProductBaseId          PMLIProdBaseId1  
    ,PRD2.ProductBaseId           PRD2ProdBaseId2  
    ,PMLI1.ProductId              PMLIProdId1  
    ,PRD1.ProductId               PRD1ProdId  
    ,PRD2.ProductId               PRD2ProdId  
    ,PRDBASE3.RevOfRcdId          PRDBS3RevOfRcdId  
    ,BOMBASE2.RevOfRcdId          BOMBASERevOfRcdId  
    FROM PRODUCTBASE  PRDBASE1 INNER JOIN PRODUCT PRD1 ON PRDBASE1.ProductBaseId = PRD1.ProductBaseId  
                                    FULL OUTER JOIN BOMBASE ON PRD1.BOMBaseId = BOMBASE.BOMBaseId  
                                    FULL OUTER JOIN BOM ON PRD1.BOMId = BOM.BOMId OR BOMBASE.RevOfRcdId = BOM.BOMId  
                                    FULL OUTER JOIN BOMBASE BOMBASE2 ON BOM.BOMBaseId = BOMBASE2.BOMBaseId  
                                    FULL OUTER JOIN ProductMaterialListItem PMLI1 ON BOM.BOMId = PMLI1.BOMId  
                                    FULL OUTER JOIN ProductBase PRDBASE2 ON PMLI1.ProductBaseId = PRDBASE2.ProductBaseId  
                                    FULL OUTER JOIN Product PRD2 ON PMLI1.ProductId = PRD2.ProductId OR  
                                                                    PRDBASE2.RevOfRcdId = PRD2.ProductId  
                                    FULL OUTER JOIN ProductBase PRDBASE3 ON PRD2.ProductBaseId = PRDBASE3.ProductBaseId  
                                    FULL OUTER JOIN UOM ON UOM.UOMId = PMLI1.UOMId  
        WHERE PRDBASE1.ProductName = @p_MasterProductName  
          AND PRD1.ProductRevision = @p_MasterProductRev;  
     
     
	--
	IF (@p_ComponentLevel > 20) RETURN -- Saftey net. SQL Server crashes after 32 or so levels of recursion.
	
	OPEN BOMComponents  
	--
	FETCH NEXT FROM BOMComponents INTO 	 
	                 @v_MasterProductBaseId, 
	                 @v_MasterProductId, 
                     @v_CompProductBaseId, 
                     @v_CompProductId, 
                     @v_ProductRev,  
                     @v_ProductName, 
                     @v_ComponentName, 	   
                     @v_ComponentRevision,
                     @v_ComponentUOM,	  
                     @v_ComponentQty, 	   
                     @v_OverConsumption, 
                     @v_UnderConsumption,    
                     @v_EffectiveFrom,   
                     @v_EffectiveThru,   
                     @v_Phantom,  
                     @v_IssueType,
                     @v_BOMBaseId1, 
                     @v_BOMBaseId2,
                     @v_PMLIProdBaseId1,
                     @v_PRD2ProdBaseId2,
                     @v_PMLIProdId1,
                     @v_PRD1ProdId,
                     @v_PRD2ProdId,
                     @v_PRDBS3RevOfRcdId,
                     @v_BOMBASERevOfRcdId
 
	--
 
	IF @p_ComponentLevel = '0'  
		--
		BEGIN  
			--
			CREATE TABLE #ExplodedBOM ( 
			               [RowId] [Integer] IDENTITY (1, 1),
			               [Material] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,  
						   [ComponentLevel] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,  
						   [ComponentRev] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,  
						   [ComponentQty] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,  
						   [OverConsumption] [varchar] (3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
						   [UnderConsumption] [varchar] (3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
						   [Substitution] [varchar] (3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
						   [EffectiveFrom] [datetime] NULL, 
						   [EffectiveThru] [datetime] NULL, 
						   [ComponentUOM] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
						   [Phantom] [varchar] (3) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
						   [IssueType] [varchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,						    
                           [ContainerId]  [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL  						  )  
			--
 
            --
			-- Check for substitutes for this Product
	        SELECT @v_SubCount = COUNT(*) 
            FROM ProductSubstitutes 
            WHERE ProductId IN (@v_MasterProductBaseId,@v_MasterProductId) 
             
            SET @v_Substitution = CASE @v_SubCount WHEN 0 THEN 'No' ELSE 'Yes' END 
	 
			INSERT INTO #ExplodedBOM (  
				          Material,  
				          ComponentLevel, 
						  ComponentRev, 						    
						  ComponentQty,  
						  OverConsumption, 
						  UnderConsumption, 
						  Substitution, 
						  EffectiveFrom, 
						  EffectiveThru, 
						  ComponentUOM, 
						  Phantom, 
						  IssueType,				   
                          ContainerId)  
				VALUES( 
						@p_MasterProductName  
				       ,@p_ComponentLevel   
				       ,'1'  
				       ,NULL 
					   ,NULL  
					   ,NULL 
					   ,NULL  
					   ,NULL 
					   ,NULL  
					   ,NULL 
					   ,NULL 
					   ,NULL  
                       ,@p_ContainerId                   
                 )  
            --
			SET @p_ComponentLevel = 1  
			--			
   		END  
	ELSE  
		--
		BEGIN  

			--
			SET @p_ComponentLevel = @p_ComponentLevel + 1  
			--
   		END  
		--
		SET @v_LevelString = @p_ComponentLevel  
		--
		WHILE @i_LoopIndex < @p_ComponentLevel  
			--
			BEGIN  
				--
				SET @v_LevelString = '   ' + @v_LevelString  
				SET @i_LoopIndex = @i_LoopIndex + 1  
				--
			END  
			--
			WHILE (@@FETCH_STATUS <> -1)  
				--
				BEGIN  
					--
					IF (@@FETCH_STATUS <> -2)  
						--
						BEGIN  
							-- 
							IF (@v_ComponentName is not null)
								--
         							BEGIN  
									--
									-- Check for substitutes for this Product
	                                SELECT @v_SubCount = COUNT(*) 
                                    FROM ProductSubstitutes 
                                    WHERE ProductId IN (@v_CompProductBaseId,@v_CompProductId) 
                                     
                                    SET @v_Substitution = CASE @v_SubCount WHEN 0 THEN 'No' ELSE 'Yes' END 
	                                --
									INSERT INTO #ExplodedBOM (  
									              Material,  
												  ComponentLevel, 
												  ComponentRev, 						    
												  ComponentQty,  
												  OverConsumption, 
												  UnderConsumption, 
												  Substitution, 
												  EffectiveFrom, 
												  EffectiveThru, 
												  ComponentUOM, 
												  Phantom, 
												  IssueType,				   
												  ContainerId)    
											VALUES( 
											       @v_ComponentName  
											       ,@p_ComponentLevel											         
												   ,@v_ComponentRevision 
												   ,@v_ComponentQty  
												   ,@v_OverConsumption   
												   ,@v_UnderConsumption 
												   ,@v_Substitution 
												   ,@v_EffectiveFrom 
												   ,@v_EffectiveThru 
												   ,@v_ComponentUOM 
												   ,@v_Phantom 
												   ,@v_IssueType  
												   ,@p_ContainerId)     
				  
 
									--
									-- Only continue with the recursion if this is NOT a self-referencing product.
									IF (@v_ComponentName + CONVERT(nvarchar,@v_ComponentRevision) <> @p_MasterProductName + CONVERT(nvarchar,@p_MasterProductRev))
            								EXEC csiBOMExplosion @v_ComponentName,@v_ComponentRevision,@p_ComponentLevel, @p_ContainerId  
									--
         							END  
								--
   						END  
						--
	--
	FETCH NEXT FROM BOMComponents INTO 	 
                     @v_MasterProductBaseId, 
	                 @v_MasterProductId, 
                     @v_CompProductBaseId, 
                     @v_CompProductId, 
                     @v_ProductRev,  
                     @v_ProductName, 
                     @v_ComponentName, 	   
                     @v_ComponentRevision,
                     @v_ComponentUOM,	  
                     @v_ComponentQty, 	   
                     @v_OverConsumption, 
                     @v_UnderConsumption,    
                     @v_EffectiveFrom,   
                     @v_EffectiveThru,   
                     @v_Phantom,  
                     @v_IssueType,
                     @v_BOMBaseId1, 
                     @v_BOMBaseId2,
                     @v_PMLIProdBaseId1,
                     @v_PRD2ProdBaseId2,
                     @v_PMLIProdId1,
                     @v_PRD1ProdId,
                     @v_PRD2ProdId,
                     @v_PRDBS3RevOfRcdId,
                     @v_BOMBASERevOfRcdId 
	--
	 
END  
  
IF @p_ComponentLevel = '1'  
   BEGIN        
      SELECT 
         RowId,
         Material,  
         ComponentLevel,  
	     ComponentRev,	      
		 ComponentQty,  
		 ComponentUOM, 
		 OverConsumption, 
		 UnderConsumption, 
		 Substitution, 
		 EffectiveFrom, 
	     EffectiveThru, 
	     Phantom, 
		 IssueType,				   
		 ContainerId         
       FROM #ExplodedBOM  
   END  
  
CLOSE BOMComponents  
DEALLOCATE BOMComponents
GO

