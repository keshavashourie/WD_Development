IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_LV_ErrorLog' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_LV_ErrorLog AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_LV_ErrorLog
AS

SELECT TOP 1000
    t.creation_datetime as "Creation Datetime", t.modulename as "Module Name", t.moduleversion as "Module Version", t.moduletype as "Module Type", t.stepno as "Step Number", t.progid as "Program ID", t.errmsg as "Error Message", t.errcode as "Error Code", t.containerid as "Container ID", t.resourceid as "Resource ID"
FROM CSI_PURGEUTIL_ERRORLOG t
ORDER BY t.creation_datetime desc, t.stepno desc

GO