/* ==========================================================================
-- csiSetNextTxnId
--   Procedure used to recreate the TXNIDSEQ sequence.
--   Drops and recreates the Sequence with the New Max TXNID + 1
--
--   Requires the csihextodec function
--
--	Date		Name		    Comments
--  May/14/2004	      Jagadesh S	SP was created
--  Dec/04/2006       Bill Lippard      Added copyright notice (SPR S9984).
--  Apr/23/2007       Bill Lippard      Updated copyright notice (SPR S9984).
--
-- ========================================================================== */
IF EXISTS (SELECT name
	   FROM   sysobjects
	   WHERE  name = 'csiSetNextTxnId'
	   AND 	  type = 'P')
    DROP procedure csiSetNextTxnId
GO

CREATE PROCEDURE csiSetNextTxnId
		@p_maxhexid varchar(15)
AS
--
-- Copyright Siemens 2023  
--
set nocount on
begin
	declare @v_newmaxid	bigint
	declare @v_sqlstmt	varchar(1000)

    exec csihextodec @p_maxhexid, @v_newmaxid output
    set @v_newmaxid = @v_newmaxid + 1

	IF EXISTS (SELECT column_name FROM information_schema.columns
			where table_name  = 'transactionidtable' and column_name = 'idval')
		execute ('alter table transactionidtable drop column idval')

		set @v_sqlstmt = 'alter table transactionidtable add idVal ' +
                	'bigint identity(' + cast(@v_newmaxid as varchar) + ',1)'
		execute(@v_sqlstmt)

-- Droping the TRANSACTIONIDTABLERESETDATA Table

    IF EXISTS (select name from sysobjects where name = 'TRANSACTIONIDTABLERESETDATA' and xtype= 'U' )
	execute ('DROP TABLE TRANSACTIONIDTABLERESETDATA')

END
go
