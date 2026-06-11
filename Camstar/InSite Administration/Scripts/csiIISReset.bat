@ECHO off
REM setlocal
REM --------------------------------------------------------------------------------------------------
REM  csiPopulateCMUpdate.bat
REM  Simple batch file used to run the IIS reset.
REM  Assumptions:
REM  Usage: 
REM    csiIISReset.bat
REM
REM Modification History:
REM Name			Date        	Action
REM --------------------------	----------	------------------------------------------------------
REM Ken LU			04/17/2015     Created to reset IIS 
REM Copyright Siemens 2023  
REM --------------------------------------------------------------------------------------------------

TIME /t
SET ERRORSTATUS=0

IISRESET.exe /RESTART
IF ERRORLEVEL 1 set errorstatus=99
GOTO EXIT
:END

:USAGE
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO   "csiIISReset.bat"
ECHO ------------------------------------------------------------------------------
ECHO Copyright  Siemens 2019  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END


:EXIT
REM endlocal
IF %ERRORSTATUS% NEQ 0 (
	ECHO Error Resetting IIS
	PAUSE
	)
IF %ERRORSTATUS% NEQ 0 EXIT %ERRORSTATUS%

:END
