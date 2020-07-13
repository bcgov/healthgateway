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
