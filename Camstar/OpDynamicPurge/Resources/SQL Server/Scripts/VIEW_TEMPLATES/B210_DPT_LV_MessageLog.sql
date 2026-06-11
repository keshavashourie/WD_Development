IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_LV_MessageLog' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_LV_MessageLog AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_LV_MessageLog
AS

SELECT TOP 200 t.messagedate as "Message Date", t.messageid as "Message ID", t.messagetext as "Message Text"
FROM CSI_PURGEUTIL_MESSAGELOGS t
/*WHERE ROWNUM < 101*/
ORDER BY t.messagedate desc, t.messageid desc

GO