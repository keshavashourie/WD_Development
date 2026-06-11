/* ==========================================================================
-- csiValidateMaxTxnId
--   Procedure used to validate the Max TxnID.
--   Gets the current MAX TXNID from 4 tables individually and checks more
--	one  distinct value exists in the Table
--
--	Date		Name		    Comments
--  May/17/2004	      Jagadesh S	SP was created
--  Dec/04/2006       Bill Lippard      Added copyright notice (SPR S9984).
--  Apr/23/2007       Bill Lippard      Updated copyright notice (SPR S9984).
-- ========================================================================== */

IF EXISTS (SELECT name
	   FROM   sysobjects
	   WHERE  name = 'csiValidateMaxTxnId'
	   AND 	  type = 'P')
    DROP procedure csiValidateMaxTxnId
GO

CREATE PROCEDURE csiValidateMaxTxnId
		@p_maxhexid varchar(15),
		@p_count int OUTPUT
as
--
-- Copyright Siemens 2023  
--
set nocount on
BEGIN

	declare @v_nibble	int
 	declare @v_sqlstmt	varchar(1000)

-- Create TRANSACTIONIDTABLERESETDATA Table if it does not exist

    IF EXISTS (select name from sysobjects where name = 'TRANSACTIONIDTABLERESETDATA' and xtype= 'U' )
	execute ('DELETE FROM  TRANSACTIONIDTABLERESETDATA')

    else
	execute ('CREATE TABLE TRANSACTIONIDTABLERESETDATA ( MAXTXNIDS CHAR(16))')

-- Get the number of Nibble (sites) to be excluded
		
	select	@v_nibble = ceiling( (convert(decimal, NumberOfBitsUsed) + 1) / 4) + 1
	from	DBIDConfiguration

-- get all the max TxnIDs into the table

    insert into TRANSACTIONIDTABLERESETDATA
	select TXNID from transrevoids
		where substring(txnid, @v_nibble, 15) = @p_maxhexid


    insert into TRANSACTIONIDTABLERESETDATA
	select TXNID from historymainline
		where substring(txnid, @v_nibble, 15) = @p_maxhexid
		  and txnid not in (select MAXTXNIDS from TRANSACTIONIDTABLERESETDATA)


    insert into TRANSACTIONIDTABLERESETDATA
	select distinct TXNID from OutboundXMLDoc
		where substring(txnid, @v_nibble, 15) = @p_maxhexid
		  and txnid not in (select MAXTXNIDS from TRANSACTIONIDTABLERESETDATA)


    insert into TRANSACTIONIDTABLERESETDATA
	select TXNID from ProcessedTxnGUID
		where substring(txnid, @v_nibble, 15) = @p_maxhexid
		  and txnid not in (select MAXTXNIDS from TRANSACTIONIDTABLERESETDATA)

-- Recreate the Sequence if the count of TXNID in the TRANSACTIONIDTABLERESETDATA is 1

    select @p_count = count(*) from TRANSACTIONIDTABLERESETDATA

END
go
