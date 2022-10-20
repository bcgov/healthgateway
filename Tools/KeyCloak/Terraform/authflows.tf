resource "keycloak_authentication_flow" "first_login" {
  realm_id = var.keycloak_realm
  alias    = "First Login"
}

resource "keycloak_authentication_execution" "execution1" {
  realm_id          = var.keycloak_realm
  parent_flow_alias = keycloak_authentication_flow.first_login.alias
  authenticator     = "idp-create-user-if-unique"
  requirement       = "ALTERNATIVE"

}

resource "keycloak_authentication_execution" "execution2" {
  realm_id          = var.keycloak_realm
  parent_flow_alias = keycloak_authentication_flow.first_login.alias
  authenticator     = "idp-auto-link"
  requirement       = "ALTERNATIVE"

  depends_on = [
    keycloak_authentication_execution.execution1
  ]
}