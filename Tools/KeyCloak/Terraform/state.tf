terraform {
  cloud {
    organization = "BCMoH"

    workspaces {
      tags = ["keycloak"]
    }
  }
}