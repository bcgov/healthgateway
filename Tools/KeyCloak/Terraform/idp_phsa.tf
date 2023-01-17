resource "keycloak_oidc_identity_provider" "phsa" {
  realm                         = data.keycloak_realm.hg_realm.id
  alias                         = "phsaazure"
  display_name                  = "PHSA Azure"
  enabled                       = true
  store_token                   = false
  trust_email                   = true
  first_broker_login_flow_alias = keycloak_authentication_flow.first_login.alias
  hide_on_login_page            = false
  sync_mode                     = "FORCE"
  authorization_url             = "${var.keycloak_idp_phsa.base_url}${var.keycloak_idp_phsa.auth_path}"
  token_url                     = "${var.keycloak_idp_phsa.base_url}${var.keycloak_idp_phsa.token_path}"
  backchannel_supported         = false
  client_id                     = var.keycloak_idp_phsa.client_id
  client_secret                 = var.keycloak_idp_phsa.client_secret
  default_scopes                = "openid profile email"
  validate_signature            = true
  jwks_url                      = "${var.keycloak_idp_phsa.base_url}${var.keycloak_idp_phsa.jwks_path}"
  extra_config = {
    "clientAuthMethod" = "client_secret_post"
    "prompt"           = "login"
  }
}

resource "keycloak_hardcoded_attribute_identity_provider_mapper" "phsa_idp" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "idp"
  identity_provider_alias = keycloak_oidc_identity_provider.phsa.alias
  attribute_name          = "idp"
  attribute_value         = "AZPHSA"
  user_session            = false
  extra_config = {
    syncMode = "INHERIT"
  }
}
