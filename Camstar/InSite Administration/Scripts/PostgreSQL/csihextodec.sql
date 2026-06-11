DO $$ 
BEGIN
 	IF EXISTS (
 		SELECT 1
 		FROM information_schema.routines
 		WHERE lower(routine_name) = lower('csihextodec')
 		AND routine_type = 'PROCEDURE'
 	) then
 		DROP PROCEDURE IF EXISTS csihextodec;
 	END IF;
END $$;

CREATE PROCEDURE csihextodec(hexnum varchar(16), out result bigint)
language plpgsql
as $$
DECLARE
  x integer;
  digits bigint;
  current_digit varchar(1);
  current_digit_dec bigint;
BEGIN
  digits := length(hexnum);
  result := 0;
  x := 0;
  
  WHILE x < digits LOOP
    x := x + 1;
    current_digit := upper(substring(hexnum, x, 1));
    
    CASE 
      WHEN current_digit IN ('A', 'B', 'C', 'D', 'E', 'F') THEN
        current_digit_dec := ascii(current_digit) - ascii('A') + 10;
      ELSE
        current_digit_dec := current_digit::int;
    END CASE;

    result := (result * 16) + current_digit_dec;
  END LOOP;
END;
$$;