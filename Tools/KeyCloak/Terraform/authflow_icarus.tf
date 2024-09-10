# ###Start SF HG Auth Flow Configuration Block
resource "keycloak_authentication_flow" "icarus_login" {
  realm_id = data.keycloak_realm.hg_realm.id
  alias    = "icarus Login"
}

resource "keycloak_authentication_execution" "icarus_cookie_execution" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.icarus_login.alias
  authenticator     = "auth-cookie"
  requirement       = "ALTERNATIVE"
}

resource "keycloak_authentication_execution" "icarus_idp_redirector_execution" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.icarus_login.alias
  authenticator     = "identity-provider-redirector"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.icarus_cookie_execution
  ]
}

resource "keycloak_authentication_execution_config" "icarus_idp_redirector_execution_config" {
  realm_id     = data.keycloak_realm.hg_realm.id
  execution_id = keycloak_authentication_execution.icarus_idp_redirector_execution.id
  alias        = "icarus-idp-redirector-config"
  config = {
    defaultProvider = "bcsc"
  }
}
resource "keycloak_authentication_execution" "icarus_execution1" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.icarus_login.alias
  authenticator     = "idp-create-user-if-unique"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.icarus_idp_redirector_execution
  ]
}

resource "keycloak_authentication_execution" "icarus_execution2" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.icarus_login.alias
  authenticator     = "idp-auto-link"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.icarus_execution1
  ]
}
# ###End SF HG Auth Flow Configuration Block