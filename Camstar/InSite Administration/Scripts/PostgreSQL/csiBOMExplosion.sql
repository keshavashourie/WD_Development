DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiBOMExplosion')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiBOMExplosion;
 	END IF;
END $$;

CREATE FUNCTION csiBOMExplosion(
	p_MasterProductName VARCHAR(50),
	p_MasterProductRev VARCHAR(50),
	p_ComponentLevel integer = 0,
	p_ContainerId VARCHAR(50) = ''
)
returns table(
	RowId INTEGER,
	Material VARCHAR(50),
    ComponentLevel VARCHAR(50),
    ComponentRev VARCHAR(50),
    ComponentQty VARCHAR(50),
    OverConsumption VARCHAR(3),
    UnderConsumption VARCHAR(3),
    Substitution VARCHAR(3),
    EffectiveFrom TIMESTAMP,
    EffectiveThru TIMESTAMP,
    ComponentUOM VARCHAR(50),
    Phantom VARCHAR(3),
    IssueType VARCHAR(30),
    ContainerId VARCHAR(50)
)
language plpgsql
as $$
DECLARE
	v_ProductRev		VARCHAR(50);  
	v_ProductName		VARCHAR(100);  
	v_ComponentName 	VARCHAR(50);  
	v_ComponentRevision	VARCHAR(50);  
	v_ComponentQty 	VARCHAR(50);  
	v_ComponentUOM 	VARCHAR(50);  
	v_LevelString 		VARCHAR(50);  
	i_LoopIndex 		integer;
	v_OverConsumption  VARCHAR(3); 
	v_UnderConsumption  VARCHAR(3); 
	v_Substitution VARCHAR(3); 
	v_EffectiveFrom timestamp;   
	v_EffectiveThru timestamp;
    v_Phantom VARCHAR(3); 
    v_IssueType VARCHAR(30); 
    v_MasterProductId VARCHAR(16); 
    v_MasterProductBaseId VARCHAR(16); 
    v_CompProductId VARCHAR(16); 
    v_CompProductBaseId VARCHAR(16); 
    v_SubCount integer;

    v_BOMBaseId1 VARCHAR(16); 
    v_BOMBaseId2 VARCHAR(16); 
	v_PMLIProdBaseId1 VARCHAR(16);
    v_PRD2ProdBaseId2 VARCHAR(16);
    v_PMLIProdId1 VARCHAR(16);
    v_PRD1ProdId VARCHAR(16);
    v_PRD2ProdId VARCHAR(16);
    v_PRDBS3RevOfRcdId VARCHAR(16);
    v_BOMBASERevOfRcdId VARCHAR(16);
   
    v_rowCount INTEGER;
   
	BOMComponents CURSOR FOR
	 SELECT
        PRDBASE1.ProductBaseId AS MasterProductBaseId,
        PRD1.ProductId AS MasterProductId,
        PRDBASE3.ProductBaseId AS CompProductBaseId,
        PRD2.ProductId AS CompProductId,
        PRD1.ProductRevision AS ProductRev,
        PRDBASE1.ProductName AS ProductName,
        PRDBASE3.ProductName AS ComponentName,
        PRD2.ProductRevision AS ComponentRevision,
        UOM.UOMName AS ComponentUOM,
        PMLI1.QtyRequired AS ComponentQty,
        CASE PMLI1.AllowOverConsumption WHEN 1 THEN 'Yes' ELSE 'No' END AS OverConsumption,
        CASE PMLI1.AllowUnderConsumption WHEN 1 THEN 'Yes' ELSE 'No' END AS UnderConsumtion,
        PMLI1.EffectiveFromDate AS EffectiveFrom,
        PMLI1.EffectiveThruDate AS EffectiveThru,
        CASE PRD2.IsPhantom WHEN 1 THEN 'Yes' ELSE 'No' END AS Phantom,
        CASE PMLI1.MaterialTxnLogic
            WHEN 1 THEN 'Serialized'
            WHEN 2 THEN 'Bulk'
            WHEN 3 THEN 'Lot and Stock Point'
            WHEN 4 THEN 'StockPoint Only'
            WHEN 5 THEN 'No Tracking'
            ELSE 'Comment Only'
        END AS IssueType,
        PRD1.BOMBaseId AS BOMBaseId1,
        PRD2.BOMBaseId AS BOMBaseId2,
        PMLI1.ProductBaseId AS PMLIProdBaseId1,
        PRD2.ProductBaseId AS PRD2ProdBaseId2,
        PMLI1.ProductId AS PMLIProdId1,
        PRD1.ProductId AS PRD1ProdId,
        PRD2.ProductId AS PRD2ProdId,
        PRDBASE3.RevOfRcdId AS PRDBS3RevOfRcdId,
        BOMBASE2.RevOfRcdId AS BOMBASERevOfRcdId
    FROM PRODUCTBASE PRDBASE1
    INNER JOIN PRODUCT PRD1 ON PRDBASE1.ProductBaseId = PRD1.ProductBaseId
    FULL OUTER JOIN BOMBASE ON PRD1.BOMBaseId = BOMBASE.BOMBaseId
    FULL OUTER JOIN BOM ON PRD1.BOMId = BOM.BOMId OR BOMBASE.RevOfRcdId = BOM.BOMId
    FULL OUTER JOIN BOMBASE BOMBASE2 ON BOM.BOMBaseId = BOMBASE2.BOMBaseId
    FULL OUTER JOIN ProductMaterialListItem PMLI1 ON BOM.BOMId = PMLI1.BOMId
    FULL OUTER JOIN ProductBase PRDBASE2 ON PMLI1.ProductBaseId = PRDBASE2.ProductBaseId
    FULL OUTER JOIN Product PRD2 ON PMLI1.ProductId = PRD2.ProductId OR PRDBASE2.RevOfRcdId = PRD2.ProductId
    FULL OUTER JOIN ProductBase PRDBASE3 ON PRD2.ProductBaseId = PRDBASE3.ProductBaseId
    FULL OUTER JOIN UOM ON UOM.UOMId = PMLI1.UOMId
    WHERE PRDBASE1.ProductName = p_MasterProductName --'PRODUCT_A'
      AND PRD1.ProductRevision = p_MasterProductRev; --'1'
	  
BEGIN

	-- assign a random string as a portal name
    -- before iterating over the cursor
    BOMComponents := random_portal_name();
	
	v_LevelString := '';
	i_LoopIndex := 0;
	
	IF (p_ComponentLevel > 20) then RETURN; -- Saftey net. SQL Server crashes after 32 or so levels of recursion.
	end if;

	OPEN BOMComponents;
	
	FETCH NEXT FROM BOMComponents INTO 	 
	                 v_MasterProductBaseId, 
	                 v_MasterProductId, 
                     v_CompProductBaseId, 
                     v_CompProductId, 
                     v_ProductRev,  
                     v_ProductName, 
                     v_ComponentName, 	   
                     v_ComponentRevision,
                     v_ComponentUOM,	  
                     v_ComponentQty, 	   
                     v_OverConsumption, 
                     v_UnderConsumption,    
                     v_EffectiveFrom,   
                     v_EffectiveThru,   
                     v_Phantom,  
                     v_IssueType,
                     v_BOMBaseId1, 
                     v_BOMBaseId2,
                     v_PMLIProdBaseId1,
                     v_PRD2ProdBaseId2,
                     v_PMLIProdId1,
                     v_PRD1ProdId,
                     v_PRD2ProdId,
                     v_PRDBS3RevOfRcdId,
                     v_BOMBASERevOfRcdId;
 
	--
	IF p_ComponentLevel = 0 THEN
		--
		drop table if exists ExplodedBom;
		--
		CREATE TABLE if not exists ExplodedBOM (
		    RowId INTEGER GENERATED ALWAYS AS IDENTITY,
		    Material VARCHAR(50),
		    ComponentLevel VARCHAR(50),
		    ComponentRev VARCHAR(50),
		    ComponentQty VARCHAR(50),
		    OverConsumption VARCHAR(3),
		    UnderConsumption VARCHAR(3),
		    Substitution VARCHAR(3),
		    EffectiveFrom TIMESTAMP,
		    EffectiveThru TIMESTAMP,
		    ComponentUOM VARCHAR(50),
		    Phantom VARCHAR(3),
		    IssueType VARCHAR(30),
		    ContainerId VARCHAR(50)
		);
		--

        --
		-- Check for substitutes for this Product
	    SELECT COUNT(*) INTO v_SubCount
		FROM ProductSubstitutes
		WHERE ProductId IN (v_MasterProductBaseId, v_MasterProductId); 
             
        IF v_SubCount = 0 THEN
			v_Substitution := 'No';
		ELSE
			v_Substitution := 'Yes';
		END IF; 
	 
		INSERT INTO ExplodedBOM (
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
			ContainerId
		) VALUES (
			p_MasterProductName,
			p_ComponentLevel,
			'1',
			NULL::VARCHAR(50),
			NULL::VARCHAR(3),
			NULL::VARCHAR(3),
			NULL::VARCHAR(3),
			null::TIMESTAMP,
			null::TIMESTAMP,
			null::VARCHAR(50),
			NULL::VARCHAR(3),
			NULL::VARCHAR(30),
			p_ContainerId
		);
        --
		p_ComponentLevel := 1;
		--			
	ELSE  
		-- 
		p_ComponentLevel := p_ComponentLevel + 1; 
		--
	END IF;
	
	v_LevelString := p_ComponentLevel;
	
	--
	WHILE i_LoopIndex < p_ComponentLevel LOOP

		v_LevelString := '   ' || v_LevelString;  
		i_LoopIndex := i_LoopIndex + 1; 
		
	END LOOP;
	--
	WHILE FOUND 
		-- WHILE (@@FETCH_STATUS <> -1)
	LOOP
		IF v_ComponentName is not null THEN
		-- Check for substitutes for this Product
	        SELECT COUNT(*) INTO v_SubCount 
			FROM ProductSubstitutes 
			WHERE ProductId IN (v_CompProductBaseId, v_CompProductId);
                                     
            v_Substitution := CASE when v_SubCount = 0 THEN 'No' ELSE 'Yes' END;
            --
			INSERT INTO ExplodedBOM (  
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
			    v_ComponentName  
				,p_ComponentLevel											         
				,v_ComponentRevision 
				,v_ComponentQty  
				,v_OverConsumption   
				,v_UnderConsumption 
				,v_Substitution 
				,v_EffectiveFrom 
				,v_EffectiveThru 
				,v_ComponentUOM 
				,v_Phantom 
				,v_IssueType  
				,p_ContainerId);   
				  
				-- Only continue with the recursion if this is NOT a self-referencing product.
				IF v_ComponentName || v_ComponentRevision != p_MasterProductName || p_MasterProductRev then
					PERFORM csiBOMExplosion(v_ComponentName, v_ComponentRevision, p_ComponentLevel, p_ContainerId);
				END IF;
				--  
			--
		END IF;				
		--
		FETCH NEXT FROM BOMComponents INTO 	 
	                 v_MasterProductBaseId, 
	                 v_MasterProductId, 
                     v_CompProductBaseId, 
                     v_CompProductId, 
                     v_ProductRev,  
                     v_ProductName, 
                     v_ComponentName, 	   
                     v_ComponentRevision,
                     v_ComponentUOM,	  
                     v_ComponentQty, 	   
                     v_OverConsumption, 
                     v_UnderConsumption,    
                     v_EffectiveFrom,   
                     v_EffectiveThru,   
                     v_Phantom,  
                     v_IssueType,
                     v_BOMBaseId1, 
                     v_BOMBaseId2,
                     v_PMLIProdBaseId1,
                     v_PRD2ProdBaseId2,
                     v_PMLIProdId1,
                     v_PRD1ProdId,
                     v_PRD2ProdId,
                     v_PRDBS3RevOfRcdId,
                     v_BOMBASERevOfRcdId; 
	--
    end loop;

	IF p_ComponentLevel = '1' THEN  
		  return QUERY 
		  SELECT *      
		   FROM ExplodedBOM eb; 
	END IF;

	CLOSE BOMComponents;

END;
$$;

create or replace function random_portal_name() returns varchar as $$
begin
    create temp sequence if not exists portal_names;
    return 'portal$' || nextval('portal_names');
end;
$$ language plpgsql;