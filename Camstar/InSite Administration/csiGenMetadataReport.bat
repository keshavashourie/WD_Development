@ECHO off
REM setlocal
REM --------------------------------------------------------------------------------------------------
REM  csiGenMetadataReport.bat
REM  Simple batch file used to generate the metadata report from the metadata export xml
REM  Assumptions:
REM     - Run from the same directory as the InSiteMetadataReporting.xslt file
REM
REM  Usage: 
REM    csiGenMetadataReport.bat <Input XML File> <Output XML File>
REM
REM Copyright Siemens 2023  


SET ERRORSTATUS=0

REM Set local variables
REM -------------------
:SETVARIABLES
SET XML=%1
IF (%XML%)==() GOTO USAGE
SET HTML=%2
IF (%HTML%)==() GOTO USAGE


:GENERATE
REM Set errorstatus if unable to generate
AltovaXML.exe -xslt2 InSiteMetadataReporting.xslt -in %XML% -out %HTML%
start %HTML%
GOTO EXIT
:END


:USAGE
ECHO -----------------------------------------------------------------------------
ECHO USAGE:
ECHO   "csiGenMetadataReport.bat <Input XML File> <Output XML File>"
ECHO Where
ECHO  "<Input XML File>"   	 - Specify input file, metada export XML file genetrated by Designer
ECHO  "<Output XML File>"	 - Specify output file for the metadata report HTML, 
ECHO  Example :- "csiGenMetadataReport.bat CompareInSiteToInSiteBase.xml MetadataExtensionsReport.html"
ECHO ------------------------------------------------------------------------------
ECHO Copyright Siemens 2023  
ECHO ------------------------------------------------------------------------------
GOTO EXIT
:END


:EXIT
:END
