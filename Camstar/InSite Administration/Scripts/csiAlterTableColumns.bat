@ECHO OFF
REM --------------------------------------------------------------------------------------------------
REM File:              csiAlterTableColumns.bat
REM Version:           1.0
REM Author:            Purushotham Neelakantachar
REM Create Date:       21-DEC-2006
REM Database:          ORACLE/DB2/SQL
REM ------------------------------------------------------------
REM Purpose: To alter the column length of columns in some InSite tables
REM ------------------------------------------------------------
REM History:
REM
REM Name			Date		Description
REM --------------------------	----------	-------------------------------------------------
REM Bill Lippard		04/23/2007	SPR S9984 - Added copyright notice.
REM 
REM Purushotham Neelakantachar	05/27/2008 	Modified the code for SPR S12618
REM						- Included a new input parameter: DB_SERVERNAME
REM						  This parameter will be used only for SQL SERVER
REM			    			- Replaced SQL Server's OSQL commands with SQLCMD
REM
REM Purushotham Neelakantachar	06/19/2008 	Modified the code for SPR S12868
REM						- "ORACLE" section
REM					   	  - Replaced the procedure call with a call
REM					     	    to execute csiAlterPurgeRestoreSetUp.or.sql file
REM						- "SQLSERVER" section
REM					   	  - Replaced the procedure call with a call
REM					     	    to execute csiAlterPurgeRestoreSetUp.sql file
REM
REM Copyright Siemens 2023  
REM --------------------------------------------------------------------------------------------------

SET ERRORSTATUS=0

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
GOTO VALIDATEINPUTPARAMETERS
:END

REM Set SQLSERVER related data
REM --------------------------
:SETSQLSERVER
SET DB_SERVERNAME=%2
SET DB_NAME=%3
SET DB_USERNAME=%4
SET DB_PASSWORD=%5
GOTO VALIDATEINPUTPARAMETERS
:END

REM Validate input parameters
REM -------------------------
:VALIDATEINPUTPARAMETERS
IF (%DB_NAME%)==() GOTO USAGE
IF (%DB_USERNAME%)==() GOTO USAGE
IF (%DB_PASSWORD%)==() GOTO USAGE
IF /I (%DB_TYPE%) EQU (ORACLE)    GOTO ORACLE
IF /I (%DB_TYPE%) EQU (SQLSERVER) GOTO SQLSERVER
IF /I (%DB_TYPE%) EQU (DB2)       GOTO DB2
GOTO USAGE
:END


:usage
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO   "csiAlterTableColumns <DB2|ORACLE|SQLSERVER> <Database Host> <Database Name> <User> <Password>"
ECHO Where
ECHO  "<DB2|ORACLE|SQLSERVER>"	- Specify database vendor
ECHO  "<Database Host>"	        - For ORACLE and DB2: Not Required
ECHO				  For SQL SERVER: Database Server Name - Required
ECHO  "<Database Name>"	        - Native database name/alias
ECHO  "<User>"			- Insite database user/logon
ECHO  "<Password>"		- Insite database user password
ECHO.
ECHO  Example :- "csiAlterTableColumns ORACLE PROD InSiteAdmin admin"
ECHO		 "csiAlterTableColumns SQLSERVER DBSERVERNAME PROD InSiteAdmin admin"
ECHO		 "csiAlterTableColumns DB2 PROD InSiteAdmin admin"
ECHO 
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END

:DB2
ECHO Altering table columns for database %DB_NAME% (DB2)
ECHO DB2 CONNECT TO %DB_NAME% USER %DB_USERNAME% USING %DB_PASSWORD% > tmproc_%DB_NAME%.cmd
ECHO DB2 -td@ -o- -vf CSIALTERPURGERESTORESETUP.db2 >> tmproc_%DB_NAME%.cmd
ECHO DB2 SELECT RTRIM(Message) FROM ALTERTABLELOG >> tmproc_%DB_NAME%.cmd
ECHO DB2 DROP TABLE ALTERTABLELOG >> tmproc_%DB_NAME%.cmd
ECHO DB2 CONNECT RESET >> tmproc_%DB_NAME%.cmd
DB2CMD /c /w /i tmproc_%DB_NAME%.cmd
DEL tmproc_%DB_NAME%.cmd
GOTO EXIT
:END

:ORACLE
CD ".\Oracle"
ECHO Altering table columns for database %DB_NAME% (ORACLE)
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% > tmproc_%DB_NAME%.sql
ECHO SET SERVEROUTPUT ON SIZE 1000000 >> tmproc_%DB_NAME%.sql
ECHO @csiAlterPurgeRestoreSetUp.or.sql >> tmproc_%DB_NAME%.sql
ECHO EXIT; >> tmproc_%DB_NAME%.sql
SQLPLUS /NOLOG @tmproc_%DB_NAME%.sql
DEL tmproc_%DB_NAME%.sql
GOTO EXIT
:END

:SQLSERVER
IF (%DB_SERVERNAME%)==() GOTO USAGE
CD ".\SQL Server"
ECHO Altering table columns for database %DB_NAME% (SQL SERVER)
SQLCMD -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -i csiAlterPurgeRestoreSetUp.sql
GOTO EXIT
:END

:EXIT
TIME /t
CD ..
IF %ERRORSTATUS% NEQ 0 ECHO Error Altering table columns for database %DB_NAME%
ENDLOCAL
:END
