resource "keycloak_openid_client" "hgkeycloak_client" {
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = var.client_hg_keycloak.id
  name                         = "Health Gateway Keycloak - ${var.environment.name}"
  description                  = "Health Gateway Keycloak Administration"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  standard_flow_enabled        = false
  direct_access_grants_enabled = false
  service_accounts_enabled     = true
   valid_redirect_uris          = var.client_hg_keycloak.valid_redirects
   web_origins                  = var.client_hg_keycloak.web_origins
  full_scope_allowed           = true
}

resource "keycloak_openid_client_default_scopes" "hgkeycloak_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgkeycloak_client.id
  default_scopes = [
    "profile",
    "web-origins",
    "roles"
  ]
}

resource "keycloak_openid_client_service_account_realm_role" "hgkeycloak_sa_admin" {
  realm_id                = data.keycloak_realm.hg_realm.id
  service_account_user_id = keycloak_openid_client.hgkeycloak_client.service_account_user_id
  role                    = data.keycloak_role.realm_admin.name
}

