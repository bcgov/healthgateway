resource "keycloak_openid_client" "hgphsasystem_client" {
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = var.client_hg_phsa_system.id
  name                         = "Health Gateway System - ${var.environment.name}"
  description                  = "Health Gateway PHSA System User Outbound integration"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  login_theme                  = "bcgov-no-brand"
  standard_flow_enabled        = false
  direct_access_grants_enabled = false
  service_accounts_enabled     = true
  valid_redirect_uris          = var.client_hg_phsa_system.valid_redirects
  web_origins                  = var.client_hg_phsa_system.web_origins
  full_scope_allowed           = false
  access_token_lifespan        = var.client_hg_phsa_system.token_lifespan
}

resource "keycloak_openid_client_default_scopes" "hgphsasystem_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgphsasystem_client.id
  default_scopes = [
    "profile",
    "web-origins",
    "roles",
    keycloak_openid_client_scope.system_notification_read_scope.name,
    keycloak_openid_client_scope.system_notification_write_scope.name
  ]
}

resource "keycloak_generic_role_mapper" "hgphsasystem_system" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgphsasystem_client.id
  role_id   = keycloak_role.System.id
}

resource "keycloak_openid_audience_protocol_mapper" "hgphsasystem_audience" {
  realm_id                 = data.keycloak_realm.hg_realm.id
  client_id                = keycloak_openid_client.hgphsasystem_client.id
  name                     = "health-gateway-audience"
  included_client_audience = keycloak_openid_client.hg_client.client_id
  add_to_id_token          = true
  add_to_access_token      = true
}

resource "keycloak_openid_client_service_account_realm_role" "hgphsasystem_sa_system_role" {
  realm_id                = data.keycloak_realm.hg_realm.id
  service_account_user_id = keycloak_openid_client.hgphsasystem_client.service_account_user_id
  role                    = keycloak_role.System.name
}

resource "keycloak_openid_user_realm_role_protocol_mapper" "hgphsasystem_realmroles" {
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hgphsasystem_client.id
  name                = "realm roles"
  multivalued         = true
  claim_name          = "roles"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}
