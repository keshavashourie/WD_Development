-- csiDeleteHistory
--
-- Delete from the various container tracking and history tables based on the contents of
-- the deletehistorylimit table.  
--    
-- Dependancies:
--   Tables - DeleteHistoryLimit 
--   Data - Requires the deletehistorylimit to be populated with the appropriate 
--          container and history data.
--
-- Input Parms: None
--
-- Called by csiDeleteMain
--
-- History
--    04/16/2003     S6799 Perform commits on individual statements to avoid potential deadlocks
--    11/01/2005 PN  S8946, Replaced TxnId with HistoryMainlineId in DELETE HistoryMainline statement
--    12/04/2006 WFL S9984 Added copyright notice.
--    04/23/2007 WFL S9984 Updated copyright notice.
--

CREATE OR REPLACE PROCEDURE csiDeleteHistory 
IS
--
-- Copyright Siemens 2023  
--
BEGIN
  DELETE FROM ContainerCurrentCrossRefs WHERE ContainerID in 
         (select distinct containerid from deletehistorylimit);
  commit;

  DELETE FROM AssociateHistoryChildCnts WHERE AssociateHistoryID IN 
          (SELECT AssociateHistoryID 
 	   FROM AssociateHistory ah, deletehistorylimit dhl 	 
           WHERE ah.HistoryMainlineID = dhl.HistoryMainlineID);
  commit;

  DELETE FROM ContainerStatusChangeHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM AssociateHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsNewBooLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsNewDurLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsNewFixLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsNewFltLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsNewIntLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsNewStrLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;	

  DELETE FROM ChgAttrHistoryDetailsNewTmsLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsOldBooLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;	

  DELETE FROM ChgAttrHistoryDetailsOldDurLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsOldFixLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;	

  DELETE FROM ChgAttrHistoryDetailsOldFltLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsOldIntLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsOldStrLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryDetailsOldTmsLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryNewObjects WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryOldObjects WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgObjectListHistoryDetails WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgObjLstHistoryDetailsNewObjs WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgObjLstHistoryDetailsOldObjs WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistoryHistoryDetails WHERE HistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM CombineHistDetChildContainers WHERE CombineHistoryID IN 
          (SELECT CombineHistoryID FROM CombineHistory ch, deletehistorylimit dhl
           WHERE ch.HistoryMainlineID = dhl.HistoryMainlineID);
  commit;

  DELETE FROM CombineHistoryDetail WHERE CombineHistoryID IN 
          (SELECT CombineHistoryID FROM CombineHistory ch, deletehistorylimit dhl
           WHERE ch.HistoryMainlineID = dhl.HistoryMainlineID);
  commit; 

  DELETE FROM CombineHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM IssueActualsHistory WHERE TxnId IN 
          (select distinct txnid from deletehistorylimit);
  commit;

  DELETE FROM IssueHistoryDetail WHERE TxnId IN 
          (select distinct txnid from deletehistorylimit);
  commit;

  DELETE FROM ComponentIssueHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;
	
  DELETE FROM RemoveHistoryDetail WHERE ComponentRemoveHistoryID IN 
          (SELECT ComponentRemoveHistoryID FROM ComponentRemoveHistory c, deletehistorylimit dhl
	   WHERE c.HistoryMainlineID = dhl.HistoryMainlineID);
  commit; 
 
  DELETE FROM ComponentRemoveHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM ComponentDefectHistoryDetail WHERE DefectHistoryID IN 
          (SELECT DefectHistoryID FROM DefectHistory d, deletehistorylimit dhl
           WHERE d.HistoryMainlineID = dhl.HistoryMainlineID);
  commit;

  DELETE FROM ComponentDistributeHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM ContainerDefectHistoryDetail WHERE DefectHistoryID IN 
          (SELECT DefectHistoryID FROM DefectHistory d, deletehistorylimit dhl
           WHERE d.HistoryMainlineID = dhl.HistoryMainlineID);
  commit;

  DELETE FROM DefectHistoryHistoryDetails WHERE DefectHistoryID IN 
          (SELECT DefectHistoryID FROM DefectHistory d, deletehistorylimit dhl
           WHERE d.HistoryMainlineID = dhl.HistoryMainlineID);
  commit;

  DELETE FROM DefectHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM DisassociateHistoryChildCnts WHERE DisassociateHistoryID IN 
          (SELECT DisassociateHistoryID FROM DisassociateHistory d, deletehistorylimit dhl
           WHERE d.HistoryMainlineID = dhl.HistoryMainlineID);
  commit;
	
  DELETE FROM DisassociateHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM HistoryMainlineHistoryDetails WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM HistoryMainlineAuthorizations WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM HoldReleaseHistoryDetail WHERE HoldReleaseHistoryID IN 
          (SELECT HoldReleaseHistoryID FROM HoldReleaseHistory h, deletehistorylimit dhl
	   WHERE h.HistoryMainlineID = dhl.HistoryMainlineID);
  commit;

  DELETE FROM HoldReleaseHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM MoveHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;
			      
  DELETE FROM MoveInHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;
			      
  DELETE FROM QtyHistoryDetails WHERE QtyHistoryID IN 
          (SELECT QtyHistoryID FROM QtyHistory q, deletehistorylimit dhl
           WHERE q.HistoryMainlineID = dhl.HistoryMainlineID);
  commit;

  DELETE FROM QtyHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;
	
  DELETE FROM SplitHistoryDetails WHERE SplitHistoryID IN 
          (SELECT SplitHistoryID FROM SplitHistory s, deletehistorylimit dhl
	   WHERE s.HistoryMainlineID = dhl.HistoryMainlineID);
  commit;
 
  DELETE FROM SplitHistory WHERE HistoryMainlineID  in 
         (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM StartHistoryDtlWorkflowStack WHERE StartHistoryDetailId  in 
         (select distinct StartHistoryDetailId from StartHistoryDetail s, deletehistorylimit dhl
          where s.HistoryMainlineID=dhl.HistoryMainlineID);
  commit;

  DELETE FROM StartHistoryDetail WHERE HistoryMainlineID  in 
         (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM ThruputHistoryDetail WHERE ThruputHistoryID IN 
          (SELECT ThruputHistoryID FROM ThruputHistory, deletehistorylimit dhl
           WHERE ThruputHistory.HistoryMainlineID = dhl.HistoryMainlineID);
  commit;
 
  DELETE FROM ThruputHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM SerializeHistoryDetails WHERE SerializeHistoryId in 
          (select distinct SerializeHistoryId from SerializeHistory s, deletehistorylimit dhl
           where s.TxnId = dhl.TxnId);
  commit;

  DELETE FROM SerializeHistory WHERE TxnId in 
          (select distinct TxnId from deletehistorylimit);
  commit;

  DELETE FROM ProcessedTxnGUID WHERE TxnId in 
          (select distinct TxnId from deletehistorylimit);
  commit;

  DELETE FROM WIPMsgHistoryDetail WHERE TxnId in 
          (select distinct TxnId from deletehistorylimit);
  commit;

  DELETE FROM ContainerRI WHERE TxnID in 
          (select distinct TxnID from deletehistorylimit);
  commit;
 
  DELETE FROM CurrentStatusRI WHERE TxnID in 
          (select distinct TxnID from deletehistorylimit);
  commit;

  DELETE FROM ReworkStatusReEntryWorkflowStk where ReworkStsReEntryWorkflowStkId  in 
         (select distinct r.reworkstatusid from ReworkStatus r, deletehistorylimit dhl, container c
          where r.CurrentStatusID = c.CurrentStatusID and dhl.containerid = c.containerid);
  commit;

  DELETE FROM ReworkStatus WHERE CurrentStatusID in 
         (select distinct c.CurrentStatusID from deletehistorylimit dhl, container c
          where dhl.containerid = c.containerid);
  commit;

  DELETE FROM ReworkStatusRI WHERE TxnID in 
          (select distinct TxnID from deletehistorylimit);
  commit;

  DELETE FROM StepPassCount WHERE CurrentStatusID in 
         (select distinct c.CurrentStatusID from deletehistorylimit dhl, container c
          where dhl.containerid = c.containerid);
  commit;

  DELETE FROM StepPassCountRI WHERE TxnID in 
          (select distinct TxnID from deletehistorylimit);
  commit;

  DELETE FROM CurrentStatusWorkflowStack WHERE CurrentStatusID in 
         (select distinct c.CurrentStatusID from deletehistorylimit dhl, container c
          where dhl.containerid = c.containerid);
  commit;

  DELETE FROM HistoryCrossRefRI WHERE TxnID in 
          (select distinct TxnID from deletehistorylimit);
  commit;

  DELETE FROM TransRevOids WHERE TxnID in 
          (select distinct TxnID from deletehistorylimit);
  commit;

-- Modified By Jagadesh To include 2 tables

  DELETE FROM ContainerDetailRI WHERE TxnID in 
          (select distinct TxnID from deletehistorylimit);
  commit;

  DELETE FROM ContainerDetail WHERE ContainerID in 
          (select distinct ContainerID from deletehistorylimit);
  commit;

  -- 
  -- Finally delete tables used in filtering and/or staging purge candidates
  -- (Container, HistoryMainline, CurrentStatus, HistoryCrossRef, ChgAttrHistory
  --  ChgAttrHistoryDetails)
  -- 
  DELETE FROM ChgAttrHistoryDetails WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from deletehistorylimit);
  commit;

  DELETE FROM ChgAttrHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from deletehistorylimit);
  commit;

  DELETE FROM CurrentStatus WHERE CurrentStatusID in 
         (select distinct c.CurrentStatusID from deletehistorylimit dhl, container c
          where dhl.containerid = c.containerid);
  commit;

  DELETE FROM HistoryMainline WHERE HistoryMainlineId in 
         (select distinct HistoryMainlineId from deletehistorylimit);
  commit;

  DELETE FROM Container WHERE ContainerID in 
         (select distinct containerid from deletehistorylimit);
  commit;

  DELETE FROM HistoryCrossRef WHERE TrackingID in 
          (select distinct containerid from deletehistorylimit);
  commit;

  --  Allow SQL errors codes to propagate to the calling program
  --  exception
  --    when others then
  --      -- DBMS_OUTPUT.put_line(tableowner||' '||tablename);
  --      DBMS_OUTPUT.put_line(SQLERRM(SQLCODE)|| ' TABLE: ' || tab_name);
  --      rollback;

end csiDeleteHistory;
/

