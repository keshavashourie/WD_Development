/* ==========================================================================
-- csiGetMaxTxnId
--   Procedure used to find the Max TxnID.
--   Requires the csihextodec function
--
--	Date		Name		    Comments
--  May/14/2004	      Jagadesh S	SP was created
--  Dec/04/2006       Bill Lippard      Added copyright notice (SPR S9984).
--  Apr/23/2007       Bill Lippard      Updated copyright notice (SPR S9984).
--
-- ========================================================================== */

CREATE or REPLACE PROCEDURE csiGetMaxTxnId (p_maxhexid OUT varchar2)
is
--
-- Copyright Siemens 2023  
--

	v_nibble	number(2) ;

BEGIN

-- Get the number of Nibble (sites) to be excluded

	select	ceil((NumberOfBitsUsed + 1) / 4) + 1
	INTO	v_nibble
	from	DBIDConfiguration ;

-- get current max used value
     select max(txnid) into p_maxhexid from
	(select nvl(max(substr(txnid, v_nibble)),0) txnid from transrevoids
	union
	 select nvl(max(substr(txnid, v_nibble)),0) txnid from historymainline
	union
	 select nvl(max(substr(txnid, v_nibble)),0) txnid from OutboundXMLDoc
	union
	 select nvl(max(substr(txnid, v_nibble)),0) txnid from ProcessedTxnGUID ) ;

END csiGetMaxTxnId ;
/
