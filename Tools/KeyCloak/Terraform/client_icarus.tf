resource "keycloak_openid_client" "icarus_client" {
  realm_id                        = data.keycloak_realm.hg_realm.id
  client_id                       = var.client_icarus.id
  name                            = "VPP Client - ${var.environment.name}"
  description                     = "VPP Client"
  enabled                         = true
  access_type                     = "CONFIDENTIAL"
  login_theme                     = "bcgov-no-brand"
  standard_flow_enabled           = true
  direct_access_grants_enabled    = false
  valid_redirect_uris             = var.client_icarus.valid_redirects
  valid_post_logout_redirect_uris = var.client_icarus.valid_logout_redirects
  web_origins                     = var.client_icarus.web_origins
  full_scope_allowed              = false
  access_token_lifespan           = var.client_icarus.token_lifespan
  authentication_flow_binding_overrides {
    browser_id = keycloak_authentication_flow.icarus_login.id
  }
}

resource "keycloak_openid_client_default_scopes" "icarus_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.icarus_client.id

  default_scopes = [
    "basic",
    "profile",
    "web-origins",
    "email",
    keycloak_openid_client_scope.audience_scope.name,
    # keycloak_openid_client_scope.immunization_read_scope.name,
    # keycloak_openid_client_scope.laboratory_read_scope.name,
    # keycloak_openid_client_scope.notification_read_scope.name,
    # keycloak_openid_client_scope.patient_read_scope.name
  ]
}
resource "keycloak_openid_client_optional_scopes" "icarus_client_optional_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.icarus_client.id

  optional_scopes = [
    "address",
    "phone",
    "microprofile-jwt",
    # keycloak_openid_client_scope.encounter_read_scope.name,
    # keycloak_openid_client_scope.medication_dispense_read_scope.name,
    # keycloak_openid_client_scope.observation_read_scope.name,
  ]
}

resource "keycloak_generic_role_mapper" "icarus_uma" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.icarus_client.id
  role_id   = data.keycloak_role.Uma_authorization.id
}

resource "keycloak_openid_user_attribute_protocol_mapper" "icarus_hdid" {
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.icarus_client.id
  name                = "hdid"
  user_attribute      = "hdid"
  claim_name          = "hdid"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

resource "keycloak_openid_user_attribute_protocol_mapper" "icarus_auth_method" {
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.icarus_client.id
  name                = "AuthMethod"
  user_attribute      = "idp"
  claim_name          = "idp"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

# resource "keycloak_openid_audience_protocol_mapper" "icarus_audience" {
#   realm_id                 = data.keycloak_realm.hg_realm.id
#   client_id                = keycloak_openid_client.icarus_client.id
#   name                     = "health-gateway-audience"
#   included_client_audience = keycloak_openid_client.hg_client.client_id
#   add_to_id_token          = true
#   add_to_access_token      = true
# }