data "keycloak_realm" "hg_realm" {
  realm = var.environment.realm
}