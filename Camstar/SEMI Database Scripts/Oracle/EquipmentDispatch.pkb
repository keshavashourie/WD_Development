CREATE OR REPLACE PACKAGE BODY EquipmentDispatch AS
--===========================================================================
-- © 2017 Siemens Product Lifecycle Management Software Inc. 
-- Author:		Andy Murphi
-- Date:		06 Jan 2012
--===========================================================================
-- Description:
--         EquipmentDispatch package body.
-- Return Data:
--         For Success: 
--         For Failure: 
--===========================================================================

--=====================
-- Package information
--=====================

---##########################################################################---

FUNCTION csiGetResourcesList(resgroup VARCHAR2)
RETURN RETEQLIST_TABLE
IS
    reteq_tab RETEQLIST_TABLE := RETEQLIST_TABLE();
    n INTEGER := 0;
    EQid VARCHAR2(20);
    
    CURSOR curEQ
    IS
      SELECT
          RGG.GroupsId
      FROM
          ResourceGroup RG
          INNER JOIN ResourceGroupGroups RGG ON RG.ResourceGroupId = RGG.ResourceGroupId
      WHERE
          (RG.ResourceGroupId = resgroup);
                        
BEGIN
    FOR r IN (SELECT RD.ResourceName
              FROM ResourceGroup RG
                    LEFT OUTER JOIN  ResourceGroupEntries RGE ON RG.ResourceGroupId = RGE.ResourceGroupId
                    LEFT OUTER JOIN  ResourceDef RD ON RGE.EntriesId = RD.ResourceId
              WHERE (RG.ResourceGroupId = resgroup))
    LOOP
        reteq_tab.extend;
        n := n + 1; 
        reteq_tab(n) := RETEQLIST(r.ResourceName); 
    END LOOP;
    
    OPEN curEQ;
    LOOP
    
      FETCH curEQ INTO EQid;
      EXIT WHEN CurEQ%NOTFOUND;
      BEGIN
          FOR r IN (SELECT * FROM TABLE(csiGetResourcesList(EQid)))
          LOOP
              reteq_tab.extend;
              n := n + 1; 
              reteq_tab(n) := RETEQLIST(r.EquipmentName); 
          END LOOP; 
      END;
    END LOOP;
    CLOSE curEQ;
    RETURN reteq_tab; 
END;

---##########################################################################---

FUNCTION csiEquipmentDispatch(equipment VARCHAR2, specnamevar VARCHAR2, specrev VARCHAR2)
RETURN RETWIPLOT_TABLE
IS
    
    retwiplot_tab RETWIPLOT_TABLE := RETWIPLOT_TABLE();
    tmpwiplotname_tab tmpWIPLotName_TABLE := tmpWIPLotName_TABLE();
    n INTEGER := 0;
    m INTEGER := 0;
    mm INTEGER := 0;
    x INTEGER := 0;
    
    useequipmentmatrix VARCHAR2(20);
    specidvar VARCHAR2(40);
    eqmlotname VARCHAR2(40);
    eqmproductid VARCHAR2(40);
    eqmprocessspecid VARCHAR2(40);
    eqmproductlineid VARCHAR2(40);
    eqmownerid VARCHAR2(40);
    eqmspecid VARCHAR2(40);
    tempproductid VARCHAR2(40);
    tempprocessspecid VARCHAR2(40);
    tempproductlineid VARCHAR2(40);
    tempownerid VARCHAR2(40);
    tempspecid VARCHAR2(40);
    insertstatus INTEGER := 0;
    
    CURSOR eqm
    IS
        SELECT
            S.UseEquipmentMatrix,
            SB.RevOfRcdId
        FROM
            SpecBase SB
            LEFT OUTER JOIN Spec S ON SB.SpecBaseId = S.SpecBaseId
        WHERE
            SB.SpecName = specnamevar;
                
    CURSOR eqm2
    IS
        SELECT
            S.UseEquipmentMatrix,
            S.SpecId
        FROM
            SpecBase SB
            LEFT OUTER JOIN Spec S ON SB.SpecBaseId = S.SpecBaseId
        WHERE
            SB.SpecName = specnamevar AND S.SpecRevision = specrev;
    
    CURSOR eqmdata
    IS
        SELECT
            C.ContainerName
            , C.ProductId
            , WL.ProcessSpecId
            , P.ProductLineId
            , C.OwnerId
            , WL.SpecId
        FROM
            Container C
            INNER JOIN A_WIPLot WL ON C.ContainerId = WL.ContainerId
            INNER JOIN A_WIPLotDetails WLD ON WL.WIPLotId = WLD.WIPLotId
            INNER JOIN Product P ON C.ProductId = P.ProductId
        WHERE 
            WL.SpecId = specidvar
            AND (WL.WIPType = 'TRACK')
            AND (WL.IsCompleted = 0)
            AND (WLD.ProcessStatus = 'ACTIVE')
        ORDER BY
            C.ProductId
            , WL.ProcessSpecId
            , P.ProductLineId
            , C.OwnerId
            , WL.SpecId;
            
BEGIN
    
    IF specrev IS NULL THEN
        OPEN eqm;
        FETCH eqm INTO useequipmentmatrix, specidvar;
        CLOSE eqm;
    ELSE
        OPEN eqm2;
        FETCH eqm2 INTO useequipmentmatrix, specidvar;
        CLOSE eqm2;
    END IF;
			
    IF useequipmentmatrix = 0 THEN
        BEGIN
            FOR r IN (SELECT
                          C.ContainerName "LOT",
                          C.Qty "QTY",
                          C.Qty2 "QTY2",
                          SB.SpecName || ':' || S.SpecRevision "SPEC",
                          TO_CHAR(SYSDATE - NVL(C.MoveInTimestamp, SYSDATE), '99990.999') "DAYSHERE",
                          CASE WHEN WL2.WIPLotId IS NULL THEN TO_CHAR(SYSDATE - C.LastMoveOutTimestamp, '99990.999') ELSE TO_CHAR(WL2.CreationTimestamp - C.LastMoveOutTimestamp, '99990.999') END "QUEUETIME",
                          PB.ProductName || ':' || P.ProductRevision "PRODUCT",
                          CASE WHEN C.Status = 1 THEN 'ACTIVE' WHEN C.Status = 2 THEN 'TERMINATED' WHEN C.Status = 5 THEN 'SHIPPED' ELSE TO_CHAR(C.Status) END "STATUS",
                          CASE WHEN NVL(C.CurrentHoldCount, 0) > 0 THEN 'YES ' || TO_CHAR(SYSDATE - NVL(C.OnHoldDate, SYSDATE), '99990.999') || ' Days' ELSE 'NO' END "ISONHOLD",
                          CASE WHEN NVL(CS.InRework, 0) > 0 THEN 'YES Loop ' || TO_CHAR(CS.ReworkLoopCount) || ' Total ' || TO_CHAR(CS.ReworkTotalCount) ELSE 'NO' END "INREWORK",
                          C.InsertionNumber "INSERTION",
                          WL1.ProcessSpecName || ':' || WL1.ProcessSpecRevision AS "INSERTIONPROCESSSPEC",
                          CASE WHEN WL1.SSName IS NULL THEN '' ELSE WL1.SSName || ':' || WL1.SSRevision END AS "INSERTIONSS",
                          (NVL(WL1.TotalRejectQty, 0) + NVL(WL1.TotalRejectBinsQty, 0) - NVL(WL1.TotalBonusBackRejectQty, 0)) AS "REJECTS",
                          (C.Qty - NVL(WL1.TotalRejectQty, 0) - NVL(WL1.TotalRejectBinsQty, 0) - NVL(WL1.TotalBonusBackRejectQty, 0)) AS "MOVEOUTQTY",
                          WL1.WIPStatus AS "WIPSTATUS",
                          WL1.WIPType AS "WIPTYPE",
                          WL1.WIPYieldResult AS "WIPYIELDRESULT",
                          C.BatchNo "BATCHNO",
                          C.EquipmentCount "EQPCOUNT",
                          C.EquipmentLoadingCount "EQPLOADINGCOUNT",
                          CASE WHEN NVL(C.FutureHoldCount, 0) > 0 THEN 'YES ' || TO_CHAR(C.FutureHoldCount) ELSE 'NO' END "FUTUREHOLDEXISTS",
                          CASE WHEN C.ScheduleDataId IS NOT NULL OR NVL(C.ScheduleCount, 0) > 0 THEN 'YES' ELSE 'NO' END "INSCHEDULE",
                          CS.CurrentSpecPass "SPECPASS",
                          S.ObjectCategory "SPECCATEGORY",
                          S.ObjectType "SPECTYPE",
                          S.Description "SPECDESCRIPTION"
                      FROM
                          Container C     
                          INNER JOIN CurrentStatus CS ON C.CurrentStatusId = CS.CurrentStatusId     
                          INNER JOIN Product P ON C.ProductId = P.ProductId    
                          INNER JOIN ProductBase PB ON P.ProductBaseId = PB.ProductBaseId    
                          INNER JOIN Spec S ON CS.SpecId = S.SpecId    
                          INNER JOIN SpecBase SB ON S.SpecBaseId = SB.SpecBaseId     
                          LEFT OUTER JOIN A_WIPLot WL1 ON C.CurrentWIPLotId = WL1.WIPLotId     
                          LEFT OUTER JOIN A_WIPLot WL2 ON C.ContainerId = WL2.ContainerId AND WL2.InsertionNumber = 1
                          LEFT OUTER JOIN A_WIPLotDetails WLD ON WL1.WipLotId = WLD.WipLotId 
                      WHERE
                          (WL1.WIPType = 'TRACK')
                          AND (WL1.IsCompleted = 0)
                          AND (WLD.ProcessStatus = 'ACTIVE')
                          AND (S.SpecId = specidvar)
                          AND (equipment IN (SELECT * FROM TABLE(csiGetResourcesList(S.ResourceGroupId)))))
            LOOP
                retwiplot_tab.extend;
                n := n + 1; 
                retwiplot_tab(n) := RETWIPLOT(r.LOT,
                                              r.QTY,
                                              r.QTY2,
                                              r.SPEC,
                                              r.DAYSHERE,
                                              r.QUEUETIME,
                                              r.PRODUCT,
                                              r.STATUS,
                                              r.ISONHOLD,
                                              r.INREWORK,
                                              r.INSERTION,
                                              r.INSERTIONPROCESSSPEC,
                                              r.INSERTIONSS,
                                              r.REJECTS,
                                              r.MOVEOUTQTY,
                                              r.WIPSTATUS,
                                              r.WIPTYPE,
                                              r.WIPYIELDRESULT,
                                              r.BATCHNO,
                                              r.EQPCOUNT,
                                              r.EQPLOADINGCOUNT,
                                              r.FUTUREHOLDEXISTS,
                                              r.INSCHEDULE,
                                              r.SPECPASS,
                                              r.SPECCATEGORY,
                                              r.SPECTYPE,
                                              r.SPECDESCRIPTION);
            END LOOP;
        END;
    ELSE
        BEGIN	
            OPEN eqmdata;
            FETCH eqmdata INTO eqmlotname, eqmproductid, eqmprocessspecid, eqmproductlineid, eqmownerid, eqmspecid;
            LOOP
                EXIT WHEN eqmdata%NOTFOUND;
 
                IF ((eqmproductid = tempproductid) AND (eqmprocessspecid = tempprocessspecid) AND (eqmproductlineid = tempproductlineid) AND (eqmownerid = tempownerid) AND (eqmspecid = tempspecid)) THEN
                    BEGIN
                        IF (insertstatus = 1) THEN
                            BEGIN                      
                                tmpwiplotname_tab.extend;
                                m := m + 1; 
                                tmpwiplotname_tab(m) := tmpWIPLotName(eqmlotname); 
                                mm := m;
                                FETCH eqmdata INTO eqmlotname, eqmproductid, eqmprocessspecid, eqmproductlineid, eqmownerid, eqmspecid;
                            END;
                        ELSE
                            FETCH eqmdata INTO eqmlotname, eqmproductid, eqmprocessspecid, eqmproductlineid, eqmownerid, eqmspecid;
                        END IF;
                    END;
                ELSE
                    BEGIN
                        FOR r IN (SELECT
                                      eqmlotname AS Lot
                                  FROM
                                  (
                                      SELECT
                                          EM.EquipmentGroupId AS EquipmentGroupId
                                          , CAST(ROW_NUMBER() OVER (ORDER BY CASE WHEN EM.ProductId IS NULL THEN 0 ELSE 1 END DESC
                                          , CASE WHEN EM.ProcessSpecId IS NULL THEN 0 ELSE 1 END DESC
                                          , CASE WHEN EM.ProductLineId IS NULL THEN 0 ELSE 1 END DESC
                                          , CASE WHEN EM.OwnerId IS NULL THEN 0 ELSE 1 END DESC
                                          , CASE WHEN EM.SpecId IS NULL THEN 0 ELSE 1 END DESC) AS INT) RN
                                      FROM
                                          A_EquipmentMatrix EM
                                          LEFT OUTER JOIN  Product P ON EM.ProductId = P.ProductId
                                          LEFT OUTER JOIN  ProductBase PB ON P.ProductBaseId = PB.ProductBaseId
                                          LEFT OUTER JOIN  A_ProcessSpec PS ON EM.ProcessSpecId = PS.ProcessSpecId
                                          LEFT OUTER JOIN  A_ProcessSpecBase PSB ON PS.ProcessSpecBaseId = PSB.ProcessSpecBaseId
                                          LEFT OUTER JOIN  A_ProductLine PL ON EM.ProductLineId = PL.ProductLineId
                                          LEFT OUTER JOIN  Owner O ON EM.OwnerId = O.OwnerId
                                          LEFT OUTER JOIN  Spec S ON EM.SpecId = S.SpecId
                                          LEFT OUTER JOIN  SpecBase SB ON S.SpecBaseId = SB.SpecBaseId
                                      WHERE
                                          (EM.ProductId = eqmproductid OR EM.ProductId IS NULL)
                                          AND (EM.ProcessSpecId = eqmprocessspecid OR EM.ProcessSpecId IS NULL)
                                          AND (EM.ProductLineId = eqmproductlineid OR EM.ProductLineId IS NULL)
                                          AND (EM.OwnerId = eqmownerid OR EM.OwnerId IS NULL)
                                          AND (EM.SpecId = eqmspecid OR EM.SpecId IS NULL)
                                  ) DesignerAdvancedQuery
                                  WHERE
                                      (RN = 1)
                                      AND (equipment IN (SELECT * FROM TABLE(csiGetResourcesList(EquipmentGroupId)))))
                        LOOP
                            tmpwiplotname_tab.extend;
                            m := m + 1; 
                            tmpwiplotname_tab(m) := tmpWIPLotName(r.Lot);
                        END LOOP;
                            	
                        IF m = mm THEN
                            insertstatus := 0;
                        ELSE
                            insertstatus := 1;
                        END IF;
                            
                        mm := m;
                        tempproductid := eqmproductid;
                        tempprocessspecid := eqmprocessspecid;
                        tempproductlineid := eqmproductlineid;
                        tempownerid := eqmownerid;
                        tempspecid := eqmspecid;
				
                        FETCH eqmdata INTO eqmlotname, eqmproductid, eqmprocessspecid, eqmproductlineid, eqmownerid, eqmspecid;
                    END;
                END IF;
            END LOOP;
            CLOSE eqmdata;
                
            FOR r IN (SELECT 
                          C.ContainerName "LOT",
                          C.Qty "QTY",
                          C.Qty2 "QTY2",
                          SB.SpecName || ':' || S.SpecRevision "SPEC",
                          TO_CHAR(SYSDATE - NVL(C.MoveInTimestamp, SYSDATE), '99990.999') "DAYSHERE",
                          CASE WHEN WL2.WIPLotId IS NULL THEN TO_CHAR(SYSDATE - C.LastMoveOutTimestamp, '99990.999') ELSE TO_CHAR(WL2.CreationTimestamp - C.LastMoveOutTimestamp, '99990.999') END "QUEUETIME",
                          PB.ProductName || ':' || P.ProductRevision "PRODUCT",
                          CASE WHEN C.Status = 1 THEN 'ACTIVE' WHEN C.Status = 2 THEN 'TERMINATED' WHEN C.Status = 5 THEN 'SHIPPED' ELSE TO_CHAR(C.Status) END "STATUS",
                          CASE WHEN NVL(C.CurrentHoldCount, 0) > 0 THEN 'YES ' || TO_CHAR(SYSDATE - NVL(C.OnHoldDate, SYSDATE), '99990.999') || ' Days' ELSE 'NO' END "ISONHOLD",
                          CASE WHEN NVL(CS.InRework, 0) > 0 THEN 'YES Loop ' || TO_CHAR(CS.ReworkLoopCount) || ' Total ' || TO_CHAR(CS.ReworkTotalCount) ELSE 'NO' END "INREWORK",
                          C.InsertionNumber "INSERTION",
                          WL1.ProcessSpecName || ':' || WL1.ProcessSpecRevision AS "INSERTIONPROCESSSPEC",
                          CASE WHEN WL1.SSName IS NULL THEN '' ELSE WL1.SSName || ':' || WL1.SSRevision END AS "INSERTIONSS",
                          (NVL(WL1.TotalRejectQty, 0) + NVL(WL1.TotalRejectBinsQty, 0) - NVL(WL1.TotalBonusBackRejectQty, 0)) AS "REJECTS",
                          (C.Qty - NVL(WL1.TotalRejectQty, 0) - NVL(WL1.TotalRejectBinsQty, 0) - NVL(WL1.TotalBonusBackRejectQty, 0)) AS "MOVEOUTQTY",
                          WL1.WIPStatus AS "WIPSTATUS",
                          WL1.WIPType AS "WIPTYPE",
                          WL1.WIPYieldResult AS "WIPYIELDRESULT",
                          C.BatchNo "BATCHNO",
                          C.EquipmentCount "EQPCOUNT",
                          C.EquipmentLoadingCount "EQPLOADINGCOUNT",
                          CASE WHEN NVL(C.FutureHoldCount, 0) > 0 THEN 'YES ' || TO_CHAR(C.FutureHoldCount) ELSE 'NO' END "FUTUREHOLDEXISTS",
                          CASE WHEN C.ScheduleDataId IS NOT NULL OR NVL(C.ScheduleCount, 0) > 0 THEN 'YES' ELSE 'NO' END "INSCHEDULE",
                          CS.CurrentSpecPass "SPECPASS",
                          S.ObjectCategory "SPECCATEGORY",
                          S.ObjectType "SPECTYPE",
                          S.Description "SPECDESCRIPTION"
                      FROM Container C     
                          INNER JOIN CurrentStatus CS ON C.CurrentStatusId = CS.CurrentStatusId     
                          INNER JOIN Product P ON C.ProductId = P.ProductId    
                          INNER JOIN ProductBase PB ON P.ProductBaseId = PB.ProductBaseId    
                          INNER JOIN Spec S ON CS.SpecId = S.SpecId    
                          INNER JOIN SpecBase SB ON S.SpecBaseId = SB.SpecBaseId     
                          LEFT OUTER JOIN A_WIPLot WL1 ON C.CurrentWIPLotId = WL1.WIPLotId     
                          LEFT OUTER JOIN A_WIPLot WL2 ON C.ContainerId = WL2.ContainerId AND WL2.InsertionNumber = 1
                          LEFT OUTER JOIN A_WIPLotDetails WLD ON WL1.WipLotId = WLD.WipLotId 
                      WHERE (C.ContainerName IN (SELECT Lot FROM TABLE(tmpwiplotname_tab))))
            LOOP
                retwiplot_tab.extend;
                n := n + 1; 
                retwiplot_tab(n) := RETWIPLOT(r.LOT,
                                              r.QTY,
                                              r.QTY2,
                                              r.SPEC,
                                              r.DAYSHERE,
                                              r.QUEUETIME,
                                              r.PRODUCT,
                                              r.STATUS,
                                              r.ISONHOLD,
                                              r.INREWORK,
                                              r.INSERTION,
                                              r.INSERTIONPROCESSSPEC,
                                              r.INSERTIONSS,
                                              r.REJECTS,
                                              r.MOVEOUTQTY,
                                              r.WIPSTATUS,
                                              r.WIPTYPE,
                                              r.WIPYIELDRESULT,
                                              r.BATCHNO,
                                              r.EQPCOUNT,
                                              r.EQPLOADINGCOUNT,
                                              r.FUTUREHOLDEXISTS,
                                              r.INSCHEDULE,
                                              r.SPECPASS,
                                              r.SPECCATEGORY,
                                              r.SPECTYPE,
                                              r.SPECDESCRIPTION); 
            END LOOP;		
        END;
    END IF;
    RETURN retwiplot_tab;
END;

--===========================================================================
-- End of package body
--===========================================================================
END;
/
show errors;
