/*-- =======================================================================
-- csihextodec
--   Function to convert hex strings to decimal values.
--   Only used during the creation of the TxnIdCounter sequence (csiResetTXNID).
--
--  History:
--      Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
--      Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
-- ======================================================================= */

create or replace FUNCTION csihextodec (hexnum in char) RETURN number IS 
--
-- Copyright Siemens 2023  
--
  x       number; 
  digits  number;                                                      
  result  number := 0;                                            
  current_digit char(1);                           
  current_digit_dec number;                                           

  BEGIN                                                               
    digits := length(hexnum);                                      
    for x in 1..digits loop                                         
        current_digit := upper(SUBSTR(hexnum, x, 1));                         
        if current_digit in ('A','B','C','D','E','F') then          
           current_digit_dec := ascii(current_digit) - ascii('A') + 10;   
        else                                                               
           current_digit_dec := to_number(current_digit);  
        end if;                                                           
        result := (result * 16) + current_digit_dec;                 
        end loop;                                                     
    return result;                                               
  END; 
/

