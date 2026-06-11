--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- Copyright Siemens 2023  
-- 
--
-- Description:
--
--     This script is called by DataStoreEmailSetUp.ps1.  It can be run directly from SQL Plus or SQL Developer but the preferred method of execution 
--     is by invoking the DataStoreEmailSetUp.ps1 script.  This sql script should be executed as the admin or SYS Oracle schema.  This script removes or appends
--     an email server host for a principal from/to the Oracle Access Control List (ACL) to allow Oracle to access an email server.  
--     The DataStore Username is needed as the principal to remove or append the email server host to the ACL and is passed into the sql script 
--     as &1 which is assigned to &&DATASTORE_USERNAME.  
--     Email server information is also passed into this script as &2 and assigned to &&EMAIL_SERVER and is the host to be removed or appended to the ACL.  
--     The action is passed into the sql script as &3.  If the action is not specified in the batch file, it will default to ADD
--	   Email Server port is passed into this script as &4 and assigned to &&EMAIL_SERVER_PORT ans is use to set the port used by email server.
--     The script needs &&DATASTORE_USERNAME, &&EMAIL_SERVER, &&ACTION, &&EMAIL_SERVER_PORT information to remove or append the email server host and principal from/to the ACL.
--
-- Execution:
--
--     Execute script via DataStoreEmailSetUp.ps1 (preferred method).  This sql script can be executed via SQL Plus or SQL Developer as the Oracle Admin or SYS schema 
--
--
--     EXAMPLE (SQL Developer interactive)			
--          Logged into SQL Developer as SYS, execute the script @DataStoreEmailSetUp.or.sql
--
--          SQL Developer will prompt the user to enter a value for &1, &2, &3, &4 which will be assigned to the input parameters for 
--          DATASTORE_USERNAME, EMAIL_SERVER, ACTION, EMAIL_SERVER_PORT
--          If the <ENTER> key is used to skip over a parameter it will be set to NULL.  
--          IF EMAIL_SERVER is set to NULL or UNSPECIFIED, the Oracle ACL will not be modified.  The script will exit without performing any work.
--          IF DATASTORE_USERNAME is set to NULL or UNSPECIFIED, the Oracle ACL will not be modified.  The script will exit without performing any work.
--          IF ACTION is set to NULL or UNSPECIFIED, the script will set ACTION to ADD
--			If EMAIL_SERVER_PORT is set to NULL or 0, the script will set EMAIL_SERVER_PORT to 25.
--
--     EXAMPLE (SQL Plus command line)	
--          sqlplus SYS/<Password>@hostname/instancename or pluggable database name as sysdba @DataStoreEmailSetUp.or.sql
--          sqlplus SYS/<Password>@10.X.X.X/instanename or pluggable database name as sysdba @DataStoreEmailSetUp.or.sql
--
--          In the above examples, the user will be prompted for the input parameter values if the parameters are not supplied on the command line
--
--          sqlplus SYS/<Password>@hostname/instancename or pluggable database name as sysdba @DataStoreEmailSetUp.or.sql '' '' '' ''
--
--          In the above example, the user supplied '' (two single quotes side by side) for each input parameter value.  
--          This is the same thing as sending a NULL value or hitting the <ENTER> for the parameter value
--
--          sqlplus SYS/<Password>@10.X.X.X/instanename as sysdba @DataStoreEmailSetUp.or.sql DataStoreUserName 10.X.X.X ADD 25
--
--          In the above example, the 4 input parameters are passed in on the command line
--
--
--      Note:     Executing SQL Plus from the command line to invoke this script requires all parameters be supplied on the command line.  
--                If all parameters are not supplied, those missing parameters will be prompted for interactively
--
--      Note:     The recommended way to run DataStoreEmailSetUp.or.sql is by invoking DataStoreEmailSetUp.ps1 (use the powershell file)
--                See the comments in the DataStoreEmailSetUp.ps1 to get the syntax to call DataStoreEmailSetUp.ps1 and pass in parameter values
--                The parameter values passed into the powershells file, will get passed into the DataStoreEmailSetUp.or.sql file. 
--
-- Input Parameters:
--
--      When values are entered from the command line, if all values are not supplied, the user will be prompted to enter values for the missing parameters
--      When values are supplied from the command line, the way to accept the default NULL value for that parameter is to enter two single quotes side by side
--      (i.e. '')
--
--     &1 which becomes &&DATASTORE_USERNAME     IF NULL or UNSPECIFIED the script exits without modifying the ACL
--     &2 which becomes &&EMAIL_SERVER           IF NULL or UNSPECIFIED the script exits without modifying the ACL
--     &3 which becomes &&ACTION                 IF NULL or UNSPECIFIED &&ACTION becomes ADD
--	   &4 which becomes &&EMAIL_SERVER_PORT		 IF NULL or 0 &&EMAIL_SERVER_PORT becomes 25
--
--
-- Output:
--
--     Log File of script execution:  DataStoreEmailSetUp.or.log 
--
-- Modification History:
--     Name                            Date            Action
--     -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--     dmaloney                        03/05/2016      New Script
--     dmaloney                        04/24/2017      Removed references to EMAIL_SENDER and EMAIL_RECIPIENTS (&3 and &4)
--                                                     Modified script to change ACL to include the DATASTORE_USERNAME (&1)
--                                                     Changed INSITE_APPLICATION_USERNAME to DATASTORE_USERNAME
--     dmaloney                        05/04/2017      Add ADD and DROP functionality
--     dmaloney                        06/04/2017      Modified comments
--     dmaloney                        06/06/2017      Added code to modify SMTP_OUT_SERVER for DROP and ADD action
--     dmaloney                        07/28/2017      Added call to depreciated SYS.DBMS_NETWORK_ACL_ADMINDELETE_PRIVILEGE to DROP principal (BUG 54808)
--     dmaloney                        10/03/2017      Updated code
--     dmaloney                        03/06/2018      Corrected spelling and formatting
--	   mbhandari					   12/10/2023	   Added parameter EmailServerPort as input parameter
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

SPOOL DataStoreEmailSetUp.or.log
WHENEVER SQLERROR EXIT
SET SERVEROUTPUT ON SIZE UNLIMITED
SET TIMING ON
SET LINESIZE 2000
SET ECHO OFF
SET WRAP OFF
SET TERMOUT ON
SET VERIFY OFF

--Undefine and script variables used in this script
UNDEF DATASTORE_USERNAME
UNDEF EMAIL_SERVER
UNDEF ACTION
UNDEF EMAIL_SERVER_PORT


PROMPT Enter    DataStore Schema Name for Parameter 1: DATASTORE_USERNAME
DEFINE DATASTORE_USERNAME = '&1'
DEFINE DATASTORE_USERNAME 
PROMPT

PROMPT Enter    Email Server Name or IP Address for Parameter 2: EMAIL_SERVER
DEFINE EMAIL_SERVER = '&2'
DEFINE EMAIL_SERVER
PROMPT


PROMPT Enter    Action for Parameter 3 (ADD or DROP): ACTION
DEFINE ACTION = '&3'
DEFINE ACTION
PROMPT

PROMPT Enter    Email Server Port for Parameter 4: EMAIL_SERVER_PORT
DEFINE EMAIL_SERVER_PORT = '&4'
DEFINE EMAIL_SERVER_PORT
PROMPT

--Email is sent via UTL_SMTP
--Email uses UTL_MAIL so it must be installed into the database.  


--An ACL is modified as the SYS user
--As SYS issue the following PL/SQL block to remove or append host/principal entries from/to the ACL
DECLARE	
n_IsAssigned                NUMBER(1);
n_AssignedPrincipalCount    NUMBER;
v_EmailServer               VARCHAR2(512);
v_SMTP_OUT_SERVER           VARCHAR2(512);
v_DataStoreUserName         VARCHAR2(512);
v_Action                    VARCHAR2(4);
v_ACL                       VARCHAR2(4096);
v_CDB                       VARCHAR2(4);
v_InstanceName              VARCHAR2(128);
v_HostName                  VARCHAR2(128);
v_PluggableDatabaseName     VARCHAR2(128);
v_Version                   VARCHAR2(32);
v_emailServerPort			NUMBER;
BEGIN

	v_EmailServer := '&&EMAIL_SERVER';
	IF v_EmailServer = 'UNDEFINED' OR v_EmailServer is NULL THEN
		RETURN;
	END IF;

	v_DataStoreUserName := UPPER('&&DATASTORE_USERNAME');
	IF v_DataStoreUserName = 'UNDEFINED' OR v_DataStoreUserName is NULL THEN
		RETURN;
	END IF;

	v_Action := UPPER('&&ACTION');
	IF v_Action = 'UNDEFINED' OR v_Action is NULL THEN
		v_Action := 'ADD';
	END IF;
	
	v_emailServerPort := '&&EMAIL_SERVER_PORT';
	IF v_emailServerPort = 0 OR v_emailServerPort is NULL THEN
		v_emailServerPort := 25;
	END IF;
  
	SELECT CDB INTO v_CDB FROM V$DATABASE;
	IF v_CDB = 'YES' THEN
		SELECT NAME INTO v_PluggableDatabaseName FROM V$PDBS;
	END IF;	
	SELECT INSTANCE_NAME, HOST_NAME, VERSION INTO v_InstanceName, v_HostName, v_Version FROM V$INSTANCE;

	DBMS_OUTPUT.ENABLE(100000);
	DBMS_OUTPUT.PUT_LINE('Host Name                   : [ ' || v_HostName || ' ]');
	DBMS_OUTPUT.PUT_LINE('Oracle Version              : [ ' || v_Version || ' ]');
	DBMS_OUTPUT.PUT_LINE('Instance Name               : [ ' || v_InstanceName || ' ]');
	DBMS_OUTPUT.PUT_LINE('Pluggable Database          : [ ' || v_CDB || ' ]');
	DBMS_OUTPUT.PUT_LINE('Pluggable Database Name     : [ ' || v_PluggableDatabaseName || ' ]');
	DBMS_OUTPUT.PUT_LINE('Action                      : [ ' || v_Action || ' ]');
	DBMS_OUTPUT.PUT_LINE('Email Server | ACL Host     : [ ' || v_EmailServer || ' ]');
	DBMS_OUTPUT.PUT_LINE('ACL Port                    : [ ' || v_emailServerPort ||' ]');
	DBMS_OUTPUT.PUT_LINE('ACL Principal               : [ ' || v_DataStoreUserName || ' ]');
	DBMS_OUTPUT.PUT_LINE( CHR(10) || CHR(13) );

	IF v_Action = 'DROP' THEN
		BEGIN
			v_ACL := NULL;
			SELECT ACL INTO v_ACL FROM DBA_NETWORK_ACLS WHERE HOST = v_EmailServer AND LOWER_PORT = v_emailServerPort AND UPPER_PORT = v_emailServerPort;
			DBMS_OUTPUT.PUT_LINE('Result                      : [ ' || v_ACL || ' ]');
		EXCEPTION
		WHEN NO_DATA_FOUND THEN
			DBMS_OUTPUT.PUT_LINE( CHR(10) || CHR(13) );
			DBMS_OUTPUT.PUT_LINE('Result                      : [ ACL not found ]');
			RETURN;
		END;

		--Get number of principals for host ACL
		SELECT COUNT(*) INTO n_AssignedPrincipalCount FROM DBA_NETWORK_ACL_PRIVILEGES WHERE ACL = v_ACL;
		DBMS_OUTPUT.PUT_LINE('Principals assigned to ACL  : [ ' ||  TO_CHAR(n_AssignedPrincipalCount) || ' ]');

		--If principal v_DataStoreUsername exists for host ACL then count will be 1, otherwise 0
		SELECT COUNT(*) INTO n_IsAssigned FROM DBA_NETWORK_ACL_PRIVILEGES WHERE PRINCIPAL = v_DataStoreUserName AND ACL = v_ACL;
		IF n_IsAssigned = 0 THEN
			DBMS_OUTPUT.PUT_LINE('Principal assigned to ACL   : [ No ]');
			RETURN;
		ELSE
			DBMS_OUTPUT.PUT_LINE('Principal assigned to ACL   : [ Yes ]');
		END IF;
		DBMS_OUTPUT.PUT_LINE( CHR(10) || CHR(13) );


		BEGIN
			SYS.DBMS_NETWORK_ACL_ADMIN.REMOVE_HOST_ACE (
    			HOST 		=>  v_EmailServer,
			LOWER_PORT 	=> v_emailServerPort,
    			UPPER_PORT 	=> v_emailServerPort,
			ACE 		=> xs$ace_type(privilege_list => xs$name_list('connect'), principal_name => v_DataStoreUserName, 
			principal_type => xs_acl.ptype_db),REMOVE_EMPTY_ACL => TRUE
			);

			COMMIT;
			DBMS_OUTPUT.PUT_LINE('Result                      : [ Principal removed from ACL ]');

			BEGIN
				v_ACL := NULL;
				SELECT ACL INTO v_ACL FROM DBA_NETWORK_ACLS WHERE HOST = v_EmailServer AND LOWER_PORT = v_emailServerPort AND UPPER_PORT = v_emailServerPort;
				DBMS_OUTPUT.PUT_LINE('Result                      : [ ACL not removed.  ACL still has principals assigned. ]');

			EXCEPTION
			WHEN NO_DATA_FOUND THEN
				DBMS_OUTPUT.PUT_LINE('Result                      : [ ACL removed ]');
			END;

		EXCEPTION
		--WHEN SYS.DBMS_NETWORK_ACL_ADMIN.PRIVILEGE_NOT_GRANTED THEN
			--NULL;
		WHEN OTHERS THEN
			DBMS_OUTPUT.PUT_LINE('Error                       : [ Error with call to DBMS_NETWORK_ACL_ADMIN.REMOVE_HOST_ACE ]');
			DBMS_OUTPUT.PUT_LINE('Error                       : [ ' || SQLERRM  || ' ]');
			DBMS_OUTPUT.PUT_LINE('Result                      : [ ACL not removed ]');

			--The host ACL did not get removed, use depreciated procedure to remove privileges and remove host ACL
			DBMS_OUTPUT.PUT_LINE( CHR(10) || CHR(13) );

			IF n_IsAssigned = 1 THEN
				DBMS_OUTPUT.PUT_LINE('Result                      : [ Attempting DROP with depreciated procedure calls to DBMS_NETWORK_ACL_ADMIN ]');

				--If the depreciated DBMS_NETWROK_ACL_ADMIN.CREATE_ACL was created, DBMS_NETWORK_ACL_ADMIN.REMOVE_HOST_ACE may not work to remove the host ACL
				BEGIN
					SYS.DBMS_NETWORK_ACL_ADMIN.DELETE_PRIVILEGE(
					acl         => v_ACL,
					principal   => v_DataStoreUserName
					);

					COMMIT;	

					DBMS_OUTPUT.PUT_LINE('Result                      : [ Principal removed from ACL ]');

				EXCEPTION
				WHEN OTHERS THEN
					DBMS_OUTPUT.PUT_LINE('Error                       : [ Error removing principal in call to DBMS_NETWORK_ACL_ADMIN.DELETE_PRIVILEGE ]');
					DBMS_OUTPUT.PUT_LINE('Error                       : [ ' || SQLERRM  || ' ]');
					RAISE;
				END;
			END IF;

			--Get number of principals for host ACL
			SELECT COUNT(*) INTO n_AssignedPrincipalCount FROM DBA_NETWORK_ACL_PRIVILEGES WHERE ACL = v_ACL;
			DBMS_OUTPUT.PUT_LINE('Principals assigned to ACL  : [ ' ||  TO_CHAR(n_AssignedPrincipalCount) || ' ]');
			
			IF n_AssignedPrincipalCount = 0 THEN
				--If there are no more principals for host ACL, drop the host ACL
				BEGIN
					SYS.DBMS_NETWORK_ACL_ADMIN.DROP_ACL (
    					ACL          => v_ACL);

					COMMIT;

					DBMS_OUTPUT.PUT_LINE('Result                      : [ ACL removed ]');
				EXCEPTION 
				WHEN OTHERS THEN
					DBMS_OUTPUT.PUT_LINE('Error                       : [ Error dropping ACL in call to DBMS_NETWORK_ACL_ADMIN.DROP_ACL ]');
					DBMS_OUTPUT.PUT_LINE('Error                       : [ ' || SQLERRM  || ' ]');
					RAISE;
				END;
			END IF;
  		END;


	
		--If no principals for host, modify smtp_out_server to remove v_EmailServer and drop host ACL
		SELECT COUNT(*) INTO n_AssignedPrincipalCount
		FROM DBA_NETWORK_ACL_PRIVILEGES
		WHERE ACL IN (SELECT ACL FROM DBA_NETWORK_ACLS WHERE HOST = v_EmailServer AND UPPER_PORT = v_emailServerPort AND LOWER_PORT = v_emailServerPort);

		IF n_AssignedPrincipalCount = 0
		THEN
			--Rebuild string for SMTP_OUT_SERVER
			BEGIN
				SELECT LISTAGG(HOST,',')  WITHIN GROUP  (ORDER BY LOWER_PORT) HOSTNAMES  
				INTO v_SMTP_OUT_SERVER FROM DBA_NETWORK_ACLS WHERE UPPER_PORT = v_emailServerPort AND LOWER_PORT = v_emailServerPort
				GROUP BY UPPER_PORT, LOWER_PORT;
			EXCEPTION
			WHEN NO_DATA_FOUND THEN
				NULL;
			WHEN OTHERS THEN
				RAISE;
			END;

			BEGIN
				EXECUTE IMMEDIATE 'ALTER SYSTEM SET SMTP_OUT_SERVER = ''' || v_SMTP_OUT_SERVER || '''';
			EXCEPTION
			WHEN OTHERS THEN
				RAISE;
			END;
		END IF;
	END IF;

     

	IF v_Action = 'ADD' THEN
		BEGIN
			SYS.DBMS_NETWORK_ACL_ADMIN.APPEND_HOST_ACE (
    			HOST 		=>  v_EmailServer,
			LOWER_PORT 	=> v_emailServerPort,
    			UPPER_PORT 	=> v_emailServerPort,
			ACE 		=> xs$ace_type(privilege_list => xs$name_list('connect'), principal_name => v_DataStoreUserName, principal_type => xs_acl.ptype_db) 
			);

			COMMIT;

			DBMS_OUTPUT.PUT_LINE('Result                      : [ Principal added to ACL host ]');
  		EXCEPTION	
		WHEN OTHERS THEN
    			DBMS_OUTPUT.PUT_LINE('Error                       : [ Error appending principal to host ACL ]');
			DBMS_OUTPUT.PUT_LINE('Error                       : [ ' || SQLERRM || ' ]');
			RAISE;
  		END;

		--If smtp_out_server doesn't contain v_EmailServer, append it to smtp_out_server
		BEGIN
			SELECT VALUE INTO v_SMTP_OUT_SERVER FROM V$PARAMETER WHERE NAME = 'smtp_out_server';
			IF v_SMTP_OUT_SERVER IS NULL 
			THEN
				EXECUTE IMMEDIATE 'ALTER SYSTEM SET SMTP_OUT_SERVER = ''' || v_EmailServer || '''';				
			ELSE
			 	IF INSTR(v_SMTP_OUT_SERVER, v_EmailServer) = 0 
				THEN
					v_EmailServer := REPLACE(v_SMTP_OUT_SERVER,' ')  || ',' || v_EmailServer;
					v_EmailServer := REPLACE(v_EmailServer,',,');
					EXECUTE IMMEDIATE 'ALTER SYSTEM SET SMTP_OUT_SERVER = ''' || v_EmailServer || '''';
				END IF;
			END IF;
		EXCEPTION
		WHEN OTHERS THEN
			RAISE;
		END;

	END IF;
END;
/


DECLARE
v_SMTP_OUT_SERVER   VARCHAR2(512);
n_Principals        NUMBER := 0;
v_emailServerPort			NUMBER;
CURSOR              curHostResult (v_emailServerPort IN NUMBER) IS
                    SELECT HOST, LOWER_PORT, UPPER_PORT, ACL FROM DBA_NETWORK_ACLS 
                    WHERE LOWER_PORT = v_emailServerPort AND UPPER_PORT = v_emailServerPort
                    ORDER BY HOST, LOWER_PORT;

CURSOR              curPrincipalResult  (pcurv_ACL VARCHAR2) IS 
                    SELECT ACL, PRINCIPAL, PRIVILEGE, IS_GRANT, TO_CHAR(START_DATE, 'DD-MON-YYYY') AS STARTDATE, TO_CHAR(END_DATE, 'DD-MON-YYYY') AS ENDDATE 
                    FROM DBA_NETWORK_ACL_PRIVILEGES
                    WHERE ACL = pcurv_ACL 
                    ORDER BY ACL, PRINCIPAL;
BEGIN
	DBMS_OUTPUT.ENABLE(100000);
	DBMS_OUTPUT.PUT_LINE('<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< ACL REPORT >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>');
	BEGIN
	
		v_emailServerPort := '&&EMAIL_SERVER_PORT';
		IF v_emailServerPort = 0 OR v_emailServerPort is NULL THEN
			v_emailServerPort := 25;
		END IF;

		FOR recHostResult IN curHostResult(v_emailServerPort)
		LOOP
			DBMS_OUTPUT.PUT_LINE(CHR(10) || CHR(13));
			DBMS_OUTPUT.PUT_LINE('Email Server(s) with upper_port = '||v_emailServerPort||', lower_port = '||v_emailServerPort||'');
			DBMS_OUTPUT.PUT_LINE('HOST :            ' || recHostResult.HOST);
			DBMS_OUTPUT.PUT_LINE('ACL :             ' || recHostResult.ACL);
			DBMS_OUTPUT.PUT_LINE('LOWER PORT :      ' || recHostResult.LOWER_PORT);
			DBMS_OUTPUT.PUT_LINE('UPPER PORT :      ' || recHostResult.UPPER_PORT);

			DBMS_OUTPUT.PUT_LINE(CHR(10) || CHR(13));
			DBMS_OUTPUT.PUT_LINE(CHR(9) || 'PRINCIPALS : ');

			FOR recPrincipalResult IN curPrincipalResult (recHostResult.ACL)
			LOOP
				n_Principals := n_Principals +1;
    			DBMS_OUTPUT.PUT_LINE(CHR(9) ||'ACL:             ' || recPrincipalResult.ACL);
				DBMS_OUTPUT.PUT_LINE(CHR(9) ||'PRINCIPAL:       ' || recPrincipalResult.PRINCIPAL);
				DBMS_OUTPUT.PUT_LINE(CHR(9) ||'PRIVILEGE:       ' || recPrincipalResult.PRIVILEGE);
				DBMS_OUTPUT.PUT_LINE(CHR(9) ||'IS GRANT:        ' || recPrincipalResult.IS_GRANT);
				DBMS_OUTPUT.PUT_LINE(CHR(9) ||'START DATE:      ' || recPrincipalResult.STARTDATE);
				DBMS_OUTPUT.PUT_LINE(CHR(9) ||'END DATE:        ' || recPrincipalResult.ENDDATE);
				DBMS_OUTPUT.PUT_LINE(CHR(10) || CHR(13));
			END LOOP;
			IF n_Principals= 0 THEN
				DBMS_OUTPUT.PUT_LINE(CHR(9) || CHR(9) ||  '[ None ]');
			END IF;
			DBMS_OUTPUT.PUT_LINE(CHR(10) || CHR(13));
		END LOOP;

		SELECT LISTAGG(HOST,',')  WITHIN GROUP  (ORDER BY LOWER_PORT) HOSTNAMES  INTO v_SMTP_OUT_SERVER
 		FROM DBA_NETWORK_ACLS WHERE LOWER_PORT = v_emailServerPort AND UPPER_PORT = v_emailServerPort
		GROUP BY LOWER_PORT;


		DBMS_OUTPUT.PUT_LINE('SMTP_OUT_SERVER parameter value : ' || v_SMTP_OUT_SERVER);

  	EXCEPTION
	WHEN NO_DATA_FOUND THEN
		NULL;
	WHEN OTHERS THEN
    		RAISE;
  	END;

	DBMS_OUTPUT.PUT_LINE(CHR(10) || CHR(13));
	DBMS_OUTPUT.PUT_LINE('<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>');
END;
/



--Undefine and script variables used in this script
UNDEF 1
UNDEF 2
UNDEF 3
UNDEF 4

UNDEF DATASTORE_USERNAME
UNDEF EMAIL_SERVER
UNDEF ACTION
UNDEF EMAIL_SERVER_PORT


SPOOL OFF

QUIT
