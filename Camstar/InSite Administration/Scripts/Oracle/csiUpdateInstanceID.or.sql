/*-- =============================================
 -- Copyright Siemens 2023  
 -- 
 -- Objective : To increment and update the InstanceIDCount Table 
 --             by a given amount (incrementAmt) 
 --
 -- Author : SeetrmaS@camstar.com
 --
 -- History:
 --   Added incrementAmt parameter so '100' is not hard-coded - Barry E. 1/7/2005
 --   Updated/added copyright notice (SPR S9984). - Bill Lippard  12/04/2006
 --   Updated copyright notice (SPR S9984).       - Bill Lippard  04/23/2007
-- ============================================= */

CREATE or REPLACE PROCEDURE csiUpdateInstanceID(
  instanceType    IN   NUMBER,
  cdodefid_val    IN   NUMBER,
  incrementAmt    IN   NUMBER,
  InstIdNewValue  OUT  VARCHAR2)
AS
--
-- Copyright Siemens 2023  
--
BEGIN
 DECLARE
 CURSOR InstIDCurs IS
         SELECT  * FROM InstanceIDCount
         WHERE CDODefID = cdodefid_val FOR UPDATE;
 InstId_val InstIDCurs%ROWTYPE;
 BEGIN
   for InstId_val in InstIDCurs
   LOOP
     if instanceType = 0 then
       csiIncrementString64(InstId_val.ClientInstanceID,incrementAmt,InstIdNewValue);
       UPDATE InstanceIDCount SET ClientInstanceID = InstIdNewValue WHERE CURRENT OF InstIDCurs;
     else
       csiIncrementString64(InstId_val.CSiInstanceID,incrementAmt,InstIdNewValue);
       UPDATE InstanceIDCount SET CSiInstanceID = InstIdNewValue WHERE CURRENT OF InstIDCurs;
     end if;
   END LOOP;
   COMMIT;
 END;
END;
/ 

