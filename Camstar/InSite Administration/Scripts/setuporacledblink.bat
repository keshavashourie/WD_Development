@ECHO OFF
REM ============================================================
REM File:              setuporacledblink.bat
REM Version:           1.0
REM Author:            Stanly Manuel
REM Create Date:       24-SEP-2004
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
REM Change log:
REM 1) Initial version oracle - Stanly (24-SEP-2004)
REM 2) Updated copyright notice (SPR S1184) - Bill Lippard
REM                                           (01-DEC-2006)
REM 3) Updated copyright notice (SPR S9984) - Bill Lippard
REM                                           (23-APR-2007)
REM ============================================================

SET errorstatus=0

SET DBLINK=%1
SET SETUP_MODE=%2
SET TNS_NAME=%3
SET TX_DBNAME=%4
SET DS_DBNAME=%5
SET TX_SCHEMA=%6
SET TX_PASSWD=%7
SET DS_SCHEMA=%8
SET DS_PASSWD=%9


IF (%DBLINK%)==() GOTO usage
IF (%TNS_NAME%)==() GOTO usage
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

:drop
ECHO Dropping the database link from %TX_DBNAME% ...
ECHO CONNECT %TX_SCHEMA%/%TX_PASSWD%@%TX_DBNAME%; > tmproc.sql
ECHO UPDATE purgerestoresetup SET value=NULL WHERE parameter = 'DB_LINK'; >> tmproc.sql
ECHO DROP DATABASE LINK %DBLINK%; >> tmproc.sql
ECHO EXIT; >> tmproc.sql
sqlplus /nolog @tmproc.sql
DEL tmproc.sql
GOTO exit
:end

:create
ECHO Creating database link in %TX_DBNAME% ...
ECHO CONNECT %TX_SCHEMA%/%TX_PASSWD%@%TX_DBNAME%; > tmproc.sql
ECHO UPDATE purgerestoresetup SET value='%DBLINK%' WHERE parameter = 'DB_LINK'; >> tmproc.sql
ECHO CREATE DATABASE LINK %DBLINK% CONNECT TO %DS_SCHEMA% IDENTIFIED BY %DS_PASSWD% USING '%TNS_NAME%'; >> tmproc.sql
ECHO EXIT; >> tmproc.sql
sqlplus /nolog @tmproc.sql
DEL tmproc.sql
GOTO exit
:end

:usage
ECHO ===============================================================================
ECHO                              ***IMPORTANT***
ECHO -------------------------------------------------------------------------------
ECHO Local Net Service Name should be configured for transaction and datastore databases
ECHO -------------------------------------------------------------------------------
ECHO USAGE:
ECHO    "setuporacledblink <DB Link> <CREATE|DROP> <TNS NAME> <Txn DBNAME> <Ds DBNAME>"
ECHO                      "<Txn DB User> <Txn DB Password> <Ds DB User> <Ds DB Password>"
ECHO WHERE:
ECHO "<DB Link>"        - Database Link Name (Example:- remote)
ECHO "<CREATE|DROP>"    - CREATE - For creating, DROP - For dropping
ECHO "<TNS_NAME>"       - Datastore database TNS name
ECHO "<Txn DBNAME>"     - Transaction Database/Datasource TNS Name (Example:- PROD)
ECHO "<Ds DBNAME>"      - Data Store Database/Datasource TNS Name (Example:- ODS)
ECHO "<Txn DB User>"    - Transaction Database User Id/Schema Name (Example:- insiteadmin)
ECHO "<Txn DB Passord>" - Transaction Database User/Schema Password
ECHO if the user and password is same as Transaction DB, leave the following blank
ECHO "<Ds DB User>"     - Data Store Database User Id/Schema Name (Example:- insiteadmin) 
ECHO "<Ds DB Passord>"  - Data Store Database User/Schema Password
ECHO Example:-
ECHO    "setuporacledblink remote CREATE ODS PROD ODS txnadmin Camstar1 odsadmin Camstar2"
ECHO OR 
ECHO    "setuporacledblink remote CREATE ODS PROD ODS db2admin db2admin" 
ECHO    (Here both transaction and datastore DB have the same user name and password)
ECHO -------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO -------------------------------------------------------------------------------
GOTO exit
:end

:exit
:end
