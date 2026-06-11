--------------------------------------------------------------------------------
-- SCRIPT: csiGetAdjustedTime.sql
--         Function to adjust a given GMT date/time by the given number of minutes
--
-- Parameters:
--      pValue - The date/time (assumed to be in GMT)
--      pOffset - The offset in minutes from GMT
--
-- Return:
--      Adjusted date/time
--
-- HISTORY:
--
-- Copyright Siemens 2023  
--
CREATE OR REPLACE
FUNCTION csiGetAdjustedTime(pValue IN DATE, pOffset IN NUMBER) RETURN DATE
--
-- Copyright Siemens 2023  
--
IS
BEGIN
   RETURN pValue + pOffset/(60*24);
END;
/
