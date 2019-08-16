#!/usr/bin/python

import keyring
import click
import requests
from requests.auth import HTTPBasicAuth
import subprocess
import shlex
import yaml
import os

URL_KEY = 'url_key'
LOGIN_KEY = 'login_key'
TOKEN_KEY = 'token_key'
SERVICE = 'dolphin'
PROJECTS_PATH = '/api/projects/'
PERMISSIONS_PATH = '/api/permissions/'

class SonarConf:
    prop = []
    test_runners = []
    paths = None
    def __init__(self, prop, test_runners, paths): 
        self.prop = prop
        self.test_runners = test_runners   
        self.paths = paths   


@click.group()
def main():
    """Simple CLI for interacting with SonarQube"""
    pass


@main.command()
@click.option('--name', '-n', required=True, help='project name to delete')
@click.option('--key', '-k', required=True, help='project key to delete')
def new(name, key):
    """Creates a new SonarQube project"""

    login, token, base_url = _retrieve_credentials()

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

    login, token, url = _retrieve_credentials()

    post_params = {'project': key}

    url = url + PROJECTS_PATH + "delete"

    response = requests.post(url=url, data=post_params, auth=HTTPBasicAuth(token, ''))
    click.echo('Status: ' + str(response.status_code))
    click.echo(response.text)


@main.command()
@click.option('--verbose', '-v', help='runs the scanner on verbose mode', is_flag=True)
@click.argument('key')
def run(key, verbose):
    """Executes SonarQube analysis'"""

    is_verbose = 'false'
    if verbose:
        is_verbose = 'true'

    login, token, url = _retrieve_credentials()
    sonar_conf = _load_scanner_config()

    sonarparams = '/d:sonar.verbose=' + is_verbose + ' /k:"{0}" ' \
                  '/d:sonar.host.url="{1}" ' \
                  '/d:sonar.login="{2}" ' \
                  + sonar_conf.prop

    params_string = sonarparams.format(key, url, token)

    click.echo()
    click.echo('Shutting down build-server...')
    output = _run_command('dotnet build-server shutdown')
    if output != 0:
        click.secho('Error, please check logs', fg='red')
        exit()

    click.echo()
    click.echo('SonarScanner begin...')
    output = _run_command('dotnet sonarscanner begin ' + params_string)
    if output != 0:
        click.secho('Error, please check logs', fg='red')
        exit()

    click.echo()
    click.echo('Building solution...')
    output = _run_command('dotnet build --no-incremental')
    if output != 0:
        click.secho('Error, please check logs', fg='red')
        exit()

    click.echo()
    click.echo('Running tests...')
    for runner_name in sonar_conf.test_runners:
        click.echo('Executing ' + runner_name + ' test')
        if runner_name == 'xunit':
            output = _run_command('dotnet test test/test.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --logger:"xunit;LogFileName=results.xml" -r ./xUnitResults ')
            if output != 0:
                click.secho('Error, please check logs', fg='red')
                exit()
        elif runner_name == 'jest':
            output = _run_command('npm --prefix ' + sonar_conf.paths['jest'] + ' test')
            if output != 0:
                click.secho('Error, please check logs', fg='red')
                exit()
        else:
            click.secho('Error, runner [' + runner_name + '] not configured', fg='red')
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
@click.option('--login', '-l', help='the SonarQube user login name')
@click.option('--token', '-t', help='user generated token on SonarQube')
@click.option('--url', '-u', help='url of the SonarQube instance')
@click.option('--show', '-s', help='show current settings', is_flag=True)
def prop(login, token, url, show):
    """SonarQube settings and credentials"""

    if login:
        keyring.set_password(SERVICE, LOGIN_KEY, login)
    if token:
        keyring.set_password(SERVICE, TOKEN_KEY, token)
    if url:
        keyring.set_password(SERVICE, URL_KEY, url)

    if login or token or url:
        click.echo('Settings saved')

    if show:
        login, token, url = _retrieve_credentials()
        click.echo('Url:\t' + url)
        click.echo('User:\t' + login)
        click.echo('Token:\t' + '*' * len(token))


@main.command()
@click.option('--login', '-l', 'login_flag', is_flag=True, help='clears the login username and token')
@click.option('--url', '-u', 'url_flag', is_flag=True, help='clears the url')
@click.option('--all', '-a', 'all_flag', is_flag=True, help='clears all data')
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


def _retrieve_credentials():
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
        click.echo('Use "prop --login [login] --token [token] --url [url]" to set the SonarQube login, token and url')
        exit()

    return login, token, url


def _run_command(command):
    print(command)
    # Windows cannot run npm if its not as a shell
    is_shell = False
    if os.name == 'nt':
        is_shell = True
    process = subprocess.Popen(shlex.split(command), stdout=subprocess.PIPE, shell=is_shell)
    while True:
        output = process.stdout.readline()
        decoded_out = output.decode("utf-8", "strict")  
        if decoded_out == '' and process.poll() is not None:
            break
        if process.poll() is None or process.poll() == 0:
            click.secho('|   ' + decoded_out.strip(), fg='green')
        else:
            click.secho('|   ' + decoded_out.strip(), fg='red')

    rc = process.poll()
    return rc


def _load_scanner_config():
    file_name = 'sonar-config.yml'
    if not os.path.isfile(file_name):
        click.echo('ERROR: SonarQube [' + file_name + ']configuration file not found.')
        exit()

    click.echo('Loading local sonarqube configuration...')
    parsed_properties = []
    test_runners = []
    paths = {}
    with open(file_name, 'r') as config:
        try:
            data = yaml.load(config, Loader=yaml.Loader)
            # load the properties
            for prop_name, value in data['properties'].items():
                parsed_properties.append('/d:' + prop_name + '="' + value+'"')

            # load the runners
            for runner_name in data['test-runners']:
                test_runners.append(runner_name)
            for path_name, value in data['paths'].items():
                paths[path_name] = value

        except yaml.YAMLError as exc:
            print(exc)

    return SonarConf(' '.join(parsed_properties), test_runners, paths)


if __name__ == '__main__':
    main()
