# Functional Tests

## Tools
	- Selenium IDE: Chrome extension that records commands and generate functional tests:
		Search and install "Selenium IDE" from chrome://extensions
		How to: https://docs.seleniumhq.org/selenium-ide/docs/en/introduction/getting-started/

	- Running .side tests:
		Install selenium-side-runner and chromedriver
		```console
		npm i -g chromedriver
		npm i -g selenium-side-runner		
		```
		
		Running tests
		```console
			selenium-side-runner <PATH>.side --base-url <WEBSITE_URL>
		```
