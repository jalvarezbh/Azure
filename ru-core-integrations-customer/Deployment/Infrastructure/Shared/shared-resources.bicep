/**
This bicep template will create all shared resources for the core product integration
*/

param environmentName string
param apimManagedIdentityId string
param appInsightsName string
param dataverseObjectId string

param localKeyVaultName string
param userAssignedManagedIdentityName string
param customStorageName string
param storageAccountType string = 'Standard_LRS'

// graph parameters
param graphAudience string
param graphAuthority string
param graphClientId string
@secure()
param graphClientSecret string
param graphTenant string

param serviceBusNamespaceName string
param serviceBusQueueName string
param serviceBusQueueNameOutbound string
param sharedResourceGroupName string

resource userAssignedManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2025-01-31-preview' = {
  name: userAssignedManagedIdentityName
  location: resourceGroup().location
}

module serviceBusModule 'servicebus-setup-module.bicep' = {
  name: 'serviceBusModule'
  scope: resourceGroup(sharedResourceGroupName)
  params: {
    serviceBusNamespaceName: serviceBusNamespaceName
    serviceBusQueueName: serviceBusQueueName
    serviceBusQueueNameOutbound: serviceBusQueueNameOutbound
    apimManagedIdentityId: apimManagedIdentityId
    userAssignedManagedIdentityPrincipalId: userAssignedManagedIdentity.properties.principalId
  }
}

// generic variables
var ResourceGroup = resourceGroup().location
var customStorageAccountid = '${resourceGroup().id}/providers/Microsoft.Storage/storageAccounts/${customStorageName}'
var endpointSuffix = ';EndpointSuffix=core.windows.net'
var sa_name = 'sa'

// storage setup (default first, used for logic and function app)
var storageName = '${toLower(sa_name)}${uniqueString(resourceGroup().id)}'
resource storageAccount 'Microsoft.Storage/storageAccounts@2018-07-01' = {
  name: storageName
  location: ResourceGroup
  kind: 'StorageV2'
  sku: {
    name: storageAccountType
  }
  properties: {}
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

// create tables in storage account
resource storageaccountCustom_tableService 'Microsoft.Storage/storageAccounts/tableServices@2021-08-01' = {
  name: 'default'  
  parent: storageAccountCustom  
}

resource storageaccountCustom_tableService_HashesTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-08-01' = {
  name: 'Hashes'  
  parent: storageaccountCustom_tableService
}

resource storageaccountCustom_tableService_table_product_transaction_log 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-08-01' = {
  name: 'InboundAccountTransactionLog'  
  parent: storageaccountCustom_tableService
}

resource storageaccount_tableService_table_inbound_products_queue 'Microsoft.Storage/storageAccounts/tableServices/tables@2021-08-01' = {
  name: 'InboundAccountsQueue'  
  parent: storageaccountCustom_tableService
}

// app insights
resource appInsights 'Microsoft.Insights/components@2014-04-01' = {
  name: appInsightsName
  location: ResourceGroup
  properties: {
    applicationId: appInsightsName
  }
}

// key vault and secrets
resource kv 'Microsoft.KeyVault/vaults@2021-11-01-preview' = {
  name: localKeyVaultName
  location: ResourceGroup
  tags: {
      Environment: environmentName
  }
  properties: {
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    tenantId: subscription().tenantId
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enableRbacAuthorization: true
    sku: {
      name: 'standard'
      family: 'A'
    }
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource serviceBusConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'ServiceBusConnectionString'
  properties: {
    value: serviceBusModule.outputs.serviceBusConnectionString
  }
}

resource customStorageTableConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'CustomStorageTableConnectionString'
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${customStorageName};AccountKey=${listKeys(customStorageAccountid,'2015-05-01-preview').key1}${endpointSuffix}'
  }
}

// key vault settings for graph api access
resource graphAudienceSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'GraphAudience'
  properties: {
    value: graphAudience
  }
}

resource graphAuthoritySecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'GraphAuthority'
  properties: {
    value: graphAuthority
  }
}

resource graphClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'GraphClientId'
  properties: {
    value: graphClientId
  }
}

resource graphClientSecretSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'GraphClientSecret'
  properties: {
    value: graphClientSecret
  }
}

resource graphTenantSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  parent: kv
  name: 'GraphTenant'
  properties: {
    value: graphTenant
  }
}

@description('This is the built-in Key Vault Secret User role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource keyVaultSecretUserRoleRoleDefinition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

// grant managed identity access to keyvault
resource keyVaultSecretUserRoleAssignmentManagedIdentity 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  scope: kv
  name: guid(resourceGroup().id, userAssignedManagedIdentity.id, keyVaultSecretUserRoleRoleDefinition.id)
  properties: {
    roleDefinitionId: keyVaultSecretUserRoleRoleDefinition.id
    principalId: userAssignedManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Storage account Table contributor rbac access setup
@description('This is the built-in Key Vault Secret User role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource Storage_Table_Data_Contributor_definition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
}


// Storage account Blob contributor rbac access setup
@description('This is the built-in Key Vault Secret User role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#key-vault-secrets-user')
resource Storage_Blob_Data_Contributor_definition 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

// grant managed identity access to storage account
resource storageAccountTableManagedIdentity 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  scope: storageAccountCustom
  name: guid(resourceGroup().id, userAssignedManagedIdentity.id, Storage_Table_Data_Contributor_definition.id)
  properties: {
    roleDefinitionId: Storage_Table_Data_Contributor_definition.id
    principalId: userAssignedManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// grant access to Blob
resource storageAccountBlobManagedIdentity 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  scope: storageAccountCustom
  name: guid(resourceGroup().id, userAssignedManagedIdentity.id, Storage_Blob_Data_Contributor_definition.id)
  properties: {
    roleDefinitionId: Storage_Blob_Data_Contributor_definition.id
    principalId: userAssignedManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// grant dataverse access to keyvault
resource keyVaultSecretUserRoleAssignmentDataverse 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  scope: kv
  name: guid(resourceGroup().id, dataverseObjectId, keyVaultSecretUserRoleRoleDefinition.id)
  properties: {
    roleDefinitionId: keyVaultSecretUserRoleRoleDefinition.id
    principalId: dataverseObjectId
    principalType: 'ServicePrincipal'
  }
}

// grant apim instance access to keyvault
resource keyVaultSecretUserRoleAssignmentApimManagedIdentity 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  scope: kv
  name: guid(resourceGroup().id, apimManagedIdentityId, keyVaultSecretUserRoleRoleDefinition.id)
  properties: {
    roleDefinitionId: keyVaultSecretUserRoleRoleDefinition.id
    principalId: apimManagedIdentityId
    principalType: 'ServicePrincipal'
  }
}
