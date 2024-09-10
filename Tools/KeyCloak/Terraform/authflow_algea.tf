# ###Start LRA HG Auth Flow Configuration Block
resource "keycloak_authentication_flow" "algea_login" {
  realm_id = data.keycloak_realm.hg_realm.id
  alias    = "algea Login"
}

resource "keycloak_authentication_execution" "algea_cookie_execution" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.algea_login.alias
  authenticator     = "auth-cookie"
  requirement       = "ALTERNATIVE"
}

resource "keycloak_authentication_execution" "algea_idp_redirector_execution" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.algea_login.alias
  authenticator     = "identity-provider-redirector"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.algea_cookie_execution
  ]
}

resource "keycloak_authentication_execution_config" "algea_idp_redirector_execution_config" {
  realm_id     = data.keycloak_realm.hg_realm.id
  execution_id = keycloak_authentication_execution.algea_idp_redirector_execution.id
  alias        = "algea-idp-redirector-config"
  config = {
    defaultProvider = "bcsc"
  }
}
resource "keycloak_authentication_execution" "algea_execution1" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.algea_login.alias
  authenticator     = "idp-create-user-if-unique"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.algea_idp_redirector_execution
  ]
}

resource "keycloak_authentication_execution" "algea_execution2" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.algea_login.alias
  authenticator     = "idp-auto-link"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.algea_execution1
  ]
}
# ###End LRA HG Auth Flow Configuration Block