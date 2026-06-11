DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csiBCPStageTables')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csiBCPStageTables;
 	END IF;
END $$;
CREATE PROCEDURE csiBCPStageTables(
  i_action varchar(10),
  owner_name varchar(128),
  dbname varchar(128),
  file_path varchar(512),
  servername varchar(128)
)
language plpgsql
as $$
DECLARE
	bcpcmd varchar(500);
	table_name varchar(128);
	table_cursor CURSOR FOR
		SELECT table_name
		FROM information_schema.tables
		WHERE table_name LIKE 'LS#%'
			  AND table_type = 'BASE TABLE'
			  AND table_schema = owner_name
		UNION
		SELECT 'CONTROLDETAILS'
		UNION
		SELECT 'UPDATECONTROL';

BEGIN

    OPEN table_cursor;
	
    LOOP
        FETCH table_cursor INTO table_name;
		EXIT WHEN NOT FOUND;
        
        IF i_action = 'EXPORT' THEN
            bcpcmd := 'COPY ' || owner_name || '.' || table_name || ' TO ''' || file_path || '/' || table_name || '.txt''';
            RAISE NOTICE '%', bcpcmd;
        ELSIF i_action = 'IMPORT' THEN
            bcpcmd := 'COPY ' || owner_name || '.' || table_name || ' FROM ''' || file_path || '/' || table_name || '.txt''';
        END IF;

        RAISE NOTICE '%', bcpcmd;
    END LOOP;
END;
$$;