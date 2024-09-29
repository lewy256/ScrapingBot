using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ScrapingBot.Extensions;
using ScrapingBot.Services;
using System;
using System.Threading.Tasks;

namespace ScrapingBot.Functions;

public static class JustJoinFunction {

    [FunctionName(nameof(ScrapingJustJoin))]
    public static async Task ScrapingJustJoin([TimerTrigger("0 0 12 * * *")] TimerInfo myTimer, ILogger logger) {
        try {
            var scraper = new JustJoinService();

            var items = await scraper.ScrapingJustJoin(logger);

            await items.InsertItemsToTableAsync(logger);
        }
        catch(Exception exception) {
            var exceptions = exception.CaptureLogExceptions(() => exception.ToString().SendEmail(), logger);
            throw new AggregateException(exceptions);
        }
    }
}
