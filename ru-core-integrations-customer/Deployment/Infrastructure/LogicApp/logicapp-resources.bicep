@description('The datacenter to use for the deployment.')
param location string
param environmentName string
param projectName string
param appInsightsName string
param logicAppName string
param appServicePlanName string
param sharedResourceGroup string
param localKeyVaultName string
param dataverseObjectId string
param apimManagedIdentityId string
param tokenValidationAudience string
param userAssignedManagedIdentityName string

var vaultUri = 'https://${localKeyVaultName}.vault.azure.net/'

// storage account parameters
param sa_name string = 'sa'
param customStorageName string
param storageAccountType string = 'Standard_LRS'

// key vault secrets (to create in keyvault)
param cpiCreateCustomerUrl string
param cpiUpdateCustomerUrl string
param cpiCreateCustomerClientId string
param cpiAccessTokenUrl string
@secure()
param cpiCreateCustomerClientSecret string

// generic variables
var ResourceGroup = resourceGroup().location
var customStorageAccountid = '${resourceGroup().id}/providers/Microsoft.Storage/storageAccounts/${customStorageName}'
var endpointSuffix = ';EndpointSuffix=core.windows.net'

// set up storage account, default used by logic and function app
var storageName = concat(toLower(sa_name), uniqueString(resourceGroup().id))

resource storageAccount 'Microsoft.Storage/storageAccounts@2018-07-01' existing = {
  name: storageName
  scope: resourceGroup(sharedResourceGroup)
}

resource userAssignedManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-01-31-preview' existing = {
  name: userAssignedManagedIdentityName
}

// set up host for logic app runtime
resource logicApp 'Microsoft.Web/sites@2022-09-01' = {
  name: logicAppName
  location: location
  kind: 'functionapp,workflowapp'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedManagedIdentity.id}': {}
    }
  }
  tags: {
    Environment: environmentName
    Project: projectName
  }
  properties: {
    serverFarmId: resourceId(sharedResourceGroup, 'Microsoft.Web/serverFarms', appServicePlanName)
    clientAffinityEnabled: false
    vnetRouteAllEnabled: true
    vnetImagePullEnabled: false
    vnetContentShareEnabled: false
    keyVaultReferenceIdentity: userAssignedManagedIdentity.id
  } 
  dependsOn: [
    storageAccount
  ]
}

resource appSettings 'Microsoft.Web/sites/config@2022-09-01' = {
  name: 'appsettings'
  kind: 'string'
  parent: logicApp
  properties: {
    netFrameworkVersion: 'v6.0'
    WORKFLOWS_RESOURCE_GROUP_NAME: resourceGroup().name
    APP_KIND: 'workflowApp'    
    AzureFunctionsJobHost__extensionBundle__id: 'Microsoft.Azure.Functions.ExtensionBundle.Workflows'
    AzureFunctionsJobHost__extensionBundle__version: '[1.*, 2.0.0)'
    AzureWebJobsStorage: 'DefaultEndpointsProtocol=https;AccountName=${storageName};AccountKey=${listKeys('${resourceGroup().id}/providers/Microsoft.Storage/storageAccounts/${storageName}','2019-06-01').keys[0].value};EndpointSuffix=core.windows.net'
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_V2_COMPATIBILITY_MODE: 'true'
    FUNCTIONS_WORKER_RUNTIME: 'node'
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: 'DefaultEndpointsProtocol=https;AccountName=${storageName};AccountKey=${listKeys('${resourceGroup().id}/providers/Microsoft.Storage/storageAccounts/${storageName}','2019-06-01').keys[0].value};EndpointSuffix=core.windows.net'
    WEBSITE_CONTENTSHARE: logicAppName
    WEBSITE_NODE_DEFAULT_VERSION: '~19'
    WORKFLOWS_SUBSCRIPTION_ID: subscription().subscriptionId
    WORKFLOWS_LOCATION_NAME: location
    BLOB_CONNECTION_RUNTIMEURL: ''
    keyVault_VaultUri: 'https://${localKeyVaultName}.vault.azure.net/'
    StorageAccountBlobEndpointURI: 'https://${customStorageName}.blob.core.windows.net/'
    StorageAccountTableEndpointURI: 'https://${customStorageName}.table.core.windows.net/'
    APPINSIGHTS_INSTRUMENTATIONKEY: appInsights.properties.InstrumentationKey
    APPLICATIONINSIGHTS_CONNECTION_STRING: 'InstrumentationKey=${appInsights.properties.InstrumentationKey};IngestionEndpoint=https://northeurope-0.in.applicationinsights.azure.com/;LiveEndpoint=https://northeurope.livediagnostics.monitor.azure.com/;ApplicationId=${appInsights.properties.ApplicationId}'
    CpiCreateCustomerClientId: '@Microsoft.KeyVault(SecretUri=${vaultUri}secrets/CpiCreateCustomerClientId/)'
    CpiCreateCustomerClientSecret: '@Microsoft.KeyVault(SecretUri=${vaultUri}secrets/CpiCreateCustomerClientSecret/)'
    CpiCreateCustomerUrl: '@Microsoft.KeyVault(SecretUri=${vaultUri}secrets/CpiCreateCustomerUrl/)'
    CpiUpdateCustomerUrl: '@Microsoft.KeyVault(SecretUri=${vaultUri}secrets/CpiUpdateCustomerUrl/)'
    CpiAccessTokenUrl: '@Microsoft.KeyVault(SecretUri=${vaultUri}secrets/CpiAccessTokenUrl/)'
    CustomStorageTableConnectionString: '@Microsoft.KeyVault(SecretUri=${vaultUri}secrets/CustomStorageTableConnectionString/)'
    FunctionAppBaseUrl: '@Microsoft.KeyVault(SecretUri=${vaultUri}secrets/functionAppBaseUri/)'
    FunctionAppKey: '@Microsoft.KeyVault(SecretUri=${vaultUri}secrets/functionAppKey/)'
    TokenValidationAudience: tokenValidationAudience
    CustomTableStorageUrl: 'https://${customStorageName}.table.core.windows.net'
    ManagedServiceIdentity: userAssignedManagedIdentity.id
  }
}

// table storage setup (custom)
resource storageAccountCustom 'Microsoft.Storage/storageAccounts@2018-07-01' = {
  name: customStorageName
  location: ResourceGroup
  kind: 'StorageV2'
  sku: {
    name: storageAccountType
  }
  properties: {}
}

// create tables
resource storageaccountCustom_tableService 'Microsoft.Storage/storageAccounts/tableServices@2021-08-01' = {
  name: 'default'  
  parent: storageAccountCustom  
}

resource storageaccountCustom_tableService_HashesTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-08-01' = {
  name: 'Hashes'  
  parent: storageaccountCustom_tableService
}

resource storageaccountCustom_tableService_table_product_transaction_log 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-08-01' = {
  name: 'InboundAccountsTransactionLog'  
  parent: storageaccountCustom_tableService
}

resource storageaccount_tableService_table_inbound_accounts_queue 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-08-01' = {
  name: 'InboundAccountsQueue'  
  parent: storageaccountCustom_tableService
}

resource storageaccount_tableService_table_outbound_accounts_queue 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-08-01' = {
  name: 'OutboundAccountsQueue'
  parent: storageaccountCustom_tableService
}

// Define the blob service within the storage account
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  name: 'default'
  parent: storageAccountCustom
}

// Define the blob container within the storage account
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: 'chainhierarchy'
  parent: blobService
  properties: {
    publicAccess: 'None'
  }
}

// app insights
resource appInsights 'Microsoft.Insights/components@2014-04-01' existing = {
  name: appInsightsName
}

// key vault and secrets
resource kv 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: localKeyVaultName 
}

resource customStorageTableConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'CustomStorageTableConnectionString'
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${customStorageName};AccountKey=${listKeys(customStorageAccountid,'2015-05-01-preview').key1}${endpointSuffix}'
  }
}

resource cpiCreateCustomerClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'CpiCreateCustomerClientId'
  properties: {
    value: cpiCreateCustomerClientId
  }
}

resource cpiCreateCustomerClientSecretSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'CpiCreateCustomerClientSecret'
  properties: {
    value: cpiCreateCustomerClientSecret
  }
}

resource cpiCreateCustomerUrlSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'CpiCreateCustomerUrl'
  properties: {
    value: cpiCreateCustomerUrl
  }
}

resource cpiUpdateCustomerUrlSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'CpiUpdateCustomerUrl'
  properties: {
    value: cpiUpdateCustomerUrl
  }
}

resource cpiAccessTokenUrlSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'CpiAccessTokenUrl'
  properties: {
    value: cpiAccessTokenUrl
  }
}

// key vault rbac access setup
@description('This is the built-in Key Vault Secret User role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

// grant access to dataverse
resource keyVaultSecretUserRoleAssignmentDataverse 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  scope: kv
  name: guid(resourceGroup().id, dataverseObjectId, keyVaultSecretUserRoleRoleDefinition.id)
  properties: {
    roleDefinitionId: keyVaultSecretUserRoleRoleDefinition.id
    principalId: dataverseObjectId
    principalType: 'ServicePrincipal'
  }
}

// grant access to apim
resource keyVaultSecretUserRoleAssignmentApimManagedIdentity 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  scope: kv
  name: guid(resourceGroup().id, apimManagedIdentityId, keyVaultSecretUserRoleRoleDefinition.id)
  properties: {
    roleDefinitionId: keyVaultSecretUserRoleRoleDefinition.id
    principalId: apimManagedIdentityId
    principalType: 'ServicePrincipal'
  }
}

output logicAppSystemAssignedIdentityTenantId string = subscription().tenantId
output LAname string = logicAppName
