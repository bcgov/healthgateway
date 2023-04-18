resource "keycloak_authentication_flow" "first_login" {
  realm_id = data.keycloak_realm.hg_realm.id
  alias    = "First Login"
}

resource "keycloak_authentication_execution" "execution1" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.first_login.alias
  authenticator     = "idp-create-user-if-unique"
  requirement       = "ALTERNATIVE"

}

resource "keycloak_authentication_execution" "execution2" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.first_login.alias
  authenticator     = "idp-auto-link"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.execution1
  ]
}

resource "keycloak_authentication_flow" "pcare_login" {
  realm_id = data.keycloak_realm.hg_realm.id
  alias    = "PCare Login"
}

resource "keycloak_authentication_execution" "pcare_idp_redirector_execution" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.pcare_login.alias
  authenticator     = "identity-provider-redirector"
  requirement       = "REQUIRED"
}

resource "keycloak_authentication_execution_config" "pcare_idp_redirector_execution_config" {
  realm_id     = data.keycloak_realm.hg_realm.id
  execution_id = keycloak_authentication_execution.pcare_idp_redirector_execution.id
  alias        = "pcare-idp-redirector-config"
  config = {
    defaultProvider = "pcare"
  }
}
resource "keycloak_authentication_execution" "pcare_execution1" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.pcare_login.alias
  authenticator     = "idp-create-user-if-unique"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.pcare_idp_redirector_execution
  ]
}

resource "keycloak_authentication_execution" "pcare_execution2" {
  realm_id          = data.keycloak_realm.hg_realm.id
  parent_flow_alias = keycloak_authentication_flow.pcare_login.alias
  authenticator     = "idp-auto-link"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.pcare_execution1
  ]
}