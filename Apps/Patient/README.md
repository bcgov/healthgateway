# Patient

The Patient project provides Web APIs specific to the patient.  Currently only HDID to PHN translation has been written and is documented via OpenAPI.

## Patient Service Configuration

You will need your IDIR to download the Client Registries Auth [zip package](https://hlth.sp.gov.bc.ca/sites/EHLTH/ihit/HSIMIT/Gateway%20Shared%20Folder/00.%20Health%20Gateway%20Agile/zz.%20Protected/HGWAY_HI1.zip).  

Unzip to a secure location on your local machine.

Create a local appsettings file:

```bash
cd $GATEWAYHOME/Apps/Patient/src/
code appsettings.local.json
```

Add the following text:

```bash
{
    "ClientRegistries": {
      "ClientCertificate": {
        "Path": "[YOUR PATH]/HGWAY_HI1.pfx",
        "Password": "[Ask team for password]"
      }
    }
}
```

## Run Patient Service

### Command line

````bash
cd $GATEWAYHOME/Apps/Patient/src/
dotnet run
````

### VS Code

* Select the Patient project
* Say Yes or Accept any popups regarding adding assets for debugging
  * Select the Patient Solution and click Enter which should create a .vscode/launch.json
* Click F5 or Run/Start Debugging

### Visual Studio

* Right Click on the solution and select Properties
* Select Multiple startup projects
* Set Patient action to **Start**
* Click Ok
* Click F5 or Debug/Start Debugging

### Visual Studio for Mac

* Right Click on the solution and select set startup project
* Ensure that the Patient solution item is checked
* Click Ok
* Select Run Menu then Start Debugging

## Patient Service Verification

* Ensure WebClient is running
* Open a Chrome session to [http://localhost:5000](http://localhost:5000)
* Open Developer tools.
* Logon to WebClient
* In the console make note of the HDID or you can likely use: P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A  
* Go to the network tab and refresh the page.  
* You will want to look for a failed call to Medication or Immunization go to the Headers tab and copy the text after Authorization: Bearer

You should be able to open a new Browser window and connect to [http://localhost:3002/swagger](http://localhost:3002/swagger)

* Click on the Authorize button and paste in the bearer token, then click Authorize and Close.
* Click on the Patient Get API to expand it.  
* Click on Try it out button.  
* Paste the HDID into the text input.  
* Click on Execute

You should see a response of 200 and a body similar to:

````console
{
  "hdid": "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A",
  "personalhealthnumber": "9735353315",
  "firstname": "BONNET",
  "lastname": "PROTERVITY",
  "birthdate": "1967-06-02T00:00:00",
  "email": ""
}
````

If you encounter an error similar to socket reset, then you need to contact the PO to have CGI add your IP to the authorized whitelist.