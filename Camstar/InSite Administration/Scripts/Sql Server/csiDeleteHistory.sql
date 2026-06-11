----------------------------------------------------------------------------------------
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
-- Modification History:
--	Name				Date		Action
--	----------------	----------	---------------------------------------------------
--  Allan O				04/16/2003	- Allow/perform commits on individual statements
--									  to avoid potential deadlocks
--  Jagadesh S			05/10/2004	- Modified the SP to use a Temp table to avoid Deadlocks
-- 	Purushotham Neel	11/01/2005 	- Replaced TxnId with HistoryMainlineId in DELETE 
--									  HistoryMainline statement. (SPR - S9307)
--  Bill Lippard        12/04/2006  - Added copyright notice (SPR S9984).
--  Bill Lippard        04/23/2007  - Updated copyright notice (SPR S9984).
----------------------------------------------------------------------------------------


IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiDeleteHistory' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiDeleteHistory
GO


CREATE PROCEDURE csiDeleteHistory 
AS
--
-- Copyright Siemens 2023  
--
declare	
  @procstatus                   Int,
  @ErrorStatus			Int

BEGIN
  set nocount on
  -- Allow autocommit for all tables except those use for filtering/staging the delete
  -- candidates.  Meved the begin tran to the last section
  -- Begin Tran deleteHistory
  Set @ErrorStatus = 0

-- 2004/03/04 Fric Appended --
  declare @tmp_limit 
    TABLE(ContainerId        CHAR(16), 
          HistoryMainlineID  CHAR(16), 
          HistoryID          CHAR(16), 
          TxnId              CHAR(16), 
          ChgAttrHistoryDetailsID CHAR(16))

  INSERT INTO  @tmp_limit select * from deletehistorylimit 
--  Execute fricDeleteCustomTables

-- change deletehistorylimit ---> @tmp_limit

  DELETE FROM ContainerCurrentCrossRefs WHERE ContainerID in 
         (select distinct containerid from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM AssociateHistoryChildCnts WHERE AssociateHistoryID IN 
          (SELECT AssociateHistoryID 
 	   FROM AssociateHistory ah, @tmp_limit dhl 	 
           WHERE ah.HistoryMainlineID = dhl.HistoryMainlineID)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ContainerStatusChangeHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM AssociateHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsNewBooLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsNewDurLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsNewFixLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsNewFltLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsNewIntLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsNewStrLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)	
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsNewTmsLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsOldBooLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)	
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsOldDurLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsOldFixLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)	
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsOldFltLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsOldIntLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsOldStrLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryDetailsOldTmsLst WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryNewObjects WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryOldObjects WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgObjectListHistoryDetails WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgObjLstHistoryDetailsNewObjs WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgObjLstHistoryDetailsOldObjs WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistoryHistoryDetails WHERE HistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM CombineHistDetChildContainers WHERE CombineHistoryID IN 
          (SELECT CombineHistoryID FROM CombineHistory ch, @tmp_limit dhl
           WHERE ch.HistoryMainlineID = dhl.HistoryMainlineID) 
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM CombineHistoryDetail WHERE CombineHistoryID IN 
          (SELECT CombineHistoryID FROM CombineHistory ch, @tmp_limit dhl
           WHERE ch.HistoryMainlineID = dhl.HistoryMainlineID) 
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM CombineHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM IssueActualsHistory WHERE TxnId IN 
          (select distinct txnid from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM IssueHistoryDetail WHERE TxnId IN 
          (select distinct txnid from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ComponentIssueHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error
	
  DELETE FROM RemoveHistoryDetail WHERE ComponentRemoveHistoryID IN 
          (SELECT ComponentRemoveHistoryID FROM ComponentRemoveHistory c, @tmp_limit dhl
	   WHERE c.HistoryMainlineID = dhl.HistoryMainlineID) 
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error
 
  DELETE FROM ComponentRemoveHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ComponentDefectHistoryDetail WHERE DefectHistoryID IN 
          (SELECT DefectHistoryID FROM DefectHistory d, @tmp_limit dhl
           WHERE d.HistoryMainlineID = dhl.HistoryMainlineID)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ComponentDistributeHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ContainerDefectHistoryDetail WHERE DefectHistoryID IN 
          (SELECT DefectHistoryID FROM DefectHistory d, @tmp_limit dhl
           WHERE d.HistoryMainlineID = dhl.HistoryMainlineID)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM DefectHistoryHistoryDetails WHERE DefectHistoryID IN 
          (SELECT DefectHistoryID FROM DefectHistory d, @tmp_limit dhl
           WHERE d.HistoryMainlineID = dhl.HistoryMainlineID)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM DefectHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM DisassociateHistoryChildCnts WHERE DisassociateHistoryID IN 
          (SELECT DisassociateHistoryID FROM DisassociateHistory d, @tmp_limit dhl
           WHERE d.HistoryMainlineID = dhl.HistoryMainlineID)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error
	
  DELETE FROM DisassociateHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM HistoryMainlineHistoryDetails WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM HistoryMainlineAuthorizations WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM HoldReleaseHistoryDetail WHERE HoldReleaseHistoryID IN 
          (SELECT HoldReleaseHistoryID FROM HoldReleaseHistory h, @tmp_limit dhl
	   WHERE h.HistoryMainlineID = dhl.HistoryMainlineID)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM HoldReleaseHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM MoveHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error
			      
  DELETE FROM MoveInHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error
			      
  DELETE FROM QtyHistoryDetails WHERE QtyHistoryID IN 
          (SELECT QtyHistoryID FROM QtyHistory q, @tmp_limit dhl
           WHERE q.HistoryMainlineID = dhl.HistoryMainlineID)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM QtyHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error
 
  DELETE FROM SplitHistoryDetails WHERE SplitHistoryID IN 
          (SELECT SplitHistoryID FROM SplitHistory s, @tmp_limit dhl
	   WHERE s.HistoryMainlineID = dhl.HistoryMainlineID)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error
 
  DELETE FROM SplitHistory WHERE HistoryMainlineID  in 
         (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM StartHistoryDtlWorkflowStack WHERE StartHistoryDetailId  in 
         (select distinct StartHistoryDetailId from StartHistoryDetail s, @tmp_limit dhl
          where s.HistoryMainlineID=dhl.HistoryMainlineID)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM StartHistoryDetail WHERE HistoryMainlineID  in 
         (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ThruputHistoryDetail WHERE ThruputHistoryID IN 
          (SELECT ThruputHistoryID FROM ThruputHistory, @tmp_limit dhl
           WHERE ThruputHistory.HistoryMainlineID = dhl.HistoryMainlineID)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error
 
  DELETE FROM ThruputHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM SerializeHistoryDetails WHERE SerializeHistoryId in 
          (select distinct SerializeHistoryId from SerializeHistory s, @tmp_limit dhl
           where s.TxnId = dhl.TxnId)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM SerializeHistory WHERE TxnId in 
          (select distinct TxnId from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ProcessedTxnGUID WHERE TxnId in 
          (select distinct TxnId from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM WIPMsgHistoryDetail WHERE TxnId in 
          (select distinct TxnId from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ContainerRI WHERE TxnID in 
          (select distinct TxnID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error
 
  DELETE FROM CurrentStatusRI WHERE TxnID in 
          (select distinct TxnID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ReworkStatusReEntryWorkflowStk where ReworkStsReEntryWorkflowStkId  in 
         (select distinct r.reworkstatusid from ReworkStatus r, @tmp_limit dhl, container c
          where r.CurrentStatusID = c.CurrentStatusID and dhl.containerid = c.containerid)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ReworkStatus WHERE CurrentStatusID in 
         (select distinct c.CurrentStatusID from @tmp_limit dhl, container c
          where dhl.containerid = c.containerid)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ReworkStatusRI WHERE TxnID in 
          (select distinct TxnID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM StepPassCount WHERE CurrentStatusID in 
         (select distinct c.CurrentStatusID from @tmp_limit dhl, container c
          where dhl.containerid = c.containerid)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM StepPassCountRI WHERE TxnID in 
          (select distinct TxnID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM CurrentStatusWorkflowStack WHERE CurrentStatusID in 
         (select distinct c.CurrentStatusID from @tmp_limit dhl, container c
          where dhl.containerid = c.containerid)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM HistoryCrossRefRI WHERE TxnID in 
          (select distinct TxnID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM TransRevOids WHERE TxnID in 
          (select distinct TxnID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

-- Modified By Jagadesh To include 2 tables

  DELETE FROM ContainerDetailRI WHERE TxnID in 
          (select distinct TxnID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ContainerDetail WHERE ContainerID in 
          (select distinct ContainerID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error


  -- 
  -- Finally delete tables used in filtering and/or staging purge candidates
  -- (Container, HistoryMainline, CurrentStatus, HistoryCrossRef, ChgAttrHistory
  --  ChgAttrHistoryDetails)
  -- 
  Begin Tran deleteHistory

  DELETE FROM ChgAttrHistoryDetails WHERE ChgAttrHistoryDetailsID in 
          (select distinct ChgAttrHistoryDetailsID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM ChgAttrHistory WHERE HistoryMainlineID in 
          (select distinct HistoryMainlineID from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM CurrentStatus WHERE CurrentStatusID in 
         (select distinct c.CurrentStatusID from @tmp_limit dhl, container c
          where dhl.containerid = c.containerid)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM HistoryMainline WHERE HistoryMainlineId in 
             (select distinct HistoryMainlineId from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM Container WHERE ContainerID in 
         (select distinct containerid from @tmp_limit)
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error

  DELETE FROM HistoryCrossRef  WHERE TrackingID in 
          (select distinct containerid from @tmp_limit) 
  IF @@ERROR <> 0 Select @ErrorStatus = @@Error


-- 2004/03/04 Fric Appended --
  DELETE FROM @tmp_limit   
------------------------------

  IF @ErrorStatus <> 0 
    BEGIN
      PRINT 'Error in csiDeleteOldTrackingData '
      RAISERROR('Error in csiDeleteOldTrackingData', 10,1)
      ROLLBACK TRAN
      RETURN 1
    END
  ELSE
    BEGIN
      -- print 'commit'
      COMMIT TRAN deleteHistory
    END
END






GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

