import keyring
import click
import requests
from requests.auth import HTTPBasicAuth
import subprocess
from subprocess import PIPE
import shlex

URL_KEY = 'url_key'
TOKEN_KEY = 'token_key'
SERVICE = 'dolphin'
PROJECTS_PATH = '/api/projects/'


@click.group()
def main():
    """Simple CLI for interacting with SonarQube"""
    pass


@main.command()
@click.option('--name', '-n', required=True)
@click.option('--key', '-k', required=True)
def new(name, key):
    """Creates a new SonarQube project"""

    token, url = _retrieve_settings()

    post_params = {
        'name': name,
        'project': key,
    }

    url = url + PROJECTS_PATH + "create"

    response = requests.post(url=url, data=post_params)

    click.echo('Status: ' + str(response.status_code))
    click.echo(response.text)


@main.command()
@click.argument('key')
def delete(key):
    """Deletes the SonarQube project"""

    token, url = _retrieve_settings()

    post_params = {'project': key}

    url = url + PROJECTS_PATH + "delete"

    response = requests.post(url=url, data=post_params, auth=HTTPBasicAuth(token, ''))
    click.echo('Status: ' + str(response.status_code))
    click.echo(response.text)


@main.command()
@click.argument('key')
def run(key):
    """Executes SonarQube analysis"""

    token, url = _retrieve_settings()

    sonarparams = '/k:"{0}" ' \
                  '/d:sonar.host.url="{1}" ' \
                  '/d:sonar.cs.opencover.reportsPaths="test/coverage.opencover.xml" ' \
                  '/d:sonar.coverage.exclusions="**Tests*.cs" ' \
                  '/d:sonar.login="{2}"'

    click.echo()
    click.echo('Running tests...')
    output = _run_command('dotnet test test/test.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover')
    if output != 0:
        click.secho('Error, please check logs', fg='red')
        exit()

    click.echo()
    click.echo('Shutting down build-server...')
    output = _run_command('dotnet build-server shutdown')
    if output != 0:
        click.secho('Error, please check logs', fg='red')
        exit()

    click.echo()
    click.echo('SonarScanner begin...')
    output = _run_command('dotnet sonarscanner begin ' + sonarparams.format(key, url, token))
    if output != 0:
        click.secho('Error, please check logs', fg='red')
        exit()

    click.echo()
    click.echo('Building solution...')
    output = _run_command('dotnet build')
    if output != 0:
        click.secho('Error, please check logs', fg='red')
        exit()

    click.echo()
    click.echo('SonarScanner end...')
    output = _run_command('dotnet sonarscanner end /d:sonar.login="{0}"'.format(token))
    if output != 0:
        click.secho('Error, please check logs', fg='red')
        exit()

    project_url = url + '/dashboard?id=' + key

    click.echo('Execution complete. Results at ' + project_url)


@main.command()
@click.option('--token', '-t', required=True)
@click.option('--url', '-u', required=True, help='url of the SonarQube instance')
def init(token, url):
    """Initializes SonarQube settings and credentials"""

    keyring.set_password(SERVICE, TOKEN_KEY, token)
    keyring.set_password(SERVICE, URL_KEY, url)
    click.echo('Settings saved')


@main.command()
def clear():
    """Clears SonarQube credentials and data from the local machine"""

    token = keyring.get_password(SERVICE, TOKEN_KEY)

    if token:
        keyring.delete_password(SERVICE, TOKEN_KEY)
        click.echo('Token deleted')
    else:
        click.echo('Token not found, its already clean')

    url = keyring.get_password(SERVICE, URL_KEY)

    if url:
        keyring.delete_password(SERVICE, URL_KEY)
        click.echo('URL deleted')
    else:
        click.echo('Url not found, its already clean')


def _retrieve_settings():
    token = keyring.get_password(SERVICE, TOKEN_KEY)
    if token is None:
        click.echo('ERROR: No user token provided')

    url = keyring.get_password(SERVICE, URL_KEY)
    if url is None:
        click.echo('ERROR: No url provided')

    if not url and not token:
        click.echo('Use "init --token [token] -url [url]" to set the SonarQube token and url')
        exit()

    return token, url


def _run_command(command):
    process = subprocess.Popen(shlex.split(command), stdout=subprocess.PIPE)
    while True:
        output = process.stdout.readline()
        if output.decode() == '' and process.poll() is not None:
            break
        if process.poll() is None or process.poll() == 0:
            click.secho('|   ' + output.strip().decode(), fg='green')
        else:
            click.secho('|   ' + output.strip().decode(), fg='red')

    rc = process.poll()
    return rc


if __name__ == '__main__':
    main()
