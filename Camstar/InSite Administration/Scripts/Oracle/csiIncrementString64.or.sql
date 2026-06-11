/*-- =============================================
  -- Copyright Siemens 2023  
  -- 
  -- Objective : To increment 64bit hex value by given amoumt (valuetoadd).
  --
  -- History:
  --   Added incrementAmt parameter so '100' is not hard-coded - Barry E. 1/7/2005
  --   Updated/added copyright notice (SPR S9984). - Bill Lippard  12/04/2006
  --   Updated copyright notice (SPR S9984).       - Bill Lippard  04/23/2007
  -- ============================================= */

CREATE or REPLACE PROCEDURE csiIncrementString64(
    oldvalue   IN  VARCHAR2,
    valuetoadd IN NUMBER,
    newvalue   OUT VARCHAR2 )
AS
--
-- @ 2016  
--
    tmpvalue        NUMBER;
 BEGIN
   tmpvalue := csihextodec(oldvalue) + valuetoadd;
   newvalue := LTRIM(TO_CHAR(tmpvalue,'0XXXXXXXXXXXXXXX'));
 END;
/ 

