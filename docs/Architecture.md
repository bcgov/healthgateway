# High-Level Health Gateway Architecture

## OAuth2 OpenID Connect (OIDC) Authentication Flow

The Health Gateway is designed to use external identity providers (IdPs) integrated through KeyCloak (RedHat SSO). The principle and initial IdP for authenticating citizens is IAS using the BC Services Card. IAS is the Identity Assurance Service offered by the BC Ministry of Citizens' Services.

<img src="diagrams/out/BCSC_OIDC_Flow.png"
     alt="OIDC Flow"
     style="float: left; margin-right: 10px;" />

In Step 1, the user navigates to the Health Gateway on their browser.  The browser loads the Health Gateway application in Step 2. In Step 3, the user selects to login using their BC Services Card. Step 4 submits an authentication request to KeyCloak (RedHat SSO) passing the selected identity provider choice, in this case 'bcsc'.  KeyCloak connects to the IAS, a provisioned Identity Provider (IdP), and in Step 5. KeyCloak submits an OAuth2 OIDC authentication request.

