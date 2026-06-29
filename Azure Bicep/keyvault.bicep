targetScope='resourceGroup'

param location string = resourceGroup().location
param dbAdminUsername string

@secure()
param dbAdminPassword string

param sqlServerName string 
param sqlDatabaseName string 


resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'racingApi-demo-kv-two'
  location: location
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    accessPolicies: []
    tenantId: subscription().tenantId
  }
}

resource racingDBAdminUsernameSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: kv
  name: 'dbAdminUsername'
  properties: {
    attributes: {
      enabled: true
      exp: 1782838223
      nbf: 1781499600
    }
    contentType: 'string'
    value: dbAdminUsername
  }
}

resource racingDBAdminPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: kv
  name: 'dbAdminPassword'
  properties: {
    attributes: {
      enabled: true
      exp: 1782838223
      nbf: 1781499600
    }
    contentType: 'string'
    value: dbAdminPassword
  }
}

resource racingDBConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: kv
  name: 'dbConnectionString'
  properties: {
    attributes: {
      enabled: true
      exp: 1782838223
      nbf: 1781499600
    }
    contentType: 'string'
    value: 'Server=tcp:${sqlServerName}.database.windows.net,1433;Initial Catalog=${sqlDatabaseName};User ID=${dbAdminUsername};Password=${dbAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}
