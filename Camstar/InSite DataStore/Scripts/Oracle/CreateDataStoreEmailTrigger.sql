--------------------------------------------------------------------------------
-- SCRIPT: CreateDataStoreEmailTrigger.sql
-- DESCR: Creates the Datastore OOTB Email Trigger
-- HISTORY:

--	05/05/2017	Dan Maloney	New Email Triiger Script 
--
-- Copyright Siemens 2023  

-- Email trigger
-- Out Of The Box Oracle DataStore Email Alerts expect an internal SMTP server that does not require authentication.  
-- Anonymous authentication is the Out Of The Box expectation for Oracle DataStore Email Alerts.
-- If your SMTP server requires authentication, you must set up an SMTP relay that does not require authentication
-- and use the SMTP relay server as the Email Server in Camstar management Studio's DataStore Confiuration Pane.

BEGIN
	EXECUTE IMMEDIATE 'DROP TRIGGER DATASTORESETUPEMAILTRG';
EXCEPTION
WHEN OTHERS THEN
	NULL;
END;
/


CREATE OR REPLACE TRIGGER DATASTOREEMAILTRG 
AFTER
INSERT OR UPDATE 
ON DATASTORESETUP
FOR EACH ROW
DECLARE

	PRAGMA AUTONOMOUS_TRANSACTION;

 	cCRLF			CONSTANT	CHAR(2) := CHR(13) || CHR(10); 

	v_DBName		VARCHAR2(1024);
  	v_EmailRecipients	VARCHAR2(1024);
	v_EmailSender      	VARCHAR2(1024);
	v_Err			VARCHAR2(4000);
	v_Loc			VARCHAR2(256);
	v_Message		VARCHAR2(4000);
	v_HTMLCSS		VARCHAR2(4000);
	v_Msg			VARCHAR2(256);
	v_ServerHost		VARCHAR2(1024) := UPPER(SYS_CONTEXT('USERENV','SERVER_HOST'));
	v_SessionUser		VARCHAR2(1024) := UPPER(SYS_CONTEXT('USERENV','SESSION_USER'));
	v_SQL			VARCHAR2(4000);
  	v_Subject   		VARCHAR2(1024);
BEGIN
	IF UPPER(:NEW.PARAMETER) = 'DATASTORE_TERMINATE' THEN
		IF UPPER(:NEW.VALUE) != UPPER(:OLD.VALUE) THEN
			v_Msg := 'Get Database Name';
			BEGIN
				-- Oracle 12c and later, CON_NAME can be used
				v_DBName := UPPER(SYS_CONTEXT('USERENV','CON_NAME'));
			EXCEPTION
			WHEN OTHERS THEN
				-- Before Oracle 12c, CON_NAME did not exist so use DB_NAME
				v_DBName := UPPER(SYS_CONTEXT('USERENV','DB_NAME'));
			END;
			
			v_Msg := 'Get Email Config info from DataStoreEmailSetUp table';
			v_SQL := 'SELECT VALUE FROM DATASTOREEMAILSETUP WHERE PARAMETER = :b1';
			EXECUTE IMMEDIATE v_SQL INTO v_EmailSender USING 'EMAIL_SENDER';
			EXECUTE IMMEDIATE v_SQL INTO v_EmailRecipients USING 'EMAIL_RECIPIENTS';
	
			-- Validate Email Server, Sender and Recipients are populated.  If not, exit without attempting to send an email
			v_Msg := 'Validate email server, email sender and email recipients';
			IF (v_EmailSender IS NULL OR v_EmailRecipients IS NULL) THEN
				RETURN;
			END IF;
			V_HTMLCSS := '<style type="text/css">
			h3 {
			font-family: "Lucida Grande", Tahoma;
			font-size: 26px;
 			font-weight: 500;
		 	font-variant: normal;
 			color: #ffa500;
			margin-top: 1px;
	 		text-align: center!important;
	 		letter-spacing: 1px;
			text-align: left;
			}
			h1 {
			font-family: "Lucida Grande", Tahoma;
			font-size: 16px;
			font-weight: 200;
			font-variant: normal;
			color: #8B8B83;
		       	margin-top: 1px;
			ext-align: center!important;
			letter-spacing: 1px;
			text-align: justify;
			}
			#box-table
			{
			font-family: "Lucida Grande", Tahoma;
			font-size: 14px;
			text-align: justify;
			border-collapse: collapse;
			border-top: 7px solid #c0c0c0;
			border-bottom: 7px solid #c0c0c0c;
			vertical-align: text-top;
			max-width:1200px; min-width:1200px;
			}
			#box-table th
			{
			font-size: 16px;
			font-weight: normal;
			background: #caff70;
			color: #000000;
			border-right: 2px solid #c0c0c0;
			border-left: 2px solid #c0c0c0;
			border-bottom: 2px solid #c0c0c0;
			text-align: center;
			vertical-align: middle;
			}
			#box-table td
			{
			font-size: 12px;
			border-right: 1px solid #c0c0c0;
			border-left: 1px solid #c0c0c0;
			border-bottom: 1px solid #c0c0c0;
			color: #000000;
			text-align: left;
			vertical-align: text-top;
			}
			</style>';
	
			IF UPPER(:NEW.VALUE) = 'Y' THEN
				v_Msg := 'DataStore has stopped.  Preparing to send email.';
				v_Subject := 'DataStore Database has stopped';

				v_Message := '<h3>The DataStore Database has stopped.</h3>' 
				||'<table style="border-collapse: collapse;">'
				||'<tr><td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #000000; font-weight: bold; width: 100px; text-align: left">Host:</td>'
				||'<td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #8B8B83; width: 400px;">' 
				|| cCRLF || cCRLF || v_ServerHost || '</td></tr>'
				||'<tr><td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #000000; font-weight: bold; width: 100px; text-align: left">Database:</td>' 
				||'<td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #8B8B83; width: 400px;">' 
				|| cCRLF || cCRLF || v_DBName || '</td></tr><tr style="height: 40px;"></tr></Table>'
				|| '<p></p>'
				|| '<h3 style="color: #000000; font-size: 20px; font-weight: 200;">'
				|| cCRLF || 'Please check ' || v_SessionUser || '.DATASTORELOG and ' || v_SessionUser || '.DATASTOREERRORS tables in the DataStore database for details.</h3>';
			
  				v_SQL := 'BEGIN UTL_MAIL.SEND(';		
				v_SQL := v_SQL || 'SENDER 	=> ' || CHR(39) || v_EmailSender || CHR(39);
       			v_SQL := v_SQL || ',RECIPIENTS 	=> ' || CHR(39) || v_EmailRecipients || CHR(39);
				v_SQL := v_SQL || ',SUBJECT 	=> ' || CHR(39) || v_Subject || CHR(39);
       	 		v_SQL := v_SQL || ',MESSAGE 	=> ' || CHR(39) ||V_HTMLCSS || v_Message || CHR(39);
       			v_SQL := v_SQL || ',MIME_TYPE 	=> ''text/html''); END;';
				EXECUTE IMMEDIATE v_SQL;
			ELSIF UPPER(:NEW.VALUE) = 'N' THEN
				v_Msg := 'DataStore has started.  Preparing to send email.';
				v_Subject := 'DataStore Database has started';

       	 		v_Message := '<h3 style="font-family: Lucida Grande, Tahoma; font-size: 26px; font-weight: normal; color: #32cd32;">'
					||'The DataStore Database  has started.</h3>'
					||'<table style="border-collapse: collapse;">' 
					||'<tr><td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #000000; font-weight: bold; width: 100px; text-align: left">Host:</td>' 
					||'<td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #8B8B83; width: 400px;">' 
					|| cCRLF || cCRLF || v_ServerHost || '</td></tr>'
					||'<tr><td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #000000; font-weight: bold; width: 100px; text-align: left">Database:</td>' 
					||'<td style="font-family: Lucida Grande, Tahoma; font-size: 16px; color: #8B8B83; width: 400px;">' 
					|| cCRLF || cCRLF || v_DBName || '</td></tr> </Table>';

 				v_SQL := 'BEGIN UTL_MAIL.SEND(';		
				v_SQL := v_SQL || 'SENDER 	=> ' || CHR(39) || v_EmailSender || CHR(39);
       			v_SQL := v_SQL || ',RECIPIENTS 	=> ' || CHR(39) || v_EmailRecipients || CHR(39);
				v_SQL := v_SQL || ',SUBJECT 	=> ' || CHR(39)  || v_Subject || CHR(39);
       	 		v_SQL := v_SQL || ',MESSAGE 	=> ' || CHR(39) || v_Message || CHR(39);
       			v_SQL := v_SQL || ',MIME_TYPE 	=> ''text/html''); END;';
				EXECUTE IMMEDIATE v_SQL;
			END IF;
		END IF;
	END IF;
EXCEPTION
WHEN OTHERS THEN
		IF UPPER(:NEW.VALUE) = 'Y' THEN
			v_Loc := 'DATASTOREEMAILTRG <stop>';
		END IF;

		IF UPPER(:NEW.VALUE) = 'N' THEN
			v_Loc := 'DATASTOREEMAILTRG <start>';
		END IF;

		v_Err := 'Exception Handler (trigger catch block) : Last message set : ' || v_Msg;
		INSERT INTO DATASTORELOG(JOB, PACKAGE_EXECUTING, LOG_SEQ, LOG_LEVEL, LOC, MESSAGE)
		VALUES (0, 'N/A', 0, 0, v_Loc, v_Err);
		COMMIT;

		v_Err := 'Exception Handler (trigger catch block) : ' || SUBSTR(SQLERRM,1,1024);
		INSERT INTO DATASTORELOG(JOB, PACKAGE_EXECUTING, LOG_SEQ, LOG_LEVEL, LOC, MESSAGE)
		VALUES (0, 'N/A', 1, 0, v_Loc, v_Err);
		COMMIT;

		v_Err := 'Exception Handler (trigger catch block) : The email server information may not be correct or the Oracle ACL may not be set up correctly.  '
		|| 'See the DataStore Reference Guide for information on configuring email alerts.';
		INSERT INTO DATASTORELOG(JOB, PACKAGE_EXECUTING, LOG_SEQ, LOG_LEVEL, LOC, MESSAGE)
		VALUES (0, 'N/A', 2, 0, v_Loc, v_Err);
		COMMIT;
END;
/




