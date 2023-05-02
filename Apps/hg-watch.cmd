@echo off
start /d "%~dp0\WebClient\src" dotnet watch run .
start /d "%~dp0\Patient\src" dotnet watch run .
start /d "%~dp0\GatewayApi\src" dotnet watch run .
start /d "%~dp0\Encounter\src" dotnet watch run .
start /d "%~dp0\Immunization\src" dotnet watch run .
start /d "%~dp0\Laboratory\src" dotnet watch run .
start /d "%~dp0\Medication\src" dotnet watch run .
start /d "%~dp0\ClinicalDocument\src" dotnet watch run .
