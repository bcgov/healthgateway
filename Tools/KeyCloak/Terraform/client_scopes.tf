resource "keycloak_openid_client_scope" "encounter_read_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "patient/Encounter.read"
  description            = "Provides patient access to Encounter data"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "immunization_read_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "patient/Immunization.read"
  description            = "Provides patient access to Immunization data"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "laboratory_read_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "patient/Laboratory.read"
  description            = "Provides patient access to Laboratory data"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "medication_dispense_read_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "patient/MedicationDispense.read"
  description            = "Provides patient access to Medication Dispense data"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "notification_read_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "patient/Notification.read"
  description            = "Provides patient access to Notification data"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "observation_read_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "patient/Observation.read"
  description            = "Provides patient access to Observation data"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "patient_read_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "patient/Patient.read"
  description            = "Provides patient access to Patient data"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "system_patient_read_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "system/Patient.read"
  description            = "Abilty to read any patient's as a system"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "system_notification_read_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "system/Notification.read"
  description            = "Abilty to read any patient's Notification settings"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "system_notification_write_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "system/Notification.write"
  description            = "Ability to write any patient's Notification settings"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "phsa_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "phsa"
  description            = "Used by kong to remove API limits for PHSA"
  include_in_token_scope = true
}

resource "keycloak_openid_client_scope" "audience_scope" {
  realm_id               = data.keycloak_realm.hg_realm.id
  name                   = "audience"
  description            = "Resolves the audience"
  include_in_token_scope = true
}

resource "keycloak_generic_protocol_mapper" "audience_scope_mapper" {
  realm_id        = data.keycloak_realm.hg_realm.id
  name            = "audience-resolver"
  protocol        = "openid-connect"
  protocol_mapper = "oidc-audience-resolve-mapper"
  client_scope_id = keycloak_openid_client_scope.audience_scope.id
  config = {
  }
}
