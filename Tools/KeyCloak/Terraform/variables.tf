variable "environment" {
  type        = string
  description = "The environment name"
}
variable "keycloak_terraform_client_id" {
  type        = string
  description = "The client_id for the Keycloak client in Master Realm"
}

variable "keycloak_terraform_client_secret" {
  type        = string
  description = "The client_secret for the Keycloak client"
  sensitive   = true
}

variable "keycloak_base_url" {
  type        = string
  description = "The base url for Keycloak"
}

variable "keycloak_realm" {
  type        = string
  description = "The keycloak realm"
}

variable "keycloak_idp_idir_client_id" {
  type        = string
  description = "The IDIR IDP client id"
}

variable "keycloak_idp_idir_client_secret" {
  type        = string
  description = "The IDIR IDP client secret"
  sensitive   = true
}

variable "keycloak_idp_azure_idir_client_id" {
  type        = string
  description = "The Azure IDIR IDP client id"
}

variable "keycloak_idp_azure_idir_client_secret" {
  type        = string
  description = "The Azure IDIR IDP client secret"
  sensitive   = true
}

variable "keycloak_idp_bcsc_base_url" {
  type        = string
  description = "The BC Services Card IDP base URL"
}

variable "keycloak_idp_bcsc_auth_path" {
  type        = string
  description = "The BC Services Card IDP Authorization URL"
  default     = "/login/oidc/authorize/"
}

variable "keycloak_idp_bcsc_token_path" {
  type        = string
  description = "The BC Services Card IDP Token URL"
  default     = "/oauth2/token"
}

variable "keycloak_idp_bcsc_userinfo_path" {
  type        = string
  description = "The BC Services Card IDP UserInfo URL"
  default     = "/oauth2/userinfo"
}

variable "keycloak_idp_bcsc_client_id" {
  type        = string
  description = "The BC Services card IDP client id"
}

variable "keycloak_idp_bcsc_client_secret" {
  type        = string
  description = "The BC Services Card IDP client secret"
  sensitive   = true
}

variable "keycloak_idp_bcsc_issuer_path" {
  type        = string
  description = "The BC Services Card IDP client Issuer URL"
  default     = "/oauth2/"
}

variable "keycloak_idp_bcsc_jwks_path" {
  type        = string
  description = "The BC Services Card IDP client JWKS URL"
  default     = "/oauth2/jwk.json"
}

variable "keycloak_idp_bcsc_mobile_base_url" {
  type        = string
  description = "The BC Services Card IDP base URL"
}

variable "keycloak_idp_bcsc_mobile_auth_path" {
  type        = string
  description = "The BC Services Card IDP Authorization URL"
  default     = "/login/oidc/authorize/"
}

variable "keycloak_idp_bcsc_mobile_token_path" {
  type        = string
  description = "The BC Services Card IDP Token URL"
  default     = "/oauth2/token"
}

variable "keycloak_idp_bcsc_mobile_userinfo_path" {
  type        = string
  description = "The BC Services Card IDP UserInfo URL"
  default     = "/oauth2/userinfo"
}

variable "keycloak_idp_bcsc_mobile_client_id" {
  type        = string
  description = "The BC Services card IDP client id"
}

variable "keycloak_idp_bcsc_mobile_client_secret" {
  type        = string
  description = "The BC Services Card IDP client secret"
  sensitive   = true
}

variable "keycloak_idp_bcsc_mobile_issuer_path" {
  type        = string
  description = "The BC Services Card IDP client Issuer URL"
  default     = "/oauth2/"
}

variable "keycloak_idp_bcsc_mobile_jwks_path" {
  type        = string
  description = "The BC Services Card IDP client JWKS URL"
  default     = "/oauth2/jwk.json"
}

variable "keycloak_idp_phsa_base_url" {
  type        = string
  description = "The PHSA Azure IDP base URL"
}

variable "keycloak_idp_phsa_auth_path" {
  type        = string
  description = "The PHSA Azure IDP Authorization URL"
  default     = "/oauth2/v2.0/authorize"
}

variable "keycloak_idp_phsa_token_path" {
  type        = string
  description = "The PHSA Azure IDP Token URL"
  default     = "/oauth2/v2.0/token"
}

variable "keycloak_idp_phsa_client_id" {
  type        = string
  description = "The PHSA Azure IDP client id"
}

variable "keycloak_idp_phsa_client_secret" {
  type        = string
  description = "The PHSA Azure IDP client secret"
  sensitive   = true
}

variable "keycloak_idp_phsa_jwks_path" {
  type        = string
  description = "The PHSA Azure IDP client JWKS URL"
  default     = "/discovery/v2.0/keys"
}

variable "client_hg_admin_blazor_valid_redirects" {
  type        = list(string)
  description = "The list of valid redirect for the HealthGateway Admin Blazor client"
}

variable "client_hg_admin_blazor_web_origins" {
  type        = list(string)
  description = "The list of valid web origins for the HealthGateway Admin Blazor client"
}

variable "client_hg_admin_valid_redirects" {
  type        = list(string)
  description = "The list of valid redirect for the HealthGateway Admin client"
}

variable "client_hg_admin_web_origins" {
  type        = list(string)
  description = "The list of valid web origins for the HealthGateway Admin client"
}

variable "client_hg_valid_redirects" {
  type        = list(string)
  description = "The list of valid redirect for the HealthGateway client"
}

variable "client_hg_web_origins" {
  type        = list(string)
  description = "The list of valid web origins for the HealthGateway client"
}

variable "client_hg_mobile_valid_redirects" {
  type        = list(string)
  description = "The list of valid redirect for the HealthGateway mobile client"
}

variable "client_hg_mobile_web_origins" {
  type        = list(string)
  description = "The list of valid web origins for the HealthGateway mobile client"
}

variable "client_hg_phsa_valid_redirects" {
  type        = list(string)
  description = "The list of valid redirect for the HealthGateway PHSA client"
}

variable "client_hg_phsa_web_origins" {
  type        = list(string)
  description = "The list of valid web origins for the HealthGateway PHSA client"
}

variable "client_hg_k6_valid_redirects" {
  type        = list(string)
  description = "The list of valid redirect for the HealthGateway K6 client"
}

variable "client_hg_k6_web_origins" {
  type        = list(string)
  description = "The list of valid web origins for the HealthGateway K6 client"
}

locals {
  development = var.environment == "Development"
}