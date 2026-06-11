PRINT "--------------------------------------------------------------------------------"
PRINT "Stored Procedures"
PRINT "--------------------------------------------------------------------------------"

PRINT "Start processing the script A160_CSI_PurgeUtil_CreateStoredProcedure"
GO
:r .\Scripts\A160_CSI_PurgeUtil_CreateStoredProcedureStubs.sql 
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A170_CSI_PurgeUtil_GetInstance"
GO
:r .\Scripts\A170_CSI_PurgeUtil_GetInstance.sql 
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A172_CSI_PurgeUtil_CheckInstanceExist"
GO
:r .\Scripts\A172_CSI_PurgeUtil_CheckInstanceExist.sql 
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A180_CSI_PurgeUtil_ErrorLog_Record"
GO
:r .\Scripts\A180_CSI_PurgeUtil_ErrorLog_Record.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A190_CSI_PurgeUtil_Global_Log"
GO
:r .\Scripts\A190_CSI_PurgeUtil_Global_Log.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A200_CSI_PurgeUtil_Global_LogMessage"
GO
:r .\Scripts\A200_CSI_PurgeUtil_Global_LogMessage.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A202_CSI_PurgeUtil_Global_LogRunStat"
GO
:r .\Scripts\A202_CSI_PurgeUtil_Global_LogRunStat.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A203_CSI_PurgeUtil_Global_LogArchiveStat"
GO
:r .\Scripts\A203_CSI_PurgeUtil_Global_LogArchiveStat.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A210_CSI_PurgeUtil_Global_GetTableColumnNames"
GO
:r .\Scripts\A210_CSI_PurgeUtil_Global_GetTableColumnNames.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A220_CSI_PurgeUtil_TableSetup_SetupExists"
GO
:r .\Scripts\A220_CSI_PurgeUtil_TableSetup_SetupExists.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A230_CSI_PurgeUtil_TableSetup_SetupTableExists"
GO
:r .\Scripts\A230_CSI_PurgeUtil_TableSetup_SetupTableExists.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A231_CSI_PurgeUtil_TableSetup_ValidateSetupTable"
GO
:r .\Scripts\A231_CSI_PurgeUtil_TableSetup_ValidateSetupTable.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A240_CSI_PurgeUtil_Global_TableExists"
GO
:r .\Scripts\A240_CSI_PurgeUtil_Global_TableExists.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A250_CSI_PurgeUtil_Global_TableColumnExists"
GO
:r .\Scripts\A250_CSI_PurgeUtil_Global_TableColumnExists.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A260_CSI_PurgeUtil_TableSetup_UpdateSetup"
GO
:r .\Scripts\A260_CSI_PurgeUtil_TableSetup_UpdateSetup.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A270_CSI_PurgeUtil_TableSetup_UpdateSetupTable"
GO
:r .\Scripts\A270_CSI_PurgeUtil_TableSetup_UpdateSetupTable.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A271_CSI_PurgeUtil_TableSetup_UpdateSetupTableSkipped"
GO
:r .\Scripts\A271_CSI_PurgeUtil_TableSetup_UpdateSetupTableSkipped.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A280_CSI_PurgeUtil_TableSetup_DeleteSetup"
GO
:r .\Scripts\A280_CSI_PurgeUtil_TableSetup_DeleteSetup.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A290_CSI_PurgeUtil_CreateORAlterArchiveTable"
GO
:r .\Scripts\A290_CSI_PurgeUtil_CreateORAlterArchiveTable.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A291_CSI_PurgeUtil_CreateORAlterAllArchiveTable"
GO
:r .\Scripts\A291_CSI_PurgeUtil_CreateORAlterAllArchiveTable.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A310_CSI_PurgeUtil_DeleByTable_tpl"
GO
:r .\Scripts\A310_CSI_PurgeUtil_DeleByTable_tpl.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A310_CSI_PurgeUtil_Rest_tpl"
GO
:r .\Scripts\A310_CSI_PurgeUtil_Rest_tpl.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A320_CSI_PurgeUtil_CreateSP"
GO
:r .\Scripts\A320_CSI_PurgeUtil_CreateSP.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A325_CSI_PurgeUtil_CreateUserView"
GO
:r .\Scripts\A325_CSI_PurgeUtil_CreateUserView.sql
PRINT "Completed processing"; PRINT " "


PRINT "Start processing the script A330_CSI_PurgeUtil_TableSetup_GenerateObject"
GO
:r .\Scripts\A330_CSI_PurgeUtil_TableSetup_GenerateObject.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A340_CSI_PurgeUtil_Execute_PurgeByInstanceId"
GO
:r .\Scripts\A340_CSI_PurgeUtil_Execute_PurgeByInstanceId.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A341_CSI_PurgeUtil_Execute_PurgeByInstance"
GO
:r .\Scripts\A341_CSI_PurgeUtil_Execute_PurgeByInstance.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A350_CSI_PurgeUtil_Execute_PurgeBySetup"
GO
:r .\Scripts\A350_CSI_PurgeUtil_Execute_PurgeBySetup.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A351_CSI_PurgeUtil_Execute_PurgeBySetupJob"
GO
:r .\Scripts\A351_CSI_PurgeUtil_Execute_PurgeBySetupJob.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A352_CSI_PurgeUtil_Execute_PurgeBySingleHierarchy"
GO
:r .\Scripts\A352_CSI_PurgeUtil_Execute_PurgeBySingleHierarchy.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A360_CSI_PurgeUtil_Execute_RestoreByInstanceId"
GO
:r .\Scripts\A360_CSI_PurgeUtil_Execute_RestoreByInstanceId.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A370_CSI_PurgeUtil_Execute_RestoreByBatchExecutionId"
GO
:r .\Scripts\A370_CSI_PurgeUtil_Execute_RestoreByBatchExecutionId.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A380_CSI_PurgeUtil_Execute_ActivateSetup"
GO
:r .\Scripts\A380_CSI_PurgeUtil_Execute_ActivateSetup.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A390_CSI_PurgeUtil_Execute_DeActivateSetup"
GO
:r .\Scripts\A390_CSI_PurgeUtil_Execute_DeActivateSetup.sql
PRINT "Completed processing"; PRINT " "

PRINT "Start processing the script A400_CSI_PurgeUtil_HousekeepArchive"
GO
:r .\Scripts\A400_CSI_PurgeUtil_HousekeepArchive.sql
PRINT "Completed processing"; PRINT " "

