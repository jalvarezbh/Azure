param functionApp_name string
param appServicePlanName string
param sharedResourceGroup string
param appInsightsName string
param customStorageName string

param localKeyVaultName string
param ResourceGroup string = resourceGroup().location
param sa_name string = 'sa'
param dataverseConfigurationEnvironmentUrl string

param serviceBusName string
param serviceBusQueueName string
param inboundCustomerTableName string
param tokenValidationAuthority string
param tokenValidationAudience string
param userAssignedManagedIdentityName string
param integrationInstanceName string = 'Default'

var serviceBusFullyQualifiedNamespace = '${serviceBusName}.servicebus.windows.net'
var tableEndpoint string = 'https://${customStorageName}.table.${environment().suffixes.storage}/' 
var storageAccountid = '${resourceGroup().id}/providers/Microsoft.Storage/storageAccounts/${storageName}'
var endpointSuffix = ';EndpointSuffix=${environment().suffixes.storage}'
var vaultUrl = 'https://${localKeyVaultName}${environment().suffixes.keyvaultDns}/'
var storageName = '${toLower(sa_name)}${uniqueString(resourceGroup().id)}'
var functionAppBaseUri = 'https://${functionApp_name}.azurewebsites.net/api/'

resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' existing = {
  name: storageName
}

resource appInsights 'Microsoft.Insights/components@2014-04-01' existing = {
  name: appInsightsName
}

resource userAssignedManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-01-31-preview' existing = {
  name: userAssignedManagedIdentityName
}

resource functionApp 'Microsoft.Web/sites@2018-02-01' = {
  name: functionApp_name
  location: ResourceGroup
  kind: 'functionapp'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedManagedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: resourceId(sharedResourceGroup, 'Microsoft.Web/serverFarms', appServicePlanName)
    alwaysOn: false
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageName};AccountKey=${listKeys(storageAccountid,'2015-05-01-preview').key1}${endpointSuffix}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageName};AccountKey=${listKeys(storageAccountid,'2015-05-01-preview').key1}${endpointSuffix}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionApp_name)
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'TableEndpointUrl'
          value: tableEndpoint
        }
        {
          name: 'InboundCustomerTableName'
          value: inboundCustomerTableName
        }
        {
          name: 'ServiceBusFullyQualifiedNamespace'
          value: serviceBusFullyQualifiedNamespace
        }
        {
          name: 'ServiceBusQueueName'
          value: serviceBusQueueName
        }
        { 
          name: 'TokenValidationAuthority'
          value: tokenValidationAuthority
        }
        {
          name: 'TokenValidationAudience'
          value: tokenValidationAudience
        }
        { 
          name: 'KeyVaultUrl'
          value: vaultUrl
        }
        {
          name: 'ManagedServiceIdentity'
          value: userAssignedManagedIdentity.properties.clientId
        }
        {
          name: 'IntegrationInstanceName'
          value: integrationInstanceName
        }
        {
          name: 'DataverseConfigurationEnvironmentUrl'
          value: dataverseConfigurationEnvironmentUrl

        }
      ]
    }
  }
  dependsOn: [
    storageAccount
  ]
}

resource kv 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: localKeyVaultName
}

@description('This is the built-in Key Vault Secret User role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

resource functionAppBaseUriSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'functionAppBaseUri'
  properties: {
    value: functionAppBaseUri
  }
}

// Assign Data Receiver role to Function App
module serviceBusModule 'servicebus-setup-module.bicep' = {
  name: 'serviceBusModule'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    serviceBusNamespaceName: serviceBusName
    serviceBusQueueName: serviceBusQueueName
    functionAppPrincipalId: userAssignedManagedIdentity.properties.principalId
  }
}
