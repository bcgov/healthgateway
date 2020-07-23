# Windows Workstation Setup

## Requirements

* Windows 10
  * Version 2004 for Linux based development
* i7 or higher processor and 16GB of RAM.

## Software Install

The following software will be installed:

* Chocolately
* Docker
* VS Code
* PG Admin
* Optional Software
* Development Setup
  * Linux based
  * Native Windows
* Next Steps

### Chocolately

Open a Windows PowerShell as an Administrator and execute the following:

```powershell
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
choco --version
exit
```

### Install Docker

Open a Windows PowerShell as an Administrator and execute the following:

```powershell
choco install wsl -y
restart-computer
```

After the computer restarts, open a Windows PowerShell as an Administrator and execute the following:

```powershell
choco install wsl2 -y
choco install docker-desktop -y
restart-computer
```

After the computer reboots, wait for Docker to start and walk through or skip the tutoria: "Get Started With Docker" which will open automatically.

### Install VS Code

Open a Windows PowerShell as an Administrator and execute the following:

```powershell
choco install vscode -y
```

### Install PGAdmin

Open a Windows PowerShell as an Administrator and execute the following:

```powershell
choco install pgadmin4 -y
```

### Optional Software

* Microsoft Cascadia Code fonts
* Go and Powerline Go

#### MS Cascadia Code fonts

Open a Windows PowerShell as an Administrator and execute the following:

```powershell
choco install cascadiafonts -y
```

Configure VS Code to use the Cascadia Code font and ligatures if installed by:

* Going to: File/Preferences/Settings
* Clicking on: Text Editor/Font
* Edit Font Family and Prepend: 'Cascadia Code' to the existing list of fonts
* Enable Ligatures by editing settings.json and setting "editor.fontLagatures": true

You should have a settings.json similar to

```console
{
    "editor.fontFamily": "'Cascadia Code', Consolas, 'Courier New', monospace",
    "editor.fontLigatures": true
}
```

Save your settings.json file.

### Development Setup

On Windows, a developer has a choice between using native windows or the Windows Subsystem for Linux 2 (WSL2) which provides a full Linux environment.

#### Linux based

##### Configure WSL2

Open a Windows PowerShell as an Administrator and execute the following:

```powershell
choco install microsoft-windows-terminal -y
```

At time of writing, Ubuntu 20.04 is not available for chocolatey install and so you have to:

* Open the Microsoft Store and search for **Ubuntu 20.04** LTS
* Click Get  
* Close the Microsoft Store.  

##### Windows Terminal Cascadia Font configuration

If the optional MS Cascadia fonts were installed, you can configure Windows Terminal to use them by opening a Windows Terminal window and

* Click on the drop down next to the + in the title/tab bar
* Click on Settings
* Find the Ubuntu 20.04 reference and add the following:

```console
    "fontFace": "Cascadia Code PL",
    "fontSize": 11,
    "closeOnExit": true,
    "cursorShape": "vintage",
    "cursorHeight": 25
```

Save the file (settings.json).

After opening an Ubuntu 20.04 window, please follow the instructions on the [Linux configuration page](./Linux.md).  

#### Native Windows

##### Git

Open a Windows PowerShell as an Administrator and execute:

````powershell
choco install git -y
````

##### .Net Core

Open a Windows PowerShell as an Administrator and execute:

````powershell
choco install dotnetcore-sdk -y
dotnet tool install --global dotnet-ef
````

##### NodeJS

Open a Windows PowerShell as an Administrator and execute:

````powershell
choco install nodejs-lts -y
````

##### Visual Studio 2019 (optional)

Open a Windows PowerShell as an Administrator and execute the installer for VS 2019 Community or Enterprise (if you're licensed):

VS 2019 Community

````powershell
choco install visualstudio2019community --package-parameters "--add Microsoft.VisualStudio.Workload.NetWeb --add Microsoft.VisualStudio.Workload.ManagedDesktop --add Component.GitHub.VisualStudio --includeRecommended" -y
````

or

VS 2019 Enterprise

````powershell
choco install visualstudio2019enterprise --package-parameters "--add Microsoft.VisualStudio.Workload.NetWeb --add Microsoft.VisualStudio.Workload.ManagedDesktop --add Component.GitHub.VisualStudio --includeRecommended" -y
````

### Next Steps

Please proceed to the [General Setup](./Configuration.md) section to complete your Health Gateway development environment.
