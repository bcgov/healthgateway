resource "keycloak_role" "AdminReviewer" {
  realm_id    = var.keycloak_realm
  name        = "AdminReviewer"
  description = "Permits the user entry to the Admin site but only allows them to access the feedback module and the main dashboard."
}

resource "keycloak_role" "AdminUser" {
  realm_id    = var.keycloak_realm
  name        = "AdminUser"
  description = "Administrator of the Health Gateway Web Application. Grants access to the admin dashboard and related functions."
}

resource "keycloak_role" "SupportUser" {
  realm_id    = var.keycloak_realm
  name        = "SupportUser"
  description = "Provides ServiceDesk access to the Covid Card Print/Mail functionality"
}

resource "keycloak_role" "System" {
  realm_id    = var.keycloak_realm
  name        = "System"
  description = "Role used to provide PHSA System access"
}

resource "keycloak_role" "Anonymous" {
  realm_id    = var.keycloak_realm
  name        = "Anonymous"
  description = "Role used for PHSA public access"
}

data "keycloak_role" "Uma_authorization" {
  realm_id = var.keycloak_realm
  name     = "uma_authorization"
}
data "keycloak_role" "realm_admin" {
  realm_id = var.keycloak_realm
  name     = "realm-admin"
}

data "keycloak_role" "realm_viewer" {
  realm_id = var.keycloak_realm
  name     = "realm-viewer"
}

data "keycloak_role" "default_hg" {
  realm_id = var.keycloak_realm
  name     = "default-roles-health-gateway-gold"
}