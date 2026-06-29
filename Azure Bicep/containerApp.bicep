
param location string = resourceGroup().location

@secure()
param ghcrPat string

resource containerEnvironment 'Microsoft.App/managedEnvironments@2025-01-01' = {
  name: 'racingApi-demo-container-env'
  location: location
  properties: {
    appLogsConfiguration: {
    }
  }
}

resource containerApp 'Microsoft.App/containerApps@2025-01-01' = {
  name: 'racing-api-demo-container-app'
  location: location
  properties: {
    managedEnvironmentId: containerEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
      secrets: [
        {
          name: 'ghcr-pat'
          value: ghcrPat
        }
      ]
      registries: [
        {
          server: 'ghcr.io'
          username: 'juanpsd1124'
          passwordSecretRef: 'ghcr-pat'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'racing-api-container-demo'
          image: 'ghcr.io/juanpsd1124/racing-api-demo:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}
