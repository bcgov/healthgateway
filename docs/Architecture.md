# High-Level HealthGateway Architecture

## Citizen Authentication using OAuth2 OpenID Connect (OIDC) Flow

The Health Gateway is designed to use external identity providers (IdPs) integrated through KeyCloak (RedHat SSO). The principle and initial IdP for authenticating citizens is IAS using the BC Services Card. IAS is the Identity Assurance Service offered by the BC Ministry of Citizens' Services.

<img src="diagrams/out/BCSC_OIDC_Flow.png"
     alt="OIDC Flow"
     style="float: left; margin-right: 10px;" />

In **Step 1**, the user navigates to the Health Gateway on their browser.  The browser loads the Health Gateway application in **Step 2**. In **Step 3**, the user selects to login using their BC Services Card. **Step 4** submits an authentication request to KeyCloak (RedHat SSO) passing the selected identity provider choice, in this case 'bcsc'.  KeyCloak connects to the IAS, a provisioned Identity Provider (IdP), and in **Step 5**. KeyCloak submits an OAuth2 OIDC authentication request.

In **Step 6**, the BC Services Card authentication flow takes over. The user follows a standard interaction for authenticating with their BC Services Card, most commonly executed using the BC Services Card mobile app. The user experience and flow is the same as experienced connecting to https://id.gov.bc.ca/account.

In **Step 7**, upon a successful login by the Citizen with their BCSC, IAS returns the id_token, a JSON Web Token for the authenticated user. In **Step 8**, KeyCloak returns an authorization code back to the client application (HealthGateway running in the Citizen's browser).  Followin normal Oauth code flow, the client app exchanges the authorization code for the bearer token of the authenticated user in **Step 9**.  

Optionally, in **Step 10**, the client app retrieves userInfo by making an explicit call to the KeyCloak server. This retrieves any additional user detail collected as part of the registration flow but not provided from the basic auth flow to BCSC.  

The last step, **Step 11** the user's JWT or bearer token is stored in session in the Browser to avoid repeatedly asking for the user to authenticate themselves. This is deleted upon logout.

## Example Access control of Medications API

The Health Gateway is composed of publicly accessible but protected service APIs that fetch Medications data and other health records. This example flow illustrates the protections of those APIs. An HTTP Not Authorized '401' Error is returned whenever sufficient access is not met. 

<img src="diagrams/out/Protected_API_Call.png"
     alt="OIDC Flow"
     style="float: left; margin-right: 10px;" />

 **Step 1** Begins the flow by loading the HealthGateway app into the Citizen's browser.   **Step 2** we repeat the login flow as described above.  The citizen then selects to view their Medications in **Step 3**.  The first thing we need is a subject identiifer as known to our provincial health records, namely the PHN.  A protected API call to GetPHN() is called in **Step 4**.  In **Step 5** the PatientAPI checks that the Bearer token supplied is valid before proceeding to exchange the HDID (UserInfo.sub) for a PHN. Obtaining the PHN is done via a SOAP call to HCIM using the HL7v3 HCIM_IN_GetDemographics query in **Step 6**.  The PHN is returned to the HealthGateway Single Page App in the Browser in **Step 7**. 

 The application now has the Citizen's PHN patient identifier to be used to get Medications, and in **Step 8** Medications are requested.  In **Step 8** the MedicationsAPI service first checks Bearer token and then connects to PharmaNet over HNI to fetch medications history using an HL7v2 message structure (**Step 10**).   The Medications are returned and displayed on the browser in **Step 11**.

 In **Step 12** the Citizen logs out of the HealthGateway.  The HealthGateway relays that logout to KeyCloak to shut down authentication context for the user (**Step 13**), and the HealthGateway session token is deleted from the Browser (**Step 14**).

 In **Step 15** if the user then attempts a call to the MedicationsAPI, passing the old token or no token, the API will reject the request after checking the token (**Step 16**) and returns an HTTP Error: '401 Not Authorised' in **Step 17**.

## Citizen as Patient

By default, citizens will have access to their own health data and other Health Gateway user related information. The authorization is managed by user identity attribute from the BC Services Card to resolve to the user's Personal Health Number, or PHN. The PHN is not persisted anywhere within the Health Gateway application and that is by design.

## User Managed Access  (Using UMA 2.0)

Future editions of the HealthGateway will introduce the use of OAuth User Managed Access 2.0 for managing access to protected resources, with the Citizen as resource owner.
