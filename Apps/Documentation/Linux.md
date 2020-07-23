# Linux Workstation Setup

The Linux software setup is also performed on Windows to provide a near identical common development environment across the two platforms.  On Windows, we will use the Windows System for Linux 2 (WSL2) environment.

## Requirements

* Ubuntu 20.04
* i7 or higher processor and 16GB of RAM.

## Software Install

The following software will be installed:

* .Net Core 3.1 and EF tooling
* Node Version Manager
* Docker
* VS Code
* PG Admin
* Optional software

### .Net Core

Please open a bash shell and perform the following:

```bash
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb;rm packages-microsoft-prod.deb
sudo apt-get update;sudo apt-get install -y apt-transport-https && sudo apt-get update && sudo apt-get install -y dotnet-sdk-3.1
dotnet tool install --global dotnet-ef
```

### Node

Using the existing bash shell:

```bash
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.35.3/install.sh | bash
nvm install --lts
sudo apt update
sudo apt upgrade
```

### Docker (skip if on Windows)

```bash
sudo apt-get update
sudo apt-get install apt-transport-https ca-certificates curl gnupg-agent software-properties-common -y
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -
sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io -y
sudo curl -L "https://github.com/docker/compose/releases/download/1.26.2/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

### Install VS Code (skip if on Windows)

```bash
sudo snap install --classic code
```

### Install PGAdmin (skip if on Windows)

```bash
curl https://www.pgadmin.org/static/packages_pgadmin_org.pub | sudo apt-key add
sudo sh -c 'echo "deb https://ftp.postgresql.org/pub/pgadmin/pgadmin4/apt/$(lsb_release -cs) pgadmin4 main" > /etc/apt/sources.list.d/pgadmin4.list && apt update'
sudo apt install pgadmin4-desktop
```

### Optional Software (skip if on Windows)

* Microsoft Cascadia Code fonts
* Go and Powerline Go

#### MS Cascadia Code fonts

```bash
wget https://github.com/microsoft/cascadia-code/releases/download/v2007.01/CascadiaCode-2007.01.zip
sudo unzip -j -d /usr/share/fonts/truetype/CascadiaCode CascadiaCode-2007.01.zip ttf/*
sudo fc-cache -f -v
rm CascadiaCode-2007.01.zip
```

Configure VS Code to use the Cascadia Code font and ligatures by

* Going to: File/Preferences/Settings
* Clicking on: Text Editor/Font
* Edit Font Family and Prepend: 'Cascadia Code' to the existing list of fonts
* Enable Ligatures by editing settings.json and setting "editor.fontLagatures": true

You should have a settings.json similar to

```console
{
    "editor.fontFamily": "'Cascadia Code', 'Droid Sans Mono', 'monospace', monospace, 'Droid Sans Fallback'",
    "editor.fontLigatures": true
}
```

Save your settings.json file.

#### Go and Powerline Go

```bash
sudo apt install golang-go -y
go get -u github.com/justjanne/powerline-go
```

Add the following to your .bashrc

```bash
GOPATH=$HOME/go
function _update_ps1() {
    PS1="$($GOPATH/bin/powerline-go -error $?)"
}
if [ "$TERM" != "linux" ] && [ -f "$GOPATH/bin/powerline-go" ]; then
    PROMPT_COMMAND="_update_ps1; $PROMPT_COMMAND"
fi
```

Please proceed to the [General Setup](./Configuration.md) section to complete your Health Gateway development environment.
