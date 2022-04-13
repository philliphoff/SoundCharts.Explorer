param appsLocation string = 'CanadaCentral'
param environmentName string
param location string = resourceGroup().location
param ingressTag string
param tileServiceTag string
param tilesetServiceTag string

@secure()
param tilesetServiceAccountKey string

@secure()
param tilesetServiceAccountName string

@secure()
param tilesetServiceConnectionString string

var workspaceName = '${environmentName}-logs'

resource workspace 'Microsoft.OperationalInsights/workspaces@2020-08-01' = {
  name: workspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    workspaceCapping: {}
  }
}

resource environment 'Microsoft.App/managedEnvironments@2022-01-01-preview' = {
  name: environmentName
  location: appsLocation
  properties: {
    type: 'managed'
    internalLoadBalancerEnabled: false
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: workspace.properties.customerId
        sharedKey: listKeys(workspace.id, workspace.apiVersion).primarySharedKey
      }
    }
  }
}

resource tileServiceContainerApp 'Microsoft.App/containerApps@2022-01-01-preview' = {
  name: 'tile-service'
  kind: 'containerapps'
  location: appsLocation
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      registries: []
      ingress: {
        'external': false
        'targetPort': 5000
      }
    }
    template: {
      containers: [
        {
          'name':'tile-service-container'
          'image': tileServiceTag
          'command':[]
          'resources':{
            'cpu':'.25'
            'memory':'.5Gi'
          }
        }
      ]
    }
  }
}

resource tilesetServiceContainerApp 'Microsoft.App/containerapps@2022-01-01-preview' = {
  name: 'tileset-service'
  kind: 'containerapps'
  location: appsLocation
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      secrets: [
        {
          name: 'soundcharts-tileset-service-account-key'
          value: tilesetServiceAccountKey
        }
        {
          name: 'soundcharts-tileset-service-account-name'
          value: tilesetServiceAccountName
        }
        {
          name: 'soundcharts-tileset-service-connection-string'
          value: tilesetServiceConnectionString
        }
      ]
      registries: []
      ingress: {
        'external': false
        'targetPort': 5000
      }
    }
    template: {
      containers: [
        {
          'name':'tileset-service-container'
          'image': tilesetServiceTag
          'command':[]
          'resources':{
            'cpu':'.25'
            'memory':'.5Gi'
          }
          'env': [
            {
              'name': 'SOUNDCHARTS_TILESET_SERVICE_CONNECTION_STRING'
              'secretRef': 'soundcharts-tileset-service-connection-string'
            }
            {
              'name': 'SOUNDCHARTS_TILESET_SERVICE_ACCOUNT_NAME'
              'secretRef': 'soundcharts-tileset-service-account-name'
            }
            {
              'name': 'SOUNDCHARTS_TILESET_SERVICE_ACCOUNT_KEY'
              'secretRef': 'soundcharts-tileset-service-account-key'
            }
          ]
        }
      ]
    }
  }
}

resource ingressContainerApp 'Microsoft.App/containerapps@2022-01-01-preview' = {
  name: 'ingress'
  kind: 'containerapps'
  location: appsLocation
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      registries: []
      ingress: {
        'external': true
        'targetPort': 5000
      }
    }
    template: {
      containers: [
        {
          'name':'ingress-container'
          'image': ingressTag
          'command':[]
          'resources':{
            'cpu':'.25'
            'memory':'.5Gi'
          }
          'env': [
            {
              'name':'TILE-SERVICE_SERVICE_ENDPOINT'
              'value':'https://${tileServiceContainerApp.properties.configuration.ingress.fqdn}'
            }
            {
              'name':'TILESET-SERVICE_SERVICE_ENDPOINT'
              'value':'https://${tilesetServiceContainerApp.properties.configuration.ingress.fqdn}'
            }
          ]
        }
      ]
    }
  }
}
