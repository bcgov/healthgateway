resource "keycloak_openid_client" "hgseq_client" {
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = var.client_hg_seq.id
  name                         = "Health Gateway Seq - ${var.environment.name}"
  description                  = "Health Gateway Seq Administration Access"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  standard_flow_enabled        = true
  direct_access_grants_enabled = false
  service_accounts_enabled     = false
  valid_redirect_uris          = var.client_hg_seq.valid_redirects
  web_origins                  = var.client_hg_seq.web_origins
  full_scope_allowed           = true
  access_token_lifespan        = var.client_hg_seq.token_lifespan
}

resource "keycloak_openid_client_default_scopes" "hgseq_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgkeycloak_client.id
  default_scopes = [
    "profile",
    "web-origins",
    "roles"
  ]
}
