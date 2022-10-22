resource "keycloak_openid_client" "hgphsa_client" {
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = "hg-phsa"
  name                         = "Health Gateway PHSA - ${var.environment}"
  description                  = "Health Gateway PHSA integration"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  login_theme                  = "bcgov"
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  service_accounts_enabled     = true
  valid_redirect_uris          = var.client_hg_phsa_valid_redirects
  web_origins                  = var.client_hg_phsa_web_origins
  full_scope_allowed           = false
}

resource "keycloak_openid_client_default_scopes" "hgphsa_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgphsa_client.id
  default_scopes = [
    "web-origins",
    keycloak_openid_client_scope.audience_scope.name,
    keycloak_openid_client_scope.patient_read_scope.name
  ]
}
resource "keycloak_openid_client_optional_scopes" "hgphsa_client_optional_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgphsa_client.id
  optional_scopes = [
    "phone",
    "microprofile-jwt",
    keycloak_openid_client_scope.system_notification_read_scope.name,
    keycloak_openid_client_scope.system_notification_write_scope.name
  ]
}
