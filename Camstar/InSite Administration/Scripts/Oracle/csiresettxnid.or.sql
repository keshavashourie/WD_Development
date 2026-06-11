/* ==========================================================================
-- csiResetTXNID
--   Procedure used to recreate the TXNIDSEQ sequence.
--   Drops and recreates the Sequence with the New Max TXNID + 1
--
--   Requires the csiGetMaxTxnId Stored Procedure
--   Requires the csiValidateMaxTxnId Stored Procedure
--   Requires the csiSetNextTxnId Stored Procedure
--
--   Usage
	Set serveroutput on
	exec csiResetTXNID
--
--	Date		Name		    Comments
--  Oct-24-2002	      			SP was created
--  May-142004	     Jagadesh S		The SP was changed completely
--  Dec-04-2006      Bill Lippard       Added copyright notice (SPR S9984).
--  Apr-23-2007      Bill Lippard       Updated copyright notice (SPR S9984).
-- ========================================================================== */

CREATE or REPLACE PROCEDURE csiResetTXNID
is

--
-- Copyright Siemens 2023  
--

	v_maxhexid	varchar2(15) ;
	v_count		number ;

BEGIN

-- Call the csiGetMaxTxnId

	csiGetMaxTxnId(v_maxhexid) ;

-- Call the csiValidateMaxTxnId

	csiValidateMaxTxnId(v_maxhexid, v_count) ;

    if v_count != 1 then
	dbms_output.put_line('Please check TRANSACTIONIDTABLERESETDATA Table for the Max TxnID') ;
	dbms_output.put_line('Create the Sequence and Drop the Table manually') ;

    else
	-- Call the csiSetNextTxnId Stored Procedure

	csiSetNextTxnId (v_maxhexid) ;
	dbms_output.put_line('Sequence has been recreated successfully') ;
    end if ;

END csiResetTXNID ;
/
