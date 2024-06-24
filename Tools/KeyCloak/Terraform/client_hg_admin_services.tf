resource "keycloak_openid_client" "hgadminservices_client" {
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = var.client_hg_admin_services.id
  name                         = "Health Gateway Administration Services - ${var.environment.name}"
  description                  = "Health Gateway Administration services"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  login_theme                  = local.development ? "bcgov-no-brand" : "bcgov-idp-login-no-brand"
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  service_accounts_enabled     = local.development ? true : false
  valid_redirect_uris          = var.client_hg_admin.valid_redirects
  web_origins                  = var.client_hg_admin.web_origins
  full_scope_allowed           = false
  access_token_lifespan        = var.client_hg_admin_services.token_lifespan
}

resource "keycloak_openid_client_default_scopes" "hgadminservices_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgadminservices_client.id
  default_scopes = [
    "email",
    "profile",
    "web-origins",
    "roles",
    keycloak_openid_client_scope.audience_scope.name,
    keycloak_openid_client_scope.immunization_read_scope.name,
    keycloak_openid_client_scope.laboratory_read_scope.name,
  ]
}
resource "keycloak_openid_client_optional_scopes" "hgadminservices_client_optional_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgadminservices_client.id
  optional_scopes = [
    "microprofile-jwt",
    "offline_access"
  ]
}

resource "keycloak_generic_role_mapper" "hgadminservices_adminreviewer" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgadminservices_client.id
  role_id   = keycloak_role.AdminReviewer.id
}

resource "keycloak_generic_role_mapper" "hgadminservices_adminuser" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgadminservices_client.id
  role_id   = keycloak_role.AdminUser.id
}

resource "keycloak_generic_role_mapper" "hgadminservices_supportuser" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgadminservices_client.id
  role_id   = keycloak_role.SupportUser.id
}

resource "keycloak_generic_role_mapper" "hgadminservices_adminanalyst" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgadminservices_client.id
  role_id   = keycloak_role.AdminAnalyst.id
}

resource "keycloak_openid_user_attribute_protocol_mapper" "hgadminservices_auth_method" {
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hgadminservices_client.id
  name                = "AuthMethod"
  user_attribute      = "idp"
  claim_name          = "idp"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

resource "keycloak_openid_audience_protocol_mapper" "hgadminservices_audience" {
  realm_id                 = data.keycloak_realm.hg_realm.id
  client_id                = keycloak_openid_client.hgadminservices_client.id
  name                     = "hg-admin-audience"
  included_client_audience = keycloak_openid_client.hgadmin_client.client_id # share same audience
  add_to_id_token          = true
  add_to_access_token      = true
}

resource "keycloak_openid_user_realm_role_protocol_mapper" "hgadminservices_realmroles" {
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hgadminservices_client.id
  name                = "realm roles"
  multivalued         = true
  claim_name          = "roles"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

resource "keycloak_openid_hardcoded_claim_protocol_mapper" "hgadminservices_emailOverride" {
  realm_id    = data.keycloak_realm.hg_realm.id
  client_id   = keycloak_openid_client.hgadminservices_client.id
  name        = "emailOverride"
  claim_name  = "email"
  claim_value = "no_email@hg.gov.bc.ca"
}
