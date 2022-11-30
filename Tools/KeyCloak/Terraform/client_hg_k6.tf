resource "keycloak_openid_client" "hgk6_client" {
  count                        = local.development ? 1 : 0
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = var.client_hg_k6.id
  name                         = "Health Gateway K6 Client  - ${var.environment.name}"
  description                  = "Health Gateway K6 Performance Testing client"
  enabled                      = true
  access_type                  = "PUBLIC"
  login_theme                  = "bcgov-no-brand"
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  valid_redirect_uris          = var.client_hg_k6.valid_redirects
  web_origins                  = var.client_hg_k6.web_origins
  full_scope_allowed           = false
}

resource "keycloak_openid_client_default_scopes" "hgk6_client_default_scopes" {
  count     = local.development ? 1 : 0
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgk6_client[0].id

  default_scopes = []
}
resource "keycloak_openid_client_optional_scopes" "hgk6_client_optional_scopes" {
  count     = local.development ? 1 : 0
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgk6_client[0].id

  optional_scopes = [
    "web-origins",
    "email",
    "offline_access",
    "microprofile-jwt",
    keycloak_openid_client_scope.patient_read_scope.name,
    keycloak_openid_client_scope.encounter_read_scope.name,
    keycloak_openid_client_scope.immunization_read_scope.name,
    keycloak_openid_client_scope.laboratory_read_scope.name,
    keycloak_openid_client_scope.medication_dispense_read_scope.name,
    keycloak_openid_client_scope.notification_read_scope.name,
    keycloak_openid_client_scope.observation_read_scope.name,
    keycloak_openid_client_scope.audience_scope.name
  ]
}

resource "keycloak_openid_user_attribute_protocol_mapper" "hgk6_hdid" {
  count               = local.development ? 1 : 0
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hgk6_client[0].id
  name                = "hdid"
  user_attribute      = "hdid"
  claim_name          = "hdid"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

resource "keycloak_openid_user_attribute_protocol_mapper" "hgk6_auth_method" {
  count               = local.development ? 1 : 0
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hgk6_client[0].id
  name                = "AuthMethod"
  user_attribute      = "idp"
  claim_name          = "idp"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

resource "keycloak_openid_user_property_protocol_mapper" "hgk6_username" {
  count               = local.development ? 1 : 0
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hgk6_client[0].id
  name                = "username"
  user_property       = "username"
  claim_name          = "preferred_username"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}

resource "keycloak_openid_audience_protocol_mapper" "hgk6_audience" {
  count                    = local.development ? 1 : 0
  realm_id                 = data.keycloak_realm.hg_realm.id
  client_id                = keycloak_openid_client.hgk6_client[0].id
  name                     = "health-gateway-audience"
  included_client_audience = keycloak_openid_client.hg_client.client_id
  add_to_id_token          = true
  add_to_access_token      = true
}

resource "keycloak_generic_role_mapper" "hgk6_uma" {
  count     = local.development ? 1 : 0
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgk6_client[0].id
  role_id   = data.keycloak_role.Uma_authorization.id
}