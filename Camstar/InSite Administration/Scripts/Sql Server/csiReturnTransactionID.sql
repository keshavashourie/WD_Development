/* ********************************************************************************
csiReturnTransactionID
   Uses the transactionidtable.idval identity column as a counter for generating 
   transactionIDs.  The rollback is used to restricts the table from growing 
   unnecassarily

History:
   Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
   Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
******************************************************************************** */
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiReturnTransactionID' 
	   AND 	  type = 'P')
    DROP procedure csiReturnTransactionID
GO

CREATE PROCEDURE csiReturnTransactionID
    @IDVal   char(16) OUTPUT
  AS
--
-- Copyright Siemens 2023  
--
  set nocount on
  begin tran
    DECLARE @s bigint 
    declare @tempstr varchar(255)
    INSERT INTO transactionidtable(transidname, transidvalue) 
       values ('TIDVal','0000000000000000')
    SELECT @s = SCOPE_IDENTITY()
    SET  @IDVal = convert(char(16), @s)
    /* Obsolete now that convert works properly in 2k SP2
      -- exec csibiginttohex @S, @tempstr output
      -- SET  @IDVal = upper(substring(@tempstr,3,16)) */
  ROLLBACK TRAN
go
