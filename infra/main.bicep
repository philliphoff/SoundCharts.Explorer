param appsLocation string = 'CanadaCentral'
param environmentName string
param location string = resourceGroup().location
param secrets array = []
param ingressTag string
param tileServiceTag string
param tilesetServiceTag string

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

resource environment 'Microsoft.Web/kubeEnvironments@2021-03-01' = {
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

resource tileServiceContainerApp 'Microsoft.Web/containerapps@2021-03-01' = {
  name: 'tile-service'
  kind: 'containerapps'
  location: appsLocation
  properties: {
    kubeEnvironmentId: environment.id
    configuration: {
      secrets: secrets
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

resource tilesetServiceContainerApp 'Microsoft.Web/containerapps@2021-03-01' = {
  name: 'tileset-service'
  kind: 'containerapps'
  location: appsLocation
  properties: {
    kubeEnvironmentId: environment.id
    configuration: {
      secrets: secrets
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
        }
      ]
    }
  }
}

resource ingressContainerApp 'Microsoft.Web/containerapps@2021-03-01' = {
  name: 'ingress'
  kind: 'containerapps'
  location: appsLocation
  properties: {
    kubeEnvironmentId: environment.id
    configuration: {
      secrets: secrets
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
