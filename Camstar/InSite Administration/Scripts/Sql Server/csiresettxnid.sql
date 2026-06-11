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
	exec csiResetTXNID
--
--	Date		Name		    Comments
--  Oct-24-2002	      			SP was created
--  May-14-2004	     Jagadesh S		The SP was changed completely
--  Dec-04-2006      Bill Lippard       Added copyright notice (SPR S9984).
--  Apr-23-2007      Bill Lippard       Updated copyright notice (SPR S9984).
-- ==========================================================================*/

IF EXISTS (SELECT name
	   FROM   sysobjects
	   WHERE  name = 'csiResetTXNID'
	   AND 	  type = 'P')
    DROP procedure csiResetTXNID
GO

CREATE PROCEDURE csiResetTXNID

as
--
-- Copyright Siemens 2023  
--
set nocount on
begin
	declare @v_maxhexid	varchar(15)
	declare @v_count	int

-- Call the csiGetMaxTxnId

	exec csiGetMaxTxnId @v_maxhexid output

-- Call the csiValidateMaxTxnId

	exec csiValidateMaxTxnId @v_maxhexid, @v_count output

    if @v_count <> 1
	begin
	    print 'Please check TRANSACTIONIDTABLERESETDATA Table for the Max TxnID'
	    print 'Create the Sequence and Drop the Table manually'
	end
    else
	begin
	    -- Call the csiSetNextTxnId Stored Procedure

	    exec csiSetNextTxnId @v_maxhexid
	    print 'Sequence has been recreated successfully'
	end

END
go
