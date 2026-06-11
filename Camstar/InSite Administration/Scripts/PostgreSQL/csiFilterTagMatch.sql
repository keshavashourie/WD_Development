----------------------------------------------------------------------------------------
-- csiFilterTagMatch
--
-- Compares filter tags of a row with passed-in tags for use with filtering  
--    
-- Input Parms: InstanceTags - Comma-separated list of tags from instance row
--              UserTags - Comma-separated list of tags assigned to the user
--              IncludeUntaggedInstances - If 1, return true for rows that have no tags.
--
-- Called by NDO/RDO SelectionValues queries
--
-- Modification History:
--	Name				Date		Action
--	----------------	----------	---------------------------------------------------
--  Paul Mojica			2017-11-08	Implementation of phase 2 for filter tags
----------------------------------------------------------------------------------------
--
-- Copyright Siemens 2024  

DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiFilterTagMatch')
 		AND routine_type = 'FUNCTION'
 	) then
 		DROP FUNCTION IF EXISTS csiFilterTagMatch;
 	END IF;
END $$;

CREATE FUNCTION csiFilterTagMatch
(
   InstanceTags VARCHAR,
   UserTags  VARCHAR,
   IncludeUntaggedInstances INTEGER
)
RETURNS INTEGER
language plpgsql
as
$$
DECLARE 
	Found INTEGER;
	Delim CHAR(1);
	DelimLength INTEGER;
	CurOffset INTEGER;
	SqlStmt VARCHAR;
	UserTag VARCHAR;
	SubUserTags VARCHAR;
BEGIN
	
	Delim := ',';
	DelimLength := Length(Delim); 

	-- If filter tag access is All instances, return true
	IF IncludeUntaggedInstances = 3 THEN
		RETURN 1;
	end if;
	
	-- If the instance has no tags, no need to do the compare logic, return 
	-- 0 if filter tag access if tagged only 
	IF InstanceTags IS NULL THEN
		-- Return false if tagged only access
		IF IncludeUntaggedInstances = 2 THEN
			RETURN 0;
		ELSE 
			RETURN 1;
		end if;
	end if;

	-- If the instance has only a single tag, don't go through the split logic	
	IF STRPOS(InstanceTags, Delim) = 0 THEN
		IF Delim || UserTags || Delim LIKE '%' || InstanceTags || '%' THEN			
			RETURN 1;
		ELSE	
			-- Return false if tagged only access
			IF (IncludeUntaggedInstances = 2) then
				RETURN 0;
			end if;
		end if;
	end if;

	-- The user tag is blank only return untagged instances
	IF Length(UserTags) = 0 then
		IF InstanceTags IS null THEN		
			-- Return true if tagged and untagged access
			IF (IncludeUntaggedInstances = 1) then
				RETURN 1;
			end if;		
		else
			RETURN 0;
		end if;
	end if;

	-- The user tag is a single, don't go through the split logic
	IF STRPOS(UserTags, Delim) = 0 THEN	
		IF Delim || InstanceTags || Delim LIKE '%' || UserTags || '%' then
			RETURN 1;
		else
			RETURN 0;
		end if;
	end if;

	CurOffset:=1;
	SubUserTags:=UserTags;
	--
	-- Split up the user tag and check to see if it exists in the instance tag list
	WHILE Length(SubUserTags)>0 loop
		
		IF POSITION(Delim IN (SubUserTags || Delim)) > 0 THEN 
			CurOffset := POSITION(Delim IN (SubUserTags || Delim));
	    ELSE 
	    	CurOffset := 0;
	    end if;	   
	   
	   if CurOffset > 0 then
	   			   		 
	   		UserTag := SUBSTRING(SubUserTags || Delim,1,CurOffset);
	   		if Length(UserTag) > 0 then
	   			if Delim || InstanceTags || Delim LIKE '%' || UserTag || '%' then
	   				return 1;
	   			end if;
	   		end if;
	   	
	   		SubUserTags := SUBSTRING(SubUserTags,CurOffset + LENGTH(Delim),Length(SubUserTags));

	   end if;
	  
	end loop;
	return 0;	
	
END;
$$