# Functional Tests

## Setup

```console
npm install
```

## Running

### Selenium IDE tests

Runs tests in localhost:

Runs tests in dev:

```console	
npm test -- --server "http://selenium-hub-gateway.pathfinder.gov.bc.ca/wd/hub" --base-url "https://dev-gateway.pathfinder.gov.bc.ca/"
```

## Tools

### Selenium IDE
Chrome extension that records and generate functional tests  
[Docs](https://docs.seleniumhq.org/selenium-ide/docs/en/introduction/getting-started/)

#### Setup
[Selenium IDE](https://docs.seleniumhq.org/selenium-ide/docs/en/introduction/getting-started/)  
or  
Search and install "Selenium IDE" from chrome://extensions

#### Running
```console
selenium-side-runner <FILE_PATH>.side --base-url <WEBSITE_URL>
```
