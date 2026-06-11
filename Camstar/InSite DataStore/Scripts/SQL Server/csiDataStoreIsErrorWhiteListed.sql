------------------------------------------------------------------------------------------------------------------------------------------------------
-- SCRIPT:      csiDataStoreIsErrorWhiteListed.sql
-- DESCR:       Queries DataStoreWhiteList to see ERROR_NUMBER() (pn_ErrorNumber) is in the table.  
--              Returns TRUE if ERROR_NUMBER() is white listed, otherwise the function returns FALSE.  
--              Executes on the ODS	
-- Copyright Siemens 2023  
------------------------------------------------------------------------------------------------------------------------------------------------------



IF EXISTS (SELECT Name FROM sysobjects WHERE Name = 'csiDataStoreIsErrorWhiteListed' AND Type = 'FN')
	DROP FUNCTION csiDataStoreIsErrorWhiteListed
GO

CREATE FUNCTION csiDataStoreIsErrorWhiteListed
------------------------------------------------------------------------------------------------------------------------------------------------------
-- Name:        csiDataStoreIsErrorWhiteListed
-- Params:      <in> @pn_ErrorNumber	INT
-- Returns:     BIT 1/0 for TRUE/FALSE
-- Descr:       Queries DataStoreWhiteList to see if ERROR_NUMBER() (@pn_ErrorNumber) is in the table.  
--		Returns TRUE if ERROR_NUMBER() is white listed, otherwise the function returns FALSE.  Executes on the ODS	
--		

-- HISTORY:
--	        06/25/2016      Dan Maloney      New function (US 51393)
--
-- Copyright Siemens 2023  
-----------------------------------------------------------------------------------------------------------------------------------------------------
(
        @pn_ErrorNumber         INT
)
RETURNS BIT
AS
BEGIN
        DECLARE @BIT_FALSE      BIT = 0
        DECLARE @BIT_TRUE       BIT = 1
        DECLARE @bit_Ret        BIT
        DECLARE @bi_Cnt	        BIGINT
	
	SELECT 
	@bi_Cnt = COUNT(*)
	FROM DataStoreWhiteList WITH(NOLOCK)
	WHERE ErrorId = @pn_ErrorNumber

	IF (@bi_Cnt = 0) 
		SET @bit_Ret = @BIT_FALSE
	ELSE
		SET @bit_Ret = @BIT_TRUE

	RETURN @bit_Ret;
END
GO


