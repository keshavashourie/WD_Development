ALTER PROCEDURE CSI_PurgeUtil_TableSetup_UpdateSetupTableSkipped
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_TableSetup_UpdateSetupTableSkipped
                     Insert skipped purging setup table in the CSI_PURGEUTIL_SETUPTABLESSKIPPED table.
  Author           : Gunn Wei Teong
  Date             : 13 Sept 2024
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvSetupName                    NVARCHAR(40)
    , @pvTableName                    NVARCHAR(30)
    , @pvInstanceCol1A                NVARCHAR(30)
    , @pvInstanceCol1B                NVARCHAR(30)
    , @pvInstanceCol1C                NVARCHAR(30)
    , @pvInstanceCol2                NVARCHAR(30)
    , @pvInstanceParentCol            NVARCHAR(30) 
    , @pvParentTable                NVARCHAR(30) 
    , @pvParentTableLinkCol         NVARCHAR(30) 
    , @pvCommitBatchSize            INTEGER = 1000
    , @pvSQLQueryPredicate          NVARCHAR(40)
    , @pvSourceDatabase                NVARCHAR(128)
    , @pvSourceSchema                NVARCHAR(128)
    , @pvTableLevel                    INTEGER
    , @pvRemarks                    NVARCHAR(1000)
    ) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE  @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
DECLARE @pvNextInstanceID           NVARCHAR(16);
--
BEGIN 
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRY
            BEGIN TRANSACTION;
            EXECUTE CSI_PurgeUtil_GetInstance @pvNextInstanceID OUTPUT, 'CSI_PURGEUTIL_SEQ';
            INSERT INTO CSI_PurgeUtil_SetupTablesSkipped
            (
                SetupTablesId
                , SetupId
                , TableName
                , InstanceCol1A
                , InstanceCol1B
                , InstanceCol1C
                , InstanceCol2
                , InstanceParentCol
                , ParentTable
                , ParentTableLinkCol
                , TxnDate
                , CommitBatchSize
                , SQLQueryPredicate
                , TableLevel
                , Remarks
            )
            VALUES
            (
                @pvNextInstanceID
                , (SELECT SetupId FROM CSI_PURGEUTIL_SETUP su WHERE UPPER(su.SetupName) = UPPER(@pvSetupName))
                , @pvTableName
                , @pvInstanceCol1A
                , @pvInstanceCol1B
                , @pvInstanceCol1C
                , @pvInstanceCol2
                , @pvInstanceParentCol
                , @pvParentTable
                , @pvParentTableLinkCol
                , GetDate()
                , @pvCommitBatchSize
                , @pvSQLQueryPredicate
                , @pvTableLevel
                , @pvRemarks
            );            --
            IF @@TRANCOUNT > 0    
                COMMIT TRANSACTION;
        END TRY
        BEGIN CATCH
            SET @ErrorMessage = ERROR_MESSAGE(); 
            SET @vProgID = @vObject_Name + '.' + 'Insert a skipped purging setup table ' + @pvTableName;  
            RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --------------------------------------------------------------------------
        SET @vProgID = @vObject_Name + '.SUCCESSFULL'; 
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 
            ROLLBACK TRANSACTION;
        IF @ErrorMessage IS NULL
            SET @ErrorMessage  = ERROR_MESSAGE();
        IF @ErrorSeverity IS NULL
            SET @ErrorSeverity = ERROR_SEVERITY(); 
        IF @ErrorState IS NULL
            SET @ErrorState = ERROR_STATE();
        IF @vProgID IS NULL
		    SET @vProgID = @vObject_Name + '.OTHER ERROR'; 
		EXECUTE CSI_PurgeUtil_Global_Log @pvProgID=@vProgID, @pvErrMsg=@ErrorMessage; 
		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END CATCH
END;
GO
