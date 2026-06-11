/* ==========================================================================
-- csiGetMaxTxnId
--   Procedure used to find the Max TxnID.
--   Requires the csihextodec function
--
--	Date		Name		    Comments
--  May/11/2004	      Jagadesh S	SP was created
--  Dec/04/2006       Bill Lippard      Added copyright notice (SPR S9984).
--  Apr/23/2007       Bill Lippard      Updated copyright notice (SPR S9984).
-- ==========================================================================*/
IF EXISTS (SELECT name
	   FROM   sysobjects
	   WHERE  name = 'csiGetMaxTxnId'
	   AND 	  type = 'P')
    DROP procedure csiGetMaxTxnId
GO

CREATE PROCEDURE csiGetMaxTxnId
	@p_maxhexid varchar(15) OUTPUT
as
-- 
-- Copyright Siemens 2023  
-- 
set nocount on
BEGIN
	declare @v_nibble	int

-- Get the number of Nibble (sites) to be excluded

	select	@v_nibble = ceiling( (convert(decimal, NumberOfBitsUsed) + 1) / 4) + 1
	from	DBIDConfiguration

-- get current max used value
     select @p_maxhexid = max(txnid) from
	(select isnull(max(substring(txnid, @v_nibble, 16)),0) txnid from historymainline
        union
         select isnull(max(substring(txnid, @v_nibble, 16)),0) txnid from transrevoids
	union
	 select isnull(max(substring(txnid, @v_nibble, 16)),0) txnid from OutboundXMLDoc
	union
	 select isnull(max(substring(txnid, @v_nibble, 16)),0) txnid from ProcessedTxnGUID ) a

END
go
