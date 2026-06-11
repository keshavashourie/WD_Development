@ECHO OFF
REM --------------------------------------------------------------------------------------------------
REM Purpose: To reset purge and restore process
REM --------------------------------------------------------------------------------------------------
REM Description: Purge or Restore process is terminated abonormally,
REM It will not start next time. To clean the temporary tables, this 
REM batch file has to be executed.
REM --------------------------------------------------------------------------------------------------
REM File:              csiResetPurgeRestore.bat
REM Version:           1.0
REM Author:            Stanly Manuel
REM Create Date:       27-AUG-2004
REM Database:          ORACLE/DB2/SQL
REM 
REM Modification History:
REM Name			Date        	Action
REM -------------------------	----------	------------------------------------------------------
REM Stanly			07/14/2004	Initial Creation DB2
REM
REM Stanly			09/21/2004	oracle version added
REM
REM Stanly			10/05/2004	Sql Server version added
REM
REM Bill Lippard		12/01/2006	Update copyright notice (SPR S9984)
REM
REM Bill Lippard		04/23/2007	Update copyright notice (SPR S9984)
REM
REM Purushotham Neelakantachar	05/27/2008	Modified the code for SPR S12618:
REM						- Included a new input parameter: DB_SERVERNAME
REM						This parameter will be used only for SQL SERVER
REM						- Replaced SQL Server's OSQL commands with SQLCMD
REM 
REM Copyright Siemens 2023  
REM --------------------------------------------------------------------------------------------------
