/*-- =============================================
 -- Copyright Siemens 2023  
 -- 
 -- Objective : Used in the ContainerCompletedTask Oracle query.
 --
 -- History:
 --   02/07/2006 - Changed return type to NUMBER
 --   12/04/2006 - Updated/Added copyright notice (SPR S9984)    Bill Lippard
 --   04/23/2007 - Updated copyright notice (SPR S9984)          Bill Lippard
 --
 -- ============================================= */
CREATE OR REPLACE 
FUNCTION csiGetContainerTaskResult(  pCurrentStatusId IN VARCHAR2 ,pTaskId IN VARCHAR2) RETURN  NUMBER IS

--
--  Copyright Siemens 2023  
--

   CURSOR c1 IS
      SELECT Pass
      FROM ContainerCompletedTask
      WHERE CurrentStatusId = pCurrentStatusId
      AND   TaskId = pTaskId;
   v_ret NUMBER;
BEGIN 
   OPEN c1;
   FETCH c1 INTO v_ret;
   CLOSE c1;
   
   RETURN v_ret;
END;
/

