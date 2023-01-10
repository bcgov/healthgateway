resource "keycloak_openid_client" "hgtickets_client" {
  realm_id                     = data.keycloak_realm.hg_realm.id
  client_id                    = var.client_hg_tickets.id
  name                         = "Health Gateway Tickets - ${var.environment.name}"
  description                  = "Health Gateway Ticket System for Web Queue"
  enabled                      = true
  access_type                  = "CONFIDENTIAL"
  login_theme                  = "bcgov-no-brand"
  standard_flow_enabled        = false
  direct_access_grants_enabled = false
  service_accounts_enabled     = true
  full_scope_allowed           = false
  access_token_lifespan        = var.client_hg_tickets.token_lifespan
}

resource "keycloak_openid_client_default_scopes" "hgtickets_client_default_scopes" {
  realm_id  = data.keycloak_realm.hg_realm.id
  client_id = keycloak_openid_client.hgtickets_client.id
  default_scopes = [
    "web-origins",
    keycloak_openid_client_scope.audience_scope.name,
  ]
}

resource "keycloak_openid_audience_protocol_mapper" "hgtickets_audience" {
  realm_id                 = data.keycloak_realm.hg_realm.id
  client_id                = keycloak_openid_client.hgtickets_client.id
  name                     = "audience"
  included_client_audience = keycloak_openid_client.hgtickets_client.client_id
  add_to_id_token          = false
  add_to_access_token      = true
}
