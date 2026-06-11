/* ==========================================================================
-- csiSetNextTxnId
--   Procedure used to recreate the TXNIDSEQ sequence.
--   Drops and recreates the Sequence with the New Max TXNID + 1
--
--   Requires the csihextodec function
--
--	Date		Name		    Comments
--  05/14/2004	Jagadesh S		SP was created
--  11/15/2004	Purushotham N	        CACHE option is replaced with NOCACHE - SPR S9694
--  12/04/2006  Bill Lippard            Added copyright notice (SPR S9984).    
--  04/23/2007  Bill Lippard            Updated copyright notice (SPR S9984).    
-- ========================================================================== */

CREATE or REPLACE PROCEDURE csiSetNextTxnId (p_maxhexid IN varchar2)
is
--
-- Copyright Siemens 2023  
--
	v_newmaxid	number;
	v_exist		number := 1;
	v_sqlstmt	varchar2(1000);

BEGIN
    v_newmaxid := csihextodec(trim(p_maxhexid)) + 1 ;

    select count(*) into v_exist
        from user_sequences where sequence_name  = 'TXNIDSEQ' ;

    if v_exist > 0 then
	execute immediate 'drop sequence TxnIdseq';
    end if ;

    v_sqlstmt := 'CREATE SEQUENCE TxnIdseq START WITH ' || v_newmaxid ||
		 ' INCREMENT BY 1 NOCACHE';
    execute immediate v_sqlstmt ;


-- Droping the TRANSACTIONIDTABLERESETDATA Table

    v_exist := 1 ;

    select count(*) into v_exist
	from user_tables
	where table_name = 'TRANSACTIONIDTABLERESETDATA' ;

    if v_exist > 0 then
	execute immediate 'drop table TRANSACTIONIDTABLERESETDATA' ;
    end if ;

END csiSetNextTxnId ;
/
