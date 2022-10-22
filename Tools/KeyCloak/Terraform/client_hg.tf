resource "keycloak_openid_client" "hg_client" {
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = "hg"
  name                         = "Health Gateway  - ${var.environment}"
  description                  = "Health Gateway web application"
  enabled                      = true
  access_type                  = "PUBLIC"
  login_theme                  = "bcgov"
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  valid_redirect_uris          = var.client_hg_valid_redirects
  web_origins                  = var.client_hg_web_origins
  full_scope_allowed           = false
}

resource "keycloak_openid_client_default_scopes" "hg_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hg_client.id

  default_scopes = [
    "profile",
    "web-origins",
    keycloak_openid_client_scope.audience_scope.name,
    keycloak_openid_client_scope.immunization_read_scope.name,
    keycloak_openid_client_scope.laboratory_read_scope.name,
    keycloak_openid_client_scope.notification_read_scope.name,
    keycloak_openid_client_scope.patient_read_scope.name
  ]
}
resource "keycloak_openid_client_optional_scopes" "hg_client_optional_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hg_client.id

  optional_scopes = [
    "address",
    "email",
    "phone",
    "microprofile-jwt",
    keycloak_openid_client_scope.encounter_read_scope.name,
    keycloak_openid_client_scope.medication_dispense_read_scope.name,
    keycloak_openid_client_scope.observation_read_scope.name,
  ]
}

resource "keycloak_openid_user_attribute_protocol_mapper" "hg_hdid" {
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hg_client.id
  name                = "hdid"
  user_attribute      = "hdid"
  claim_name          = "hdid"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

resource "keycloak_openid_user_attribute_protocol_mapper" "hg_auth_method" {
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hg_client.id
  name                = "AuthMethod"
  user_attribute      = "idp"
  claim_name          = "idp"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

resource "keycloak_openid_user_property_protocol_mapper" "hg_username" {
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hg_client.id
  name                = "username"
  user_property       = "username"
  claim_name          = "preferred_username"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

resource "keycloak_openid_audience_protocol_mapper" "hg_audience" {
  realm_id                 = data.keycloak_realm.hg_realm.id
  client_id                = keycloak_openid_client.hg_client.id
  name                     = "hg-audience"
  included_client_audience = keycloak_openid_client.hg_client.client_id
  add_to_id_token          = true
  add_to_access_token      = true
}

resource "keycloak_generic_role_mapper" "hg_uma" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hg_client.id
  role_id   = data.keycloak_role.Uma_authorization.id
}