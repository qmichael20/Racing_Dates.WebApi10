targetScope='resourceGroup'

param location string = resourceGroup().location

@description('Nombre del servidor SQL')
param sqlServerName string

@description('Nombre de la base de datos SQL')
param sqlDatabaseName string

@description('Usuario administrador SQL')
param sqlAdminUsername string

@description('Contraseña del administrador SQL')
@secure()
param sqlAdminPassword string

resource sqlServer 'Microsoft.Sql/servers@2024-11-01-preview' = {
  name: sqlServerName
  location: location

  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2024-11-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location

  sku: {
    name: 'GP_S_Gen5_2'
    tier: 'GeneralPurpose'
  }

  properties: {
  collation: 'SQL_Latin1_General_CP1_CI_AS'
  useFreeLimit: true
  zoneRedundant: false
  availabilityZone: 'NoPreference'
  highAvailabilityReplicaCount: 0
  }
}

resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2024-11-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'

  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}
