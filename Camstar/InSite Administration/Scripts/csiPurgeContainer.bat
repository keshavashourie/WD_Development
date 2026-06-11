@ECHO OFF
REM -----------------------------------------------------------------------------------------------------------------------------
REM This script will connect to the user input database and executes the CSIPURGECOTNAINER stored 
REM procedure. 
REM
REM Following are the databases supported by this script:
REM - ORACLE
REM - DB2
REM - SQL SERVER
REM 
REM 
REM Modification History:
REM Name			Date        	Action
REM --------------		----------	----------------
REM Stanly Manuel		08/27/2004  	Initial Creation
REM
REM Stanly Manuel		09/21/2004  	Added Oracle version
REM
REM Stanly Manuel		10/05/2004  	Added SQL Server version
REM
REM Purushotham Neelakantachar  07/12/2006  	Following modifications were done:
REM						- Modified SQL SERVER code to include ORDER BY clause in the SELECT statement call
REM				    		- Modified DB2 code to include ORDER BY clause in the SELECT statement call
REM
REM Bill Lippard		12/01/2006	Updated copyright notice (SPR S1184)
REM
REM Bill Lippard		04/23/2007	Updated copyright notice (SPR S9984)
REM
REM Purushotham Neelakantachar	05/29/2008 	Modified the code for SPR S12618
REM						- Included new logic to handle input arguments and validations
REM						- Included a new input parameter: DB_SERVERNAME
REM						  This parameter will be used only for SQL SERVER
REM			    			- Replaced SQL Server's OSQL commands with SQLCMD
REM						- Included an new input parameter - PURGE_TYPE
REM
REM Purushotham Neelakantachar	02/16/2009 	Modified the code for SPR S14757
REM						- Included new logic to handle spaces for input argument FACTORY_NAME
REM						- Added SQL Plus formatting commands
REM						- Logged all the output to a file on the current directory
REM
REM Copyright Siemens 2023  
REM -----------------------------------------------------------------------------------------------------------------------------

