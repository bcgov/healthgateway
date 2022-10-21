resource "keycloak_openid_client" "hgadmin_client" {
  realm_id                     = var.keycloak_realm
  client_id                    = "HealthGatewayAdmin"
  name                         = "Health Gateway Administration"
  description                  = "Health Gateway Administration web application"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  login_theme                  = "bcgov"
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  service_accounts_enabled     = true
  valid_redirect_uris          = var.client_hg_admin_valid_redirects
  web_origins                  = var.client_hg_admin_web_origins
  full_scope_allowed           = false
}

resource "keycloak_openid_client_default_scopes" "hgadmin_client_default_scopes" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hgadmin_client.id
  default_scopes = [
    "email",
    "profile",
    "web-origins",
    "roles",
    keycloak_openid_client_scope.audience_scope.name,
    keycloak_openid_client_scope.immunization_read_scope.name,
    keycloak_openid_client_scope.laboratory_read_scope.name,
    keycloak_openid_client_scope.system_notification_read_scope.name,
    keycloak_openid_client_scope.system_notification_write_scope.name
  ]
}
resource "keycloak_openid_client_optional_scopes" "hgadmin_client_optional_scopes" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hgadmin_client.id
  optional_scopes = [
    "phone",
    "microprofile-jwt"
  ]
}

resource "keycloak_generic_role_mapper" "hgadmin_adminreviewer" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hgadmin_client.id
  role_id   = keycloak_role.AdminReviewer.id
}

resource "keycloak_generic_role_mapper" "hgadmin_adminuser" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hgadmin_client.id
  role_id   = keycloak_role.AdminUser.id
}

resource "keycloak_generic_role_mapper" "hgadmin_supportuser" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hgadmin_client.id
  role_id   = keycloak_role.SupportUser.id
}

resource "keycloak_generic_role_mapper" "hgadmin_system" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hgadmin_client.id
  role_id   = keycloak_role.System.id
}

resource "keycloak_generic_role_mapper" "hgadmin_anonymous" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hgadmin_client.id
  role_id   = keycloak_role.Anonymous.id
}