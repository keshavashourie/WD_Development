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
-- Copyright Siemens 2023  
IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csiFilterTagMatch' 
	   AND 	  type = 'FN')
    DROP FUNCTION csiFilterTagMatch
GO
CREATE FUNCTION csiFilterTagMatch
(
   @InstanceTags NVARCHAR(MAX),
   @UserTags  NVARCHAR(MAX),
   @IncludeUntaggedInstances int
)
RETURNS Bit
AS
BEGIN
	DECLARE @Found Int
	DECLARE @Delim NVarchar(1)
	DECLARE @DelimLength Int
	DECLARE @Offset Int 
	DECLARE @LastOffset Int 
	DECLARE @SqlStmt nvarchar(MAX) 
	DECLARE @UserTag nvarchar(MAX) 

	SET @Delim = ','
	SET @DelimLength = Len(@Delim) 

	-- If filter tag access is All instances, return true
	IF @IncludeUntaggedInstances = 3
		RETURN 1
	
	-- If the instance has no tags, no need to do the compare logic, return 
	-- 0 if filter tag access if tagged only 
	IF @InstanceTags IS NULL 
	BEGIN 
		-- Return false if tagged only access
		IF @IncludeUntaggedInstances = 2
			RETURN 0
		ELSE 
			RETURN 1
	END
	
	-- If the instance has only a single tag, don't go through the split logic
	IF CHARINDEX(@Delim,@InstanceTags) = 0
	BEGIN
		IF @Delim + @UserTags + @Delim LIKE '%' + @InstanceTags + '%'			
			RETURN 1
		ELSE	
			-- Return false if tagged only access
			IF (@IncludeUntaggedInstances = 2)
			BEGIN
				RETURN 0
			END
	END
	
	-- The user tag is blank only return untagged instances
	IF LEN(@UserTags) = 0
	BEGIN
		IF @InstanceTags IS NULL
		BEGIN
			-- Return true if tagged and untagged access
			IF (@IncludeUntaggedInstances = 1)
				RETURN 1
		END
		ELSE
			RETURN 0
	END
	
	-- The user tag is a single, don't go through the split logic
	IF CHARINDEX(@Delim,@UserTags) = 0
	BEGIN
		IF @Delim + @InstanceTags + @Delim LIKE '%' + @UserTags + '%'
			RETURN 1
		ELSE
			RETURN 0
	END
	SET @Offset=1 
	SET @LastOffset=1 
	--
	-- Split up the user tag and check to see if it exists in the instance tag list
	WHILE (@Offset>0)
	BEGIN 
		SET @Offset=CHARINDEX(@Delim,@UserTags + @Delim,@LastOffset) 		
		IF (@Offset>0) 
		BEGIN
			SET @UserTag = SUBSTRING(@UserTags + @Delim,@LastOffset,@Offset-@LastOffset)	
			IF (LEN(@UserTag) > 0) -- Ignore zero-length "tags"
			BEGIN
				IF @Delim + @InstanceTags + @Delim LIKE '%' + @UserTag + '%'
					RETURN 1
			END
			SET @Offset=@Offset+@DelimLength 
			SET @LastOffset=@Offset 
		END 
	END
	RETURN 0
END
GO
