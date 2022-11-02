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
    jwks_path     = optional(string, "/oauth2/jwk.json")
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
    jwks_path     = optional(string, "/oauth2/jwk.json")
  })
  sensitive   = true
  description = "The configuration settings for the BCSC Mobile IDP"
}

variable "keycloak_idp_phsa" {
  type = object({
    base_url      = string
    auth_path     = optional(string, "/oauth2/v2.0/authorize")
    token_path    = optional(string, " /oauth2/v2.0/token")
    client_id     = string
    client_secret = string
    jwks_path     = optional(string, "/discovery/v2.0/keys")
  })
  sensitive   = true
  description = "The configuration settings for the PHSA IDP"
}

variable "client_hg_admin_blazor" {
  type = object({
    id              = optional(string, "hg-admin-blazor")
    valid_redirects = list(string)
    web_origins     = list(string)
  })
  description = "HealthGateway Admin Blazor client configuration"
}

variable "client_hg_admin" {
  type = object({
    id              = optional(string, "hg-admin")
    valid_redirects = list(string)
    web_origins     = list(string)
  })
  description = "HealthGateway Admin client configuration"
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
  })
  description = "HealthGateway mobile client configuration"
}

variable "client_hg_phsa" {
  type = object({
    id              = optional(string, "hg-phsa")
    valid_redirects = list(string)
    web_origins     = list(string)
  })
  description = "HealthGateway PHSA client configuration"
}
variable "client_hg" {
  type = object({
    id              = optional(string, "hg")
    valid_redirects = list(string)
    web_origins     = list(string)
  })
  description = "HealthGateway client configuration"
}

locals {
  development = var.environment.name == "Development"
}