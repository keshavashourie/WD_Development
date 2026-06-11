-- Copyright Siemens 2023  
IF (OBJECT_ID('TempDB..#IssuedHierarchy') IS NOT NULL)
   DROP TABLE #IssuedHierarchy
GO
IF EXISTS (SELECT Name 
	     FROM SYSOBJECTS 
 	    WHERE Name = 'csiGetIssuedHierarchy' 
	      AND Type = 'P')
	--
	DROP PROCEDURE csiGetIssuedHierarchy
	--
GO
CREATE PROCEDURE csiGetIssuedHierarchy(@pLotOrContainerName varchar(100), @pRecursion int = 0)
AS
   DECLARE @vChildContainerId char(16)
   DECLARE @vToContainerId char(16)
   DECLARE @vToContainerName varchar(255)
   DECLARE @vFromContainerId char(16)
   DECLARE @vFromContainerName varchar(255)
   DECLARE @vQty int
   DECLARE @vUOMId char(16)
   DECLARE @vUOMName varchar(255)
   --
   DECLARE @vProductId char(16)
   DECLARE @vProductName varchar(255)
   DECLARE @vProductRevision char(16)
   DECLARE @vProductDescription varchar(512)
   DECLARE @vProductType varchar(255)
   DECLARE @vProductFamily varchar(255)
   --
   DECLARE @vTxnId char(16)
   DECLARE @vTxnDate datetime
   DECLARE @vTxnDateGMT datetime
   DECLARE @vEmployeeName varchar(255)
   DECLARE @vWorkflowStepName varchar(255)
   --
   DECLARE @vCustomerName varchar(255)
   --
   DECLARE @c1 CURSOR
   DECLARE @vNewRecursion int
   DECLARE @vFetchStatus int
BEGIN
   SET NOCOUNT ON

   IF (@pRecursion = 0) 
   BEGIN
       CREATE TABLE #IssuedHierarchy
                   (RecurseLevel int 
                   ,TopLevelContainerName varchar(255)
                   ,FromContainerName varchar(255)
                   ,ToContainerName varchar(255)
                   ,Qty int
                   ,QtyUOM varchar(50)
                   ,ProductId char(16)
                   ,ProductName varchar(255)
                   ,ProductRevision char(16)
                   ,ProductDescription varchar(512)
                   ,ProductType varchar(255)
                   ,ProductFamily varchar(255)
                   ,TxnDate datetime
                   ,TxnDateGMT datetime
                   ,EmployeeName varchar(255)
                   ,WorkflowStepName varchar(255)
                   ,CustomerName varchar(255))
   END
   
   IF (@pRecursion>10) RETURN
   
   -- Either the child ContainerName or Lot will be passed in.
   SET @c1 = CURSOR FAST_FORWARD FOR
   SELECT iah.ToContainerId
            ,iah.FromContainerId
            ,iah.Qty
            ,ihd.UOMId
            ,iah.ProductId
            ,iah.TxnId  
      FROM IssueActualsHistory iah, IssueHistoryDetail ihd
      WHERE (FromContainerId = (SELECT ContainerId FROM Container WHERE ContainerName = @pLotOrContainerName)
            OR iah.FromLot = @pLotOrContainerName)
      AND ihd.TxnId = iah.TxnId
          
   OPEN @c1
   FETCH NEXT FROM @c1 INTO @vToContainerId, @vFromContainerId, @vQty, @vUOMId, @vProductId, @vTxnId
   SET @vFetchStatus = @@FETCH_STATUS
   IF (@vFetchStatus <> 0) 
   BEGIN
      -- We are at a top-level container. So, go back through the Issue temp table and create this
      -- top-level container's child tree
      WITH n(TopLevelContainer, ToContainerName, FromContainerName, Thelevel, Qty, QtyUOM, ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName) AS
        (SELECT @pLotOrContainerName, ToContainerName, FromContainerName, 1 As TheLevel,Qty,QtyUOM,ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName
        FROM #IssuedHierarchy
        WHERE ToContainerName = @pLotOrContainerName
        AND TopLevelContainerName IS NULL
        UNION ALL
        SELECT @pLotOrContainerName, child.ToContainerName, child.FromContainerName, n.TheLevel + 1 As Level,child.Qty,child.QtyUOM,child.ProductId,child.ProductName, child.ProductRevision, child.ProductDescription, child.ProductType, child.ProductFamily,child.TxnDate, child.TxnDateGMT, child.EmployeeName, child.WorkflowStepName, child.CustomerName
        FROM #IssuedHierarchy AS child, n
        WHERE n.FromContainerName = child.ToContainerName
        AND TopLevelContainerName IS NULL)
        INSERT INTO #IssuedHierarchy(RecurseLevel,TopLevelContainerName,ToContainerName,FromContainerName,Qty,QtyUOM,ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName) SELECT TheLevel, TopLevelContainer, ToContainerName, FromContainerName,Qty,QtyUOM,ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName FROM n
   END
   
   WHILE (@vFetchStatus = 0 )
   BEGIN
      -- Based on the base results from IssueActualsHistory, get the rest of the information
      -- * UOM
      -- * ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily
      -- * Customer
      -- * TxnDate of the issue, EmployeeName, WorklowStep of issue
      -- Get UOM information
      SET @vUOMName = ''
      IF (@vUOMId IS NOT NULL)
         SELECT @vUOMName=UOMName
         FROM UOM
         WHERE UOMId = @vUOMId
      
      -- Get Product information
      SELECT @vProductName=pb.ProductName, @vProductRevision=p.ProductRevision, @vProductDescription=p.Description, @vProductType=pt.ProductTypeName, @vProductFamily=pf.ProductFamilyName
      FROM Product p LEFT OUTER JOIN ProductFamily pf ON p.ProductFamilyId = pf.ProductFamilyId, ProductBase pb, ProductType pt
      WHERE p.ProductId = @vProductId
      AND pb.ProductBaseId = p.ProductBaseId
      AND pt.ProductTypeId = p.ProductTypeId
      
      -- Get HistoryMainline information
      SELECT @vTxnDate=hml.TxnDate, @vTxnDateGMT=hml.TxnDateGMT, @vEmployeeName=e.EmployeeName, @vWorkflowStepName=wfs.WorkflowStepName
      FROM HistoryMainline hml, Employee e, WorkflowStep wfs
      WHERE hml.TxnId=@vTxnId
      AND e.EmployeeId = hml.EmployeeId
      AND hml.WorkflowStepId = wfs.WorkflowStepId

      -- Get ToContainer information
      SELECT @vCustomerName = cust.CustomerName
            ,@vToContainerName = c.ContainerName
      FROM Container c
      LEFT OUTER JOIN Customer cust ON cust.CustomerId = c.CustomerId
      WHERE c.ContainerId = @vToContainerId     

      SET @vFromContainerName = @pLotOrContainerName
      
      SET @vNewRecursion = @pRecursion+1
      INSERT INTO #IssuedHierarchy(FromContainerName,ToContainerName,Qty,QtyUOM,ProductId,ProductName, ProductRevision, ProductDescription, ProductType, ProductFamily,TxnDate, TxnDateGMT, EmployeeName, WorkflowStepName, CustomerName) 
         VALUES (@vFromContainerName, @vToContainerName, @vQty, @vUOMName, @vProductId,@vProductName,@vProductRevision,@vProductDescription,@vProductType,@vProductFamily,@vTxnDate,@vTxnDateGMT,@vEmployeeName,@vWorkflowStepName,@vCustomerName)        
      
      EXEC csiGetIssuedHierarchy @vToContainerName, @vNewRecursion
      
      FETCH NEXT FROM @c1 INTO @vToContainerId, @vFromContainerId, @vQty, @vUOMId, @vProductId, @vTxnId  
      SET @vFetchStatus = @@FETCH_STATUS
   END
   CLOSE @c1
   DEALLOCATE @c1
   --
   IF (@pRecursion=0)
   BEGIN
      -- Clean up all of the "working" records, then execute the final return query
      DELETE FROM #IssuedHierarchy WHERE TopLevelContainerName IS NULL
      -- In some cases, UOM is not set, so pull it from the Container directly
      UPDATE #IssuedHierarchy
      SET QtyUOM = (SELECT UOMName
                    FROM Container C, UOM
                    WHERE ContainerName = FromContainerName
                    AND c.UOMId = UOM.UOMId)
      WHERE QtyUOM IS NULL OR QtyUOM = '';
      SELECT * FROM #IssuedHierarchy ORDER BY TopLevelContainerName, RecurseLevel
   END
END
GO
--EXEC csiGetIssuedHierarchy 'Container_SI34_A_A121', null
--GO
