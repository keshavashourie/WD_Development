/* ********************************************************************************
csihextodec
  Procedure to convert hex strings to decimal values.
  Only used during the creation of the TxnIdCounter table (csiResetTXNID).
Parameters:
    hexnum - hex string
    result - integer value of the hex string provided
    
Usage: exec csihextodec <hexstring> <@intvar> OUTPUT

History:
   Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
   Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).

NOTE: 
   Should be a function, but Limitation of SQLServer requires that user 
   function calls be qualified with the owner name.
********************************************************************************** */

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csihextodec' 
	   AND 	  type = 'P')
    DROP procedure csihextodec
GO

create procedure csihextodec 
    @hexnum as varchar(16),  @result bigint output
  AS
  --
  -- Copyright Siemens 2023  
  --
  set nocount on
  BEGIN
    DECLARE @x       bigint 
    DECLARE @digits  bigint                           
    DECLARE @current_digit varchar(1)                           
    DECLARE @current_digit_dec bigint                                           

    set @digits = len(@hexnum)                                     
    set @result=0
    set @x=0
    while (@x < @digits) 
      begin
        set @x=@x+1                                     
        set @current_digit = upper(SUBSTRING(@hexnum, @x, 1))                         
        if @current_digit in ('A','B','C','D','E','F')           
           set @current_digit_dec = ascii(@current_digit) - ascii('A') + 10  
        else                                                               
           set @current_digit_dec = cast(@current_digit as int)                                                         
        set @result = (@result * 16) + @current_digit_dec                 
      end      
  END
go
