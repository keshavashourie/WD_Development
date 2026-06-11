/* ********************************************************************************
csibiginttohex
   The following is used to convert bigint values to hex strings for 
   compatibility with the current txnid structure
   NOTE: 
   The sqlserver convert function does not produce accurate results 
   with bigint values
   Alternative is to use master.xp_varbintohexstr extended procedure
   but it is limited to int values only - big ints get converted as garbage
     declare @b binary(4), @str varchar(255)
     select @b = 3455643
     exec master..xp_varbintohexstr @b, @str out
     print @str

  History:
    Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
    Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
******************************************************************************** */

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csibiginttohex' 
	   AND 	  type = 'P')
    DROP procedure csibiginttohex
GO

create procedure csibiginttohex 
     @intvalue bigint,  
     @charvalue varchar(255) output 
  as
  -- 
  -- Copyright Siemens 2023  
  --
  begin  
    declare @i int
    declare @length int
    declare @hexstring varchar(18)
    declare @binvalue varbinary(255)

    set @binvalue = convert(varbinary(255),@intvalue) 
    set @charvalue = '0x'
    set @i = 1
    set @hexstring = '0123456789abcdef'
    
    /* THE FOLLOWING IS USED TO SET LOOP COUNTER DIFFERENTLY FOR BIGINT VALUES */
    if @intvalue <= 2147483647
       set @length = datalength(@binvalue)
    else 
       set @length = len(convert(char, @intvalue))

    while (@i <= @length)
    begin
      declare @tempint bigint
      declare @firstint bigint
      declare @secondint bigint

      set @tempint =  convert(bigint, substring(@binvalue,@i,1))
      set @firstint = floor(@tempint/16)
      set @secondint = @tempint - (@firstint*16)

      set @charvalue = @charvalue +
        substring(@hexstring, @firstint+1, 1) +
        substring(@hexstring, @secondint+1, 1)

     set @i = @i + 1
    end
  end
go
