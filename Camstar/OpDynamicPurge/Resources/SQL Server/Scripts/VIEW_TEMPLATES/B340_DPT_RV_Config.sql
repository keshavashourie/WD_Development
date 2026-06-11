IF NOT EXISTS (SELECT name FROM sysobjects WHERE name = 'DPT_RV_Config' AND type = 'V')
    EXECUTE sp_executesql N'CREATE VIEW DPT_RV_Config AS SELECT 1 AS dummy';
GO
ALTER VIEW DPT_RV_Config
AS

SELECT t.*
FROM CSI_PURGEUTIL_CONFIG t

GO
