CREATE OR REPLACE PROCEDURE MOVE_TABLE ( p_TableOwner			IN VARCHAR2
					,p_TableName			IN VARCHAR2
					,p_DestinationTablespace	IN VARCHAR2 )
IS
--
------------------------------------------------------------------------------------------------
-- This program moves a table including LOB columns to a new tablespace
--
--
-- CAUTION ****	CAUTION ****	CAUTION ****	CAUTION ****	CAUTION
-- 
-- Before running this script, please ensure to back up the database
-- and verify the back ups are good.
-- 
-- CAUTION ****	CAUTION ****	CAUTION ****	CAUTION ****	CAUTION
--
--
--  Modification History:
--  Name			Date		Action
--  --------------------------	----------	----------------
--  Purushotham Neelakantachar	10/12/2009  	Initial Creation
--
--  Copyright Siemens 2023  
------------------------------------------------------------------------------------------------
--
v_SQLStmt		VARCHAR2(1024);
--
n_ErrLocator		NUMBER;
--
BEGIN
	--
	IF ( NVL(p_TableOwner ,'XXX') <> 'XXX' AND
	     NVL(p_TableName ,'XXX') <> 'XXX' AND
	     NVL(p_DestinationTablespace ,'XXX') <> 'XXX' )
	THEN
		--
		n_ErrLocator := 5;
		--
		v_SQLStmt := 'ALTER TABLE '||p_TableOwner||'.'||p_TableName||' MOVE TABLESPACE '||p_DestinationTablespace;
		--
		n_ErrLocator := 10;
		--
		EXECUTE IMMEDIATE v_SQLStmt;
		--
		DBMS_OUTPUT.PUT_LINE('MOVE_TABLE - Table "'||p_TableName||'" moved to "'||p_DestinationTablespace||'" tablespace successfully');
		--
		v_SQLStmt := NULL;
		--
		FOR CurrLOB IN ( SELECT Table_Name
				       ,Column_Name
				       ,Segment_Name
				       ,Tablespace_Name
				   FROM USER_LOBS
				  WHERE Table_Name = p_TableName )
		LOOP
			--
			v_SQLStmt := 'ALTER TABLE '||CurrLOB.Table_Name||' MOVE LOB('||CurrLOB.Column_Name||') STORE AS (TABLESPACE '||p_DestinationTablespace||')';
			--
			n_ErrLocator := 15;
			--
			EXECUTE IMMEDIATE v_SQLStmt;
			--
			DBMS_OUTPUT.PUT_LINE('MOVE_TABLE - LOB column "'||CurrLOB.Column_Name||'" moved to "'||p_DestinationTablespace||'" tablespace successfully');
			--
		END LOOP;
		--
	ELSE
		--
		DBMS_OUTPUT.PUT_LINE('MOVE_TABLE - Table Owner or Table Name or Tablespace Name is invalid. NULL values are not allowed');
		--
	END IF;
	--
EXCEPTION
	WHEN OTHERS THEN
		--
		RAISE_APPLICATION_ERROR(-20401,'MOVE_TABLE - Error moving LOB segments - ErrLoc: '||n_ErrLocator||' ErrMsg: '||SQLERRM);
		--
END;
/
