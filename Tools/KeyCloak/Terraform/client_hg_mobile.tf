resource "keycloak_openid_client" "hg_mobile_client" {
  realm_id                     = var.keycloak_realm
  client_id                    = "HealthGatewayMobile"
  name                         = "Health Gateway Mobile"
  description                  = "Health Gateway Mobile applications"
  enabled                      = true
  access_type                  = "PUBLIC"
  login_theme                  = "bcgov-idp-stopper"
  standard_flow_enabled        = true
  direct_access_grants_enabled = false
  valid_redirect_uris          = var.client_hg_mobile_valid_redirects
  web_origins                  = var.client_hg_mobile_web_origins
  full_scope_allowed           = false
}

resource "keycloak_openid_client_default_scopes" "hg_mobile_client_default_scopes" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hg_mobile_client.id

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
resource "keycloak_openid_client_optional_scopes" "hg_mobile_client_optional_scopes" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hg_mobile_client.id

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

resource "keycloak_generic_role_mapper" "hg_mobile_uma" {
  realm_id  = var.keycloak_realm
  client_id = keycloak_openid_client.hg_mobile_client.id
  role_id   = data.keycloak_role.Uma_authorization.id
}

resource "keycloak_openid_audience_protocol_mapper" "hg_mobile_audience" {
  realm_id                 = var.keycloak_realm
  client_id                = keycloak_openid_client.hg_mobile_client.id
  name                     = "health-gateway-audience"
  included_client_audience = keycloak_openid_client.hg_client.client_id
  add_to_id_token          = true
  add_to_access_token      = true
}