resource "keycloak_role" "AdminReviewer" {
  realm_id    = data.keycloak_realm.hg_realm.id
  name        = "AdminReviewer"
  description = "Permits the user entry to the Admin site but only allows them to access the feedback module and the main dashboard."
}

resource "keycloak_role" "AdminUser" {
  realm_id    = data.keycloak_realm.hg_realm.id
  name        = "AdminUser"
  description = "Administrator of the Health Gateway Web Application. Grants access to the admin dashboard and related functions."
}

resource "keycloak_role" "SupportUser" {
  realm_id    = data.keycloak_realm.hg_realm.id
  name        = "SupportUser"
  description = "Provides ServiceDesk access to the Covid Card Print/Mail functionality"
}

resource "keycloak_role" "System" {
  realm_id    = data.keycloak_realm.hg_realm.id
  name        = "System"
  description = "Role used to provide PHSA System access"
}

resource "keycloak_role" "Anonymous" {
  realm_id    = data.keycloak_realm.hg_realm.id
  name        = "Anonymous"
  description = "Role used for PHSA public access"
}

data "keycloak_role" "Uma_authorization" {
  realm_id = data.keycloak_realm.hg_realm.id
  name     = "uma_authorization"
}
data "keycloak_role" "realm_admin" {
  realm_id = data.keycloak_realm.hg_realm.id
  name     = "realm-admin"
}

data "keycloak_role" "realm_viewer" {
  realm_id = data.keycloak_realm.hg_realm.id
  name     = "realm-viewer"
}

data "keycloak_role" "default_hg" {
  realm_id = data.keycloak_realm.hg_realm.id
  name     = "default-roles-health-gateway-gold"
}