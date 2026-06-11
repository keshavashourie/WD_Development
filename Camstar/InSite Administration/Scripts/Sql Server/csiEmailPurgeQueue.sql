--------------------------------------------------------------------------------
-- SCRIPT: csiEmailPurgeQueue.sql
-- DESCR: 
-- HISTORY:
--
-- Copyright Siemens 2023  
--
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiEmailPurgeQueue' 
	   AND 	  type = 'P')
    DROP PROCEDURE csiEmailPurgeQueue
GO

CREATE PROCEDURE csiEmailPurgeQueue      
AS
--
-- Copyright Siemens 2023  
--
    DECLARE @RetentionPeriod int
    DECLARE @FailureRetentionPeriod int
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    SELECT @RetentionPeriod=CAST(TValue AS INT)
    FROM InSiteSiteInfo
    WHERE TName='EmailNotificationRetentionPeriod'

    SELECT @FailureRetentionPeriod=CAST(TValue AS INT)
    FROM InSiteSiteInfo
    WHERE TName='EmailNotificationFailureRetentionPeriod'

    -- Successful E-mails
    DELETE FROM EmailQueue
    WHERE ProcessingStatus=1
    AND DATEDIFF(hour,CreatedDate,GETDATE())>@RetentionPeriod

    -- Failed E-mails
    DELETE FROM EmailQueue
    WHERE ProcessingStatus=2
    AND DATEDIFF(hour,CreatedDate,GETDATE())>@FailureRetentionPeriod
END

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO
