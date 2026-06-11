DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiEmailPurgeQueue')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiEmailPurgeQueue;
 	END IF;
END $$;

CREATE PROCEDURE csiEmailPurgeQueue()
language plpgsql
as $$
DECLARE
	RetentionPeriod integer;
    FailureRetentionPeriod integer;
BEGIN
	
	SELECT TValue::integer INTO RetentionPeriod
	FROM InSiteSiteInfo
	WHERE TName = 'EmailNotificationRetentionPeriod';

	SELECT TValue::integer INTO FailureRetentionPeriod
	FROM InSiteSiteInfo
	WHERE TName = 'EmailNotificationFailureRetentionPeriod';

	-- Successful E-mails
	DELETE FROM EmailQueue
	WHERE ProcessingStatus = 1
	AND EXTRACT(EPOCH FROM (clock_timestamp() - CreatedDate)) / 3600 > RetentionPeriod;

	-- Failed E-mails
	DELETE FROM EmailQueue
	WHERE ProcessingStatus = 2
	AND EXTRACT(EPOCH FROM (clock_timestamp() - CreatedDate)) / 3600 > FailureRetentionPeriod;
END;
$$;