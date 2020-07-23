# Immunization

The Immunization project provides Immunization data in the form of a Web API with OpenAPI documentation.

## Immunization Service Configuration

```bash
cd $GATEWAYHOME/Apps/Immunization/src/
code appsetttings.local.json
```

Add the following text:

```bash
{
  "Panorama": {
    "ApiKey": "[Get key from team]"
  }
}
```

## Run Immunization Service

### Command line

```bash
cd $GATEWAYHOME/Apps/Immunization
dotnet run
```

### VS Code

* Select the Immunization project
* Say Yes or Accept any popups regarding adding assets for debugging
  * Select the Immunization Solution and click Enter which should create a .vscode/launch.json
* Click F5 or Run/Start Debugging

### Visual Studio

* Right Click on the solution and select Properties
* Select Multiple startup projects
* Set Immunization action to **Start**
* Click Ok
* Click F5 or Debug/Start Debugging

## Immunization Service Verification

* Ensure WebClient is running
* Open a Chrome session to [http://localhost:5000](ttp://localhost:5000)
* Open Developer tools.
* Logon to WebClient
* In the console make note of the HDID or you can likely use: P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A  
* Go to the network tab and refresh the page.  
* You will want to look for a failed call to Medication or Immunization go to the Headers tab and copy the text after Authorization: Bearer
* Open Chrome session to [http://localhost:3001/swagger](http://localhost:3001/swagger)
* Click on the Authorize button and paste in the bearer token, then click Authorize and Close.
* Click on the Immunization Get API to expand it.  
* Click on Try it out button.  
* Paste the HDID into the text input.  
* Click on Execute

You should see a response of 200 and a body similar to:

```console
{
}
```

TODO: Complete this section when Immunization is udpated.