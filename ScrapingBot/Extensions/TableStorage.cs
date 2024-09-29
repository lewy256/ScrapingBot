using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using ScrapingBot.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScrapingBot.Extensions;

public static class TableStorage {

    public static async Task InsertItemsToTableAsync(this IEnumerable<OfferGroup> items, ILogger logger) {
        var tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "OfferGroup");

        await tableClient.CreateIfNotExistsAsync();

        var transactionActions = new List<TableTransactionAction>();

        foreach(var item in items) {
            transactionActions.Add(new TableTransactionAction(TableTransactionActionType.Add, item));

            if(transactionActions.Count == 100) {
                try {
                    await tableClient.SubmitTransactionAsync(transactionActions);
                    transactionActions.Clear();
                    logger.LogInformation("Batch insert succeeded.");
                }
                catch(Exception ex) {
                    logger.LogError($"Batch insert failed: {ex.Message}");
                    throw;
                }
            }
        }

        if(transactionActions.Count > 0) {
            try {
                await tableClient.SubmitTransactionAsync(transactionActions);
                logger.LogInformation("Batch insert succeeded.");
            }
            catch(Exception ex) {
                logger.LogError($"Batch insert failed: {ex.Message}");
                throw;
            }
        }
    }
}
