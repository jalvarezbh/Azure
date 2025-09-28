using System.Net.Http.Headers;
using System.Net.Http.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using ru.core.integrations.customer.core.Model.Configuration;
using ru.core.integrations.customer.core.Model.Dataverse;
using ru.core.integrations.customer.core.Model.Inbound;
using ru.core.integrations.customer.core.Services.Inbound;

namespace ru.core.integrations.customer.core.Repositories.Dataverse;

public class DataverseAccountRepository : IDataverseAccountRepository
{
    private readonly IDataverseClientFactory _clientFactory;
    private readonly IInboundIntermediateUpdateModelMapperFactory _mapperFactory;
    private readonly ILogger<DataverseAccountRepository> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenCredential _tokenCredential;

    public DataverseAccountRepository(IDataverseClientFactory clientFactory,
        IInboundIntermediateUpdateModelMapperFactory mapperFactory,
        ILogger<DataverseAccountRepository> logger,
        IHttpClientFactory httpClientFactory, 
        TokenCredential tokenCredential)
    {
        _clientFactory = clientFactory;
        _mapperFactory = mapperFactory;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _tokenCredential = tokenCredential;
    }

    public async Task<InboundBatchUpdateStatusModel> PerformBatchUpdateAsync(IntermediateCrmBatchUpdateModel toUpdate,
        DataverseEndpointConfiguration endpointConfiguration)
    {
        if (toUpdate.Updates.Count == 0)
        {
            return new InboundBatchUpdateStatusModel(); // nothing to update
        }

        var client = _clientFactory.CreateClient(endpointConfiguration.DataverseUrl);
        var mapper = _mapperFactory.CreateOrganizationRequestMapper(endpointConfiguration);

        var dvContext = new DataverseContext(client);

        InboundBatchUpdateStatusModel statusModel = new InboundBatchUpdateStatusModel();
        
        foreach (var update in toUpdate.Updates)
        {

            var accountEntity = mapper.MapToAccountEntity(update);

            if (update.UpdateType == UpdateType.Update)
            {
                dvContext.Attach(accountEntity);
                dvContext.UpdateObject(accountEntity);
            }
            else
            {
                dvContext.AddObject(accountEntity);
            }


            SaveChangesResultCollection? result = null;

            try
            {
                result = dvContext.SaveChanges();
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save changes to Dataverse for account {AccountNumber} in environment {Environment}",
                    update.AccountNumber, endpointConfiguration.DataverseUrl);

            }

            dvContext.Detach(accountEntity);

            var statusItem = new InboundBatchUpdateStatusModelItem()
            {
                ETag = update.ETag,
                SapKey = update.AccountNumber,
                DataverseEnvironment = endpointConfiguration.DataverseUrl,
                Failed = false,
                Message = update.UpdateType == UpdateType.Update ? "Updated" : "Created"
            };

            statusModel.Items.Add(statusItem);

            if (result.HasError)
            {
                statusItem.Failed = true;
                statusItem.Message = result[0].Error.Message;
            }
        }

        return statusModel;
    }

    public async Task<AccountSingle> GetAccountAsync(Guid accountId, DataverseEndpointConfiguration endpointConfiguration)
    {
        var client = _httpClientFactory.CreateClient(endpointConfiguration.Name);

        var token = _tokenCredential.GetToken(
            new TokenRequestContext(new[] { $"{endpointConfiguration.DataverseUrl}/.default" }),
            CancellationToken.None);

        var account = client
            .DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.Token);

        var response = await client.GetAsync($"/api/data/v9.2/accounts({accountId})");

        var data = await response.Content.ReadFromJsonAsync<AccountSingle>();

        return data;
    }

    public void GetAccounts(DataverseEndpointConfiguration endpointConfiguration)
    {
        var client = _clientFactory.CreateClient(endpointConfiguration.DataverseUrl);

        var query = new QueryExpression("account")
        {
            ColumnSet = new ColumnSet("name", "accountnumber"),
            TopCount = 5
        };

        var accounts = client.RetrieveMultiple(query);
    }

    public async Task<IEnumerable<AccountIdSapIdPair>> GetAccountsIdsBySapCustomerIdsAsync(string[] sapCustomerIds, 
        DataverseEndpointConfiguration endpointConfiguration)
    {
        //todo: rewrite to use the SDK
        if (sapCustomerIds.Length == 0)
        {
            return Array.Empty<AccountIdSapIdPair>(); // no IDs to search for
        }

        var client = _clientFactory.CreateClient(endpointConfiguration.DataverseUrl);
        var query = new QueryExpression("account")
        {
            ColumnSet = new ColumnSet("name", "accountnumber"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("accountnumber", ConditionOperator.In, sapCustomerIds)
                }
            }
        };

        var accounts = await client.RetrieveMultipleAsync(query);

        var accountIdSapIdPairs = accounts.Entities
            .Select(e => new AccountIdSapIdPair(
                e.Id, 
                e.GetAttributeValue<string>("accountnumber")))
            .ToArray();

        return accountIdSapIdPairs;
    }
}

public interface IDataverseAccountRepository
{
    Task<InboundBatchUpdateStatusModel> PerformBatchUpdateAsync(IntermediateCrmBatchUpdateModel toUpdate,
        DataverseEndpointConfiguration endpointConfiguration);
    Task<IEnumerable<AccountIdSapIdPair>> GetAccountsIdsBySapCustomerIdsAsync(string[] sapCustomerIds,
        DataverseEndpointConfiguration endpointConfiguration);
    void GetAccounts(DataverseEndpointConfiguration endpointConfiguration);
    Task<AccountSingle> GetAccountAsync(Guid accountId, DataverseEndpointConfiguration endpointConfiguration);
}

public record AccountIdSapIdPair(Guid AccountId, string SapId);
