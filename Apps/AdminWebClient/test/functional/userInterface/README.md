# Functional Tests

## Tools

### Selenium IDE

Browser extension that records and generate tests in JSON format

[Docs](https://docs.seleniumhq.org/selenium-ide/docs/en/introduction/getting-started/)

#### Setup Tool

Chrome extension [Selenium IDE](https://chrome.google.com/webstore/detail/selenium-ide/mooikfkahbdckldjjndioackbalphokd?hl=en)  

Firefox extension [Selenium IDE](https://addons.mozilla.org/en-US/firefox/addon/selenium-ide/)



## Setup Test Execution

```console
npm install
```

#### ChromeDriver Setup in macOS:

```console
brew cask install chromedriver
```

#### ChromeDriver Setup in WIN:

Download [chromedriver](https://sites.google.com/a/chromium.org/chromedriver/downloads) matching the current version of chrome installed in your system.

Extract to a folder of your preference and update the PATH environment variable.

#### ChromeDriver Setup in Linux:

```console
sudo npm -g install chromedriver
ln -sf /usr/lib/node_modules/chromedriver/lib/chromedriver/chromedriver ~/bin/chromedriver
```

## Running

### Using chromedriver

LOCALHOST:

```console	
npm test
```

DEV:

```console	
npm test -- --base-url "https://dev.healthgateway.gov.bc.ca/"
```

### Using selenium-hub server (No chromedriver required)

DEV:

```console	
npm test -- --server "http://selenium-hub-gateway.pathfinder.gov.bc.ca/wd/hub" --base-url "https://dev.healthgateway.gov.bc.ca/"
```

LOCALHOST:

To use selenium-hub against localhost the website needs to be exposed to the internet,
it can be done by using a tool like [ngrok](https://ngrok.com/)

```console	
npm test -- --server "http://selenium-hub-gateway.pathfinder.gov.bc.ca/wd/hub" --base-url "https://<URL_ID>.ngrok.io/"
```

Selenium-hub also supports tests in other browsers like firefox, to run:

```console	
npm test -- --config-file ./firefox.side.yml --server "http://selenium-hub-gateway.pathfinder.gov.bc.ca/wd/hub" --base-url "https://dev.healthgateway.gov.bc.ca/"
```
