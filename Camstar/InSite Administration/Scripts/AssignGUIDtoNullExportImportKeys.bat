@ECHO OFF
REM -----------------------------------------------------------------------------------------------------------------------------
REM © 2017 Siemens Product Lifecycle Management Software Inc.
REM
REM AsignGUIDtoNullExportImportKeys.bat
REM 	Batch file used to run the AsignGUIDtoNullExportImportKeys.or.sql or AsignGUIDtoNullExportImportKeys.sql scripts.
REM
REM Assumptions:
REM	%CamstarInstall%=C:\Program Files\Camstar
REM
REM   	Location of this .bat file (AsignGUIDtoNullExportImportKeys.bat):
REM		%CamstarInstall%\InSite Administration\Scripts
REM
REM     Location of the SQL Scripts:
REM	 	AsignGUIDtoNullExportImportKeys.or.sql(ORACLE): %CamstarInstall%\InSite Administration\Scripts\Oracle
REM	 	AsignGUIDtoNullExportImportKeys.sql(SQLSERVER): %CamstarInstall%\InSite Administration\Scripts\SQL Server
REM
REM     Location of the log from the execution of this script:
REM		%CamstarInstall%\InSite Administration\Scripts
REM
REM   	The appropriate sql tool (ie sqlplus or sqlcmd) is in the users %PATH%
REM
REM Execution:
REM    	Run from Camstar Home directory (same as location of this .bat file): 
REM		%CamstarInstall%\InSite Administration\Scripts
REM
REM 	Example: 
REM		(ORACLE):	AssignGUIDtoNullExportImportKeys.bat ORACLE PROD InSiteAdmin admin 0 1 0 0 0 0 0 
REM		(ORACLE):	AssignGUIDtoNullExportImportKeys.bat ORACLE PROD InSiteAdmin admin 
REM		(SQLSERVER):	AssignGUIDtoNullExportImportKeys.bat SQLSERVER PRODSERVER PROD InSiteAdmin admin 
REM		(SQLSERVER):	AssignGUIDtoNullExportImportKeys.bat SQLSERVER 10.10.10.10 PROD InSiteAdmin admin 
REM		(SQLSERVER):	AssignGUIDtoNullExportImportKeys.bat SQLSERVER 10.10.10.10 PROD InSiteAdmin admin 0 0 0 1 0 0 0
REM
REM Usage (Input Parameters):
REM 	AsignGUIDtoNullExportImportKeys.bat "<ORACLE|SQLSERVER>" "<Database Host if SQLSERVER>" "<Database>" "<user>" "<password>"  ...continue next line 
REM 		"<Modeling Flag [1|0]>" "<History Flag [1|0]>" "<System Flag [1|0]>" "<Tracking Flag [1|0]>" "<None Flag [1|0]>" "<Undefined Flag [1|0]>" "<Unknown Flag [1|0]>" 
REM
REM	The flag input parameters are 1 or 0 values that will be passed into the AssignGUIDtoNullExportImport.or.sql (Oracle) or AssignGUIDtoNullExportImportKeys.sql (SQL Server) 
REM	Theses flag parameters determine which CDODefinition tables will be updated as the flag parameters are mapped to values for CDODefinition.StorageCategoryId column
REM	For Example, a TrackingFlag value of 1 is mapped to the StorageCategoryId value of 6 and if the other flag values are all zero, then only CDODefinition tables that are
REM	Tracking tables will be updated if their ExportImportKey column has a NULL value
REM	The flag parameters can be any combination of 0 and 1 values, depending on the type of CDODefinition tables you want to target for an update 
REM
REM	Where:
REM 	"<ORACLE|SQLSERVER>"   		- Specify database vendor
REM 	"<Database Host>"      		- For ORACLE: Is not required 
REM                           		- For SQL SERVER: Is required and is the Database Server Name or IP Address 
REM 	"<Database Name>"      		- Native database name or alias 
REM 	"<User>"               		- Insite database user logon 
REM 	"<Password>"         		- Insite database user password 
REM	"<Modeling Flag  [1|0]>"	- Flag to be passed into database script that maps to corresponing CDODefinition.StorageCategoryId value of 1 if the Flag value is 1
REM	"<History Flag 	 [1|0]>"	- Flag to be passed into database script that maps to corresponing CDODefinition.StorageCategoryId value of 2 if the Flag value is 1
REM	"<System Flag 	 [1|0]>"	- Flag to be passed into database script that maps to corresponing CDODefinition.StorageCategoryId value of 5 if the Flag value is 1
REM	"<Tracking Flag  [1|0]>"	- Flag to be passed into database script that maps to corresponing CDODefinition.StorageCategoryId value of 6 if the Flag value is 1
REM	"<None Flag 	 [1|0]>"	- Flag to be passed into database script that maps to corresponing CDODefinition.StorageCategoryId value of 8 if the Flag value is 1
REM	"<Undefined Flag [1|0]>"	- Flag to be passed into database script that maps to corresponing CDODefinition.StorageCategoryId value of 0 if the Flag value is 1
REM	"<Unknown Flag	 [1|0]>"	- Flag to be passed into database script that maps to corresponing CDODefinition.StorageCategoryId value of 4 if the Flag value is 1
REM	
REM Error Detection:  
REM		Exit status of sqlplus or sqlcmd is checked.  A comment indicating an error occurred is written to the log file
REM		For Oracle, the log file is also scanned for ORA- messages and if any are found a comment indicating an error occurred is written to the log file
REM
REM Modification History:
REM Name			Date		Action
REM --------------		----------	----------------
REM Dan Maloney			04/22/2015	New batch file
REM Dan Maloney			02/16/2015	Modified script to enhance logging, and accept input parameters to be passed to database scripts
REM						Modified batch file error handling
REM
REM -----------------------------------------------------------------------------------------------------------------------------

:INITIALIZE
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
REM Initialize local script variables
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
SETLOCAL
SET STARTTIME=%date% %time%
SET INSITE_HOME=%CD%
SET DB_VENDOR=
SET DB_SERVERNAME=
SET DB_NAME=
SET DB_USERNAME=
SET DB_PASSWORD=
SET INCMODELINGFLAG=
SET INCHISTORYFLAG=
SET INCSYSTEMFLAG=
SET INCTRACKINGFLAG=
SET INCNONEFLAG=
SET INCUNDEFINEDFLAG=
SET INCUNKNOWNFLAG=
SET LOGFILE=
SET DBSCRIPT=
SET CURRENTRETURNCODE=0
:END

:SETVARIABLES
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
REM Set DB_VENDOR 
REM Branch in the .bat script to the appropriate section to execute based on whether DB_VENDOR is ORACLE or SQLSERVER or NULL
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
SET DB_VENDOR=%1
REM IF (%DB_VENDOR%)==() GOTO USAGE
IF /I (%DB_VENDOR%) EQU (ORACLE) GOTO SETORACLE
IF /I (%DB_VENDOR%) EQU (SQLSERVER) GOTO SETSQLSERVER
GOTO USAGE
:END

:SETORACLE
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
REM Set ORACLE related data
REM Set local environment variables with values passed in as parameters from command line.  11 arguements are expected for Oracle although the last
REM 7 are optional and if no value is supplied for the last 7 parameters (stored procedure input parameters), they will default to predetermined values
REM DB_VENODR already has been set, set remaining 10 local variables to passed in parameter values then jump to input parameter validation section of code
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
SET DB_NAME=%2
SET DB_USERNAME=%3
SET DB_PASSWORD=%4
SET PROC_ModelingFlag=%5
SET PROC_HistoryFlag=%6
SET PROC_SystemFlag=%7
SET PROC_TrackingFlag=%8
SET PROC_NoneFlag=%9
SHIFT
SET PROC_UndefinedFlag=%9
SHIFT
SET PROC_UnknownFlag=%9
GOTO VALIDATEINPUTPARAMETERS
:END

:SETSQLSERVER
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
REM Set SQLSERVER related data
REM Set local environment variables with values passed in as aparametersfrom command line.  12 arguements are expected for SQL Server although the last 
REM 7 are optional and if no value is supplied for the last 7 parameters (stored procedure input parameters), they will default to predetermined values
REM DB_VENODR already has been set, set remaining 11 local variables to passed in parameter values then jump to input parameter validation section of code
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
SET DB_SERVERNAME=%2
SET DB_NAME=%3
SET DB_USERNAME=%4
SET DB_PASSWORD=%5
SET PROC_ModelingFlag=%6
SET PROC_HistoryFlag=%7
SET PROC_SystemFlag=%8
SET PROC_TrackingFlag=%9
SHIFT
SET PROC_NoneFlag=%9
SHIFT
SET PROC_UndefinedFlag=%9
SHIFT
SET PROC_UnknownFlag=%9
GOTO VALIDATEINPUTPARAMETERS
:END

:VALIDATEINPUTPARAMETERS
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
REM Validate input parameters
REM Check local variable values to see if they are NULL and either jump to the USAGE section of code or set the local variables to default values
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
IF (%DB_NAME%)==() GOTO USAGE
IF (%DB_USERNAME%)==() GOTO USAGE
IF (%DB_PASSWORD%)==() GOTO USAGE
IF (%PROC_ModelingFlag%)==() SET PROC_ModelingFlag=1
IF (%PROC_HistoryFlag%)==() SET PROC_HistoryFlag=0
IF (%PROC_SystemFlag%)==() SET PROC_SystemFlag=0
IF (%PROC_TrackingFlag%)==() SET PROC_TrackingFlag=0
IF (%PROC_NoneFlag%)==() SET PROC_NoneFlag=0
IF (%PROC_UndefinedFlag%)==() SET PROC_UndefinedFlag=0
IF (%PROC_UnknownFlag%)==() SET PROC_UnknownFlag=0
IF /I (%DB_VENDOR%) EQU (ORACLE)    GOTO ORACLE
IF /I (%DB_VENDOR%) EQU (SQLSERVER) GOTO SQLSERVER
GOTO USAGE
:END

:ORACLE
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
REM Oracle specific processing
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
SET LOGFILE="%INSITE_HOME%\AssignGUIDtoNullExportImportKeys_%DB_NAME%.log"
SET DBSCRIPT="%INSITE_HOME%\Oracle\oracle_%DB_NAME%.sql"

ECHO © 2016 Siemens Product Lifecycle Management Software Inc. > %LOGFILE%
ECHO. >> %LOGFILE%
ECHO Start Time: %STARTTIME% >> %LOGFILE%
ECHO. >> %LOGFILE%
ECHO Insite Home: %INSITE_HOME% >> %LOGFILE%
ECHO. >> %LOGFILE%


ECHO [ DB Script Information ] >> %LOGFILE%
ECHO DB Script:  %DBSCRIPT% >> %LOGFILE%
ECHO. >> %LOGFILE% 

ECHO [ Log File Information ] >> %LOGFILE% 
ECHO Log file:  %LOGFILE% >> %LOGFILE%
ECHO Generated in directory: "%INSITE_HOME%" >> %LOGFILE%
ECHO. >> %LOGFILE%

ECHO [ Validated input parameters ] >> %LOGFILE% 
ECHO Databse Vendor {DB_VENDOR} : %DB_VENDOR% >> %LOGFILE%
ECHO Datasbe Server (HOST) {DB_SERVERNAME} : %DB_SERVERNAME% >> %LOGFILE%
ECHO Database Name  {DB_NAME} : %DB_NAME% >> %LOGFILE%
ECHO Database User Name {DB_USERNAME}: %DB_USERNAME% >> %LOGFILE%
ECHO Database Password {DB_PASSWORD}: %DB_PASSWORD% >> %LOGFILE%
ECHO. >> %LOGFILE%

ECHO [ Assigning system generated GUID for targeted tables with a NULL ] >> %LOGFILE%
ECHO [ value in the EXPORTIMPRTKEY column.  Target tables are based on ] >> %LOGFILE%
ECHO [ Input parameters:                                               ] >> %LOGFILE%
ECHO PROC_ModelingFlag: %PROC_ModelingFlag% >> %LOGFILE%
ECHO PROC_HistoryFlag: %PROC_HistoryFlag% >> %LOGFILE%
ECHO PROC_Systemlag: %PROC_SystemFlag% >> %LOGFILE%
ECHO PROC_TrackiingFlag:  %PROC_TrackingFlag% >> %LOGFILE%
ECHO PROC_NoneFlag:  %PROC_NoneFlag% >> %LOGFILE%
ECHO PROC_UndefinedFlag:  %PROC_UndefinedFlag% >> %LOGFILE%
ECHO PROC_UnknownFlag:  %PROC_UnknownFlag% >> %LOGFILE%
ECHO. >> %LOGFILE%

ECHO [ Database script content ] >> %LOGFILE%
ECHO Database Script: %DBSCRIPT% >> %LOGFILE%
ECHO. >> %LOGFILE%
ECHO WHENEVER SQLERROR EXIT FAILURE  > %DBSCRIPT%
ECHO CONNECT %DB_USERNAME%/%DB_PASSWORD%@%DB_NAME% >>  %DBSCRIPT%
ECHO WHENEVER SQLERROR CONTINUE  >> %DBSCRIPT%
ECHO SET SERVEROUTPUT ON SIZE 1000000 >> %DBSCRIPT%
ECHO SET FEEDBACK ON >> %DBSCRIPT%
ECHO SET ECHO OFF >> %DBSCRIPT%
ECHO SET LINESIZE 300 >> %DBSCRIPT%
ECHO SET PAGESIZE 0 >> %DBSCRIPT%
ECHO SET HEADING OFF >> %DBSCRIPT%
ECHO SET SQLBLANKLINES ON >> %DBSCRIPT%
ECHO START "%INSITE_HOME%\Oracle\AssignGUIDtoNullExportImportKeys.or.sql" >> %DBSCRIPT%
ECHO EXIT >> %DBSCRIPT%
more %DBSCRIPT% >> %LOGFILE%
ECHO. >> %LOGFILE%
ECHO. >> %LOGFILE%

ECHO [ Database script execution ] >> %LOGFILE%
ECHO Executing Script: %DBSCRIPT% >> %LOGFILE%
ECHO. >> %LOGFILE%
sqlplus /NOLOG  @%DBSCRIPT% %PROC_ModelingFlag% %PROC_HistoryFlag% %PROC_SystemFlag% %PROC_TrackingFlag% %PROC_NoneFlag% %PROC_UndefinedFlag% %PROC_UnknownFlag% >> %LOGFILE%
SET CURRENTRETURNCODE=%ERRORLEVEL%
ECHO. >> %LOGFILE%

ECHO [ Error Handling of call to SQL Plus ] >> %LOGFILE%
ECHO Return Code from call to SQL Plus : %CURRENTRETURNCODE% >> %LOGFILE%
ECHO. >> %LOGFILE%
IF %CURRENTRETURNCODE% NEQ 0 GOTO EXIT

ECHO [ Delete dynamically create database script ] >> %LOGFILE%
ECHO del %DBSCRIPT% >> %LOGFILE%
DEL %DBSCRIPT% 
ECHO. >> %LOGFILE%

ECHO [ Error Handling of DOS del command to delete database script ] >> %LOGFILE%

IF EXIST %DBSCRIPT% (
		ECHO File %DBSCRIPT% was not deleted >> %LOGFILE%
		REM override del command failure return code with a 0
		SET CURRENTRETURNCODE=0
	) ELSE (
		ECHO File %DBSCRIPT% deleted >> %LOGFILE%
		REM override del command failure return code with a 0
		SET CURRENTRETURNCODE=0
	)

ECHO Return Code from DOS file exists command : %CURRENTRETURNCODE% >> %LOGFILE%
ECHO. >> %LOGFILE%
REM override del command failure return code with a 0
SET CURRENTRETURNCODE=0

ECHO [ Searching log file for oracle error messages ] >> %LOGFILE%
ECHO Executing dos findstr command to search log file for oracle error messages >> %LOGFILE%
FINDSTR /I "ORA-" %LOGFILE% >> %LOGFILE%
SET CURRENTRETURNCODE=%ERRORLEVEL%
ECHO. >> %LOGFILE%

ECHO [ Error Handling of DOS findstr command to searh log file for oracle error messages ] >> %LOGFILE%
IF %CURRENTRETURNCODE% EQU 1 (
		REM A CURRENTRETURNCODE of 1 means the string was NOT found (i.e. Oracle error not found in log file)
		REM override findsrt command success return code by setting CURRENTRETURNCODE to a 0 if findsrt returns a 1
		ECHO Return Code from DOS findstr commands : %CURRENTRETURNCODE%	- oracle error not found >> %LOGFILE%
		SET CURRENTRETURNCODE=0
	) ELSE (
		REM A CURRENTRETURNCODE of 0 means the string was found (i.e. Oracle error found in log file)
		REM override findsrt command failure return code by setting CURRENTRETURNCODE to a 1 if findsrt returns 0
		ECHO Return Code from DOS findstr commands : %CURRENTRETURNCODE%	- oracle error found >> %LOGFILE%
		SET CURRENTRETURNCODE=1
	)
ECHO Updated Return Code : %CURRENTRETURNCODE% >> %LOGFILE%
ECHO. >> %LOGFILE%
GOTO EXIT
:END



:SQLSERVER
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
REM Oracle specific processing
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
IF (%DB_SERVERNAME%)==() GOTO USAGE
SET LOGFILE="%INSITE_HOME%\AssignGUIDtoNullExportImportKeys_%DB_NAME%.log"
SET DBSCRIPT="%INSITE_HOME%\SQL Server\AssignGUIDtoNullExportImportKeys.sql"
SET SQLSERVERLOG="%INSITE_HOME%\SQL Server\sqlserver_AssignGUIDtoNullExportImportKeys_%DB_NAME%.log"

ECHO © 2016 Siemens Product Lifecycle Management Software Inc. > %LOGFILE%
ECHO. >> %LOGFILE%
ECHO Start Time: %STARTTIME% >> %LOGFILE%
ECHO. >> %LOGFILE%
ECHO Insite Home: %INSITE_HOME% >> %LOGFILE%
ECHO. >> %LOGFILE%

ECHO [ DB Script Information ] >> %LOGFILE%
ECHO DB Script:  %DBSCRIPT% >> %LOGFILE%
ECHO SQLSERVERLOG: %SQLSERVERLOG% >> %LOGFILE%
ECHO. >> %LOGFILE% 

ECHO [ Log File Information ] >> %LOGFILE% 
ECHO Log file:  %LOGFILE% >> %LOGFILE%
ECHO Generated in directory: "%INSITE_HOME%" >> %LOGFILE%
ECHO. >> %LOGFILE%

ECHO [ Validated input parameters ] >> %LOGFILE% 
ECHO Databse Vendor {DB_VENDOR} : %DB_VENDOR% >> %LOGFILE%
ECHO Datasbe Server (HOST) {DB_SERVERNAME} : %DB_SERVERNAME% >> %LOGFILE%
ECHO Database Name  {DB_NAME} : %DB_NAME% >> %LOGFILE%
ECHO Database User Name {DB_USERNAME}: %DB_USERNAME% >> %LOGFILE%
ECHO Database Password {DB_PASSWORD}: %DB_PASSWORD% >> %LOGFILE%
ECHO. >> %LOGFILE%

ECHO [ Assigning system generated GUID for targeted tables with a NULL ] >> %LOGFILE%
ECHO [ value in the EXPORTIMPRTKEY column.  Target tables are based on ] >> %LOGFILE%
ECHO [ Input parameters:                                               ] >> %LOGFILE%
ECHO PROC_ModelingFlag: %PROC_ModelingFlag% >> %LOGFILE%
ECHO PROC_HistoryFlag: %PROC_HistoryFlag% >> %LOGFILE%
ECHO PROC_Systemlag: %PROC_SystemFlag% >> %LOGFILE%
ECHO PROC_TrackiingFlag:  %PROC_TrackingFlag% >> %LOGFILE%
ECHO PROC_NoneFlag:  %PROC_NoneFlag% >> %LOGFILE%
ECHO PROC_UndefinedFlag:  %PROC_UndefinedFlag% >> %LOGFILE%
ECHO PROC_UnknownFlag:  %PROC_UnknownFlag% >> %LOGFILE%
ECHO. >> %LOGFILE%

ECHO [ Database script execution ] >> %LOGFILE%
ECHO Verifying user login >> %LOGFILE%
ECHO. >> %LOGFILE%
sqlcmd -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -b -Q "SELECT DB_NAME() AS DB_NAME" -o %SQLSERVERLOG%
SET CURRENTRETURNCODE=%ERRORLEVEL%
more %SQLSERVERLOG% >> %LOGFILE%
ECHO. >> %LOGFILE%
IF %CURRENTRETURNCODE% NEQ 0  GOTO EXIT

ECHO Executing Script: %DBSCRIPT% >> %LOGFILE%
ECHO. >> %LOGFILE%
sqlcmd -S %DB_SERVERNAME% -d %DB_NAME% -U %DB_USERNAME% -P %DB_PASSWORD% -i %DBSCRIPT% -v PROC_ModelingFlag=%PROC_ModelingFlag%  PROC_History=%PROC_HistoryFlag%  PROC_SystemFlag=%PROC_SystemFlag%   PROC_TrackingFlag=%PROC_TrackingFlag%   PROC_NoneFlag=%PROC_NoneFlag%  PROC_UndefinedFlag=%PROC_UndefinedFlag%  PROC_UnknownFlag=%PROC_UnknownFlag% -o %SQLSERVERLOG%
SET CURRENTRETURNCODE=%ERRORLEVEL%
more %SQLSERVERLOG% >> %LOGFILE%
ECHO. >> %LOGFILE%
IF %CURRENTRETURNCODE% NEQ 0  GOTO EXIT

ECHO [ Delete dynamically create database script ] >> %LOGFILE%
ECHO del %SQLSERVERLOG% >> %LOGFILE%
DEL %SQLSERVERLOG%
ECHO. >> %LOGFILE%

ECHO [ Error Handling of DOS del command to delete database script ] >> %LOGFILE%
IF EXIST %SQLSERVERLOG% (
		ECHO File %SQLSERVERLOG% was not deleted >> %LOGFILE%
		REM override del command failure return code with a 0
		SET CURRENTRETURNCODE=0
	) ELSE (
		ECHO File %SQLSERVERLOG% deleted >> %LOGFILE%
		REM override del command failure return code with a 0
		SET CURRENTRETURNCODE=0
	)

ECHO Return Code from DOS file exists command : %CURRENTRETURNCODE% >> %LOGFILE%
ECHO. >> %LOGFILE%
REM override del command failure return code with a 0
SET CURRENTRETURNCODE=0
GOTO EXIT
:END


:USAGE
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
REM Usage to display the input parameter syntax for this script
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
IF (%DB_VENDOR%)==() SET DB_VENDOR=UNSPECIFIED
IF (%DB_NAME%)==() SET DB_NAME=UNSPECIFIED
IF (%DB_USERNAME%)==() SET DB_USERNAME=UNSPECIFIED
IF (%DB_PASSWORD%)==() SET DB_PASSWORD=UNSPECIFIED
SET LOGFILE="%INSITE_HOME%\AssignGUIDtoNullExportImportKeys_%DB_NAME%.log"
ECHO © 2016 Siemens Product Lifecycle Management Software Inc. > %LOGFILE%
ECHO. >> %LOGFILE%
ECHO Start Time: %STARTTIME% >> %LOGFILE%
ECHO. >> %LOGFILE%
ECHO Insite Home: %INSITE_HOME% >> %LOGFILE%
ECHO. >> %LOGFILE%
ECHO Usage (Input Parameters): >> %LOGFILE%
ECHO	AsignGUIDtoNullExportImportKeys.bat "<ORACLE|SQLSERVER>" "<Database Host if SQLSERVER>" "<Database>" "<user>" "<password>"  ...continue next line >> %LOGFILE%
ECHO		"<Modeling Flag [1|0]>" "<History Flag [1|0]>" "<System Flag [1|0]>" "<Tracking Flag [1|0]>" "<None Flag [1|0]>" "<Undefined Flag [1|0]>" "<Unknown Flag [1|0]>" >> %LOGFILE%
ECHO. >> %LOGFILE%
ECHO 	The flag input parameters are 1 or 0 values that will be bassed into the AssignGUIDtoNullExportImport.or.sql (Oracle) or AssignGUIDtoNullExportImportKeys.sql (SQL Server) >> %LOGFILE%
ECHO 	Theses flag parameters determine which CDODefinition tables will be updated as the flag parameters are mapped to values for CDODefinition.StorageCategoryId column >> %LOGFILE%
ECHO 	For Example, a TrackingFlag value of 1 is mapped to the StorageCategoryId value of 6 and if the other flg values are all zero, then only CDODefinition tables that are >> %LOGFILE%
ECHO 	Tracking tables will be updated if their ExportImportKey column has a NULL value >> %LOGFILE%
ECHO 	The flag parameters can be any combination of 0 and 1 values, depending on the type of CDODefinition tables you want to target for an update >> %LOGFILE%
ECHO. >> %LOGFILE%
ECHO 	Where: >> %LOGFILE%
ECHO 		"<ORACLE|SQLSERVER>"   		- Specify database vendor>> %LOGFILE%
ECHO 		"<Database Host>"      		- For ORACLE: Not Required >> %LOGFILE%
ECHO 						- For SQL SERVER: Is Required and is the Database Server Name or IP Address >> %LOGFILE%
ECHO 		"<Database Name>"      		- Native database name or alias >> %LOGFILE%
ECHO 		"<User>"               		- Insite database user logon >> %LOGFILE%
ECHO 		"<Password>"         		- Insite database user password >> %LOGFILE%
ECHO 		"<Modeling Flag  [1|0]>"	- Flag to be passed into database script that mapps to corresponing CDODefinition.StorageCategoryId value of 1 if the Flag value is 1 >> %LOGFILE%
ECHO 		"<History Flag 	 [1|0]>"	- Flag to be passed into database script that mapps to corresponing CDODefinition.StorageCategoryId value of 2 if the Flag value is 1 >> %LOGFILE%
ECHO 		"<System Flag 	 [1|0]>"	- Flag to be passed into database script that mapps to corresponing CDODefinition.StorageCategoryId value of 5 if the Flag value is 1 >> %LOGFILE%
ECHO 		"<Tracking Flag  [1|0]>"	- Flag to be passed into database script that mapps to corresponing CDODefinition.StorageCategoryId value of 6 if the Flag value is 1 >> %LOGFILE%
ECHO 		"<None Flag 	 [1|0]>"	- Flag to be passed into database script that mapps to corresponing CDODefinition.StorageCategoryId value of 8 if the Flag value is 1 >> %LOGFILE%
ECHO 		"<Undefined Flag [1|0]>"	- Flag to be passed into database script that mapps to corresponing CDODefinition.StorageCategoryId value of 0 if the Flag value is 1 >> %LOGFILE%
ECHO 		"<Unknown Flag	 [1|0]>"	- Flag to be passed into database script that mapps to corresponing CDODefinition.StorageCategoryId value of 4 if the Flag value is 1 >> %LOGFILE%
ECHO. >> %LOGFILE%
ECHO 	Example : >> %LOGFILE%
ECHO 		(ORACLE): 	AssignGUIDtoNullExportImportKeys.bat ORACLE APPDEVORA001 InSiteAdmin admin 1 0 0 0 0 0 0 >> %LOGFILE%
ECHO 		(ORACLE):	AssignGUIDtoNullExportImportKeys.bat ORACLE PROD InSiteAdmin admin 0 1 0 0 0 0 0 >> %LOGFILE%
ECHO 		(SQLSERVER):	AssignGUIDtoNullExportImportKeys.bat SQLSERVER DBSERVERNAME PROD InSiteAdmin admin >> %LOGFILE%
ECHO 		(SQLSERVER):	AssignGUIDtoNullExportImportKeys.bat SQLSERVER 10.10.10.10E PROD InSiteAdmin admin >> %LOGFILE%
ECHO. >> %LOGFILE%
SET CURRENTRETURNCODE=1
GOTO EXIT
:END



:EXIT
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
REM Processing to check current error level and display log file to the console and exit with the current error level
REM --------------------------------------------------------------------------------------------------------------------------------------------------------
ECHO. 
ECHO Log file is located at: %LOGFILE%
ECHO.
IF %CURRENTRETURNCODE% NEQ 0 (
		ECHO Error In Assignment of system generated GUID for all null values for ExportImportKey column >> %LOGFILE%
		ECHO AssignGUIDtoNullExportImportKeys.bat FAILED >> %LOGFILE%
	) ELSE (
		ECHO AssignGUIDtoNullExportImportKeys.bat SUCCEEDED >> %LOGFILE%
	)

REM MORE %LOGFILE%
REM IF %CURRENTRETURNCODE% NEQ 0 (
REM		EXIT %CURRENTRETURNCODE%
REM	) ELSE (
REM		EXIT 0
REM	)

:END

