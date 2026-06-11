/*-- =======================================================================
-- csiReturnTransactionID
-- Return char string of next value of TxnIdseq sequence
-- ======================================================================= */

CREATE or REPLACE PROCEDURE csiReturnTransactionID( transIdNewValue OUT  VARCHAR2 ) 
as
--
-- Copyright Siemens 2023  
--
  BEGIN
    select to_char(TxnIdseq.nextval) into transIdNewValue from DUAL; 
    /* -- the following will convert number to hex string
      -- transIdNewValue := LTRIM(TO_CHAR(numerator,'0XXXXXXXXXXXXXXX'));
    */
  END;
/ 

