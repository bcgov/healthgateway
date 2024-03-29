resource "keycloak_oidc_identity_provider" "bcsc_pcare" {
  realm                         = data.keycloak_realm.hg_realm.id
  alias                         = "pcare"
  display_name                  = "BCSC Pcare"
  enabled                       = true
  store_token                   = false
  trust_email                   = true
  hide_on_login_page            = true
  first_broker_login_flow_alias = keycloak_authentication_flow.first_login.alias
  sync_mode                     = "FORCE"
  authorization_url             = "${var.keycloak_idp_bcsc_pcare.base_url}${var.keycloak_idp_bcsc_pcare.auth_path}"
  login_hint                    = true
  token_url                     = "${var.keycloak_idp_bcsc_pcare.base_url}${var.keycloak_idp_bcsc_pcare.token_path}"
  backchannel_supported         = false
  user_info_url                 = "${var.keycloak_idp_bcsc_pcare.base_url}${var.keycloak_idp_bcsc_pcare.userinfo_path}"
  client_id                     = var.keycloak_idp_bcsc_pcare.client_id
  client_secret                 = var.keycloak_idp_bcsc_pcare.client_secret
  issuer                        = "${var.keycloak_idp_bcsc_pcare.base_url}${var.keycloak_idp_bcsc_pcare.issuer_path}"
  default_scopes                = "openid profile email"
  validate_signature            = true
  jwks_url                      = "${var.keycloak_idp_bcsc_pcare.base_url}${var.keycloak_idp_bcsc_pcare.jwks_path}"
  extra_config = {
    "clientAuthMethod" = "client_secret_post"
    "prompt"           = "login"
  }
  depends_on = [
    keycloak_authentication_flow.pcare_login
  ]
}

resource "keycloak_attribute_importer_identity_provider_mapper" "bcsc_pcare_given_name" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "given_name"
  claim_name              = "given_names"
  identity_provider_alias = keycloak_oidc_identity_provider.bcsc_pcare.alias
  user_attribute          = "firstName"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_hardcoded_attribute_identity_provider_mapper" "bcsc_pcare_idp" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "idp"
  identity_provider_alias = keycloak_oidc_identity_provider.bcsc_pcare.alias
  attribute_name          = "idp"
  attribute_value         = "BCSC"
  user_session            = false
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "bcsc_pcare_email" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "email"
  claim_name              = "email"
  identity_provider_alias = keycloak_oidc_identity_provider.bcsc_pcare.alias
  user_attribute          = "email"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "bcsc_pcare_display_name" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "display_name"
  claim_name              = "display_name"
  identity_provider_alias = keycloak_oidc_identity_provider.bcsc_pcare.alias
  user_attribute          = "fullName"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "bcsc_pcare_family_name" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "family_name"
  claim_name              = "family_name"
  identity_provider_alias = keycloak_oidc_identity_provider.bcsc_pcare.alias
  user_attribute          = "lastName"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "bcsc_pcare_username" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "username"
  claim_name              = "sub"
  identity_provider_alias = keycloak_oidc_identity_provider.bcsc_pcare.alias
  user_attribute          = "username"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "bcsc_pcare_hdid" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "hdid"
  claim_name              = "sub"
  identity_provider_alias = keycloak_oidc_identity_provider.bcsc_pcare.alias
  user_attribute          = "hdid"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "bcsc_pcare_iasenv" {
  realm                   = data.keycloak_realm.hg_realm.id
  name                    = "iasenv"
  claim_name              = "authoritative_party_identifier"
  identity_provider_alias = keycloak_oidc_identity_provider.bcsc_pcare.alias
  user_attribute          = "iasenv"
  extra_config = {
    syncMode = "INHERIT"
  }
}