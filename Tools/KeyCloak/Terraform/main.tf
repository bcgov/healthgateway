terraform {
  required_providers {
    keycloak = {
      source  = "mrparkers/keycloak"
      version = ">= 4.0.0"
    }
  }
}

provider "keycloak" {
  client_id     = var.keycloak_terraform_client.id
  client_secret = var.keycloak_terraform_client.secret
  url           = var.environment.base_url
  realm         = var.environment.realm
}

terraform {
  cloud {
    organization = "BCMoH"
    workspaces {
      tags = ["keycloak"]
    }
  }
}