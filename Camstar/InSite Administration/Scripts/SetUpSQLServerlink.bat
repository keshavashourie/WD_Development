@ECHO OFF
REM -----------------------------------------------------------------------------------------------------------------------------
REM File:              setupsqlserverlink.bat
REM Version:           1.0
REM Author:            Stanly Manuel
REM Create Date:       05-OCT-2004
REM Database:          ORACLE
REM
REM Copyright Siemens 2023  
REM
REM ------------------------------------------------------------
REM Purpose: To create the dblink between transaction and 
REM          datastore db for container restore
REM ------------------------------------------------------------
REM Description: None
REM ------------------------------------------------------------
REM Modification History:
REM Name			Date		Action
REM --------------		----------	----------------
REM Stanly			10/05/2004	Initial version sql server
REM
REM Bill Lippard		12/01/2006	Updated copyright notice (SPR S1184)
REM
REM Bill Lippard		04/23/2006	Updated copyright notice (SPR S9984)
REM
REM Purushotham Neelakantachar	05/29/2008 	Modified the code for SPR S12618
REM						- Included a new input parameter: DB_HOSTAME
REM						  This parameter will be used only for SQL SERVER
REM			    			- Included -S option to SQL Server's SQLCMD
REM 
REM Copyright Siemens 2023  
REM -----------------------------------------------------------------------------------------------------------------------------

TIME /t
SET ERRORSTATUS=0

SET DB_HOSTNAME=%1
SET SERVERNAME=%2
SET SETUP_MODE=%3
SET TX_DBNAME=%4
SET DS_DBNAME=%5
SET TX_SCHEMA=%6
SET TX_PASSWD=%7
SET DS_SCHEMA=%8
SET DS_PASSWD=%9

IF (%DB_HOSTNAME%)==() GOTO usage
IF (%SERVERNAME%)==() GOTO usage
IF (%SETUP_MODE%)==() GOTO usage
IF (%TX_DBNAME%)==() GOTO usage
IF (%DS_DBNAME%)==() GOTO usage
IF (%TX_SCHEMA%)==() GOTO usage
IF (%TX_PASSWD%)==() GOTO usage
IF (%DS_SCHEMA%)==() SET DS_SCHEMA=%TX_SCHEMA%
IF (%DS_PASSWD%)==() SET DS_PASSWD=%TX_PASSWD%

IF /I (%DBLINK%) EQU (HELP) GOTO usage
IF /I (%SETUP_MODE%) EQU (DROP) GOTO drop
IF /I (%SETUP_MODE%) EQU (CREATE) GOTO create
GOTO usage

:DROP
ECHO Dropping the server link from %TX_DBNAME% ...
SQLCMD -S %DB_HOSTNAME% -d %TX_DBNAME% -U %TX_SCHEMA% -P %TX_PASSWD% -b -Q "UPDATE purgerestoresetup SET value=NULL WHERE parameter IN ('LINKED_SERVER','DATASTORE_DB','DATASTORE_SCHEMA')"
GOTO EXIT
:END

:CREATE
ECHO Creating server link in %TX_DBNAME% ...
SQLCMD -S %DB_HOSTNAME% -d %TX_DBNAME% -U %TX_SCHEMA% -P %TX_PASSWD% -b -Q "UPDATE purgerestoresetup SET value='%SERVERNAME%' WHERE parameter ='LINKED_SERVER'"
SQLCMD -S %DB_HOSTNAME% -d %TX_DBNAME% -U %TX_SCHEMA% -P %TX_PASSWD% -b -Q "UPDATE purgerestoresetup SET value='%DS_DBNAME%' WHERE parameter ='DATASTORE_DB'"
SQLCMD -S %DB_HOSTNAME% -d %TX_DBNAME% -U %TX_SCHEMA% -P %TX_PASSWD% -b -Q "UPDATE purgerestoresetup SET value='%DS_SCHEMA%' WHERE parameter ='DATASTORE_SCHEMA'"
IF /I (%SERVERNAME%) EQU (LOCAL) GOTO EXIT
SQLCMD -S %DB_HOSTNAME% -d %TX_DBNAME% -U %TX_SCHEMA% -P %TX_PASSWD% -b -Q "EXECUTE sp_addlinkedserver '%SERVERNAME%'"
SQLCMD -S %DB_HOSTNAME% -d %TX_DBNAME% -U %TX_SCHEMA% -P %TX_PASSWD% -b -Q "EXECUTE sp_addlinkedsrvlogin '%SERVERNAME%',FALSE,NULL,'%DS_SCHEMA%','%DS_PASSWD%'"
GOTO EXIT
:END

:USAGE
ECHO ===============================================================================
ECHO To setup the linked server between TRANSACTION and DATASTORE database
ECHO -------------------------------------------------------------------------------
ECHO USAGE:
ECHO    "SetSQLServerLink <Database Host> <Server Name> <CREATE|DROP> <Txn DBNAME> <Ds DBNAME>"
ECHO                      "<Txn DB User> <Txn DB Password> <Ds DB User> <Ds DB Password>"
ECHO WHERE:
ECHO "<Database Host>   - OLTP Database Host Name or IP Address
ECHO "<Server Name>"    - Datastore database server name
ECHO                    - Give LOCAL if both databases are running on the same machine
ECHO "<CREATE|DROP>"    - CREATE - For creating, DROP - For dropping
ECHO "<Txn DBNAME>"     - Transaction Database data source Name (DSN)(Example:- SQLPROD)
ECHO "<Ds DBNAME>"      - Data Store Database Name (Example:- ODS)
ECHO "<Txn DB User>"    - Transaction Database User Id/Schema Name (Example:- txnadmin)
ECHO "<Txn DB Passord>" - Transaction Database User/Schema Password
ECHO if the user and password is same as Transaction DB, leave the following blank
ECHO "<Ds DB User>"     - Data Store Database User Id/Schema Name (Example:- odsadmin) 
ECHO "<Ds DB Passord>"  - Data Store Database User/Schema Password
ECHO Example:-
ECHO    "SetSQLServerLink OLTPDATABASESERVERNAME odshost CREATE PROD ODS txnadmin Camstar1 odsadmin Camstar2"
ECHO    "SetSQLServerLink 10.10.10.10 odshost CREATE PROD ODS txnadmin Camstar1 odsadmin Camstar2"
ECHO OR 
ECHO    "setupsqlserverlink OLTPDATABASESERVERNAME odshost CREATE PROD ODS insiteadmin insite" 
ECHO    "setupsqlserverlink 10.10.10.10 odshost CREATE PROD ODS insiteadmin insite" 
ECHO    (Here both transaction and datastore DB have the same user name and password)
ECHO OR 
ECHO    "setupsqlserverlink OLTPDATABASESERVERNAME LOCAL CREATE PROD ODS insiteadmin insite"
ECHO    "setupsqlserverlink 10.10.10.10 LOCAL CREATE PROD ODS insiteadmin insite"
ECHO    (Here both databases are running on the same server)
ECHO -------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO -------------------------------------------------------------------------------
GOTO EXIT
:END

:EXIT
TIME /t
:END
