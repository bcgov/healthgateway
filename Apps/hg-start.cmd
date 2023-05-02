@echo off
start /d "%~dp0\WebClient\src" dotnet run .
start /d "%~dp0\Patient\src" dotnet run .
start /d "%~dp0\GatewayApi\src" dotnet run .
start /d "%~dp0\Encounter\src" dotnet run .
start /d "%~dp0\Immunization\src" dotnet run .
start /d "%~dp0\Laboratory\src" dotnet run .
start /d "%~dp0\Medication\src" dotnet run .
start /d "%~dp0\ClinicalDocument\src" dotnet run .
