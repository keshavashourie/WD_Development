------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreIsLocked.sql
-- DESCR:       Performs synchronization logic to determine if the txn can be executed.  Executes on the ODS
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiDataStoreIsLocked' AND Type = 'FN')
	DROP FUNCTION csiDataStoreIsLocked
GO

CREATE FUNCTION csiDataStoreIsLocked
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreIsLocked
-- Params:      <in> @pnv_TableType     NVARCHAR(8)
--              <in> @pc_TxnId          CHAR(16)
--              <in> @pnv_TxnType       NVARCHAR(10)
-- Returns:     BIT 1 or 0
-- Descr:       Performs synchronization logic to determine if the txn can be executed. Executes on the ODS	
--		

-- HISTORY:
--              06/25/2016      Dan Maloney      New function
--		07/27/2017	Dan Maloney      Removed unused variables @I_LOG_LEVEL_MAX, @I_LOG_LEVEL_MIN, @I_LOG_LEVEL_ERROR (US 51393)
--		07/27/2107	Dan Maloney      Removed unused parameter @pbi_LastId (US 51393)
--		07/27/2107	Dan Maloney      Removed unused pvariable @nv_Msg (US 51393)
--
-- Copyright Siemens 2023  
-----------------------------------------------------------------------------------------------------------------------------------------------------
(
        @pnv_TableType          NVARCHAR(8),
        @pc_TxnId               CHAR(16),
        @pnv_TxnType            NVARCHAR(10)
)
RETURNS BIT
AS
BEGIN
	--Variables defined as constants
        DECLARE @C_DUMMYTYPE                    CHAR(1) = 'D'
        DECLARE @C_TXNTYPE                      CHAR(1) = 'T'   
        DECLARE @C_UPDATES                      CHAR(7) = 'UPDATES'

        DECLARE @bit_InsertsPending             BIT
        DECLARE @nv_Loc	                        NVARCHAR(64) 

	SET @nv_Loc = N'csiDataStoreIsLocked'
	SET @bit_InsertsPending = 0

   	-- Rules for locking:
   	-- 	For gTableType = 'UPDATES'
   	--  	1) If TxnType='T' (i.e. cTXNTYPE), all inserts must be completed for this txnid and lower txn ids

	IF (@pnv_TableType = @C_UPDATES)
		IF (@pnv_TxnType = @C_TXNTYPE OR @pnv_TxnType = @C_DUMMYTYPE)
			--If processing DATASTOREUPDATESMAST and transaction is a regular transaction or a dummy transaction, 
			--return TRUE (1) if inserts are pending or FALSE (0) if inserts are not pending  
			EXEC @bit_InsertsPending  = csiDataStoreAreInsertsPending 
				@pc_TxnId = @pc_TxnId

	RETURN @bit_InsertsPending
END
GO
