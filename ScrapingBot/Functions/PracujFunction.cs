using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ScrapingBot.Extensions;
using ScrapingBot.Services;
using System;
using System.Threading.Tasks;

namespace ScrapingBot.Functions;

public static class PracujFunction {

    [FunctionName(nameof(ScrapingPracuj))]
    public static async Task ScrapingPracuj([TimerTrigger("0 0 13 * * *")] TimerInfo myTimer, ILogger logger) {
        try {
            var scraper = new PracujService();

            var items = await scraper.ScrapingPracuj(logger);

            await items.InsertItemsToTableAsync(logger);
        }
        catch(Exception exception) {
            var exceptions = exception.CaptureLogExceptions(() => exception.ToString().SendEmail(), logger);
            throw new AggregateException(exceptions);
        }
    }
}
