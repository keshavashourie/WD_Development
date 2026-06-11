ALTER PROCEDURE CSI_PurgeUtil_TableSetup_ValidateSetupTable
/* ---------------------------------------------------------------------------
  Description      : CSI_PurgeUtil_TableSetup_ValidateSetupTable
                     Validate rows entering purging setup table (CSI_PURGEUTIL_SETUPTABLE).
  Author           : Gunn Wei Teong 
  Date             : 12 Sept 2024
  Compile in       : Source schema
  Called By        : 
  Call             : None
--------------------------------------------------------------------------- */
    ( @pvFound         INT OUTPUT 
    , @pvSetupName     NVARCHAR(40)
    , @pvTableName     NVARCHAR(40)
    , @pvTableLevel       INT
    , @pvParentTable   NVARCHAR(40)
    ) 
AS
DECLARE @ErrorMessage               NVARCHAR(4000); -- Message text.
DECLARE @ErrorSeverity              INT;            -- Severity.
DECLARE @ErrorState                 INT;            -- State.
DECLARE @vObject_Name               NVARCHAR(128) = OBJECT_NAME(@@PROCID);
DECLARE @vProgID                    NVARCHAR(255) = @vObject_Name;
--
DECLARE  @vCSI_PurgeUtil_ErrorLog_Tab CSI_PurgeUtil_ErrorLog_Tab;
BEGIN 
    SET NOCOUNT ON;
    SET @vProgID = @vObject_Name + '.START'; 
    BEGIN TRY -- RAISERROR with severity 11-19 will cause execution to jump to the CATCH block.
        ---------------------------------------------------------------------------
        -- Validations
        ---------------------------------------------------------------------------
        SET @vProgID = @vObject_Name + '.Validate parameters';
        IF @pvSetupName IS Null 
            RAISERROR ('SetupName must not be blank', 16, 1);
        IF @pvTableName IS Null 
            RAISERROR ('TableName must not be blank', 16, 1);
        IF @pvTableLevel IS Null 
            RAISERROR ('TableLevel must not be blank', 16, 1);
        IF NOT @pvTableLevel = 1
        BEGIN
            IF @pvParentTable IS Null 
                RAISERROR ('ParentTable must not be blank', 16, 1);
        END;
        ---------------------------------------------------------------------------
        BEGIN TRY 
            IF NOT @pvTableLevel = 1
            BEGIN
                IF (UPPER(@pvTableName) = UPPER(@pvParentTable))
                BEGIN
                    SET @pvFound = 2;
                    RETURN;
                END;
                
                SELECT @pvFound = COUNT(1)
                FROM CSI_PurgeUtil_Setup su
                INNER JOIN CSI_PurgeUtil_SetupTables sut ON sut.SetupId = su.SetupId
                WHERE UPPER(su.SetupName) = UPPER(@pvSetupName)
                    AND UPPER(sut.TableName) = UPPER(@pvTableName)
                    AND sut.TableLevel = @pvTableLevel
                    AND UPPER(sut.ParentTable) = UPPER(@pvParentTable)
                    ;
            END;
            ELSE 
            BEGIN
                SELECT @pvFound = COUNT(1)
                FROM CSI_PurgeUtil_Setup su
                INNER JOIN CSI_PurgeUtil_SetupTables sut ON sut.SetupId = su.SetupId
                WHERE UPPER(su.SetupName) = UPPER(@pvSetupName)
                    AND UPPER(sut.TableName) = UPPER(@pvTableName)
                    AND sut.TableLevel = @pvTableLevel
                    ;
            END;
        END TRY
        BEGIN CATCH
            SET @ErrorMessage = ERROR_MESSAGE(); 
            SET @vProgID = @vObject_Name + '.' + 'Validate check on purging setup table ' + @pvTableName + ' for Setup ' + @pvSetupName; 
            RAISERROR (@ErrorMessage, 16, 1);
        END CATCH
        --------------------------------------------------------------------------
        SET @vProgID = @vObject_Name + '.SUCCESSFULL';
        RETURN 0;
    END TRY
    BEGIN CATCH
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