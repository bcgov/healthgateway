import keyring
import click
import requests
from requests.auth import HTTPBasicAuth
import subprocess
import shlex

URL_KEY = 'url_key'
LOGIN_KEY = 'login_key'
TOKEN_KEY = 'token_key'
SERVICE = 'dolphin'
PROJECTS_PATH = '/api/projects/'
PERMISSIONS_PATH = '/api/permissions/'


@click.group()
def main():
    """Simple CLI for interacting with SonarQube"""
    pass


@main.command()
@click.option('--name', '-n', required=True)
@click.option('--key', '-k', required=True)
def new(name, key):
    """Creates a new SonarQube project"""

    login, token, base_url = _retrieve_settings()

    url = base_url + PROJECTS_PATH + "create"
    post_params = {
        'name': name,
        'project': key,
    }
    response = requests.post(url=url, data=post_params)
    click.echo('Status: ' + str(response.status_code))
    click.echo(response.text)

    click.echo('Adding permissions...')
    url = base_url + PERMISSIONS_PATH + "add_user"
    post_params = {
        'login': login,
        'permission': 'admin',
        'projectKey': key,
    }
    response = requests.post(url=url, data=post_params, auth=HTTPBasicAuth('admin', 'password321'))
    click.echo('Status: ' + str(response.status_code))
    click.echo(response.text)


@main.command()
@click.argument('key')
def delete(key):
    """Deletes the SonarQube project"""

    login, token, url = _retrieve_settings()

    post_params = {'project': key}

    url = url + PROJECTS_PATH + "delete"

    response = requests.post(url=url, data=post_params, auth=HTTPBasicAuth(token, ''))
    click.echo('Status: ' + str(response.status_code))
    click.echo(response.text)


@main.command()
@click.argument('key')
def run(key):
    """Executes SonarQube analysis"""

    login, token, url = _retrieve_settings()

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
@click.option('--login', '-l', required=True)
@click.option('--token', '-t', required=True)
@click.option('--url', '-u', help='url of the SonarQube instance')
def init(login, token, url):
    """Initializes SonarQube settings and credentials"""

    keyring.set_password(SERVICE, LOGIN_KEY, login)
    keyring.set_password(SERVICE, TOKEN_KEY, token)
    keyring.set_password(SERVICE, URL_KEY, url)
    click.echo('Settings saved')


@main.command()
@click.option('--login', '-l', 'login_flag', is_flag=True)
@click.option('--url', '-u', 'url_flag', is_flag=True)
@click.option('--all', '-a', 'all_flag', is_flag=True)
def clear(login_flag, url_flag, all_flag):
    """Clears SonarQube credentials and data from the local machine"""

    if not login_flag and not url_flag and not all_flag:
        ctx = click.get_current_context()
        click.echo(ctx.get_help())
        ctx.exit()

    if all_flag:
        login_flag = all_flag
        url_flag = all_flag

    if login_flag:
        login = keyring.get_password(SERVICE, LOGIN_KEY)
        if login:
            keyring.delete_password(SERVICE, LOGIN_KEY)
            click.echo('User login deleted')
        else:
            click.echo('User login not found, its already clean')

        token = keyring.get_password(SERVICE, TOKEN_KEY)
        if token:
            keyring.delete_password(SERVICE, TOKEN_KEY)
            click.echo('Token deleted')
        else:
            click.echo('Token not found, its already clean')

    if url_flag:
        url = keyring.get_password(SERVICE, URL_KEY)
        if url:
            keyring.delete_password(SERVICE, URL_KEY)
            click.echo('URL deleted')
        else:
            click.echo('Url not found, its already clean')


def _retrieve_settings():
    login = keyring.get_password(SERVICE, LOGIN_KEY)
    if login is None:
        click.echo('ERROR: No user login provided')

    token = keyring.get_password(SERVICE, TOKEN_KEY)
    if token is None:
        click.echo('ERROR: No user token provided')

    url = keyring.get_password(SERVICE, URL_KEY)
    if url is None:
        click.echo('ERROR: No url provided')

    if not url or not token or not login:
        click.echo('Use "init --login [login] --token [token] --url [url]" to set the SonarQube login, token and url')
        exit()

    return login, token, url


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
