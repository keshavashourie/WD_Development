
DO $$ 
BEGIN
	IF EXISTS (
		SELECT 1 
		FROM information_schema.tables 
		WHERE lower(table_name) = lower('IssuedHierarchy')
	) THEN
		DROP TABLE IF EXISTS IssuedHierarchy;
	END IF;
END $$;


DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiGetIssuedHierarchy')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiGetIssuedHierarchy;
 	END IF;
END $$;

CREATE PROCEDURE csiGetIssuedHierarchy(
	pLotOrContainerName varchar(100), 
	pRecursion integer = 0
)
LANGUAGE plpgsql
as $$
DECLARE
    vChildContainerId char(16);
    vToContainerId char(16);
    vToContainerName varchar(255);
    vFromContainerId char(16);
    vFromContainerName varchar(255);
    vQty integer;
    vUOMId char(16);
    vUOMName varchar(255);
   --
    vProductId char(16);
    vProductName varchar(255);
    vProductRevision char(16);
    vProductDescription varchar(512);
    vProductType varchar(255);
    vProductFamily varchar(255);
   --
    vTxnId char(16);
    vTxnDate timestamp;
    vTxnDateGMT timestamp;
    vEmployeeName varchar(255);
    vWorkflowStepName varchar(255);
   --
    vCustomerName varchar(255);
   --
    c1 CURSOR FOR
	  SELECT iah.ToContainerId
            ,iah.FromContainerId
            ,iah.Qty
            ,ihd.UOMId
            ,iah.ProductId
            ,iah.TxnId  
      FROM IssueActualsHistory iah, IssueHistoryDetail ihd
      WHERE (FromContainerId = (SELECT ContainerId FROM Container WHERE ContainerName = pLotOrContainerName)
            OR iah.FromLot = pLotOrContainerName)
      AND ihd.TxnId = iah.TxnId;
    vNewRecursion integer;
    vFetchStatus integer;
BEGIN
	if (pRecursion = 0) then
		CREATE TEMPORARY TABLE IssuedHierarchy (
			RecurseLevel integer 
            ,TopLevelContainerName varchar(255)
            ,FromContainerName varchar(255)
            ,ToContainerName varchar(255)
            ,Qty integer
            ,QtyUOM varchar(50)
            ,ProductId char(16)
            ,ProductName varchar(255)
            ,ProductRevision char(16)
            ,ProductDescription varchar(512)
            ,ProductType varchar(255)
            ,ProductFamily varchar(255)
            ,TxnDate timestamp
            ,TxnDateGMT timestamp
            ,EmployeeName varchar(255)
            ,WorkflowStepName varchar(255)
            ,CustomerName varchar(255)
		);
	END IF;
	
	IF (pRecursion>10) THEN RETURN;
	end if;
	
	-- Either the child ContainerName or Lot will be passed in.
	OPEN c1;
	
	FETCH c1 INTO vToContainerId, vFromContainerId, vQty, vUOMId, vProductId, vTxnId;
	IF FOUND THEN
		-- We are at a top-level container. So, go back through the Issue temp table and create this
		-- top-level container's child tree
		WITH n(TopLevelContainer, ToContainerName, FromContainerName, Thelevel, Qty, QtyUOM, ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName) AS
		(SELECT pLotOrContainerName, ToContainerName, FromContainerName, 1 As TheLevel,Qty,QtyUOM,ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName
        FROM IssuedHierarchy
        WHERE ToContainerName = pLotOrContainerName
        AND TopLevelContainerName IS NULL
        UNION ALL
        SELECT pLotOrContainerName, child.ToContainerName, child.FromContainerName, n.TheLevel + 1 As Level,child.Qty,child.QtyUOM,child.ProductId,child.ProductName, child.ProductRevision, child.ProductDescription, child.ProductType, child.ProductFamily,child.TxnDate, child.TxnDateGMT, child.EmployeeName, child.WorkflowStepName, child.CustomerName
        FROM IssuedHierarchy AS child, n
        WHERE n.FromContainerName = child.ToContainerName
        AND TopLevelContainerName IS NULL)
        INSERT INTO IssuedHierarchy(RecurseLevel,TopLevelContainerName,ToContainerName,FromContainerName,Qty,QtyUOM,ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName) SELECT TheLevel, TopLevelContainer, ToContainerName, FromContainerName,Qty,QtyUOM,ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName FROM n;
	END IF;
	
	LOOP
		EXIT WHEN NOT FOUND;
		vUOMName := '';
		IF (vUOMId IS NOT NULL) THEN
			SELECT UOMName INTO vUOMName
			FROM UOMId
			WHERE UOMId = vUOMId;
		END IF;
		
		-- Get Product information
		SELECT pb.ProductName, p.ProductRevision, p.Description, pt.ProductTypeName, pf.ProductFamilyName
		INTO vProductName, vProductRevision, vProductDescription, vProductType, vProductFamily
		FROM Product p LEFT OUTER JOIN ProductFamily pf ON p.ProductFamilyId = pf.ProductFamilyId, ProductBase pb, ProductType pt
		WHERE p.ProductId = vProductId
		AND pb.ProductBaseId = p.ProductBaseId
		AND pt.ProductTypeId = p.ProductTypeId;
		  
		-- Get HistoryMainline information
		SELECT hml.TxnDate, hml.TxnDateGMT, e.EmployeeName, wfs.WorkflowStepName
		INTO vTxnDate, vTxnDateGMT, vEmployeeName, vWorkflowStepName
		FROM HistoryMainline hml, Employee e, WorkflowStep wfs
		WHERE hml.TxnId=vTxnId
		AND e.EmployeeId = hml.EmployeeId
		AND hml.WorkflowStepId = wfs.WorkflowStepId;

		-- Get ToContainer information
		SELECT cust.CustomerName, c.ContainerName
		INTO vCustomerName, vToContainerName
		FROM Container c
		LEFT OUTER JOIN Customer cust ON cust.CustomerId = c.CustomerId
		WHERE c.ContainerId = vToContainerId;   

		vFromContainerName := pLotOrContainerName;
		
		vNewRecursion := pRecursion+1;
		
		INSERT INTO IssuedHierarchy(FromContainerName,ToContainerName,Qty,QtyUOM,ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName) 
        VALUES (vFromContainerName, vToContainerName, vQty, vUOMName, vProductId,vProductName,vProductRevision,vProductDescription,vProductType,vProductFamily,vTxnDate,vTxnDateGMT,vEmployeeName,vWorkflowStepName,vCustomerName);      
      
		CALL csiGetIssuedHierarchy(vToContainerName, vNewRecursion);
		
		FETCH c1 INTO vToContainerId, vFromContainerId, vQty, vUOMId, vProductId, vTxnId;
		
	END LOOP;
	CLOSE c1;
	--
	IF (pRecursion = 0) THEN
		-- Clean up all of the "working" records, then execute the final return query
		DELETE FROM IssuedHierarchy WHERE TopLevelContainerName IS NULL;
		-- In some cases, UOM is not set, so pull it from the Container directly
		UPDATE IssuedHierarchy
		SET QtyUOM = (SELECT UOMName
					FROM Container C, UOM
					WHERE ContainerName = FromContainerName
					AND c.UOMId = UOM.UOMId)
		WHERE QtyUOM IS NULL OR QtyUOM = '';
		--SELECT * FROM IssuedHierarchy ORDER BY TopLevelContainerName, RecurseLevel;
	END IF;
END;
$$;

--CALL csiGetIssuedHierarchy('Container_SI34_A_A121', null);