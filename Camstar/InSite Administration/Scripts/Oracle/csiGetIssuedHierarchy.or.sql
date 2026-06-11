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
	 WHERE Table_Name = 'ISSUEDHIERARCHY';
	--
	DBMS_OUTPUT.PUT_LINE('ISSUEDHIERARCHY Table Found: '||v_TableFound);
	--
EXCEPTION
	WHEN NO_DATA_FOUND THEN
		--
		DBMS_OUTPUT.PUT_LINE('ISSUEDHIERARCHY Table Found: '||v_TableFound);
		--
		EXECUTE IMMEDIATE 'CREATE GLOBAL TEMPORARY TABLE ISSUEDHIERARCHY ( 
                                RecurseLevel number
                               ,TopLevelContainerName varchar2(255)
                               ,FromContainerName varchar2(255)
                               ,ToContainerName varchar2(255)
                               ,Qty number
                               ,QtyUOM varchar2(50)
                               ,ProductId char(16)
                               ,ProductName varchar2(255)
                               ,ProductRevision char(16)
                               ,ProductDescription varchar2(512)
                               ,ProductType varchar2(255)
                               ,ProductFamily varchar2(255)
                               ,TxnDate date
                               ,TxnDateGMT date
                               ,EmployeeName varchar2(255)
                               ,WorkflowStepName varchar2(255)
                               ,CustomerName varchar2(255)
                )
						   ON COMMIT PRESERVE ROWS';
		--
	WHEN OTHERS THEN
		--
		DBMS_OUTPUT.PUT_LINE('Error creating ISSUEDHIERARCHY table: '||SQLERRM);
		--
		RAISE_APPLICATION_ERROR(-20001,'Error retrieving ISSUEDHIERARCHY table information: '||SQLERRM);
		--
END;
/
CREATE OR REPLACE PROCEDURE csiGetIssuedHierarchy(pLotOrContainerName varchar2, pRecursion number default 0)
AS
   vChildContainerId char(16); 

   vToContainerId char(16);
   vToContainerName varchar2(255);
   vFromContainerId char(16);
   vFromContainerName varchar2(255);
   vQty NUMBER;
   vUOMId char(16);
   vUOMName varchar2(255);
   --
   vProductId char(16);
   vProductName varchar2(255);
   vProductRevision char(16);
   vProductDescription varchar2(512);
   vProductType varchar2(255);
   vProductFamily varchar2(255);
   --
   vTxnId char(16);
   vTxnDate date;
   vTxnDateGMT date;
   vEmployeeName varchar2(255);
   vWorkflowStepName varchar2(255);
   --
   vCustomerName varchar2(255);
   --
   vNewRecursion number;
   CURSOR c1 IS
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
BEGIN

   IF (pRecursion = 0) THEN EXECUTE IMMEDIATE 'TRUNCATE TABLE ISSUEDHIERARCHY'; END IF;
   
   IF (pRecursion>10) THEN RETURN; END IF; -- Safety net
   
   OPEN c1;
   FETCH c1 INTO vToContainerId
                ,vFromContainerId
                ,vQty
                ,vUOMId
                ,vProductId
                ,vTxnId;
   
   IF (c1%NOTFOUND) THEN
      -- We are at a top-level container. So, go back through the Issue temp table and create this
      -- top-level container's child tree
      INSERT INTO ISSUEDHIERARCHY(RecurseLevel,TopLevelContainerName,ToContainerName,FromContainerName,Qty,QtyUOM,ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName)
         SELECT Level,pLotOrContainerName TopLevelContainer, ToContainerName, FromContainerName, Qty, QtyUOM, ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName
         FROM ISSUEDHIERARCHY
         WHERE TopLevelContainerName IS NULL
         CONNECT BY PRIOR FromContainerName = ToContainerName
         START WITH ToContainerName = pLotOrContainerName;
   END IF;
   WHILE (c1%FOUND) LOOP
      -- Based on the base results from IssueActualsHistory, get the rest of the information
      -- * UOM
      -- * ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily
      -- * Customer
      -- * TxnDate of the issue, EmployeeName, WorklowStep of issue
      
      -- Get UOM information
      vUOMName := '';
      IF (vUOMId IS NOT NULL) THEN
         SELECT UOMName
         INTO vUOMName
         FROM UOM
         WHERE UOMId = vUOMId;
      END IF;
         
      -- Get Product information
      SELECT pb.ProductName, p.ProductRevision, p.Description, pt.ProductTypeName, pf.ProductFamilyName
      INTO vProductName,vProductRevision,vProductDescription,vProductType,vProductFamily
      FROM Product p LEFT OUTER JOIN ProductFamily pf ON p.ProductFamilyId = pf.ProductFamilyId, ProductBase pb, ProductType pt
      WHERE p.ProductId = vProductId
      AND pb.ProductBaseId = p.ProductBaseId
      AND pt.ProductTypeId = p.ProductTypeId;
      
      -- Get HistoryMainline information
      SELECT hml.TxnDate, hml.TxnDateGMT, e.EmployeeName, wfs.WorkflowStepName
      INTO vTxnDate,vTxnDateGMT,vEmployeeName,vWorkflowStepName
      FROM HistoryMainline hml, Employee e, WorkflowStep wfs
      WHERE hml.TxnId=vTxnId
      AND e.EmployeeId = hml.EmployeeId
      AND hml.WorkflowStepId = wfs.WorkflowStepId;

      -- Get ToContainer information
      SELECT cust.CustomerName,c.ContainerName
      INTO vCustomerName,vToContainerName
      FROM Container c
      LEFT OUTER JOIN Customer cust ON cust.CustomerId = c.CustomerId
      WHERE c.ContainerId = vToContainerId;    

      -- Get FromContainer information
      vFromContainerName := pLotOrContainerName;
      
      vNewRecursion := pRecursion+1;
      INSERT INTO ISSUEDHIERARCHY(FromContainerName,ToContainerName,Qty,QtyUOM,ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName) 
         VALUES (vFromContainerName, vToContainerName, vQty, vUOMName,vProductId,vProductName,vProductRevision,vProductDescription,vProductType,vProductFamily,vTxnDate,vTxnDateGMT,vEmployeeName,vWorkflowStepName,vCustomerName);
         
      csiGetIssuedHierarchy(vToContainerName, vNewRecursion);
      
      vChildContainerId := vToContainerId;
      vToContainerId := NULL;
      FETCH c1 INTO vToContainerId
                ,vFromContainerId
                ,vQty
                ,vUOMId
                ,vProductId
                ,vTxnId;
   END LOOP;

   CLOSE c1;

   IF (pRecursion=0) THEN
      -- Clean up all of the "working" records, then execute the final return query
      DELETE FROM ISSUEDHIERARCHY WHERE TopLevelContainerName IS NULL;
      -- In some cases, UOM is not set, so pull it from the Container directly
      UPDATE ISSUEDHIERARCHY
      SET QtyUOM = (SELECT UOMName
                    FROM Container C, UOM
                    WHERE ContainerName = FromContainerName
                    AND c.UOMId = UOM.UOMId)
      WHERE QtyUOM IS NULL OR QtyUOM = '';
      COMMIT;     
   END IF;
   
EXCEPTION
   WHEN NO_DATA_FOUND THEN NULL;
END;
/
CREATE or REPLACE PACKAGE csiIssuedHierarchy_data AS
TYPE IssuedCurTyp IS REF CURSOR RETURN
ISSUEDHIERARCHY%ROWTYPE;
TYPE tab_issuedhierarchy is TABLE OF issuedhierarchy%rowtype;
END csiIssuedHierarchy_data;
/
CREATE OR REPLACE PROCEDURE csiGetIssuedHierarchy_BO(pLotOrContainerName varchar2, p_cvData IN OUT csiIssuedHierarchy_data.IssuedCurTyp) AS
BEGIN
   csiGetIssuedHierarchy(pLotOrContainerName);
   OPEN p_cvData FOR SELECT * FROM ISSUEDHIERARCHY ORDER BY TopLevelContainerName, RecurseLevel;
END;
/
