resource "keycloak_authentication_flow" "iapyx_login" {
  count    = local.devtest ? 1 : 0
  realm_id = data.keycloak_realm.hg_realm.id
  alias    = "iapyx Login"
}

resource "keycloak_authentication_execution" "iapyx_cookie_execution" {
  count             = local.devtest ? 1 : 0
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = local.devtest ? keycloak_authentication_flow.iapyx_login[0].alias : null
  authenticator     = "auth-cookie"
  requirement       = "ALTERNATIVE"
}

resource "keycloak_authentication_execution" "iapyx_idp_redirector_execution" {
  count             = local.devtest ? 1 : 0
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = local.devtest ? keycloak_authentication_flow.iapyx_login[0].alias : null
  authenticator     = "identity-provider-redirector"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.iapyx_cookie_execution
  ]
}

resource "keycloak_authentication_execution_config" "iapyx_idp_redirector_execution_config" {
  count        = local.devtest ? 1 : 0
  realm_id     = data.keycloak_realm.hg_realm.id
  execution_id = local.devtest ? keycloak_authentication_execution.iapyx_idp_redirector_execution[0].id : null
  alias        = "iapyx-idp-redirector-config"
  config = {
    defaultProvider = "bcsc"
  }
}
resource "keycloak_authentication_execution" "iapyx_execution1" {
  count             = local.devtest ? 1 : 0
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = local.devtest ? keycloak_authentication_flow.iapyx_login[0].alias : null
  authenticator     = "idp-create-user-if-unique"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.iapyx_idp_redirector_execution
  ]
}

resource "keycloak_authentication_execution" "iapyx_execution2" {
  count             = local.devtest ? 1 : 0
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.iapyx_login[0].alias
  authenticator     = "idp-auto-link"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.iapyx_execution1
  ]
}
