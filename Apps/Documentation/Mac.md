# Mac Workstation Setup

## Requirements

* MacOS Catalina
* i7 or higher processor and 16GB of RAM.

## Software Install

* Homebrew and Bash
* Chrome
* .Net Core 3.1 and EF tooling
* Node
* Docker
* VS Code
* PGAdmin
* Optional software

During the installation process you will be asked for your password multiple times.

### Homebrew and Bash

Open your default terminal window and perform the following:

```console
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install.sh)"
```

You will have to accept the install of XCode to continue - this will take sometime to complete.

Once complete, execute the following commands in the same terminal window:

```console
brew install bash
echo '/usr/local/bin/bash' | sudo tee -a /etc/shells;
chsh -s /usr/local/bin/bash
```

### Chrome

```bash
brew cask install google-chrome
```

### .Net Core

```bash
brew cask install dotnet-sdk
dotnet tool install --global dotnet-ef
```

### Node

```bash
brew install node
```

### Docker

```bash
brew cask install docker
```

Start Docker from the launchpad which should immediately prompt you with

* Docker Desktop needs privileged access

Click ok and enter your password and then click "Install Helper" to complete the installation.  

A docker tutorial "Get Started With Docker" will open automatically and you can skip or or not.

### VS Code

```bash
brew cask install visual-studio-code
```

### PGAdmin

```bash
brew cask install pgadmin4
```

### Optional Software

#### MS Cascadia Fonts (optional)

```bash
curl  -LOJ  https://github.com/microsoft/cascadia-code/releases/download/v2007.01/CascadiaCode-2007.01.zip
sudo unzip -j -d /Library/Fonts CascadiaCode-2007.01.zip ttf/*
rm CascadiaCode-2007.01.zip
```

Configure VS Code to use the Cascadia Code font and ligatures by

* Launching code
* Going to: Code/Preferences/Settings
* Clicking on: Text Editor/Font
* Edit Font Family and Prepend: 'Cascadia Code' to the existing list of fonts
* Enable Ligatures by editing settings.json and setting "editor.fontLagatures": true

You should have a settings.json similar to

```console
{
    "editor.fontFamily": "'Cascadia Code', Menlo, Monaco, 'Courier New', monospace",
    "editor.fontLigatures": true
}
```

Save your settings.json file.

#### iTerm2

```bash
brew cask install iterm2
```

Configure iTerm to use Cascadia Code font and ligatures by

* Launching iTerm
* Going to Profiles/Open Profiles...
* Click Edit Profiles
* Select Default (left pane)
* Select Text Tab (right pane)
* Change the font to Cascadia Code PL
* Check Use Ligatures
* Close settings and restart iTerm

#### Visual Studio (optional)

```bash
brew cask install visual-studio
```

Perform the following to configure Visual Studio to use the correct NodeJS:

* From a bash shell execute

```bash
which nvm
```

* Copy the path that was reported from above and launch Visual Studio
* Select menu Visual Studio/Preferences...
* Select External Tools
* Click Add

#### Go and Powerline Go (optional)

```bash
brew install go
go get -u github.com/justjanne/powerline-go
```

Add the following to your .bash_profile

```bash
GOPATH=$HOME/go
function _update_ps1() {
    PS1="$($GOPATH/bin/powerline-go -error $?)"
}
if [ "$TERM" != "linux" ] && [ -f "$GOPATH/bin/powerline-go" ]; then
    PROMPT_COMMAND="_update_ps1; $PROMPT_COMMAND"
fi
```

Close your iTerm window and restart.

Ensure that you run the application in Chrome as Safari had a known authentication error.  

Please proceed to the [General Setup](./Configuration.md) section to complete your Health Gateway development environment.
