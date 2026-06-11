@echo off
REM setlocal
REM  InstallProcedures.bat
REM  Simple batch file used install/create the database stored procedures
REM  required for the base Insite application.
REM
REM  Assumptions:
REM     - The appropriate sql tool(ie SQLPLUS, SQLCMD, DB2(DB2CLP)) is in the users PATH
REM
REM Modification History:
REM Name			Date        	Action
REM -------------------------	----------	------------------------------------------------------
REM Purushotham Neelakantachar	01/30/2006      DB2: Included call for the following objects. (S9851/S9897)
REM						- csiSiteStateAfterUpdateTrigger.db2 - creates a database trigger
REM						  on SITESTATE table .
REM						- csiLoadRecursiveCDOsData.db2 - creates the database stored
REM						  procedure.
REM						- Call to execute csiLoadRecursiveCDOsData stored procedure to
REM						  populate the staging table RecursiveCDOsData with initial data.
REM
REM Bill Lippard		06/20/2006      Added pause to exit if an error was encountered.
REM
REM Bill Lippard		12/01/2006      Added copyright notice (SPR S1184)
REM
REM Bill Lippard		02/24/2007	Modified exit code to pause on error (S11455)
REM
REM Bill Lippard		04/23/2007      Updated copyright notice (SPR S9984)
REM
REM Purushotham Neelakantachar	04/23/2007	Added code to install Modeling Export Import Purge database package
REM						for ORACLE version
REM
REM Purushotham Neelakantachar	04/23/2007	Removed LiveSync installation commands (SPR S9985)
REM
REM Purushotham Neelakantachar	04/23/2007	Oracle Version: Included code to spool the output to a file
REM
REM Purushotham Neelakantachar	06/13/2007      Removed pause after usage display (SPR S11478)
REM
REM Bill Lippard		06/19/2007      Changed call to csirestorecontainer.sql to call 
REM                 				csirestorecontainer.or.sql for consistency (SPR S12070)
REM
REM Purushotham Neelakantachar	02/29/2008	Added code to install Modeling Export Import Purge - SQL Server version
REM
REM Purushotham Neelakantachar	05/27/2008	Modified the code for SPR S12757
REM						- Included logic to exit with a error code
REM
REM Purushotham Neelakantachar	05/27/2008	Modified the code for SPR S12618
REM						- Included a new input parameter: DB_SERVERNAME
REM						This parameter will be used only for SQL SERVER
REM						- Replaced SQL Server's OSQL commands with SQLCMD
REM
REM Barry Etter			06/13/2008	Added RBAC scripts.
REM
REM Purushotham Neelakantachar	06/30/2008	Modified the code for Document Attachment Purge
REM						- Renamed csiExportImpurge related .sql files. Below are the new 
REM						  file names:	"csiEnterprisePurge.or.sql"
REM								"csiEnterprisePurge.sql"
REM
REM Barry Etter        		08/01/2008 	Added E-mail Notification Purge scripts
REM
REM Purushotham Neelakantachar	02/02/2009	Modified the code for SPR S14920
REM 						- Added code to trap Oracle compilation errors
REM						- Changed the order of execution for csiEnterprisePurge.or.sql
REM
REM Barry Etter			03/12/2009	Added User Access Roles scripts
REM
REM Purushotham Neelakantachar	05/26/2009	Modified the code for TFS #14629 
REM 						- Removed calls to below mentioned scripts
REM						  - Camstar Enterprise Purge
REM							- csiEnterprisePurge.or.sql
REM						  - Old Purge/Restore
REM							- csiPurgeContainer.or.sql
REM							- csiRestoreContainer.or.sql
REM							- csiPurgeRestoreSetUp.or.sql
REM						- Updated copyright notice
REM
REM Barry Etter			06/23/2009	Added Quality scripts
REM
REM Purushotham Neelakantachar	08/12/2009	Added Quality Object Inquiry scripts ( Oracle )
REM						- Migration of Designer Query to database
REM
REM Purushotham Neelakantachar	08/17/2009	Added Quality Object Inquiry scripts ( SQL Server )
REM						- Migration of Designer Query to database
REM
REM Copyright Siemens 2023  
REM --------------------------------------------------------------------------------------------------


TIME /t
SET ERRORSTATUS=0
SET INSITE_HOME=%CD%

REM Set local variables
REM -------------------
:SETVARIABLES
SET DB_TYPE=%1
IF (%DB_TYPE%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE) GOTO SETORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SETSQLSERVER
GOTO USAGE
:END

REM Set ORACLE related data
REM -----------------------
:SETORACLE
SET DB_NAME=%2
SET DB_USERNAME=%3
SET DB_PASSWORD=%4
SET BASE_DIR=%5
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set SQLSERVER related data
REM --------------------------
:SETSQLSERVER
SET DB_SERVERNAME=%2
SET DB_NAME=%3
SET DB_USERNAME=%4
SET DB_PASSWORD=%5
SET BASE_DIR=%6
GOTO VALIDATEINPUTPARAMETERS
:END

REM Validate input parameters
REM -------------------------
:VALIDATEINPUTPARAMETERS
SET INSITE_HOME=%CD%
IF (%DB_NAME%)==() GOTO USAGE
IF (%DB_USERNAME%)==() GOTO USAGE
IF (%DB_PASSWORD%)==() GOTO USAGE
IF (%BASE_DIR%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE)    GOTO ORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SQLSERVER
GOTO USAGE
:END


:ORACLE
ECHO Installing InSite Stored Procedures into Oracle database %DB_NAME% ...
ECHO Installing InSite Stored Procedures into Oracle database %DB_NAME% ... > "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
ECHO ----------------------------------------------------
ECHO Database Vendor            : %DB_TYPE%
ECHO Database Name              : %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO Base Directory             : %BASE_DIR%
ECHO ----------------------------------------------------
CD "%BASE_DIR%\Oracle"
ECHO Current Directory: %CD% >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log" >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
ECHO WHENEVER SQLERROR EXIT FAILURE  > tempproc_%DB_NAME%.sql
ECHO SET SERVEROUTPUT ON SIZE 1000000 >> tempproc_%DB_NAME%.sql
ECHO SET LINESIZE 300 >> tempproc_%DB_NAME%.sql
ECHO SET PAGESIZE 1000 >> tempproc_%DB_NAME%.sql
ECHO SET ECHO OFF >> tempproc_%DB_NAME%.sql
ECHO SET HEADING OFF >> tempproc_%DB_NAME%.sql
ECHO SET VERIFY OFF >> tempproc_%DB_NAME%.sql
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME%  >> tempproc_%DB_NAME%.sql
ECHO @csiHexTodec.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiIncrementString64.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiReturnTransactionID.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiUpdateInstanceID.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiGetMaxTxnId.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiValidateMaxTxnId.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiSetNextTxnId.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiresettxnid.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiCreateIndexes.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiGetPKColumns.or.sql  >> tempproc_%DB_NAME%.sql
ECHO @csiGetColumnList.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiCopyTable.or.sql >> tempproc_%DB_NAME%.sql    
ECHO @csiIDcontrol.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiBOMExplosion.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiGetContainerTaskResult.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiAuthRetrieveUserPermissions.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiAuthRetrieveUserRoles.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiEmailPurgeQueue.or.sql >> tempproc_%DB_NAME%.sql
ECHO @UserAccessRolesFunctions.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiGetAdjustedTime.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiCDOInstanceCount.or.sql >> tempproc_%DB_NAME%.sql
ECHO @QualityFunctions.or.sql >> tempproc_%DB_NAME%.sql
ECHO @csiQualityObjectInquiry.or.sql >> tempproc_%DB_NAME%.sql
ECHO @CommonFunctions.or.sql >> tempproc_%DB_NAME%.sql
ECHO EXIT >> tempproc_%DB_NAME%.sql
SQLPLUS /NOLOG @tempproc_%DB_NAME%.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	DEL tempproc_%DB_NAME%.sql
	GOTO EXIT
)
DEL tempproc_%DB_NAME%.sql
CD ..
FINDSTR /I /C:"SP2-" ".\StoredProcsInstall_%DB_NAME%.log" > NUL
IF %ERRORLEVEL%==0 SET ERRORSTATUS=99
FINDSTR /I /C:"ORA-" ".\StoredProcsInstall_%DB_NAME%.log" > NUL
IF %ERRORLEVEL%==0 SET ERRORSTATUS=99
IF %ERRORSTATUS% NEQ 99 ECHO. >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 99 ECHO InSite Stored procedures installation successful >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 99 ECHO InSite Stored procedures installation successful
GOTO EXIT
:END

:SQLSERVER
IF (%DB_SERVERNAME%)==() GOTO USAGE
ECHO Installing InSite Stored Procedures into SQL Server database %DB_NAME% ...
ECHO Installing InSite Stored Procedures into SQL Server database %DB_NAME% ... > "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
ECHO ----------------------------------------------------
ECHO Database Vendor            : %DB_TYPE%
ECHO Database Host              : %DB_SERVERNAME%
ECHO Database Name              : %DB_NAME%
ECHO Database User Name         : %DB_USERNAME%
ECHO Base Directory             : %BASE_DIR%
ECHO ----------------------------------------------------
CD "%INSITE_HOME%\SQL Server"
ECHO Current Directory: %CD% >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log" 
REM Set errorstatus if unable to connect to the database
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "select substring(db_name(),1,50)"  >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiBigInttoHex.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiHextoDec.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiIncrementString64.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiReturnTransactionID.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiUpdateInstanceID.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiGetMaxTxnId.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiValidateMaxTxnId.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiSetNextTxnId.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiresettxnid.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiGenTableDDL.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiGenStageDDL.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiBCPStageTables.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiCreateIndexes.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiGetPKColumns.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiGetColumnList.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiCopyTable.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiGetDerivedCDOs.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiBOMExplosion.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiAuthRetrieveUserPermissions.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiAuthRetrieveUserRoles.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiEmailPurgeQueue.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i UserAccessRolesFunctions.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiIDControl.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiGetAdjustedTime.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i QualityFunctions.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiQualityObjectInquiry.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i CommonFunctions.sql >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF ERRORLEVEL 1 (
	SET ERRORSTATUS=99
	GOTO EXIT
)
IF %ERRORSTATUS% NEQ 99 ECHO. >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 99 ECHO InSite Stored procedures installation successful >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 99 ECHO InSite Stored procedures installation successful
GOTO EXIT
:END

:DB2
ECHO Compile procedures into DB2 database %DB_NAME% ...
cd "%5\db2"
ECHO db2 connect to %DB_NAME% user %DB_USERNAME% using %DB_PASSWORD% > tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf CSIHEXTODEC.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf csiGetMaxTxnId.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf csiValidateMaxTxnId.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf csiSetNextTxnId.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf CSIRESETTXNID.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf CSIINCREMENTSTRING64.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf CSIRETURNTRANSACTIONID.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf CSIUPDATEINSTANCEID.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf csiCreateIndexes.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf csiGetPKColumns.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf csiGetColumnList.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf csiCopyTable.db2 >> tempproc_%DB_NAME%.cmd

ECHO db2 -td@ -o- -vf csiIDcontrol.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf csiBOMExplosion.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf csiLoadRecursiveCDOsData.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 -td@ -o- -vf csiSiteStateAfterUpdateTrigger.db2 >> tempproc_%DB_NAME%.cmd
ECHO db2 connect reset >> tempproc_%DB_NAME%.cmd
db2cmd /c /w /i tempproc_%DB_NAME%.cmd
del tempproc_%DB_NAME%.cmd
ECHO db2 CONNECT TO %DB_NAME% USER %DB_USERNAME% USING %DB_PASSWORD% > temproc_%DB_NAME%.cmd
ECHO db2 CALL csiLoadRecursiveCDOsData  >> temproc_%DB_NAME%.cmd
ECHO db2 CONNECT RESET >> temproc_%DB_NAME%.cmd
DB2CMD /c /w /i temproc_%DB_NAME%.cmd
DEL temproc_%DB_NAME%.cmd
CD ..
goto exit
:end

:USAGE
ECHO ------------------------------------------------------------------------------
ECHO USAGE:  
ECHO   "InstallProcedures <ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password> <Base dir>"
ECHO  "<ORACLE|SQLSERVER>"      - Specify database vendor
ECHO  "<Database Host>"         - For ORACLE and DB2: Not Required
ECHO                              For SQL SERVER: Database Host Name or IP Address - Required
ECHO  "<Database Name>"         - Native Database Name/Alias
ECHO  "<User>"                  - Insite database user/logon
ECHO  "<Password>"              - Insite database user password
ECHO  "<Base Dir>"              - Insite Server scripts directory. Ex: "c:\program files\camstar\insite administration\scripts"
ECHO  Examples: 
ECHO  "InstallProcedures ORACLE PROD insiteadmin admin ."
ECHO  "InstallProcedures SQLSERVER DBSERVERNAME PROD insiteadmin admin ."
ECHO  "InstallProcedures SQLSERVER 10.10.10.10 PROD insiteadmin admin ."
ECHO.
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END

:EXIT
ECHO. >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF %ERRORSTATUS%==99 ECHO InSite Stored procedures installation failed >> "%INSITE_HOME%/StoredProcsInstall_%DB_NAME%.log"
IF %ERRORSTATUS% NEQ 0 (
	ECHO Error Installing Procedures - ErrorStatus: %ERRORSTATUS%
	PAUSE
	)
IF (%DB_SERVERNAME%) NEQ " " ECHO Log file "StoredProcsInstall_%DB_NAME%.log" generated in "%INSITE_HOME%" directory
IF %ERRORSTATUS% NEQ 0 EXIT %ERRORSTATUS%
:END
