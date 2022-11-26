resource "keycloak_openid_client" "hgpublic_client" {
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = var.client_hg_public.id
  name                         = "Health Gateway Public - ${var.environment.name}"
  description                  = "Health Gateway PHSA integration"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  login_theme                  = "bcgov"
  standard_flow_enabled        = true
  direct_access_grants_enabled = true
  service_accounts_enabled     = true
  valid_redirect_uris          = var.client_hg_public.valid_redirects
  web_origins                  = var.client_hg_public.web_origins
  full_scope_allowed           = false
}

resource "keycloak_openid_client_default_scopes" "hgpublic_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgpublic_client.id
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

resource "keycloak_generic_role_mapper" "hgpublic_anonymous" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgpublic_client.id
  role_id   = keycloak_role.Anonymous.id
}

resource "keycloak_openid_audience_protocol_mapper" "hgpublic_audience" {
  realm_id                 = data.keycloak_realm.hg_realm.id
  client_id                = keycloak_openid_client.hgpublic_client.id
  name                     = "health-gateway-audience"
  included_client_audience = keycloak_openid_client.hg_client.client_id
  add_to_id_token          = true
  add_to_access_token      = true
}

resource "keycloak_openid_client_service_account_realm_role" "hgpublic_sa_anonymous_role" {
  realm_id                = data.keycloak_realm.hg_realm.id
  service_account_user_id = keycloak_openid_client.hgpublic_client.service_account_user_id
  role                    = keycloak_role.Anonymous.name
}

resource "keycloak_openid_user_realm_role_protocol_mapper" "hgpublic_realmroles" {
  realm_id            = data.keycloak_realm.hg_realm.id
  client_id           = keycloak_openid_client.hgpublic_client.id
  name                = "realm roles"
  multivalued         = true
  claim_name          = "roles"
  claim_value_type    = "String"
  add_to_id_token     = true
  add_to_access_token = true
  add_to_userinfo     = true
}
