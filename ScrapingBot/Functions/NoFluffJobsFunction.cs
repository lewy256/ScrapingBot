using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using ScrapingBot.Entities;
using ScrapingBot.Extensions;
using ScrapingBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScrapingBot.Functions;

public static class NoFluffJobsFunction {

    [FunctionName(nameof(ScrapingNoFluffJobs))]
    public static async Task ScrapingNoFluffJobs([TimerTrigger("0 0 14 * * *")] TimerInfo myTimer, [DurableClient] IDurableOrchestrationClient starter, ILogger log) {
        string instanceId = await starter.StartNewAsync(nameof(Orchestrator));

        log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
    }

    [FunctionName(nameof(Orchestrator))]
    public static async Task Orchestrator([OrchestrationTrigger] IDurableOrchestrationContext context) {
        var categories = await context.CallActivityAsync<string[]>(nameof(GetCategories), null);
        var experiences = await context.CallActivityAsync<string[]>(nameof(GetExperiences), null);

        int processors = Environment.ProcessorCount;

        int categoryGroups = (categories.Length + 4) / processors;

        await Enumerable.Range(0, processors).ParallelForEachAsync(100, index => {
            int start = index * categoryGroups;

            return context.CallActivityAsync(nameof(ProcessItems), new Payload() {
                Categories = categories.Slice(start, categoryGroups),
                ExperienceLevels = experiences
            });
        });
    }

    [FunctionName(nameof(ProcessItems))]
    public static async Task ProcessItems([ActivityTrigger] Payload payload, ILogger logger) {
        try {
            var offerGroups = await NoFluffJobsService.ScrapingNoFluffJobs(payload.Categories, payload.ExperienceLevels, logger);

            await offerGroups.InsertItemsToTableAsync(logger);
        }
        catch(Exception exception) {
            var exceptions = exception.CaptureLogExceptions(() => exception.ToString().SendEmail(), logger);
            throw new AggregateException(exceptions);
        }
    }

    [FunctionName(nameof(GetCategories))]
    public static async Task<List<string>> GetCategories([ActivityTrigger] IDurableActivityContext context, ILogger logger) {
        try {
            var categories = await NoFluffJobsService.GetCategories();

            return categories;
        }
        catch(Exception exception) {
            var exceptions = exception.CaptureLogExceptions(() => exception.ToString().SendEmail(), logger);
            throw new AggregateException(exceptions);
        }
    }

    [FunctionName(nameof(GetExperiences))]
    public static async Task<List<string>> GetExperiences([ActivityTrigger] IDurableActivityContext context, ILogger logger) {
        try {
            var experienceLevels = await NoFluffJobsService.GetExperiences();

            return experienceLevels;
        }
        catch(Exception exception) {
            var exceptions = exception.CaptureLogExceptions(() => exception.ToString().SendEmail(), logger);
            throw new AggregateException(exceptions);
        }
    }
}

