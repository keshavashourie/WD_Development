--===========================================================================
-- © 2017 Siemens Product Lifecycle Management Software Inc. 
-- Author:		Siew Pai Oak
-- Release Date:	11 December 2006
-- Release Version:	4.2.0001
--===========================================================================
-- Description:
--         Views for enhanced lot history.
-----------------------------------------------------------------------------
-- Release Notes for 4.2.0001 (11 December 2006):
-----------------------------------------------------------------------------
-- 1)	First release version.
--===========================================================================

CREATE OR REPLACE VIEW V_ENHANCEDLOTHISTORY
AS
SELECT
    B.CONTAINERNAME "LOTID"
    , A.CDOName || DECODE(A.CallByCDOName, A.CDOName, '', ' (' || A.CallByCDOName || ')') "SERVICE"
    , TO_CHAR(A.TxnDate, 'YYYY-MM-DD hh24:mi:ss') "TXNDATE"
    , A.Username "USER"
    , A.FromSpecName "FROMSTEP"
    , A.FromQty      "FROMQTY"
    , A.FromQty2     "FROMQTY2"
    , A.Qty          "QTY"
    , A.Qty2         "QTY2"
    , A.SpecName     "TOSTEP"
    , A.ResourceName "EQUIPMENT"
    , A.ShiftName "SHIFT"
    , NULL     "ATTR_MODIFIED"
    , NULL     "ATTR_OLDVALUE"
    , NULL     "ATTR_NEWVALUE"
    , NULL     "REASONNAME"
    , NULL     "BONUS_REJ_QTY"
    , DECODE(A.STATUS, 0, 'DELETED', 1, 'ACTIVE', 2, 'TERMINATED', 3, 'INTRANSIT', 4, 'ISSUED', 5, 'SHIPPED', 'UNDEFINED') "LOTSTATUS"
    , NULL     "TARGETLOT"
    , NULL     "TARGETLOTQTY"
    , NULL     "TARGETLOTQTY2"
    , NULL     "SOURCELOT"
    , NULL     "SOURCELOTQTY"
    , NULL     "SOURCELOTQTY2"
    , A.CDOTXNSEQUENCE  "CDOTXNSEQ"
FROM
    HistoryMainline A
    , Container B
WHERE
    A.HISTORYID = B.CONTAINERID
    AND (
        NOT (
            A.CALLBYCDONAME IN ('LotModifyAttrs', 'ModifyAttrs')
            AND A.CDONAME = 'ModifyAttrs'
            )
        AND NOT (
            A.CDONAME IN ('SplitLot', 'CombineLot', 'RejectLot', 'BonusLot', 'StartLot', 'MaterialLotStart')
            )
        )
UNION
SELECT
    B.CONTAINERNAME "LOTID"
    , A.CDOName || DECODE(A.CallByCDOName, A.CDOName, '', ' (' || A.CallByCDOName || ')') "SERVICE"
    , TO_CHAR(A.TxnDate, 'YYYY-MM-DD hh24:mi:ss') "TXNDATE"
    , A.Username "USER"
    , A.FromSpecName "FROMSTEP"
    , decode( nvl(D.ATTRIBUTENAME, 'XXX'), 'Qty', to_number(D.Oldvalue), NULL)  "FROMQTY"
    , decode( nvl(D.ATTRIBUTENAME, 'XXX'), 'Qty2', to_number(D.Oldvalue), NULL) "FROMQTY2"
    , decode( nvl(D.ATTRIBUTENAME, 'XXX'), 'Qty', to_number(D.Newvalue), NULL)  "QTY"
    , decode( nvl(D.ATTRIBUTENAME, 'XXX'), 'Qty2', to_number(D.Newvalue), NULL) "QTY2"
    , A.SpecName     "TOSTEP"
    , A.ResourceName "EQUIPMENT"
    , A.ShiftName "SHIFT"
    , D.ATTRIBUTENAME  "ATTR_MODIFIED"
    , D.Oldvalue       "ATTR_OLDVALUE"
    , D.Newvalue       "ATTR_NEWVALUE"
    , C.MODIFYATTRSREASONNAME "REASONNAME"
    , NULL       "BONUS_REJ_QTY"
    , DECODE(A.STATUS, 0, 'DELETED', 1, 'ACTIVE', 2, 'TERMINATED', 3, 'INTRANSIT', 4, 'ISSUED', 5, 'SHIPPED', 'UNDEFINED') "LOTSTATUS"
    , NULL       "TARGETLOT"
    , NULL       "TARGETLOTQTY"
    , NULL       "TARGETLOTQTY2"
    , NULL       "SOURCELOT"
    , NULL       "SOURCELOTQTY"
    , NULL       "SOURCELOTQTY2"
    , A.CDOTXNSEQUENCE  "CDOTXNSEQ"
FROM
    HistoryMainline A
    , Container B
    , A_ModifyAttrsHistory C
    , A_ModifyAttrsHistoryDetails D
WHERE
    A.HISTORYID = B.ContainerId
    AND (
        A.Callbycdoname in ('LotModifyAttrs', 'ModifyAttrs')
        AND A.Cdoname = 'ModifyAttrs'
        )
    AND C.HISTORYMAINLINEID = A.HISTORYMAINLINEID
    AND D.MODIFYATTRSHISTORYID = C.MODIFYATTRSHISTORYID
UNION
SELECT
    B.CONTAINERNAME "LOTID"
    , A.CDOName || DECODE(A.CallByCDOName, A.CDOName, '', ' (' || A.CallByCDOName || ')') "SERVICE"
    , TO_CHAR(A.TxnDate, 'YYYY-MM-DD hh24:mi:ss') "TXNDATE"
    , A.Username "USER"
    , A.FromSpecName "FROMSTEP"
    , NULL           "FROMQTY"
    , NULL           "FROMQTY2"
    , NULL           "QTY"
    , NULL           "QTY2"
    , A.SpecName     "TOSTEP"
    , E.EQUIPMENTNAME "EQUIPMENT"
    , A.ShiftName "SHIFT"
    , NULL     "ATTR_MODIFIED"
    , NULL     "ATTR_OLDVALUE"
    , NULL     "ATTR_NEWVALUE"
    , NULL     "REASONNAME"
    , NULL     "BONUS_REJ_QTY"
    , DECODE(F.STATUS, 0, 'DELETED', 1, 'ACTIVE', 2, 'TERMINATED', 3, 'INTRANSIT', 4, 'ISSUED', 5, 'SHIPPED', 'UNDEFINED') "LOTSTATUS"
    , A.CONTAINERNAME "TARGETLOT"
    , F.Qty           "TARGETLOTQTY"
    , F.Qty2          "TARGETLOTQTY2"
    , NULL     "SOURCELOT"
    , NULL     "SOURCELOTQTY"
    , NULL     "SOURCELOTQTY2"
    , A.CDOTXNSEQUENCE "CDOTXNSEQ"
FROM
    HistoryMainline A
    , Container B
    , Combinehistory E
    , Combinehistorydetail F
WHERE
    F.Fromcontainerid = B.ContainerId
    AND E.COMBINEHISTORYID = F.COMBINEHISTORYID
    AND A.HISTORYMAINLINEID = E.HISTORYMAINLINEID
UNION
SELECT
    B.CONTAINERNAME "LOTID"
    , A.CDOName || DECODE(A.CallByCDOName, A.CDOName, '', ' (' || A.CallByCDOName || ')') "SERVICE"
    , TO_CHAR(A.TxnDate, 'YYYY-MM-DD hh24:mi:ss') "TXNDATE"
    , A.Username "USER"
    , A.FromSpecName "FROMSTEP"
    , A.FROMQTY      "FROMQTY"
    , A.FROMQTY2     "FROMQTY2"
    , A.QTY          "QTY"
    , A.QTY2         "QTY2"
    , A.SpecName     "TOSTEP"
    , E.EQUIPMENTNAME "EQUIPMENT"
    , A.ShiftName    "SHIFT"
    , NULL     "ATTR_MODIFIED"
    , NULL     "ATTR_OLDVALUE"
    , NULL     "ATTR_NEWVALUE"  
    , NULL     "REASONNAME"
    , NULL     "BONUS_REJ_QTY"
    , DECODE(A.STATUS, 0, 'DELETED', 1, 'ACTIVE', 2, 'TERMINATED', 3, 'INTRANSIT', 4, 'ISSUED', 5, 'SHIPPED', 'UNDEFINED') "LOTSTATUS"
    , NULL     "TARGETLOT"
    , NULL     "TARGETLOTQTY"
    , NULL     "TARGETLOTQTY2"
    , F.FROMCONTAINERNAME  "SOURCELOT"
    , F.QTY    "SOURCELOTQTY"
    , F.QTY2   "SOURCELOTQTY2"
    , A.CDOTXNSEQUENCE "CDOTXNSEQ"
FROM
    HistoryMainline A
    , Container B
    , Combinehistory E
    , Combinehistorydetail F
WHERE
    A.HISTORYID = B.CONTAINERID
    AND E.COMBINEHISTORYID = F.COMBINEHISTORYID
    AND A.HISTORYMAINLINEID = E.HISTORYMAINLINEID
UNION
SELECT
    B.CONTAINERNAME "LOTID"
    , A.CDOName || DECODE(A.CallByCDOName, A.CDOName, '', ' (' || A.CallByCDOName || ')') "SERVICE"
    , TO_CHAR(A.TxnDate, 'YYYY-MM-DD hh24:mi:ss') "TXNDATE"
    , A.Username "USER"
    , A.FromSpecName "FROMSTEP"
    , NULL     "FROMQTY"
    , NULL     "FROMQTY2"
    , H.QTY    "QTY"
    , H.QTY2   "QTY2"
    , A.SpecName     "TOSTEP"
    , G.EQUIPMENTNAME "EQUIPMENT"
    , A.ShiftName    "SHIFT"
    , NULL     "ATTR_MODIFIED"
    , NULL     "ATTR_OLDVALUE"
    , NULL     "ATTR_NEWVALUE"  
    , NULL     "REASONNAME"
    , NULL     "BONUS_REJ_QTY"
    , 'ACTIVE' "LOTSTATUS"
    , NULL     "TARGETLOT"
    , NULL     "TARGETLOTQTY"
    , NULL     "TARGETLOTQTY2"
    , A.CONTAINERNAME  "SOURCELOT"
    , H.QTY    "SOURCELOTQTY"
    , H.QTY2   "SOURCELOTQTY2"
    , A.CDOTXNSEQUENCE "CDOTXNSEQ"
FROM
    HistoryMainline A
    , Container B
    , SPLITHISTORY G
    , SPLITHISTORYDETAILS H
WHERE
    H.TOCONTAINERID = B.CONTAINERID
    AND G.SPLITHISTORYID = H.SPLITHISTORYID
    AND A.HISTORYMAINLINEID = G.HISTORYMAINLINEID
UNION
SELECT
    B.CONTAINERNAME "LOTID"
    , A.CDOName || DECODE(A.CallByCDOName, A.CDOName, '', ' (' || A.CallByCDOName || ')') "SERVICE"
    , TO_CHAR(A.TxnDate, 'YYYY-MM-DD hh24:mi:ss') "TXNDATE"
    , A.Username "USER"
    , A.FromSpecName "FROMSTEP"
    , A.FROMQTY      "FROMQTY"
    , A.FROMQTY2     "FROMQTY2"
    , A.QTY          "QTY"
    , A.QTY2         "QTY2"
    , A.SpecName     "TOSTEP"
    , G.EQUIPMENTNAME "EQUIPMENT"
    , A.ShiftName    "SHIFT"
    , NULL     "ATTR_MODIFIED"
    , NULL     "ATTR_OLDVALUE"
    , NULL     "ATTR_NEWVALUE"  
    , NULL     "REASONNAME"
    , NULL     "BONUS_REJ_QTY"
    , DECODE(A.STATUS, 0, 'DELETED', 1, 'ACTIVE', 2, 'TERMINATED', 3, 'INTRANSIT', 4, 'ISSUED', 5, 'SHIPPED', 'UNDEFINED') "LOTSTATUS"
    , H.TOCONTAINERNAME "TARGETLOT"
    , H.QTY             "TARGETLOTQTY"
    , H.QTY2            "TARGETLOTQTY2"
    , NULL     "SOURCELOT"
    , NULL     "SOURCELOTQTY"
    , NULL     "SOURCELOTQTY2"
    , A.CDOTXNSEQUENCE "CDOTXNSEQ"
FROM
    HistoryMainline A
    , Container B
    , SPLITHISTORY G
    , SPLITHISTORYDETAILS H
WHERE
    A.HISTORYID = B.CONTAINERID
    AND G.HISTORYMAINLINEID = A.HISTORYMAINLINEID
    AND H.SPLITHISTORYID = G.SPLITHISTORYID
UNION
SELECT
    B.CONTAINERNAME "LOTID"
    , A.CDOName || DECODE(A.CallByCDOName, A.CDOName, '', ' (' || A.CallByCDOName || ')') "SERVICE"
    , TO_CHAR(A.TxnDate, 'YYYY-MM-DD hh24:mi:ss') "TXNDATE"
    , A.Username "USER"
    , A.FromSpecName "FROMSTEP"
    , A.FROMQTY      "FROMQTY"
    , A.FROMQTY2     "FROMQTY2"
    , A.QTY          "QTY"
    , A.QTY2         "QTY2"
    , A.SpecName     "TOSTEP"
    , A.RESOURCENAME "EQUIPMENT"
    , A.ShiftName    "SHIFT"
    , NULL     "ATTR_MODIFIED"
    , NULL     "ATTR_OLDVALUE"
    , NULL     "ATTR_NEWVALUE"  
    , J.LOSSREASONNAME  "REASONNAME"
    , J.TOTALQTY * (-1) "BONUS_REJ_QTY"
    , DECODE(A.STATUS, 0, 'DELETED', 1, 'ACTIVE', 2, 'TERMINATED', 3, 'INTRANSIT', 4, 'ISSUED', 5, 'SHIPPED', 'UNDEFINED') "LOTSTATUS"
    , NULL     "TARGETLOT"
    , NULL     "TARGETLOTQTY"
    , NULL     "TARGETLOTQTY2"
    , NULL     "SOURCELOT"
    , NULL     "SOURCELOTQTY"
    , NULL     "SOURCELOTQTY2"
    , A.CDOTXNSEQUENCE "CDOTXNSEQ"
FROM
    HistoryMainline A
    , Container B
    , A_REJECTLOTHISTORY I
    , A_REJECTLOTHISTORYDETAILS J
WHERE
    A.HISTORYID = B.CONTAINERID
    AND I.HISTORYMAINLINEID = A.HISTORYMAINLINEID
    AND J.REJECTLOTHISTORYID = I.REJECTLOTHISTORYID
UNION
SELECT
    B.CONTAINERNAME "LOTID"
    , A.CDOName || DECODE(A.CallByCDOName, A.CDOName, '', ' (' || A.CallByCDOName || ')') "SERVICE"
    , TO_CHAR(A.TxnDate, 'YYYY-MM-DD hh24:mi:ss') "TXNDATE"
    , A.Username "USER"
    , A.FromSpecName "FROMSTEP"
    , A.FROMQTY      "FROMQTY"
    , A.FROMQTY2     "FROMQTY2"
    , A.QTY          "QTY"
    , A.QTY2         "QTY2"
    , A.SpecName     "TOSTEP"
    , A.RESOURCENAME "EQUIPMENT"
    , A.ShiftName    "SHIFT"
    , NULL     "ATTR_MODIFIED"
    , NULL     "ATTR_OLDVALUE"
    , NULL     "ATTR_NEWVALUE"  
    , L.BONUSREASONNAME "REASONNAME"
    , L.TOTALQTY        "BONUS_REJ_QTY"
    , DECODE(A.STATUS, 0, 'DELETED', 1, 'ACTIVE', 2, 'TERMINATED', 3, 'INTRANSIT', 4, 'ISSUED', 5, 'SHIPPED', 'UNDEFINED') "LOTSTATUS"
    , NULL     "TARGETLOT"
    , NULL     "TARGETLOTQTY"
    , NULL     "TARGETLOTQTY2"
    , NULL     "SOURCELOT"
    , NULL     "SOURCELOTQTY"
    , NULL     "SOURCELOTQTY2"
    , A.CDOTXNSEQUENCE "CDOTXNSEQ"
FROM
    HistoryMainline A
    , Container B
    , A_BONUSLOTHISTORY K
    , A_BONUSLOTHISTORYDETAILS L
WHERE
    A.HISTORYID = B.CONTAINERID
    AND K.HISTORYMAINLINEID = A.HISTORYMAINLINEID
    AND L.BONUSLOTHISTORYID = K.BONUSLOTHISTORYID
UNION
SELECT
    B.CONTAINERNAME "LOTID"
    , A.CDOName || DECODE(A.CallByCDOName, A.CDOName, '', ' (' || A.CallByCDOName || ')') "SERVICE"
    , TO_CHAR(A.TxnDate, 'YYYY-MM-DD hh24:mi:ss') "TXNDATE"
    , A.Username "USER"
    , A.FromSpecName "FROMSTEP"
    , A.FROMQTY      "FROMQTY"
    , A.FROMQTY2     "FROMQTY2"
    , A.QTY          "QTY"
    , A.QTY2         "QTY2"
    , A.SpecName     "TOSTEP"
    , A.RESOURCENAME "EQUIPMENT"
    , A.ShiftName    "SHIFT"
    , NULL     "ATTR_MODIFIED"
    , NULL     "ATTR_OLDVALUE"
    , NULL     "ATTR_NEWVALUE"  
    , N.STARTREASONNAME "REASONNAME"
    , NULL     "BONUS_REJ_QTY"
    , DECODE(A.STATUS, 0, 'DELETED', 1, 'ACTIVE', 2, 'TERMINATED', 3, 'INTRANSIT', 4, 'ISSUED', 5, 'SHIPPED', 'UNDEFINED') "LOTSTATUS"
    , NULL     "TARGETLOT"
    , NULL     "TARGETLOTQTY"
    , NULL     "TARGETLOTQTY2"
    , NULL     "SOURCELOT"
    , NULL     "SOURCELOTQTY"
    , NULL     "SOURCELOTQTY2"
    , A.CDOTXNSEQUENCE "CDOTXNSEQ"
FROM
    HistoryMainline A
    , Container B
    , STARTHISTORYDETAIL M
    , STARTREASON N
WHERE
    A.HISTORYID = B.CONTAINERID
    AND M.HISTORYMAINLINEID = A.HISTORYMAINLINEID
    AND N.STARTREASONID = M.STARTREASONID
UNION
SELECT
    B.CONTAINERNAME "LOTID"
    , 'CURRENT STATUS -->' "SERVICE"
    , TO_CHAR(SYSDATE, 'YYYY-MM-DD hh24:mi:ss') "TXNDATE"
    , NULL   "USER"
    , Y.WORKFLOWSTEPNAME "FROMSTEP"
    , NULL   "FROMQTY"
    , NULL   "FROMQTY2"
    , B.QTY  "QTY"
    , B.QTY2 "QTY2"
    , NULL   "TOSTEP"
    , NULL   "EQUIPMENT"
    , NULL   "SHIFT"
    , NULL   "ATTR_MODIFIED"
    , NULL   "ATTR_OLDVALUE"
    , NULL   "ATTR_NEWVALUE"
    , NULL   "REASONNAME"
    , NULL   "BONUS_REJ_QTY"
    , DECODE(B.STATUS, 0, 'DELETED', 1, 'ACTIVE', 2, 'TERMINATED', 3, 'INTRANSIT', 4, 'ISSUED', 5, 'SHIPPED', 'UNDEFINED') "LOTSTATUS"
    , NULL   "TARGETLOT"
    , NULL   "TARGETLOTQTY"
    , NULL   "TARGETLOTQTY2"
    , NULL   "SOURCELOT"
    , NULL   "SOURCELOTQTY"
    , NULL   "SOURCELOTQTY2"
    , 999    "CDOTXNSEQ"
FROM
    Container B
    , CURRENTSTATUS X
    , WORKFLOWSTEP  Y
WHERE
    X.CURRENTSTATUSID = B.CURRENTSTATUSID
    AND Y.WORKFLOWSTEPID = X.WORKFLOWSTEPID
/
