/* csiFilterTagMatch
--     Compares filter tags of a row with passed-in tags for use with filtering  
--    
-- Input Parms: InstanceTags - Comma-separated list of tags from instance row
--              UserTags - Comma-separated list of tags assigned to the user
--              IncludeUntaggedInstances - If 1, return true for rows that have no tags.
--
-- Called by NDO/RDO SelectionValues queries
--
-- Modification History:
--	Name				  Date		    Action
--  Paul Mojica   2017-11-07  Implement phase 2 with filter tag access
--	----------------	----------	---------------------------------------------------
--
-- Copyright Siemens 2023  
*/
CREATE OR REPLACE FUNCTION csiFilterTagMatch(pInstanceTags IN VARCHAR2, pUserTags IN VARCHAR2, pIncludeUntaggedInstances IN NUMBER) RETURN NUMBER
AS
	vFound       NUMBER;
	vDelim       VARCHAR2(1) :=',';
	vDelimLength NUMBER;
	vOffset      NUMBER := 1; 
	vLastOffset  NUMBER := 1; 
	vUserTag     CLOB; 
BEGIN
   vDelimLength := LENGTH(vDelim);
   
   -- If filter tag access is All instances, return true
    IF (pIncludeUntaggedInstances = 3) THEN
      RETURN 1;
	END IF;
      
   -- If the instance has no tags, no need to do the compare logic, return 
   -- 0 if filter tag access if tagged only 
	IF (pInstanceTags IS NULL) THEN 
		-- Return false if tagged only access
		IF (pIncludeUntaggedInstances = 2) THEN
			RETURN 0;	
		ELSE
			RETURN 1;
		END IF;
	END IF;

	-- If the instance has only a single tag, don't go through the split logic
	IF (INSTR(pInstanceTags,vDelim) = 0) THEN
		IF (vDelim||pUserTags||vDelim LIKE '%'||pInstanceTags||'%') THEN
			RETURN 1;
		ELSE
			-- Return false if tagged only access
			IF (pIncludeUntaggedInstances = 2) THEN
				RETURN 0;	
			END IF;
		END IF;
	END IF;
  
	-- The usesr tag is blank, only return untagged instances
	IF (LENGTH(pUserTags) = 0) THEN
		IF (pInstanceTags IS NULL) THEN
			-- Return true if tagged and untagged access
			IF (pIncludeUntaggedInstances = 1) THEN
				RETURN 1;
			END IF;
		ELSE
			RETURN 0;
		END IF;
	END IF;
  
	-- The user tag is a single, don't go through the split logic
	IF (INSTR(pUserTags,vDelim) = 0) THEN
		IF (vDelim||pInstanceTags||vDelim LIKE '%'||pUserTags||'%') THEN
			RETURN 1;
		ELSE
			RETURN 0;
		END IF;
	END IF;
  
	--
	-- Split up the user tag and check to see if it exists in the instance tag list
	WHILE (vOffset!=0) LOOP
		vOffset:=INSTR(pUserTags||vDelim,vDelim,vLastOffset); 		
		IF (vOffset>0) THEN
			vUserTag := SUBSTR(pUserTags||vDelim,vLastOffset,vOffset-vLastOffset);	
			IF (LENGTH(vUserTag) > 0) THEN-- Ignore zero-length "tags"
				IF (vDelim||pInstanceTags||vDelim LIKE '%'||vUserTag||'%') THEN
					RETURN 1;
        END IF;
			END IF;
			vOffset:=vOffset+vDelimLength;
			vLastOffset:=vOffset; 
		END IF;
	END LOOP;
	RETURN 0;
END;
/
