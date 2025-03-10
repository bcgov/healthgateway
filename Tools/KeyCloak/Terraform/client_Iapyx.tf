resource "keycloak_openid_client" "iapyx_client" {
  count                        = local.devtest ? 1 : 0
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = var.client_iapyx.id
  name                         = "Regional Demo Auth App - ${var.environment.name}"
  description                  = "Regional Demo Auth App"
  enabled                      = true
  access_type                  = "PUBLIC"
  login_theme                  = "bcgov-no-brand"
  standard_flow_enabled        = true
  pkce_code_challenge_method   = "S256"
  direct_access_grants_enabled = false
  valid_redirect_uris          = var.client_iapyx.valid_redirects
  web_origins                  = var.client_iapyx.web_origins
  full_scope_allowed           = false
  access_token_lifespan        = var.client_iapyx.token_lifespan
  authentication_flow_binding_overrides {
    browser_id = keycloak_authentication_flow.iapyx_login[0].id
  }
}

resource "keycloak_openid_client_default_scopes" "iapyx_client_default_scopes" {
  count     = local.devtest ? 1 : 0
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = local.devtest ? keycloak_openid_client.iapyx_client[0].id : null

  default_scopes = [
    "profile",
    "web-origins",
    "email"
  ]
}
resource "keycloak_openid_client_optional_scopes" "iapyx_client_optional_scopes" {
  count     = local.devtest ? 1 : 0
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = local.devtest ? keycloak_openid_client.iapyx_client[0].id : null

  optional_scopes = [
    "address",
    "phone",
    "microprofile-jwt",
  ]
}

resource "keycloak_generic_role_mapper" "iapyx_uma" {
  count     = local.devtest ? 1 : 0
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = local.devtest ? keycloak_openid_client.iapyx_client[0].id : null
  role_id   = data.keycloak_role.Uma_authorization.id
}

resource "keycloak_openid_user_attribute_protocol_mapper" "iapyx_hdid" {
  count               = local.devtest ? 1 : 0
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = local.devtest ? keycloak_openid_client.iapyx_client[0].id : null
  name                = "hdid"
  user_attribute      = "hdid"
  claim_name          = "hdid"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

resource "keycloak_openid_user_attribute_protocol_mapper" "iapyx_auth_method" {
  count               = local.devtest ? 1 : 0
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = local.devtest ? keycloak_openid_client.iapyx_client[0].id : null
  name                = "AuthMethod"
  user_attribute      = "idp"
  claim_name          = "idp"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}
