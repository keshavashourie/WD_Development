/*
csiCopyTable
     Simple proceure for copying a table
Parameters:
    Source_Owner - Owner of the source table
    Source_Name - Name of the source table
    Target_Owner - Owner of the target table
    Target_Table - name of the target table
Usage: exec csiCopyTable <Source_Owner>, <Source_Table>,<Target_Owner>,
                         <Target_Table>,['S'/'T']
History:
  Bill Lippard      12/04/2006      Added copyright notice (SPR S9984).
  Bill Lippard      04/23/2007      Updated copyright notice (SPR S9984).
*/

IF EXISTS (SELECT name 
	   FROM   sysobjects 
	   WHERE  name = 'csicopytable' 
	   AND 	  type = 'P')
    DROP PROCEDURE csicopytable
GO

CREATE PROCEDURE csicopytable 
  @source_owner   varchar(255),
  @source_name    varchar(255),
  @target_owner   varchar(255),
  @target_name    varchar(255),
  @gen_columns    char(1) = 'N'   /* SPECIFY WHICH TABLE TO USE TO GENERATE COLUMN LIST
                                     'S' = SOURCE TABLE 'T' = TARGET TABLE */

AS

 -- 
 -- Copyright Siemens 2023  
 --
 declare @sqlstatement varchar(8000)
 declare @column_list    varchar(8000)

 BEGIN
    IF EXISTS (SELECT name FROM sysobjects 
               WHERE name = @target_name AND type = 'U' and uid=user_id(@target_owner))
      IF @gen_columns = 'T' 
         BEGIN
           /* GENERATE COLUMN LIST FOR THE TARGET TABLE
              ADDED FOR LIVESYNC "SYNC" PROCESSING  */
           exec csigetcolumnlist @target_owner, @target_name, @column_list output
           set @sqlstatement = 'truncate table ' + @target_owner + '.' + @target_name
           exec(@sqlstatement)
           set @sqlstatement = 'insert into '+ @target_owner + '.' + @target_name + 
                               '(' + @column_list +')' +
                               ' select ' + @column_list + ' from ' + @source_owner + '.' + 
                                 @source_name
           exec(@sqlstatement)
         END
      ELSE
        BEGIN
          IF @gen_columns = 'S'
            BEGIN
            /* GENERATE COLUMN LIST FOR THE SOURCE TABLE
               ADDED FOR LIVESYNC "RESTORE" PROCESSING WHERE TARGET TABLE SCHEMA 
               MAY HAVE BEEN MODIFIED */
            exec csigetcolumnlist @source_owner, @source_name, @column_list output
            set @sqlstatement = 'truncate table ' + @target_owner + '.' + @target_name
            exec(@sqlstatement)
            set @sqlstatement = 'insert into '+ @target_owner + '.' + @target_name + 
                                '(' + @column_list +')' +
                                ' select ' + @column_list + ' from ' + @source_owner + '.' + 
                                  @source_name
            exec(@sqlstatement)
            END
          ELSE
            print 'Target Table exists and no column list specified'
        END
    ELSE   /* TARGET TABLE DOES NOT EXIST PERFORM CREATE WITH THE INSERT INTO SYNTAX */
      BEGIN
         set @sqlstatement = 'select * into '+ @target_owner + '.' + @target_name +
                         ' from ' + @source_owner + '.' + @source_name
         exec(@sqlstatement)
      END
 END
GO





