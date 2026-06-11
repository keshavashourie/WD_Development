/* ********************************************************************************
csiIDControl
  Procedure used to Update the IDControl table for a specified IDType
  Parameters
     IDType 
     NEXTID  output variable
  Usage: exec csiIDControl <idtype>, <nextid> OUTPUT

  History:
     Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
     Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
     Jeremy Phelps     2023-03-01      Add seqCount to allow incrementing by 
                                       an amount atomically.
********************************************************************************** */

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiIDControl' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiIDControl
GO

CREATE PROCEDURE csiIDControl 
  @idtype     nvarchar(30),
  @nextid     int OUTPUT,
  @seqcount   int = 1
AS
--
-- Copyright Siemens 2023  
--
 BEGIN TRAN
   Set NoCount On
   Update IDControl set nextid = nextid + @seqcount where IDType = @idtype
   Select @nextid = nextid from IDControl where IDType = @idtype
   IF @@rowcount = 0 or @@error !=0 /* no rows where hit by our update */
     BEGIN
       ROLLBACK TRAN
       PRINT 'Error Occurred, no rows were updated'
     END
   ELSE
     COMMIT TRAN
GO

-- =============================================
-- example to execute the store procedure
-- =============================================
-- DDL for TransactionIdtable:

-- DECLARE @nextid int
-- EXECUTE csiIDControl 'UserLabel', @nextid output
-- Print @nextid
-- GO


