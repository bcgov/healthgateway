# Dolphin - CLI for SonarQube

Dolphin provides a simple Command Line Interface (CLI) for interacting with a SonarQube instance.

## Installation
If python is not installed it can be downloaded at [Python Downloads](https://www.python.org/downloads/).

### Dependencies
#### Python dependencies installation:

`$ pip install [dependency]`

or
~~~
$ pip install keyring
$ pip install click
$ pip install requests
~~~

#### About the dependencies:
- [keyring](https://pypi.org/project/keyring/)
- [click](https://pypi.org/project/click/)
- [requests](https://pypi.org/project/requests/)

## Usage
##### Initialize settings:
Stores the credentials and sonarqube URL using the OS keyring (only needs to be done at the begging or after using clear).

`$ python [installation_path]/dolphin.py prop -l [user_login] -t [user_token] -u [sonar_url] {-s}`

##### Create a project
Create a new project with the given name and key. The user is assigned admin privileges over the project

`$ python [installation_path]/dolphin.py new -n [project_name] -k [project_key]`

##### Execute scan
Runs tests, initializes SonarQube, builds and syncs the report with the SonarQube instance

`$ python [installation_path]/dolphin.py run [project_key]`

##### Delete project
Deletes the SonarQube project that matches the [project_key]

`$ python [installation_path]/dolphin.py delete [project_key]`

##### Clear settings
Removes the information stored during setting from the local machine.

`$ python [installation_path]/dolphin.py clear {--login} {--url} {--all}`

## Environment Settings

### Windows
Dolphin can be executed directly by using the bat file provided removing the need to call the interpreter. Once the installation path is added to the PATH environment variable it can be called like:

`$ dolphin.bat run my_key`

### Linux
Depending on the shell the script can be directly executed without having to call the interpreter.