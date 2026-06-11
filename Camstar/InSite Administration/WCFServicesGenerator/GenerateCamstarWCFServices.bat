REM Copyright Siemens 2023  
WCFServicesGenerator\WCFBuilderLauncher.exe -nogui -nosilverlight

REM Copy WCF server libraries to CamstarWCFServices virtual folder.
xcopy "..\GeneratedAssemblies\ToServer" "..\Camstar WCF Services" /s/e/y

REM Copy WCF client libraries to Camstar Portal folder.
xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClient.dll" "..\Camstar Portal\Bin\" /y
REM xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClient.pdb" "..\Camstar Portal\Bin\" /y
xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClientBase.dll" "..\Camstar Portal\Bin\" /y
REM xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClientBase.pdb" "..\Camstar Portal\Bin\" /y

REM Copy WCF client libraries to PERSConverter folder.
xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClient.dll" "..\InSite Conversion Tool\PERSConverter\" /y
REM xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClient.pdb" "..\InSite Conversion Tool\PERSConverter\" /y
xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClientBase.dll" "..\InSite Conversion Tool\PERSConverter\" /y
REM xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClientBase.pdb" "..\InSite Conversion Tool\PERSConverter\" /y

REM Copy WCF client libraries to Portal Studio folder
if exist "..\Portal Studio\Bin\" (
xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClient.dll" "..\Portal Studio\Bin\" /y
REM xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClient.pdb" "..\Portal Studio\Bin\" /y
xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClientBase.dll" "..\Portal Studio\Bin\" /y
REM xcopy "..\GeneratedAssemblies\ToClient\Camstar.WCFClientBase.pdb" "..\Portal Studio\Bin\" /y
)
