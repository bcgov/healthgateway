# Medication Service

The Medication project provides web APIs for:

* Unauthenticated access to Federal and Provincial drug data
* Filled Prescription information
* OpenAPI documentation for implemented APIs

## Medication Service Configuration

Medication needs no configuration for Development and works out of the box.  
Open a new terminal window

## Run Medication Service

### Command line

````bash
cd $GATEWAYHOME/Apps/Medication/src/
dotnet run
````

### VS Code

* Select the Medication project
* Say Yes or Accept any popups regarding adding assets for debugging
  * Select the Medication Solution and click Enter which should create a .vscode/launch.json
* Click F5 or Run/Start Debugging

### Visual Studio

* Right Click on the solution and select Properties
* Select Multiple startup projects
* Set Medication action to **Start**
* Click Ok
* Click F5 or Debug/Start Debugging

## Medication Service Verification

* Ensure WebClient is running
* Open a Chrome session to [http://localhost:5000](ttp://localhost:5000)
* Open Developer tools.
* Logon to WebClient
* In the console make note of the HDID or you can likely use: P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A  
* Go to the network tab and refresh the page.  
* You will want to look for a failed call to Medication or Immunization go to the Headers tab and copy the text after Authorization: Bearer

You should be able to open a new Browser window and connect to [http://localhost:3003/swagger](http://localhost:3003/swagger)

* Click on the Authorize button and paste in the bearer token, then click Authorize and Close.
* Click on the MedicationStatement Get API to expand it.  
* Click on Try it out button.  
* Paste the HDID into the text input.  
* Click on Execute

You should see a response of 200 and a body similar to:

````console
{
  "resourcePayload": [
    {
      "prescriptionIdentifier": "9529523",
      "prescriptionStatus": "\u0000",
      "dispensedDate": "2019-12-30T00:00:00",
      "practitionerSurname": "PZVVPS",
      "directions": ".. SHAKE WELL .. DRINK 50MLS CARRY 80MLS DAILY DAILY  MAY BE POISONOUS TO OTHERS",
      "dateEntered": null,
      "pharmacyId": "",
      "medicationSumary": {
        "din": "66999990",
        "brandName": "Unknown brand name",
        "genericName": "",
        "quantity": 390,
        "maxDailyDosage": 0,
        "drugDiscontinuedDate": null
      },
      ...
    }]
}
````