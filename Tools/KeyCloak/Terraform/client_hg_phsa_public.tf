resource "keycloak_openid_client" "hgphsapublic_client" {
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = var.client_hg_phsa_public.id
  name                         = "Health Gateway Public - ${var.environment.name}"
  description                  = "Health Gateway PHSA Public User Outbound integration"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  login_theme                  = "bcgov"
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  service_accounts_enabled     = true
  valid_redirect_uris          = var.client_hg_phsa_public.valid_redirects
  web_origins                  = var.client_hg_phsa_public.web_origins
  full_scope_allowed           = false
}

resource "keycloak_openid_client_default_scopes" "hgphsapublic_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgphsapublic_client.id
  default_scopes = [
    "profile",
    "web-origins",
    "roles",
    keycloak_openid_client_scope.audience_scope.name,
    keycloak_openid_client_scope.patient_read_scope.name,
    keycloak_openid_client_scope.laboratory_read_scope.name,
    keycloak_openid_client_scope.immunization_read_scope.name,
  ]
}

resource "keycloak_generic_role_mapper" "hgphsapublic_anonymous" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgphsapublic_client.id
  role_id   = keycloak_role.Anonymous.id
}

resource "keycloak_openid_audience_protocol_mapper" "hgphsapublic_audience" {
  realm_id                 = data.keycloak_realm.hg_realm.id
  client_id                = keycloak_openid_client.hgphsapublic_client.id
  name                     = "health-gateway-audience"
  included_client_audience = keycloak_openid_client.hg_client.client_id
  add_to_id_token          = true
  add_to_access_token      = true
}

resource "keycloak_openid_client_service_account_realm_role" "hgphsapublic_sa_anonymous_role" {
  realm_id                = data.keycloak_realm.hg_realm.id
  service_account_user_id = keycloak_openid_client.hgphsapublic_client.service_account_user_id
  role                    = keycloak_role.Anonymous.name
}

resource "keycloak_openid_user_realm_role_protocol_mapper" "hgphsapublic_realmroles" {
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hgphsapublic_client.id
  name                = "realm roles"
  multivalued         = true
  claim_name          = "roles"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}
