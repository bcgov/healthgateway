@echo off
start /d "%~dp0\Apps\WebClient\src" dotnet run .
start /d "%~dp0\Apps\Patient\src" dotnet run .
start /d "%~dp0\Apps\GatewayApi\src" dotnet run .
start /d "%~dp0\Apps\Encounter\src" dotnet run .
start /d "%~dp0\Apps\Immunization\src" dotnet run .
start /d "%~dp0\Apps\Laboratory\src" dotnet run .
start /d "%~dp0\Apps\Medication\src" dotnet run .
start /d "%~dp0\Apps\ClinicalDocument\src" dotnet run .
