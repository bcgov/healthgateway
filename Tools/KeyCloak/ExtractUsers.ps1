param (
  [Parameter(Mandatory)]
  [string]$SecretsFile,

  [Parameter()]
  [string]$Role
)

function getKeyCloakConfig
{
  param (
    [Parameter(Mandatory)]
    [string] $secretFile
  )
  $configuration = Get-Content -Path $secretFile | ConvertFrom-Json
  return $configuration
}

function GetAccessToken
{
  param (
    [Parameter(Mandatory)]
    $keycloakConfig
  )
  $tokenUri = $keycloakConfig.BaseUrl + "/realms/" + $keycloakConfig.Realm + "/protocol/openid-connect/token"
  $body = @{grant_type="client_credentials"
          client_id=$keycloakConfig.Client
          client_secret=$keycloakConfig.Secret}
  $contentType = 'application/x-www-form-urlencoded'
  Invoke-WebRequest -Method POST -Uri $tokenUri -body $body -ContentType $contentType | ConvertFrom-Json
}

function GetUsersInRole
{
  param (
    [Parameter(Mandatory)]
    $keycloakConfig,
    [Parameter(Mandatory)]
    [string] $accessToken,
    [Parameter(Mandatory)]
    [string] $roleName
  )
  $headers = @{
    Authorization = "Bearer $accessToken"
  }
  $uri = $keycloakConfig.BaseUrl + "/admin/realms/" + $keycloakConfig.Realm + "/roles/" + $roleName + "/users?first=0&max=5000"
  Invoke-RestMethod -Method GET -Uri $uri -Headers $headers
}

function main
{
  param (
    [Parameter(Mandatory)]
    [string]$secretsFile,
  
    [Parameter()]
    [string]$role
  )
  $keycloakConfig = getKeyCloakConfig($secretsFile)

  $roles = @( $role )
  $token = GetAccessToken($keycloakConfig)
  $users = GetUsersInRole $keycloakConfig $token.access_token $role
  $roleMappings = @{}
  $refined = foreach($user in $users)
  {
    $roleMappings.Add($user.username, $roles)
    @{
      username = $user.username
      enabled = $user.enabled
    }
  }
  $output = @{
    users = $refined
    roleMappings = $roleMappings
  }
  $output | ConvertTo-Json

}

$ErrorActionPreference = "Stop"
main $SecretsFile $Role