// @description('The datacenter to use for the deployment.')
// param location string
// param commondataserviceName string

// param serviceBusName string
// param keyvaultName string
// param serviceBusResourceGroup string
// param sharedServiceBusName string
// param localKeyVaultName string

// param logicAppSystemAssignedIdentityTenantId string
// param logicAppSystemAssignedIdentityObjectId string

// @secure()
// param dynamicsCrmSecret string
// @secure()
// param dynamicsCrmClientId string
// param dynamicsCrmDirectoryId string


// resource sb 'Microsoft.ServiceBus/namespaces@2021-11-01' existing = {
//   name: sharedServiceBusName
//   scope: resourceGroup(serviceBusResourceGroup)
// }

// var serviceBusEndpoint = '${sb.id}/AuthorizationRules/RootManageSharedAccessKey'

// resource serviceBusConnection 'Microsoft.Web/connections@2016-06-01' = {
//   name: serviceBusName
//   location: location
//   kind: 'V2'
//   properties: {
//     displayName: 'ServiceBus'
//     parameterValues: {
//       connectionString: listKeys(serviceBusEndpoint, sb.apiVersion).primaryConnectionString
//     }
//     api: {
//       id: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Web/locations/${location}/managedApis/${serviceBusName}'
//       type: 'Microsoft.Web/locations/managedApis'
//     }    
//   }
// }

// resource serviceBusName_logicAppSystemAssignedIdentityObjectId 'Microsoft.Web/connections/accessPolicies@2016-06-01' = {
//   parent: serviceBusConnection
//   name: 'ServiceBusAssignedIdentity'
//   location: location
//   properties: {
//     principal: {
//       type: 'ActiveDirectory'
//       identity: {
//         tenantId: logicAppSystemAssignedIdentityTenantId
//         objectId: logicAppSystemAssignedIdentityObjectId
//       }
//     }
//   }
// }

// resource keyvaultConnection 'Microsoft.Web/connections@2016-06-01' = {
//   name: keyvaultName
//   location: location
//   kind: 'V2'
//   properties: {    
//     displayName: 'Keyvault'
//     parameterValueType: 'Alternative'
//     alternativeParameterValues: {
//       vaultName: localKeyVaultName
//     }
//     api: {
//       id: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Web/locations/${location}/managedApis/${keyvaultName}'
//     }
//   }
// }

// resource keyvaultName_logicAppSystemAssignedIdentityObjectId 'Microsoft.Web/connections/accessPolicies@2016-06-01' = {
//   parent: keyvaultConnection
//   name: 'KeyvaultAssignedIdentity'
//   location: location
//   properties: {
//     principal: {
//       type: 'ActiveDirectory'
//       identity: {
//         tenantId: logicAppSystemAssignedIdentityTenantId
//         objectId: logicAppSystemAssignedIdentityObjectId
//       }
//     }
//   }
// }

// resource commondataservice 'Microsoft.Web/connections@2016-06-01' = {
//   name: commondataserviceName
//   location: location
//   kind: 'V2'
//   properties: {
//     displayName: 'Dataverse'
//     parameterValues: {
//       'token:clientId': dynamicsCrmClientId
//       'token:TenantId': dynamicsCrmDirectoryId
//       'token:grantType': 'client_credentials'
//       'token:clientSecret': dynamicsCrmSecret
//     }
//     api: {
//       id: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Web/locations/${location}/managedApis/${commondataserviceName}'
//       type: 'Microsoft.Web/locations/managedApis'
//     }
//   }
//   dependsOn: []
// }

// resource commondataserviceName_logicAppSystemAssignedIdentityObjectId 'Microsoft.Web/connections/accessPolicies@2016-06-01' = {
//   parent: commondataservice
//   name: 'CommonDataServicAssignedIdentity'
//   location: location
//   properties: {
//     principal: {
//       type: 'ActiveDirectory'
//       identity: {
//         tenantId: logicAppSystemAssignedIdentityTenantId
//         objectId: logicAppSystemAssignedIdentityObjectId
//       }
//     }
//   }
// }

// resource office365Connection 'Microsoft.Web/connections@2016-06-01' = {
//   name: 'office365'
//   location: location
//   kind: 'V2'
//   properties: {
//     displayName: 'Office 365 Connection'
//     api: {
//       id: '/subscriptions/${subscription().subscriptionId}/providers/Microsoft.Web/locations/${location}/managedApis/office365'
//     }
//     parameterValues: {
      
//     }
//   }
// }

// resource office365_logicAppSystemAssignedIdentityObjectId 'Microsoft.Web/connections/accessPolicies@2016-06-01' = {
//   parent: office365Connection
//   name: 'Office365AssignedIdentity'
//   location: location
//   properties: {
//     principal: {
//       type: 'ActiveDirectory'
//       identity: {
//         tenantId: logicAppSystemAssignedIdentityTenantId
//         objectId: logicAppSystemAssignedIdentityObjectId
//       }
//     }
//   }
// }


// output serviceBusConnectionRuntimeUrl string = serviceBusConnection.properties.connectionRuntimeUrl
// output keyvaultConnectionRuntimeUrl string = keyvaultConnection.properties.connectionRuntimeUrl
// output commondataserviceConnectionRuntimeUrl string = commondataservice.properties.connectionRuntimeUrl
// output office365ConnectionRuntimeUrl string = office365Connection.properties.connectionRuntimeUrl

output dummy string = 'dummy'
