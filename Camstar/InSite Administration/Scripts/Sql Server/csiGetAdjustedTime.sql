/* ********************************************************************************
csiGetAdjustedTime
  Function to adjust a given GMT date/time by the given number of minutes

Parameters:
    pValue - The date/time (assumed to be in GMT)
    pOffset - The offset in minutes from GMT

Return:
   Adjusted date/time
    
History:

NOTE: 
********************************************************************************** */
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiGetAdjustedTime' 
	   AND 	  type = 'FN')
    DROP FUNCTION csiGetAdjustedTime
GO
CREATE FUNCTION csiGetAdjustedTime (@pValue As DateTime, @pOffset As Integer)  RETURNS DateTime
AS
--
-- Copyright Siemens 2023  
--
BEGIN
    RETURN DATEADD(MINUTE, @pOffset, @pValue);    
END
GO
