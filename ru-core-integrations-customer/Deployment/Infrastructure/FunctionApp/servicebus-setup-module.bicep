param serviceBusNamespaceName string
param serviceBusQueueName string
param functionAppPrincipalId string
// param functionAppId string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' existing = {
  name: serviceBusQueueName
  parent: serviceBusNamespace
}

@description('This is the built-in Service Bus Data Receiver role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#service-bus-data-receiver')
resource serviceBusDataReceiverRoleDef 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0' // Azure Service Bus Data Receiver
}

resource serviceBusQueueRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(serviceBusQueue.id, functionAppPrincipalId, serviceBusDataReceiverRoleDef.id)
  scope: serviceBusQueue
  properties: {
    roleDefinitionId: serviceBusDataReceiverRoleDef.id
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}
