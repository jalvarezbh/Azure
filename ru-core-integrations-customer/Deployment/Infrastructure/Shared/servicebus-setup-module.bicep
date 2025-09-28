param serviceBusNamespaceName string
param serviceBusQueueName string
param serviceBusQueueNameOutbound string
param apimManagedIdentityId string
param userAssignedManagedIdentityPrincipalId string

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' existing = {
  name: serviceBusNamespaceName
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: serviceBusQueueName
  parent: serviceBusNamespace
}

resource serviceBusQueueOutbound 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  name: serviceBusQueueNameOutbound
  parent: serviceBusNamespace
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(serviceBusQueue.id, 'Azure Service Bus Data Sender')
  scope: serviceBusQueue
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39') // Azure Service Bus Data Sender
    principalId: apimManagedIdentityId
    principalType: 'ServicePrincipal'
  }
}

resource serviceBusQueueAuthRule 'Microsoft.ServiceBus/namespaces/queues/authorizationRules@2024-01-01' = {
  name: 'ReadOnly'
  parent: serviceBusQueue
  properties: {
    rights:[ 'Listen']
  }
}

// grant access to read service bus queue
@description('This is the built-in Service Bus Data Receiver role. See https://docs.microsoft.com/azure/role-based-access-control/built-in-roles#service-bus-data-receiver')
resource serviceBusDataReceiverRoleDef 'Microsoft.Authorization/roleDefinitions@2018-01-01-preview' existing = {
  scope: subscription()
  name: '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0' // Azure Service Bus Data Receiver
}

resource serviceBusQueueRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(serviceBusQueue.id, userAssignedManagedIdentityPrincipalId, serviceBusDataReceiverRoleDef.id)
  scope: serviceBusQueue
  properties: {
    roleDefinitionId: serviceBusDataReceiverRoleDef.id
    principalId: userAssignedManagedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

@secure()
output serviceBusConnectionString string = serviceBusQueueAuthRule.listKeys().primaryConnectionString
