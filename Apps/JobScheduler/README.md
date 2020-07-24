# Job Scheduler

## Job Scheduler Configuration

```bash
cd $GATEWAYHOME/Apps/JobScheduler/src/
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

## Run Job Scheduler

### Command line

```bash
cd $GATEWAYHOME/Apps/JobScheduler
dotnet run
```

### VS Code

* Select the JobScheduler project
* Say Yes or Accept any popups regarding adding assets for debugging
  * Select the JobScheduler Solution and click Enter which should create a .vscode/launch.json
* Click F5 or Run/Start Debugging

### Visual Studio

* Right Click on the solution and select Properties
* Select Multiple startup projects
* Set JobScheduler action to **Start**
* Click Ok
* Click F5 or Debug/Start Debugging

### Visual Studio for Mac

* Right Click on the solution and select set startup project
* Ensure that the JobScheduler solution item is checked
* Click Ok
* Select Run Menu then Start Debugging

## Job Scheduler Verification

* Open a Chrome session to [http://localhost:5005](ttp://localhost:5005)
* Authenticate with your IDIR
* Job Scheduler Dashboard displayed