create or replace TYPE retEQList AS OBJECT (EquipmentName varchar2(40));
/
show errors;

create or replace TYPE retEQList_table AS TABLE OF retEQList;
/
show errors;

create or replace TYPE retWIPLot AS OBJECT (Lot varchar2(40), Qty float, Qty2 float, Spec varchar2(60), DaysHere float, QueueTime float, Product varchar2(60), Status varchar2(40), IsOnHold varchar2(40), InRework varchar2(40), Insertion int, InsertionProcessSpec varchar2(90), InsertionSS varchar2(90), Rejects float, MoveOutQty float, WIPStatus varchar2(40), WIPType varchar2(40), WIPYieldResult varchar2(40), BatchNo varchar2(40), EqpCount int, EqpLoadingCount int, FutureHoldExists varchar2(40), InSchedule varchar2(40), SpecPass int, SpecCategory varchar2(40), SpecType varchar2(40), SpecDescription varchar2(255));
/
show errors;

create or replace TYPE retWIPLot_table AS TABLE OF retWIPLot;
/
show errors;

create or replace TYPE tmpWIPLotName AS OBJECT (Lot varchar2(40));
/
show errors;

create or replace TYPE tmpWIPLotName_TABLE AS TABLE OF tmpWIPLotName;
/
show errors;

CREATE OR REPLACE PACKAGE EquipmentDispatch IS
--===========================================================================
-- © 2017 Siemens Product Lifecycle Management Software Inc. 
-- Author:		Andy Murphi
-- Release Date:	06 Jan 2012
-- Release Version:	1.000
--===========================================================================
-- Description:
--	EquipmentDispatch package specification.

-----------------------------------------------------------------------------
-- Release Notes for 1.000 (06 Jan 2012):
-----------------------------------------------------------------------------
-- 1)	First release version.

--===========================================================================


--===============================================================
-- Type declarations
--===============================================================

--===============================================================
-- Package information
--===============================================================


--===============================================================
-- Function and Procedure declarations
--===============================================================
FUNCTION csiGetResourcesList(resgroup VARCHAR2) RETURN RETEQLIST_TABLE;

FUNCTION csiEquipmentDispatch(equipment VARCHAR2, specnamevar VARCHAR2, specrev VARCHAR2) RETURN RETWIPLOT_TABLE;

--===========================================================================
-- End of package specification
--===========================================================================
END;
/
show errors;
