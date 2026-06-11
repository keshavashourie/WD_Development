REM Copyright Siemens 2023  
::Personalization convertion from 5.X to v6

@ECHO OFF

::Start
ECHO ***** PERSConverter *****
ECHO Converts v5.x Pages and WebParts to workspace enabled personalization.
ECHO SystemContent will automatically be converted to the CSI workspace.
ECHO PublishedContent will be converted to the currently highest active workspace.
ECHO.

::Info
ECHO Reading files from %~dp0Source
ECHO Writing files to %~dp0Converted
ECHO.

::Execute
PERSConverter Source Converted

PAUSE
