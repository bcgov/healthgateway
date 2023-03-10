@echo off
start /d "%~dp0\Apps\WebClient\src" dotnet watch run .
start /d "%~dp0\Apps\Patient\src" dotnet watch run .
start /d "%~dp0\Apps\GatewayApi\src" dotnet watch run .
start /d "%~dp0\Apps\Encounter\src" dotnet watch run .
start /d "%~dp0\Apps\Immunization\src" dotnet watch run .
start /d "%~dp0\Apps\Laboratory\src" dotnet watch run .
start /d "%~dp0\Apps\Medication\src" dotnet watch run .
start /d "%~dp0\Apps\ClinicalDocument\src" dotnet watch run .
