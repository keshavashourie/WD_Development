/* ********************************************************************************
   Copyright Siemens 2023  

csiIncrementString64
  To increment 64bit hex value by @valuetoadd
Parameters:
    oldvalue - hex string
    newvalue - OUTPUT hexstring
    
Usage: exec csiIncrementString64 <hexstring> <@valuetoadd> <@var> OUTPUT

  History:
  *) Added valuetoadd parameter so '100' would not be hard-coded - Barry E. 1/7/2005
  *) Added copyright notice (SPR S9984) - Bill Lippard 12/04/2006
  *) Added copyright notice (SPR S9984) - Bill Lippard 04/23/2007
********************************************************************************** */

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiIncrementString64' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiIncrementString64
GO

CREATE PROCEDURE csiIncrementString64 
  @oldvalue   char(16),
  @valuetoadd int,
  @newvalue   char(16) OUTPUT
AS
--
-- Copyright Siemens 2023  
--
 Begin 
   set nocount on
   DECLARE @tmpvalue  bigint
   DECLARE @tempstr char (18)

   exec csihextodec @oldvalue, @tmpvalue output
   set @tmpvalue = @tmpvalue + @valuetoadd
   exec csibiginttohex @tmpvalue, @tempstr output
   SET  @newvalue = upper(substring(@tempstr,3,16))
 END
GO

-- =============================================
-- example to execute the store procedure
-- =============================================
-- DECLARE @chValue char(16)
-- EXECUTE csiIncrementString64 '1234567890123456', 100, @chValue OUTPUT
-- EXECUTE csiIncrementString64 'ffffffffffffffff', 25, @chValue OUTPUT
-- EXECUTE csiIncrementString64 '0000000000001b59', 50, @chValue OUTPUT
-- GO


