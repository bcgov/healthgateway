param (
  [Parameter(Mandatory)]
  [string]$SecretsFile,
  
  [Parameter(Mandatory)]
  [string]$UsersFile,

  [Parameter()]
  [switch]$Remove,

  [Parameter()]
  [string]$Password
)

if([string]::IsNullOrWhiteSpace($Password))
{
   $Password = $null
}

function getKeyCloakConfig
{
  param (
    [Parameter(Mandatory)]
    [string] $secretFile
  )
  $rawProperties=cat $secretFile
  $propertiesToConvert=($rawProperties -replace '"','') -join [Environment]::NewLine
  $properties=ConvertFrom-StringData $propertiesToConvert
  $keycloakConfig = @{
    BaseUrl = $properties.keycloak_base_url
    Realm = $properties.keycloak_realm
    Client = $properties.keycloak_terraform_client_id
    Secret = $properties.keycloak_terraform_client_secret
  }
  return $keycloakConfig
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

function GetUser
{
  param (
    [Parameter(Mandatory)]
    $keycloakConfig,
    [Parameter(Mandatory)]
    [string] $accessToken,
    [Parameter(Mandatory)]
    [string] $username
  )
  $headers = @{
    Authorization = "Bearer $accessToken"
  }
  $uri = $keycloakConfig.BaseUrl + "/admin/realms/" + $keycloakConfig.Realm + "/users?briefRepresentation=true&username=$($username)&exact=true"
  Invoke-RestMethod -Method GET -Uri $uri -Headers $headers
}

function CreateUser
{
  param (
    [Parameter(Mandatory)]
    $keycloakConfig,
    [Parameter(Mandatory)]
    [string] $accessToken,
    [Parameter(Mandatory)]
    $user
  )
  $headers = @{
    Authorization = "Bearer $accessToken"
  }
  $uri = $keycloakConfig.BaseUrl + "/admin/realms/" + $keycloakConfig.Realm + "/users"
  Invoke-RestMethod -Method POST -Uri $uri -Headers $headers -Body $user -ContentType "application/json" | ConvertFrom-Json
}

function RemoveUser
{
  param (
    [Parameter(Mandatory)]
    $keycloakConfig,
    [Parameter(Mandatory)]
    [string] $accessToken,
    [Parameter(Mandatory)]
    $user
  )
  $headers = @{
    Authorization = "Bearer $accessToken"
  }
  $uri = $keycloakConfig.BaseUrl + "/admin/realms/" + $keycloakConfig.Realm + "/users/$($user.id)"
  Invoke-RestMethod -Method DELETE -Uri $uri -Headers $headers
}

function GetRole
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
  $uri = $keycloakConfig.BaseUrl + "/admin/realms/" + $keycloakConfig.Realm + "/roles/" + $roleName
  Invoke-RestMethod -Method GET -Uri $uri -Headers $headers
}

function AddRole
{
  param (
    [Parameter(Mandatory)]
    $keycloakConfig,
    [Parameter(Mandatory)]
    [string] $accessToken,
    [Parameter(Mandatory)]
    $role,
    [Parameter(Mandatory)]
    [guid] $userid
  )
  $headers = @{
    Authorization = "Bearer $accessToken"
  }
  $id=$role.id
  $name=$role.name
  $body = @"
  [{"id":"$id","name":"$name"}]
"@
  $uri = $keycloakConfig.BaseUrl + "/admin/realms/" + $keycloakConfig.Realm + "/users/$userid/role-mappings/realm"
  Invoke-RestMethod -Method POST -Uri $uri -Headers $headers -Body $body -ContentType "application/json"
}

function main
{
  param (
    [Parameter(Mandatory)]
    [string]$secretsFile,
    
    [Parameter(Mandatory)]
    [string]$usersFile,

    [Parameter(Mandatory)]
    [bool]$remove,

    [Parameter()]
    [string]$password
  )
  Write-Host "Loading secrets from file at $($secretsFile)"
  $keycloakConfig = getKeyCloakConfig($secretsFile)

  Write-Host "Loading users from file at $($usersFile)"
  $loadData = Get-Content -Path $usersFile | ConvertFrom-Json

  $token = GetAccessToken($keycloakConfig)
  $keycloakRoles = @{}
  foreach($user in $loadData.users)
  {
    if (!$Remove)
    {
      Write-Host "Creating user $($user.username)"
      if ($null -ne $user.credentials -AND $null -ne $password)
      {
        Write-Host "Overriding password for $($user.username)"
        $user.credentials[0].value = $password
      }
      CreateUser $keycloakConfig $token.access_token ($user | ConvertTo-Json)
      $keycloakUser = Getuser $keycloakConfig $token.access_token $user.username
      $roleMappings = $loadData.roleMappings.($user.UserName)
      if ( $null -ne $roleMappings)
      {
        Write-Host "Found role mappings for $($keycloakUser.Username)"
        foreach($role in $roleMappings)
        {
          if ($null -eq $keycloakRoles[$role])
          {
            Write-Host "Role $($role) not found looking up in keycloak"
            $keycloakRole = GetRole $keycloakConfig $token.access_token $role
            $keycloakRoles.Add($role, $keycloakRole)
          }
          Write-Host "Adding $($role) to $($keycloakUser.username)"
          AddRole $keycloakConfig $token.access_token $keycloakRoles[$role] $keycloakUser.id
        }
      }
    }
    else
    {
      Write-Host "Searching for user $($user.username)"
      $keycloakUser = Getuser $keycloakConfig $token.access_token $user.userName
      if ($null -ne $keycloakUser)
      {
        Write-Host "Removing user $($user.username)"
        RemoveUser $keycloakConfig $token.access_token $keycloakUser
      }
      else
      {
        Write-Host "User $($user.username) was not found and was skipped..."
      }
    }
  }
}

$ErrorActionPreference = "Stop"
main $SecretsFile $UsersFile $Remove $Password