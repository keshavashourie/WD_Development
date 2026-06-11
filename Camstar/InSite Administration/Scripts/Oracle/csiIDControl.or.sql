/* ********************************************************************************
csiIDControl
  Procedure used to Update the IDControl table for a specified IDType
  Parameters
     IDType 
     NEXTID  output variable
  Usage: exec csiIDControl(<idtype>, <nextid>)

  History:
     Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
     Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
     Jeremy Phelps     2023-03-01      Add seqCount to allow incrementing by 
                                       an amount atomically.
********************************************************************************** */

CREATE or REPLACE PROCEDURE csiIDControl(
  v_idtype    IN   VARCHAR2,
  v_nextid    OUT  NUMBER,
  v_seqCount IN NUMBER DEFAULT 1)
AS
--
-- Copyright Siemens 2023  
--
BEGIN
  Update IDControl set nextid = nextid + v_seqCount where upper(IDType) = upper(v_idtype) 
    returning nextid into v_nextid;
  commit;
END;
/ 

