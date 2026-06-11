------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreAreInsertsPending.sql
-- DESCR:       Determines if INSERTS are pending. Used in synchronization.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiDataStoreAreInsertsPending' AND Type = 'FN')
	DROP FUNCTION csiDataStoreAreInsertsPending
GO

CREATE FUNCTION csiDataStoreAreInsertsPending
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreAreInsertsPending
-- Params:      <in>  @pc_TxnId		CHAR(16)
-- Returns:     BIT 1 or 0
-- Descr:       Determines if INSERTS are pending. Used in synchronization. Executes on the ODS	
--		

-- HISTORY:
--              07/14/2016      Dan Maloney      New function

--
-- Copyright Siemens 2023  
-----------------------------------------------------------------------------------------------------------------------------------------------------
(
        @pc_TxnId                       CHAR(16)
)
RETURNS BIT
AS
BEGIN
	--Variables defined as constants
        DECLARE @BIT_FALSE              BIT = 0
        DECLARE @BIT_TRUE               BIT = 1

        DECLARE @bit_AreInsertsPending  BIT
        DECLARE @i_Cnt                  INT
        DECLARE @nv_Loc	                NVARCHAR(64) 
        DECLARE	@nv_Msg	                NVARCHAR(MAX) 

	SET @nv_Loc = N'csiDataStoreAreInsertsPending'
	SET @nv_Msg = N'Check to see if inserts are pending in DataStoreSync for ProcessedTxnId = ' + @pc_TxnId

	SELECT @i_Cnt = COUNT(*)
	FROM DataStoreSync WITH(NOLOCK)
	WHERE ProcessedTxnId = @pc_TxnId

	IF (@i_Cnt = 0)
		SET @bit_AreInsertsPending = @BIT_TRUE
	ELSE
		SET @bit_AreInsertsPending = @BIT_FALSE

	RETURN @bit_AreInsertsPending
END
GO
