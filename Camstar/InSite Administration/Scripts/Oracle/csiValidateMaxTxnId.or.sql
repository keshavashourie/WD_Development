/* ==========================================================================
-- csiValidateMaxTxnId
--   Procedure used to validate the Max TxnID.
--   Gets the current MAX TXNID from 4 tables individually and checks more
--	one  distinct value exists in the Table
--
--	Date		Name		    Comments
--  May/14/2004	      Jagadesh S	SP was created
--  Dec/04/2006       Bill Lippard      Added copyright notice (SPR S9984).
--  Apr/23/2007       Bill Lippard      Updated copyright notice (SPR S9984).
--
-- ========================================================================== */

CREATE or REPLACE PROCEDURE csiValidateMaxTxnId (p_maxhexid IN varchar2, p_count OUT number)
is
--
-- Copyright Siemens 2023  
--
	v_exist		number := 1;
	v_nibble	number(2);
 	v_sqlstmt	varchar2(1000);

BEGIN

-- Create TRANSACTIONIDTABLERESETDATA Table if it does not exist

    select count(*) into v_exist
	from user_tables
	where table_name = 'TRANSACTIONIDTABLERESETDATA' ;


    if v_exist > 0 then
	execute immediate 'DELETE FROM  TRANSACTIONIDTABLERESETDATA' ;
    else
	execute immediate 'CREATE TABLE TRANSACTIONIDTABLERESETDATA ( MAXTXNIDS CHAR(16))' ;
    end if ;

-- Get the number of v_nibble (sites) to be excluded

	select	ceil((NumberOfBitsUsed + 1) / 4) + 1
	INTO	v_nibble
	from	DBIDConfiguration ;

-- get all the max TxnIDs into the table

    v_sqlstmt := 'insert into TRANSACTIONIDTABLERESETDATA select TXNID from transrevoids where substr(txnid, '
		 || v_nibble || ' ) = '''
		 || p_maxhexid || '''' ;

    execute immediate v_sqlstmt ;


    v_sqlstmt := 'insert into TRANSACTIONIDTABLERESETDATA select TXNID from historymainline where substr(txnid, '
		 || v_nibble || ' ) = '''
		 || p_maxhexid || ''' and txnid not in (select MAXTXNIDS from TRANSACTIONIDTABLERESETDATA)';

    execute immediate v_sqlstmt ;


    v_sqlstmt := 'insert into TRANSACTIONIDTABLERESETDATA select distinct TXNID from OutboundXMLDoc where substr(txnid, '
		 || v_nibble || ' ) = '''
		 || p_maxhexid || ''' and txnid not in (select MAXTXNIDS from TRANSACTIONIDTABLERESETDATA)';

    execute immediate v_sqlstmt ;


    v_sqlstmt := 'insert into TRANSACTIONIDTABLERESETDATA select TXNID from ProcessedTxnGUID where substr(txnid, '
		 || v_nibble || ' ) = '''
		 || p_maxhexid || ''' and txnid not in (select MAXTXNIDS from TRANSACTIONIDTABLERESETDATA) ';

    execute immediate v_sqlstmt ;


--  Get the count of records in the TRANSACTIONIDTABLERESETDATA Table

    v_sqlstmt := 'select count(1) from TRANSACTIONIDTABLERESETDATA' ;

    execute immediate v_sqlstmt into p_count ;

END csiValidateMaxTxnId ;
/
