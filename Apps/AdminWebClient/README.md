# Admin WebClient

The Admin WebClient provides Administrators of Health Gateway the ability to manaage various tasks.

## Admin Web Client Configuration

```bash
cd $GATEWAYHOME/Apps/AdminWebClient/src/
code appsetttings.local.json
```

Add the following text:

```bash
{
  "OpenIdConnect": {
    "ClientSecret": "[Ask team for Client Secret]"
  }
}
```

save the file.

## Run Admin WebClient

### Command line

Open a new terminal Window to run the .Net application

```bash
cd $GATEWAYHOME/Apps/AdminWebClient/src/
dotnet run
```

Open another terminal Window to run the VUE Cli

```bash
cd $GATEWAYHOME/Apps/AdminWebClient/src/
npm run serve
```

### VS Code

* Select the AdminWebClient project
* Say Yes or Accept any popups regarding adding assets for debugging
  * Select the AdminWebClient Solution and click Enter which should create a .vscode/launch.json
* Click F5 or Run/Start Debugging

### Visual Studio

* Right Click on the solution and select Properties
* Select Multiple startup projects
* Set AdminWebClient action to **Start**
* Click Ok
* Click F5 or Debug/Start Debugging

## Admin WebClient Verification

* Open a Chrome session to [http://localhost:5010/admin](http://localhost:5010/admin)
* Authenticate with your IDIR
* All functionality should work