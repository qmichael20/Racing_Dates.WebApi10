targetScope='subscription'

param resourceGroupName string
param location string

param sqlServerName string = 'racing-db-server-demo'
param sqlDatabaseName string = 'RacingDBDemo'

param dbAdminUsername string

@secure()
param dbAdminPassword string

@secure()
param ghcrPat string

resource newRG 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: resourceGroupName
  location: location
}

module keyvault 'keyvault.bicep' = {
  name: 'deployKeyVault'
  scope: newRG
  params: {
    sqlServerName: sqlServerName
    sqlDatabaseName: sqlDatabaseName
    dbAdminUsername: dbAdminUsername
    dbAdminPassword: dbAdminPassword
  }
}

module database 'database.bicep' = {
  name: 'deployDatabase'
  scope: newRG
  params: {
    sqlServerName: sqlServerName
    sqlDatabaseName: sqlDatabaseName
    sqlAdminUsername: dbAdminUsername
    sqlAdminPassword: dbAdminPassword
  }
}

module containerApp 'containerApp.bicep' = {
  name: 'deployContainerApp'
  scope: newRG
  params: {
    location: location
    ghcrPat: ghcrPat
  }
}


