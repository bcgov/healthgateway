resource "keycloak_oidc_identity_provider" "idiridp" {
  realm                 = var.keycloak_realm
  alias                 = "idir"
  display_name          = "IDIR"
  enabled               = true
  store_token           = false
  trust_email           = true
  sync_mode             = "IMPORT"
  authorization_url     = "${var.keycloak_base_url}/realms/standard/protocol/openid-connect/auth"
  token_url             = "${var.keycloak_base_url}/realms/standard/protocol/openid-connect/token"
  logout_url            = "${var.keycloak_base_url}/realms/standard/logout/protocol/openid-connect"
  backchannel_supported = true
  user_info_url         = "${var.keycloak_base_url}/realms/standard/protocol/openid-connect/userinfo"
  client_id             = var.keycloak_idp_idir_client_id
  client_secret         = var.keycloak_idp_idir_client_secret
  issuer                = "${var.keycloak_base_url}/realms/standard"
  default_scopes        = "openid profile email"
  validate_signature    = true

  jwks_url = "${var.keycloak_base_url}/realms/standard/protocol/openid-connect/certs"
  extra_config = {
    "clientAuthMethod" = "client_secret_post"
    "prompt"           = "login"
  }
}

resource "keycloak_user_template_importer_identity_provider_mapper" "username_importer" {
  realm                   = var.keycloak_realm
  name                    = "username"
  identity_provider_alias = keycloak_oidc_identity_provider.idiridp.alias
  template                = "$${CLAIM.idir_username}@$${ALIAS}"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "idir_userid" {
  realm                   = var.keycloak_realm
  name                    = "idir_userid"
  claim_name              = "idir_username"
  identity_provider_alias = keycloak_oidc_identity_provider.idiridp.alias
  user_attribute          = "idir_userid"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "idir_guid" {
  realm                   = var.keycloak_realm
  name                    = "idir_guid"
  claim_name              = "idir_user_guid"
  identity_provider_alias = keycloak_oidc_identity_provider.idiridp.alias
  user_attribute          = "idir_guid"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_attribute_importer_identity_provider_mapper" "displayname" {
  realm                   = var.keycloak_realm
  name                    = "displayName"
  claim_name              = "display_name"
  identity_provider_alias = keycloak_oidc_identity_provider.idiridp.alias
  user_attribute          = "displayName"
  extra_config = {
    syncMode = "INHERIT"
  }
}

resource "keycloak_hardcoded_attribute_identity_provider_mapper" "idp" {
  realm                   = var.keycloak_realm
  name                    = "idp"
  identity_provider_alias = keycloak_oidc_identity_provider.idiridp.alias
  attribute_name          = "idp"
  attribute_value         = "IDIR"
  user_session            = false
  extra_config = {
    syncMode = "INHERIT"
  }
}