# WebClient

## WebClient Configuration

Create a new file: 

```bash
$GATEWAYHOME/Apps/WebClient/src/appsetttings.local.json
```

Add the following text, changing [YOUR IP] to your actual IP.

```bash
{
  "ContentSecurityPolicy": {
    "connect-src": "https://spt.apps.gov.bc.ca/com.snowplowanalytics.snowplow/tp2 https://sso-dev.pathfinder.gov.bc.ca/ http://localhost:* ws://localhost:* http://[YOUR IP]:* ws://[YOUR IP]:*",
    "frame-src": "https://sso-dev.pathfinder.gov.bc.ca/",
    "script-src": "https://www2.gov.bc.ca/StaticWebResources/static/sp/sp-2-14-0.js 'sha256-q+lOQm0t+vqQq4IdjwI4OwRI9fKfomdxduL1IJYjkA4='"
  }
}
```

Save the file.

## Run WebClient

### Command line

Open a new terminal/command Window to run the .Net application

```bash
cd $GATEWAYHOME/Apps/WebClient/src/
dotnet run
```

Open another terminal Window to run the VUE Cli

```bash
cd $GATEWAYHOME/Apps/WebClient/src/
npm run serve
```

### VS Code

* Select the WebClient project
* Say Yes or Accept any popups regarding adding assets for debugging
  * Select the Solution and click Enter which should create a .vscode/launch.json
* Click F5 or Run/Start Debugging

### Visual Studio

* Right Click on the solution and select Properties
* Select Multiple startup projects
* Set WebClient action to **Start**
* Click Ok
* Click F5 or Debug/Start Debugging

### Visual Studio for Mac

* Right Click on the solution and select set startup project
* Click on Create Run Configuration
* Ensure that the WebClient solution item is checked
* Click Ok
* Select Run Menu then Start Debugging

## WebClient Verification

* Open a Chrome session to [http://localhost:5000](http://localhost:5000)
* Authenticate with Virtual BC Services Card
* You will see a banner Error.  
* You should be able to Add/Edit/Delete Notes.
