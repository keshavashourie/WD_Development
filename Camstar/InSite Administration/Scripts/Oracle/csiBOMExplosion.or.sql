-- Copyright Siemens 2023  
DECLARE
	--
	v_TableFound		VARCHAR2(30) := 'FALSE';
	--
BEGIN
	--
	SELECT 'TRUE'
	  INTO v_TableFound
	  FROM USER_TABLES
	 WHERE Table_Name = 'EXPLODEDBOM';
	--
	DBMS_OUTPUT.PUT_LINE('EXPLODEDBOM Table Found: '||v_TableFound);
	--
EXCEPTION
	WHEN NO_DATA_FOUND THEN
		--
		DBMS_OUTPUT.PUT_LINE('EXPLODEDBOM Table Found: '||v_TableFound);
		--
		EXECUTE IMMEDIATE 'CREATE GLOBAL TEMPORARY TABLE EXPLODEDBOM ( RecId NUMBER, Material VARCHAR2(50), 
						   ComponentLevel VARCHAR2(50), 
						   ComponentRev VARCHAR2(50), 
						   ComponentQty VARCHAR2(50), 
						   OverConsumption VARCHAR2(3),
						   UnderConsumption VARCHAR2(3),
						   Substitution VARCHAR2(3),
						   EffectiveFrom DATE ,
						   EffectiveThru DATE,
						   ComponentUOM VARCHAR2(50),
						   Phantom VARCHAR2(3),
						   IssueType VARCHAR2(30),						   
               ContainerId  VARCHAR2(50))
						   ON COMMIT PRESERVE ROWS';
		--
	WHEN OTHERS THEN
		--
		DBMS_OUTPUT.PUT_LINE('Error creating EXLODEDBOM table: '||SQLERRM);
		--
		RAISE_APPLICATION_ERROR(-20001,'Error retrieving EXPLODEDBOM table information: '||SQLERRM);
		--
END;
/
CREATE OR REPLACE PROCEDURE  "CSIBOMEXPLOSION" ( p_MasterProductName    VARCHAR2 
                          ,p_MasterProductRev     VARCHAR2 DEFAULT '1' 
                          ,p_ComponentLevel       NUMBER   DEFAULT 0 
						  ,p_ContainerId          VARCHAR2 DEFAULT NULL ) 
AS 
-----------------------------------------------------------------------------------------------
-- This procedure will explode the BOM Components and stores it in a temporaty table. Reports
-- can be generated using the temporary table. This procedure is a recursive procedure.
--
--  Modification History:
--  Name                Date            Action
--  --------------      ----------      ----------------
--  Purushotham N.      11/15/2005      Initial Creation. ( SPR - S9792)
--  Barry E.            12/21/2005      Added p_ContainerId parameter and column
--  Bill Lippard        12/04/2006      Updated copyright notice (SPR S9984).
--  Bill Lippard        04/23/2007      Updated copyright notice (SPR S9984).
--
-- @ 2016  
-----------------------------------------------------------------------------------------------
	--
	n_ErrLocator        NUMBER; 
	n_ComponentLevel    NUMBER := NVL(p_ComponentLevel,0); 
	n_ComponentSequence NUMBER; -- Not used for now...  
  v_SubCount          NUMBER;
  v_Substitution      VARCHAR2(3);
	--
	v_ErrMsg            VARCHAR2(512); 
	--
	-- Cursor to retrieve the BOM and its components.
	--
	CURSOR BOM_Cur IS 
	SELECT PRDBASE1.ProductBaseId MasterProductBaseId
	      ,PRD1.ProductId              MasterProductId
	      ,PRDBASE3.ProductBaseId      CompProductBaseId
	      ,PRD2.ProductId              CompProductId
        ,PRD1.ProductRevision        ProductRev 	
	      ,PRDBASE1.ProductName        ProductName 
	      ,PRDBASE3.ProductName        ComponentName 
	      ,PRD2.ProductRevision        ComponentRevision 
        ,UOM.UOMName                 ComponentUOM 
	      ,PMLI1.QtyRequired ComponentQty
	      ,case PMLI1.AllowOverConsumption when 1 then 'Yes' else 'No' End OverConsumption
	      ,case PMLI1.AllowUnderConsumption when 1 then 'Yes' else 'No' End UnderConsumtion
	      ,PMLI1.EffectiveFromDate EffectiveFrom
	      ,PMLI1.EffectiveThruDate EffectiveThru
    	  ,case PRD2.IsPhantom when 1 then 'Yes' else 'No' End  Phantom
    	  ,Case PMLI1.MaterialTxnLogic 
		        WHEN 1 THEN 'Serialized'		
		        WHEN 2 THEN 'Bulk'		
		        WHEN 3 THEN 'Lot and Stock Point'
		        WHEN 4 THEN 'StockPoint Only'		
		        WHEN 5 THEN 'No Tracking'		
		        Else 'Comment Only'	
	      END  IssueType
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
                                    LEFT OUTER JOIN Product PRD2 ON (PMLI1.ProductId = PRD2.ProductId OR PRDBASE2.RevOfRcdId = PRD2.ProductId )
                                    FULL OUTER JOIN ProductBase PRDBASE3 ON PRD2.ProductBaseId = PRDBASE3.ProductBaseId 
                                    FULL OUTER JOIN UOM ON UOM.UOMId = PMLI1.UOMId 	
        WHERE PRDBASE1.ProductName = p_MasterProductName 
          AND PRD1.ProductRevision = p_MasterProductRev; 
	--
BEGIN 
	--
    	DBMS_OUTPUT.PUT_LINE('Input - Master Product: '||p_MasterProductName||' Component Level: '||n_ComponentLevel); 
    	--
      n_ErrLocator := 1;
      IF ( n_ComponentLevel > 20) THEN
         RETURN; -- Saftey net to prevent infinite recursion.
      END IF;
      n_ErrLocator := 2;
    	IF ( n_ComponentLevel = 0 ) THEN 
        	--
        	-- Delete the previous run BOM Explosion data from the temporary table
        	--
        	n_ErrLocator := 5; 
        	--
        	EXECUTE IMMEDIATE 'TRUNCATE TABLE EXPLODEDBOM'; 
    		--
    		-- Insert the Master Product at the top of the BOM Tree.
    		--
    		BEGIN 
            		--
       			DBMS_OUTPUT.PUT_LINE('Inserting into EXPLODEDBOM table - Master Product: '||p_MasterProductName); 
    			--      
    			n_ErrLocator := 10; 
    			--
    			INSERT INTO EXPLODEDBOM ( RecId
                                    ,ComponentLevel 
    					 	                    ,Material 
    					 	                    ,ComponentRev 
    					 	                    ,ComponentQty 
    					 	                    ,ComponentUOM 
						                        ,ContainerId						                        
						                        ,OverConsumption
						                        ,UnderConsumption
						                        ,Substitution 
						                        ,EffectiveFrom
						                        ,EffectiveThru
						                        ,Phantom
						                        ,IssueType) 
    			 	 VALUES( 1
              ,n_ComponentLevel 
    			 		,p_MasterProductName 
    					,'1' 
    					,NULL 
    					,NULL 
              ,p_ContainerId
					    ,NULL
					    ,NULL
					    ,NULL
					    ,NULL
					    ,NULL
					    ,NULL
					    ,NULL ); 
    			--
		EXCEPTION 
			WHEN OTHERS THEN 
    				--
    				DBMS_OUTPUT.PUT_LINE('Error inserting EXPLODEDBOM table - Master Product - ErrLoc: '||n_ErrLocator||' Errmsg: '||SQLERRM||' - Exiting the program...'); 
				    RAISE_APPLICATION_ERROR(-20001,'Error inserting EXPLODEDBOM table - Master Product - ErrLoc: '||n_ErrLocator||' Errmsg: '||SQLERRM||' - Exiting the program...'); 
    				--
    				RETURN; 
    				--
    		END; 
    		--
    		-- Initialize component level.
    		--
    		n_ComponentLevel := 1; 
    		--
   	ELSE 
		--
    		n_ComponentLevel := n_ComponentLevel + 1; 
    		--
	END IF; -- IF n_ComponentLevel = 0
    	--
	-- Explode this Master Product and also its children.
	--
  n_ErrLocator := 7;
	FOR CurrComponent IN BOM_Cur LOOP 
		--
    n_ErrLocator := 11;
		IF ( NVL(CurrComponent.ComponentName,'XXX') <> 'XXX' ) THEN 
    			--
--    			n_ComponentSequence := NVL(n_ComponentSequence,0) + 1;
    			--
			BEGIN 
        DBMS_OUTPUT.PUT_LINE('Inserting into EXPLODEDBOM table - Sequence: '||n_ComponentSequence||' Component Level: '||n_ComponentLevel||' Material: '||CurrComponent.ComponentName); 
				--
				n_ErrLocator := 15; 
				--
        -- Check for substitutes for this Product
        SELECT COUNT(*)
        INTO v_SubCount
        FROM ProductSubstitutes
        WHERE ProductId IN (CurrComponent.CompProductBaseId,CurrComponent.CompProductId);
            
        v_Substitution := CASE v_SubCount WHEN 0 THEN 'No' ELSE 'Yes' END;

				INSERT INTO EXPLODEDBOM( RecId
                  ,ComponentLevel 
    					 	 	,Material 
    					 	 	,ComponentRev 
    					 	 	,ComponentQty 
    					 	 	,ComponentUOM 
							    ,ContainerId
							    ,OverConsumption
						 	    ,UnderConsumption
						 	    ,Substitution 
						 	    ,EffectiveFrom
						 	    ,EffectiveThru
						 	    ,Phantom
						 	    ,IssueType) 
					 VALUES( (SELECT MAX(RecId)+1 FROM EXPLODEDBOM)
            ,n_ComponentLevel 
					 	,CurrComponent.ComponentName 
						,CurrComponent.ComponentRevision 
						,CurrComponent.ComponentQty 
						,CurrComponent.ComponentUOM 
						,p_ContainerId
						,CurrComponent.OverConsumption
						,CurrComponent.UnderConsumtion
						,v_Substitution
						,CurrComponent.EffectiveFrom
						,CurrComponent.EffectiveThru
						,CurrComponent.Phantom
						,CurrComponent.IssueType); 
				--
			EXCEPTION 
				WHEN OTHERS THEN 
					--
					DBMS_OUTPUT.PUT_LINE('Error inserting EXPLODEDBOM table - ErrLoc: '||n_ErrLocator||' Errmsg: '||SQLERRM||' - Exiting the program...'); 
					--
					RETURN; 
					--
			END; 
			   --
			   n_ErrLocator := 20; 
         -- Only continue with the recursion if this is NOT a self-referencing product.
         IF (CurrComponent.ComponentName||TO_CHAR(CurrComponent.ComponentRevision) <> p_MasterProductName||TO_CHAR(p_MasterProductRev)) THEN
            csiBOMExplosion ( CurrComponent.ComponentName 
   			                 ,CurrComponent.ComponentRevision 
   				               ,n_ComponentLevel 
							           ,p_ContainerId ); 
         END IF;
     END IF; -- If CurrComponent.ComponentName <> NULL
	END LOOP; 
	--
  n_ErrLocator := 25; 
	COMMIT; 
	--
EXCEPTION 
    WHEN OTHERS THEN 
        --
        ROLLBACK; 
        DBMS_OUTPUT.PUT_LINE('OTHERS Error - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM); 
        --
END;
/
CREATE or REPLACE PACKAGE csiWorkflowBOMExplosion_data AS
TYPE BOMCurTyp IS REF CURSOR RETURN
EXPLODEDBOM%ROWTYPE;
END csiWorkflowBOMExplosion_data;
/
CREATE OR REPLACE PROCEDURE csiBOMExplosion_BO(p_MasterProductName VARCHAR2,p_MasterProductRev VARCHAR2 DEFAULT '1', p_cvBOM IN OUT csiWorkflowBOMExplosion_data.BOMCurTyp) AS
BEGIN
   csiBOMExplosion(p_MasterProductName,p_MasterProductRev);
   OPEN p_cvBOM FOR SELECT * FROM EXPLODEDBOM;
END;
/

