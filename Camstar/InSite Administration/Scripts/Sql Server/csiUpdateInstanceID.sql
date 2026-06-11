/* ********************************************************************************
-- Copyright Siemens 2023  

csiUpdateInstanceID
  Procedure used to Update the instanceIdCount table for a specified CDO
  Parameters
     instanceType 
     CDODefID
     InstIdNewValue OUPUT   
  Usage: exec csiUpdateInstanceID <instanceType>, <CDODefID>, <@var> OUTPUT

  History:
  *) Added the WITH clause for the hint - stanly 06/21/2004
  *) Added @incrementAmt parameter so '100' would not be hard-coded - Barry E. 1/7/2005
  *) Updated/added copyright notice(s) (SPR S9984) - Bill Lippard  12/04/2006.
  *) Updated copyright notice(s) (SPR S9984)       - Bill Lippard  04/23/2007.
********************************************************************************** */

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiUpdateInstanceID' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiUpdateInstanceID
GO

CREATE PROCEDURE csiUpdateInstanceID 
  @instanceType     int,
  @CDODefID         int,
  @incrementAmt     int,
  @InstIdNewValue   char(16) OUTPUT
AS
--
-- Copyright Siemens 2023  
--
 BEGIN TRAN
    DECLARE @InstIdOldValue   char(16)
    /* LOCK THE RECORD WITH UPDLOCK HINT
       SO THAT ONLY ONE USER CAN ACCESS THE RECORD AT ANY POINT IN TIME */
    IF @instanceType = 0
      SELECT  @InstIdOldValue = ClientInstanceID FROM InstanceIDCount WITH (updlock holdlock)
      WHERE   CDODefID = @CDODefID
    ELSE
      SELECT  @InstIdOldValue = CSiInstanceID   FROM InstanceIDCount WITH (updlock holdlock)
      WHERE   CDODefID = @CDODefID
     
    Set NoCount On
    
    EXECUTE csiIncrementString64 @InstIdOldValue, @incrementAmt, @InstIdNewValue OUTPUT
    IF @instanceType = 0
      UPDATE  InstanceIDCount 
      SET ClientInstanceID = @InstIdNewValue
      WHERE   CDODefID = @CDODefID
    ELSE
      UPDATE  InstanceIDCount
      SET     CSiInstanceID = @InstIdNewValue
      WHERE   CDODefID = @CDODefID

    IF @@rowcount = 0 or @@error !=0 /* no rows where hit by our update */
     BEGIN
      ROLLBACK TRAN
       PRINT 'Error Occurred, no rows were updated'
       RETURN
     END
    COMMIT TRAN
GO

-- =============================================
-- example to execute the store procedure
-- =============================================
-- DDL for TransactionIdtable:

-- DECLARE @InstIdVal char(16)
-- EXECUTE csiUpdateInstanceID 0, 1250, 100, @InstIdVal OUTPUT
-- Print 'Instance Id value = ' + @InstIdVal
-- GO


