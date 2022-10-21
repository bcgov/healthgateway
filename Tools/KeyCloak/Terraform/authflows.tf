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