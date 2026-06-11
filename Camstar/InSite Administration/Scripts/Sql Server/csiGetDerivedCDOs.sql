/* ============================================================
File:              csiGetDerivedCDOs.sql
Version:           1.0
Create Date:       03-MAY-2005
Database:          SQLSERVER
---------------------------------------------------------------
Purpose: To retrieve all the Derived CDOs of the specified CDO, recursively
---------------------------------------------------------------
Change Log:
   7/12/2005 - Changed PROCEDURE to FUNCTION in DROP statement. Removed personal
               information from comments.
  12/04/2006 - Added copyright notice (SPR S9984)   Bill Lippard
  04/23/2007 - Updated copyright notice (SPR S9984) Bill Lippard
------------------------------------------------------------*/
IF EXISTS (SELECT name
	   FROM   sysobjects
	   WHERE  name = 'csiGetDerivedCDOs'
	   AND 	  type = 'TF')
    DROP FUNCTION csiGetDerivedCDOs
GO
CREATE FUNCTION csiGetDerivedCDOs(@IncludeParent bit, @CDODefID int)
RETURNS @retDerivedCDOs TABLE (CDODefID int)
AS
--
-- Copyright Siemens 2023  
--
BEGIN
	IF (@IncludeParent=1)
	BEGIN
		INSERT INTO @retDerivedCDOs VALUES (@CDODefID)
 	END

	DECLARE @ChildID int

	DECLARE RetrieveChildren CURSOR STATIC LOCAL FOR
	SELECT CDODefId FROM CDODefinition WHERE ParentCDOId=@CDODefID

	OPEN RetrieveChildren

	FETCH NEXT FROM RetrieveChildren INTO @ChildID

	WHILE (@@FETCH_STATUS = 0)
	BEGIN
		INSERT INTO @retDerivedCDOs
		SELECT * FROM csiGetDerivedCDOs(0,@ChildID)

		INSERT INTO @retDerivedCDOs VALUES(@ChildID)

		FETCH NEXT FROM RetrieveChildren INTO @ChildID
	END

	CLOSE RetrieveChildren
	DEALLOCATE RetrieveChildren

	RETURN
END
GO
