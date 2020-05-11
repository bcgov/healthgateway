# PHSA CDC Laboratory Integration
8 May 2020.
The BC Center for Disease Control is performing COVID-19 lab tests. The results of which will be available to citizens of BC through the HealthGateway.

To achieve this, PHSA has stood up a set of RESTful services protected by the citizen credentials obtained through authentication authorization on the HealthGateway.

Like any protected resource server, the logial resource owner is the citizen. The endpoint will only provide the lab test results when the following OAuth2 assertions are true:

1. The JWT passed to the service is valid by verifying the digital signature by way of the HealthGatway's Keycloak public key.
2. That the JWT contains a valid subject identifier that correlates to the personal health number found in the lab results. This is determined the service integrating to the BC HCIM services.
3. That the JWT contains the correct audience and scope requests for a lab observation query.

Only the resource owner, at this point, can query for their lab results through this API. The current solution does not support UMA 2.0 and delegation.

## Typical flow


<img src="diagrams/out/PHSA_CDC_Laboratory_Flow.png"
     alt="PHSA CDC Lab Results Flow"
     style="float: left; margin-right: 5000px; border: 1px solid #ccc; padding: 10px; border-radius: 5px;" />
  

In **Step 1**, the user navigates to the Health Gateway on their browser.  The browser loads the Health Gateway application in **Step 2**. In **Step 3**, the user selects to login using their BC Services Card. **Step 4** submits an authentication request to KeyCloak (RedHat SSO) passing the selected identity provider choice, in this case 'bcsc'.  KeyCloak connects to the IAS, a provisioned Identity Provider (IdP), and in **Step 5**. KeyCloak submits an OAuth2 OIDC authentication request.
