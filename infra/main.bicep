param name string
param secrets array = []

var location = resourceGroup().location
var appsLocation = 'CanadaCentral'
var environmentName = '${name}'
var workspaceName = '${name}-logs'

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

resource containerApp 'Microsoft.Web/containerapps@2021-03-01' = {
  name: 'tile-service'
  kind: 'containerapps'
  location: appsLocation
  properties: {
    kubeEnvironmentId: environment.id
    configuration: {
      secrets: secrets
      registries: []
      ingress: {
        'external':true
        'targetPort':5000
      }
    }
    template: {
      containers: [
        {
          'name':'tile-service-container'
          'image':'soundcharts.azurecr.io/tile-service:main-2022-03-04T21-12-08Z-SHA161dfe42'
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
