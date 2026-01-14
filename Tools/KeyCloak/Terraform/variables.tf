variable "environment" {
  type = object({
    name     = string
    base_url = string
    realm    = string
  })
  description = "Basic Keycloak environment configuration"
}

variable "keycloak_terraform_client" {
  type = object({
    id     = string
    secret = string
  })
  sensitive   = true
  description = "The keycloak client and secret used by Terraform to create resources"
}

variable "keycloak_idp_idir_client" {
  type = object({
    id     = string
    secret = string
  })
  sensitive   = true
  description = "The keycloak client and secret used for the IDIR IDP"
}

variable "keycloak_idp_azure_idir_client" {
  type = object({
    id     = string
    secret = string
  })
  sensitive   = true
  description = "The keycloak client and secret used for the Azure IDIR IDP"
}

variable "keycloak_idp_bcsc" {
  type = object({
    base_url      = string
    auth_path     = optional(string, "/login/oidc/authorize/")
    token_path    = optional(string, "/oauth2/token")
    userinfo_path = optional(string, "/oauth2/userinfo")
    client_id     = string
    client_secret = string
    issuer_path   = optional(string, "/oauth2/")
    jwks_path     = optional(string, "/oauth2/jwk")
  })
  sensitive   = true
  description = "The configuration settings for the BCSC IDP"
}

variable "keycloak_idp_bcsc_mobile" {
  type = object({
    base_url      = string
    auth_path     = optional(string, "/login/oidc/authorize/")
    token_path    = optional(string, "/oauth2/token")
    userinfo_path = optional(string, "/oauth2/userinfo")
    client_id     = string
    client_secret = string
    issuer_path   = optional(string, "/oauth2/")
    jwks_path     = optional(string, "/oauth2/jwk")
  })
  sensitive   = true
  description = "The configuration settings for the BCSC Mobile IDP"
}

variable "keycloak_idp_bcsc_pcare" {
  type = object({
    base_url      = string
    auth_path     = optional(string, "/login/oidc/authorize/")
    token_path    = optional(string, "/oauth2/token")
    userinfo_path = optional(string, "/oauth2/userinfo")
    client_id     = string
    client_secret = string
    issuer_path   = optional(string, "/oauth2/")
    jwks_path     = optional(string, "/oauth2/jwk")
  })
  sensitive   = true
  description = "The configuration settings for the BCSC Primary Care IDP"
}

variable "keycloak_idp_phsa" {
  type = object({
    base_url      = string
    auth_path     = optional(string, "/oauth2/v2.0/authorize")
    token_path    = optional(string, "/oauth2/v2.0/token")
    client_id     = string
    client_secret = string
    jwks_path     = optional(string, "/discovery/v2.0/keys")
  })
  sensitive   = true
  description = "The configuration settings for the PHSA IDP"
}

variable "client_hg_admin" {
  type = object({
    id              = optional(string, "hg-admin")
    valid_redirects = list(string)
    web_origins     = list(string)
  })
  description = "HealthGateway Admin client configuration"
}

variable "client_hg_admin_services" {
  type = object({
    id             = optional(string, "hg-admin-services")
    token_lifespan = number
  })
  description = "HealthGateway Admin services client configuration"
}

variable "client_hg_k6" {
  type = object({
    id              = optional(string, "hg-k6")
    valid_redirects = list(string)
    web_origins     = list(string)
  })
  description = "HealthGateway K6 client configuration"
}

variable "client_hg_mobile" {
  type = object({
    id              = optional(string, "hg-mobile")
    valid_redirects = list(string)
    web_origins     = list(string)
    token_lifespan  = number
  })
  description = "HealthGateway mobile client configuration"
}

variable "client_hg_mobile_android" {
  type = object({
    id = optional(string, "hg-mobile-android")
  })
  description = "HealthGateway mobile android client configuration"
}

variable "client_hg_mobile_ios" {
  type = object({
    id = optional(string, "hg-mobile-ios")
  })
  description = "HealthGateway mobile ios client configuration"
}

variable "client_pcare" {
  type = object({
    id              = optional(string, "pcare")
    valid_redirects = list(string)
    web_origins     = list(string)
    token_lifespan  = number
  })
  description = "Primary Care client configuration"
}

variable "client_icarus" {
  type = object({
    id                     = optional(string, "icarus")
    valid_redirects        = list(string)
    valid_logout_redirects = list(string)
    web_origins            = list(string)
    token_lifespan         = number
  })
  description = "Health Gateway Salesforce client configuration"
}

variable "client_hg_phsa" {
  type = object({
    id              = optional(string, "hg-phsa")
    valid_redirects = list(string)
    web_origins     = list(string)
  })
  description = "HealthGateway PHSA client configuration"
}

variable "client_erebus" {
  type = object({
    id              = optional(string, "erebus")
    valid_redirects = list(string)
    web_origins     = list(string)
  })
  description = "LRA PHSA client configuration"
}

variable "client_hg" {
  type = object({
    id              = optional(string, "hg")
    valid_redirects = list(string)
    web_origins     = list(string)
  })
  description = "HealthGateway client configuration"
}

variable "client_hg_phsa_public" {
  type = object({
    id              = optional(string, "hg-phsa-public")
    valid_redirects = list(string)
    web_origins     = list(string)
    token_lifespan  = number
  })
  description = "HealthGateway Public Authentication for PHSA"
}

variable "client_hg_phsa_system" {
  type = object({
    id              = optional(string, "hg-phsa-system")
    valid_redirects = list(string)
    web_origins     = list(string)
    token_lifespan  = number
  })
  description = "HealthGateway System Authentication for PHSA"
}

variable "client_hg_keycloak" {
  type = object({
    id              = optional(string, "hg-keycloak")
    valid_redirects = list(string)
    web_origins     = list(string)
    token_lifespan  = number
  })
  description = "HealthGateway Keycloak Administration Client"
}

locals {
  development = var.environment.name == "Development"
  devtest     = var.environment.name == "Development" || var.environment.name == "Test"
}