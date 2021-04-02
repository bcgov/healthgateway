# HealthGateway KeyCloak Configuration Guide

The HealthGateway uses Keycloak, an OAuth2 compliant authorization server to control access to its protected resources. It connects the user via OIDC to external Identity Providers, in particular the BC Services Card identity and authentication service. For administrators Keycloak also uses government employee's Active Directory services.

## Environments

The HealthGateway project has three realms assigned to it for Dev, Test and Production. Each environment is configured separately:

### Development Keycloak

<https://sso-dev.pathfinder.gov.bc.ca/auth/realms/ff09qn3f>

### Test and Demo Keycloak

<https://sso-test.pathfinder.gov.bc.ca/auth/realms/ff09qn3f>

### Production Keycloak

<https://sso.pathfinder.gov.bc.ca/auth/realms/ff09qn3f>

## Realm Settings

Realm settings establish the basic realm ID, in our case a license-plate style realm id is assigned by the Keycloak administrators. The fact that it is enabled, and that we will use User-Managed Access (UMA Compliant) for delegating access to resources whether person-to-person, or person-to-client app.

| Setting                       | Value                                                                     |
| ----------------------------- | ------------------------------------------------------------------------- |
| Name                          | ff0qn3f                                                                   |
| Enabled                       | ON                                                                        |
| User-Managed Access           | ON                                                                        |
| OpenID Endpoint Configuration | {base-keycloak-uri}/auth/realms/ff09qn3f/.well-known/openid-configuration |

OpenID endpoint discovery URL is an important end point for discovering the useful endpoints for client OAuth software, of particular utility. For example, for Development environment the well known endpoints for openid are found at:

| Dev                           | URL                                                                                          |
| ----------------------------- | -------------------------------------------------------------------------------------------- |
| OpenID Endpoint Configuration | <https://sso-dev.pathfinder.gov.bc.ca/auth/realms/ff09qn3f/.well-known/openid-configuration> |

## Clients

Aside from the Keycloak built-in clients, such as 'account', 'admin-cli', we add the following clients for HealthGateway:

| Client ID           | Access Type  | Consent Required | Direct Access Grant | Service Accounts | Description                                                                                                                                                                                           |
| ------------------- | ------------ | ---------------- | ------------------- | ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| healthgateway       | public       | OFF              | OFF                 | OFF              | This is the main web client application. It uses OIDC to authenticate the user. It is an SPA responsive design web application                                                                        |
| healthgateway-admin | confidential | OFF              | ON                  | ON               | This is the administrative web application used by staff. It requires an IDIR account to gain access.                                                                                                 |
| phsa-cdc            | confidential | OFF              | ON                  | ON               | This is a new client that represents the PHSA client application to access HealthGateway services, particularly our Patient information service. It was added to support COVID-19 lab results access. |

> We also add new clients for trusted third-party applications to be consented by the citizen to access the resources on their behalf. This is not yet supported, but is planned, further empowering citizens of BC to make use of additional health services by conveniently allowing applications to access their health records for them.

## Client Scopes

Client Scopes determine what resource owner claims can be asked for. For example, if a client is assigned the scope 'patient/Patient.read', and HL7 FHIR type scope, and 'Requires Consent' is turned off for that client, then that client can call resources that require the presence of that scope in the access_token.

When a scope is optional, it just means that the client must explicitely request it as part of the OAuth2 flow that gathers the access_token, i.e. code flow with OIDC, as an example.

Clients, such as those listed above, can only request or be assigned scopes that are defined in this section of the configuration.

The scopes below listed below are only those that are not OpenID Connect built-in, but have been added and configured.

| Scope Name                      | Description                                                           | Display on Consent Screen | Mapper           |
| ------------------------------- | --------------------------------------------------------------------- | ------------------------- | ---------------- |
| audience                        | Resolves the audience of the client                                   | OFF                       | audience-resolve |
| patient/Immunization.read       | HL7 FHIR scope to read a specific patient's immunization records      | ON                        | n/a              |
| patient/Laboratory.read         | HL7 FJIR-inspired scope to read a patient's Laboratory records        | ON                        | n/a              |
| patient/MedicationDispense.read | HL7 FHIR scope to read a patient's Medication History                 | ON                        | n/a              |
| patient/Observation.read        | HL7 FHIR scope to read patient observations                           | ON                        | n/a              |
| patient/Patient.read            | HL7 FHIR scope to read patient demographics record                    | ON                        | n/a              |
| system/Patient.read             | HL7 FHIR scope to read any patient's immunization records             | OFF                       | n/a              |
| system/\*.read                  | HL7 FHIR wildcard scope allowing read access to any patient's records | OFF                       | n/a              |

> As we add more services, we will introduce new resource scopes following HL7 FHIR and SMART on FHIR as much as is practicable.

### Temporary Scopes

The following shortened scope names were put in place for our integration with PHSA for COVID-19 Lab test results and for notification settings (SMS, Email). These were to workaround the issue of the Keycloak openid JWT tipping past the 4K Cookie size limit when using the wordy HL7 FHIR scopes. The plan is to refactor our code to overcome the 4K cookie size limit by chunking the JWT across multiple cookies. These scopes will go away in a future release.

> In addition, we will be removing access_hnclient scope as the HNSecure network is no longer used to access medication statements.

| Scope Name      | Description                                                                | Display on Consent Screen | Mapper |
| --------------- | -------------------------------------------------------------------------- | ------------------------- | ------ |
| access_hnclient | scope to allow only the medication service to access HNClient proxy        |
| PL              | scope to read the authenticated patient's Laboratory records               | OFF                       | n/a    |
| PN              | scope to access/modify authenticated patient's notification settings       | OFF                       | n/a    |
| SL              | system-level client access to read anyone's Lab records                    | OFF                       | n/a    |
| SN              | system-level client access to read/update any user's notification settings | OFF                       | n/a    |

## Realm Roles

The following are realm-level roles, and default roles for OAuth2 and UMA.

| Role Name       | Composite | Description                                                                                                         |
| --------------- | --------- | ------------------------------------------------------------------------------------------------------------------- |
| access_hnclient | False     | Full access to HL7v2 message API through HNClient proxy. Only the medication-service client.                        |
| access_patient  | False     | Provides acces to Patient data                                                                                      |
| AdminUser       | False     | Administrator user role of the Health Gateway solution. Grants access to the admin dashboard and related functions. |
| HangfireUser    | False     | The role a user must be in to access the HealthGateway JobScheduler Dashboard, an implementation of Hangfire.       |

### Notes

-   'access_hnclient' will be deprecated in a future release as we have deprecated our access to PharmaNet via HNSecure private network, as it is nearing end of life.
-   'access_patient' may be replaced by system/\*.\* once we have fixed our issue hitting the 4k Cookie size limit when creating JWTs.

## Identity Providers

For production we only using BC IDIM and the BC Services Card as our external Identity Provider (OIDC). This is a Level-3 Identity-Assured artefact and has a convenient digital app representation available on Android and iOS mobile devices.

For site administrators we support government Active Directory (IDIR domain) and GitHub accounts for testing and development only. We also configure passwords for OIDC password grant flow for system accounts and for load testing.

| Name             | Provider      | Enabled | Hidden | Link Only |
| ---------------- | ------------- | ------- | ------ | --------- |
| BC Services Card | oidc          | True    | True   | False     |
| IDIR             | keycloak-oidc | True    | False  | False     |

> Since we do not use the built-in Keycloak user login themed templates for our primary web application and we do not use the BC Services Card to logon to the Keycloak console we have Hidden the choice of BC Services Card from the default Keycloak login. Instead, the Health Gatway web application has its own logon buttons with its own look and feel and passes the Identity Provider choice as a 'hint' to Keycloak during the openid connect login flow.

## User Federation

No user federation is configured.

## Authentication

| Auth Type      | Requirement                                          |
| -------------- | ---------------------------------------------------- |
| Cookie         | Alternative                                          |
| Kerberos       | Disabled                                             |
| IdP Redirector | Alternative                                          |
| Forms          | Alternative, with Required password and optional OTP |

## Groups

Two groups: Citizens and Realm Administrator

## Users

> For our load testing, we have provisioned password-based authentication for test users to allow our K6 load and stress test scripts to login as differing accounts. This applies to TEST environment only, with some added to DEV environment.

Usernames for K6: loadtest_01 to loadtest_13

Each of these users borrows an HDID manually set as a user-attribute 'hdid' for these test users, and borrowed from the BC Services Cards Test accounts.

---

## Detailed Client Configurations

The following section details the client configurations. It also provides an archetype configuration for a third-party client, if one were to be added as an offering to Health Gateway users.

### Settings

| Client ID           | Description                                   | Consent Required | Protocol       | Access Type  | Standard Flow | Implicit Flow | Direct Access Grant | Service Accounts Enabled | Authorization Enabled (UMA) | Credentials          |
| ------------------- | --------------------------------------------- | ---------------- | -------------- | ------------ | ------------- | ------------- | ------------------- | ------------------------ | --------------------------- | -------------------- |
| healthgateway       | Health Gateway Web Client {env}               | OFF              | openid-connect | public       | ON            | OFF           | OFF                 | OFF                      | OFF                         | N/A                  |
| healthgateway-admin | Health Gateway Administration web application | OFF              | openid-connect | confidential | ON            | OFF           | ON                  | ON                       | OFF                         | Client Id and Secret |
| phsa-cdc            | PHSA CDC Lab Report Services                  | OFF              | openid-connect | confidential | ON            | OFF           | ON                  | ON                       | OFF                         | Client Id and Secret |

### Roles

| Client ID           | Roles           |
| ------------------- | --------------- |
| healthgateway       | No client roles |
| healthgateway-admin | No client roles |
| phsa-cdc            | No client roles |

### Client Scopes Assigned

| Client ID           | Assigned Client Scopes                                      | Assigned Optional Client Scopes |
| ------------------- | ----------------------------------------------------------- | ------------------------------- |
| healthgateway       | web-origins, audience                                       |
| healthgateway-admin | web-origins, audience, email, profile, roles, SL, SN        | offline_access                  |
| phsa-cdc            | web-origins, audience, system/Patient.read, profile, PL, PN | offline_access                  |

### Custom Mappers

> Here, we only show the non-built-in mappers that were added.

| Client ID           | Mapper Name                   | Category     | Type            | Priority | Mapper Value/Setting | Add to ID Token | Add to access token | Add to userinfo |
| ------------------- | ----------------------------- | ------------ | --------------- | -------- | -------------------- | --------------- | ------------------- | --------------- |
| healthgateway       | hdid                          | Token mapper | User Attribute  | 0        | hdid                 | ON              | ON                  | ON              |
| healthgateway       | health-gateway-audience       | Token Mapper | Audience        | 0        | healthgateway        | OFF             | ON                  | N/A             |
| healthgateway       | given name                    | Token maper  | User Property   | 0        | firstName            | OFF             | OFF                 | ON              |
| healthgateway       | family name                   | Token maper  | User Property   | 0        | lastName             | OFF             | OFF                 | ON              |
| healthgateway-admin | realm roles                   | Token Mapper | User Realm Role | 0        | user_realm_roles     | ON              | ON                  | ON              |
| healthgateway-admin | admin-audience-mapper         | Token Mapper | Audience        | 0        | healthgateway-admin  | OFF             | ON                  | N/A             |
| phsa-cdc            | hdid                          | Token mapper | User Attribute  | 0        | hdid                 | ON              | ON                  | ON              |
| phsa-cdc            | healthgateway-audience-mapper | Token Mapper | Audience        | 0        | healthgateway        | OFF             | ON                  | N/A             |

### Scope

| Client ID           | Assigned Roles                          |
| ------------------- | --------------------------------------- |
| healthgateway       | offline_access, uma_authorization       |
| healthgateway-admin | AdminUser, HangfireUser, offline_access |
| phsa-cdc            | offline_access                          |

### Service Account Roles

> Applies only to clients with Service Accounts enabled

| Client ID           | Assigned Roles                                             |
| ------------------- | ---------------------------------------------------------- |
| healthgateway       | N/A                                                        |
| healthgateway-admin | AdminUser, HangfireUser, offline_access, uma_authorization |
| phsa-cdc            | offline_access, uma_authorization                          |
