resource "keycloak_openid_client" "erebus_client" {
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = var.client_erebus.id
  name                         = "PHSA LRA - ${var.environment.name}"
  description                  = "PHSA LRA inbound integration"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  login_theme                  = "bcgov-no-brand"
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  service_accounts_enabled     = true
  valid_redirect_uris          = var.client_erebus.valid_redirects
  web_origins                  = var.client_erebus.web_origins
  full_scope_allowed           = false
}

resource "keycloak_openid_client_default_scopes" "erebus_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.erebus_client.id
  default_scopes = [
    "web-origins",
    keycloak_openid_client_scope.audience_scope.name,
    keycloak_openid_client_scope.phsa_scope.name,
    keycloak_openid_client_scope.system_lra_read_scope.name
  ]
}
resource "keycloak_openid_client_optional_scopes" "erebus_client_optional_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.erebus_client.id
  optional_scopes = [
    "phone",
    "microprofile-jwt"
  ]
}

resource "keycloak_openid_audience_protocol_mapper" "erebus_audience" {
  realm_id                 = data.keycloak_realm.hg_realm.id
  client_id                = keycloak_openid_client.erebus_client.id
  name                     = "health-gateway-audience"
  included_client_audience = keycloak_openid_client.hg_client.client_id
  add_to_id_token          = true
  add_to_access_token      = true
}
