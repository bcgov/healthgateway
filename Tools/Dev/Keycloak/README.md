# HealthGateway KeyCloak Configuration Guide

The HealthGateway uses Keycloak, an OAuth2 compliant authorization server to control access to its protected resources. It connects the user via OIDC to external Identity Providers, in particular the BC Services Card identity and authentication service.   For administrators Keycloak also uses government employee's Active Directory services. 

## Environments

The HealthGateway project has three realms assigned to it for Dev, Test and production. Each environment is configured separately:

### Development Keycloak

<https://sso-dev.pathfinder.gov.bc.ca/auth/realms/ff09qn3f>

### Test and Demo Keycloak

<https://sso-tes.pathfinder.gov.bc.ca/auth/realms/ff09qn3f>

### Production Keycloak

<https://sso.pathfinder.gov.bc.ca/auth/realms/ff09qn3f>

## Configurations

### Realm Settings

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

### Clients

Aside from the Keycloak built-in clients, such as 'account', 'admin-cli', we add the following clients for HealthGateway:

| healthgateway | This is the main web client application. It uses OIDC to authenticate the user. It is an SPA responsive design web application |
| healthgateway-admin | This is the administrative web application used by staff. It requires an IDIR account to gain access. |
| medication-service | This represents the HealthGateway medication service resource server. We use this client to ensure that only it can access the PharmaNet HNSecure endpoints. This can be deprecated after deploying the ODR service connection |
| phsa-cdc | This is a new client that represents the PHSA client application to access HealthGateway services, particularly our Patient information service. It was added to support COVID-19 lab results access. |


We also add new clients for trusted third-party applications to be consented by the citizen to access the resources on their behalf. This is not yet supported, but is planned, further empowering citizens of BC to make use of additional health services by conveniently allowing applications to access their health records for them.



### Client Scopes

### Roles

### Identity Providers



