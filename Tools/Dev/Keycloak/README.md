# HealthGateway KeyCloak Configuration Guide

The HealthGateway uses Keycloak, an OAuth2 compliant authorization server to control access to its protected resources. It connects the user via OIDC to external Identity Providers, in particular the BC Services Card identity and authentication service.   For administrators Keycloak also uses government employee's Active Directory services. 

## Environments

The HealthGateway project has three realms assigned to it for Dev, Test and production. Each environment is configured separately:

### Development Keycloak

<https://sso-dev.pathfinder.gov.bc.ca/auth/realms/ff09qn3f>

### Test and Demo Keycloak

<https://sso-test.pathfinder.gov.bc.ca/auth/realms/ff09qn3f>

### Production Keycloak

<https://sso.pathfinder.gov.bc.ca/auth/realms/ff09qn3f>

## Realm Settings

Realm settings establish the basic realm ID, in our case a license-plate style realm id is assigned by the Keycloak administrators.  The fact that it is enabled, and that we will use User-Managed Access (UMA Compliant) for delegating access to resources whether person-to-person, or person-to-client app.

| Setting | Value |
| -------- | -------- |
| Name | ff0qn3f |
| Enabled | ON |
| User-Managed Access | ON |
| OpenID Endpoint Configuration | {base-keycloak-uri}/auth/realms/ff09qn3f/.well-known/openid-configuration |

OpenID endpoint discovery URL is an important end point for discovering the useful endpoints for client OAuth software, of particular utility.  For example, for Development environment the well known endpoints for openid are found at:

| Dev | URL |
| ------ | ------|
|OpenID Endpoint Configuration | <https://sso-dev.pathfinder.gov.bc.ca/auth/realms/ff09qn3f/.well-known/openid-configuration> |

## Clients

Aside from the Keycloak built-in clients, such as 'account', 'admin-cli', we add the following clients for HealthGateway:

| Client ID | Access Type | Consent Required | Direct Access Grant | Service Accounts | Description |
| ------- | ------ | ------ | ------ | ------ | ------ |
| healthgateway | public | OFF | OFF | OFF | This is the main web client application. It uses OIDC to authenticate the user. It is an SPA responsive design web application |
| healthgateway-admin | confidential | OFF | ON | ON |  This is the administrative web application used by staff. It requires an IDIR account to gain access. |
| phsa-cdc | confidential | OFF | ON | ON | This is a new client that represents the PHSA client application to access HealthGateway services, particularly our Patient information service. It was added to support COVID-19 lab results access. |

> We also add new clients for trusted third-party applications to be consented by the citizen to access the resources on their behalf. This is not yet supported, but is planned, further empowering citizens of BC to make use of additional health services by conveniently allowing applications to access their health records for them.

## Client Scopes

Client Scopes determine what resource owner claims can be asked for. For example, if a client is assigned the scope 'patient/Patient.read', and HL7 FHIR type scope, and 'Requires Consent' is turned off for that client, then that client can call resources that require the presence of that scope in the access_token.

When a scope is optional, it just means that the client must explicitely request it as part of the OAuth2 flow that gathers the access_token, i.e. code flow with OIDC, as an example.

Clients, such as those listed above, can only request or be assigned scopes that are defined in this section of the configuration.

The scopes below listed below are only those that are not OpenID Connect built-in, but have been added and configured.

| Scope Name | Description | Display on Consent Screen | Mapper |
| ------ | -------- | ---------- | ------ |
| audience | Resolves the audience of the client | OFF | audience-resolve |
| patient/Immunization.read | HL7 FHIR scope to read a specific patient's immunization records | ON | n/a |
| patient/Laboratory.read | HL7 FJIR-inspired scope to read a patient's Laboratory records | ON | n/a |
| patient/MedicationDispense.read | HL7 FHIR scope to read a patient's Medication History | ON | n/a |
| patient/Patient.read | HL7 FHIR scope to read patient demongraphics record | ON | n/a |
| system/Immunization.read | HL7 FHIR scope to read any patient's immunization records | OFF | n/a |

### Temporary Scopes

The following terse scopes were put in place for our integration with PHSA for COVID-19 Lab test results and for notification settings (SMS, Email). These were to workaround the issue of the Keycloak openid JWT tipping past the 4K Cookie size limit when using the wordy HL7 FHIR scopes. The plan is to refactor our code to overcome the 4K cookie size limit by chunking the JWT across multiple cookies.  These scopes will go away in a future release.

| Scope Name | Description | Display on Consent Screen | Mapper |
| ------ | -------- | ---------- | ------ |
| PL | scope to read the authenticated patient's Laboratory records | OFF | n/a |
| PN | scope to access/modify authenticated patient's notification settings | OFF | n/a |
| SL | system-level client access to read anyone's Lab records | OFF | n/a |
| SN | system-level client access to read/update any user's notification settings | OFF | n/a

## Realm Roles

The following are realm-level roles, and default roles for OAuth2 and UMA.

| Role Name | Composite | Description |
| ------ | ------ | ------ |
| access_hnclient | False | Full access to HL7v2 message API through HNClient proxy. Only the medication-service client. |
| access_patient | False | Provides acces to Patient data|
| AdminUser | False | Administrator user role of the Health Gateway solution. Grants access to the admin dashboard and related functions. |
| HangfireUser | False | The role a user must be in to access the HealthGateway JobScheduler Dashboard, an implementation of Hangfire. |

### Notes

- 'access_hnclient' will be deprecated in a future release as we have deprecated our access to PharmaNet via HNSecure private network, as it is nearing end of life.
- 'access_patient' may be replaced by system/*.* once we have fixed our issue hitting the 4k Cookie size limit when creating JWTs.

## Identity Providers

For production we only using BC IDIM and the BC Services Card as our external Identity Provider (OIDC). This is a Level-3 Identity-Assured artefact and has a convenient digital app representation available on Android and iOS mobile devices. 

For site administrators we support government Active Directory (IDIR domain) and GitHub accounts for testing and development only. We also configure passwords for OIDC password grant flow for system accounts and for load testing.

| Name | Provider | Enabled | Hidden | Link Only |
| ----- | ------ | ------ | ------ | ------ |
| BC Services Card | oidc | True | True | False |
| IDIR | keycloak-oidc | True | False | False |

> Since we do not use the built-in user login theme templates for our primary web application, and we do not use the BC Services Card to logon to the Keycloak console we have Hidden the choice of BC Services Card from the default Keycloak login. Instead, the application has its own logon buttons and look and feel and passes the Identity Provider choice as a 'hint' to Keycloak during the openid login flow.


## Detailed Client Configurations

The following section details the client configurations. It also provides an archetype configuration for a third-party client, if one were to be added as an offering to Health Gateway users.

### Settings

### Roles

### Client Scopes

### Mappers

### Scope
