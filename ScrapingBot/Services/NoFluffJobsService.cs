using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ScrapingBot.Entities;
using ScrapingBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ScrapingBot.Services;

public static class NoFluffJobsService {
    public static async Task<List<string>> GetExperiences() {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        string url = "https://nofluffjobs.com/pl/?lang=en";

        await page.SetViewportSizeAsync(1440, 840);
        await page.GotoAsync(url);

        await page
            .GetByRole(AriaRole.Button, new() { Name = "More", Exact = true })
            .ClickAsync();

        await page
            .GetByRole(AriaRole.Button, new() { Name = "Seniority", Exact = true })
            .ClickAsync();

        var pageContent = await page.ContentAsync();
        var document = new HtmlDocument();
        document.LoadHtml(pageContent);

        var spans = document.DocumentNode
            .SelectNodes("//*[@id=\"cdk-accordion-child-2\"]/div/nfj-filter-universal-section/div/section/div/nfj-filter-control/nfj-checkbox/label/span");

        var experienceLevels = new List<string>();

        if(spans is null) {
            throw new NullCollectionException(nameof(spans), nameof(GetExperiences), url);
        }

        foreach(var span in spans) {
            string text = span.InnerText.Replace(" ", "").Replace("\n", "");

            if(text != String.Empty) {
                experienceLevels.Add(text);
            }
        }

        if(experienceLevels.Count == 0) {
            throw new EmptyCollectionException(nameof(experienceLevels), nameof(GetExperiences), url);
        }

        return experienceLevels;
    }

    public static async Task<List<string>> GetCategories() {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        string url = "https://nofluffjobs.com/pl/?lang=en";

        await page.SetViewportSizeAsync(1440, 840);
        await page.GotoAsync(url);

        await page
            .GetByRole(AriaRole.Button, new() { Name = "More", Exact = true })
            .ClickAsync();

        await page
            .GetByRole(AriaRole.Button, new() { Name = "Categories and technologies IT", Exact = true })
            .ClickAsync();

        var pageContent = await page.ContentAsync();
        var document = new HtmlDocument();
        document.LoadHtml(pageContent);

        var spans = document.DocumentNode
            .SelectNodes("//*[@id=\"cdk-accordion-child-4\"]/div/div/div[2]/div/div/nfj-filter-control/nfj-checkbox/label/span");

        var categories = new List<string>();

        if(spans is null) {
            throw new NullCollectionException(nameof(spans), nameof(GetCategories), url);
        }

        foreach(var span in spans) {
            string text = span.InnerText.Replace(" ", "").Replace("\n", "");
            if(text != String.Empty) {
                categories.Add(text);
            }
        }

        if(categories.Count == 0) {
            throw new EmptyCollectionException(nameof(categories), nameof(GetCategories), url);
        }

        return categories;
    }

    private static async Task<int> CountOffers(string url, IPage page) {
        await page.SetViewportSizeAsync(1440, 840);
        await page.GotoAsync(url);

        var locator = page.GetByRole(AriaRole.Button, new() { Name = "See more offers", Exact = true });

        bool isVisible = true;

        while(isVisible) {
            try {
                await page.Mouse.WheelAsync(0, page.ViewportSize.Height);

                await locator.ClickAsync(new LocatorClickOptions() {
                    Timeout = 1000
                });
            }
            catch(TimeoutException) {
                isVisible = false;
            }
        }

        var pageContent = await page.ContentAsync();
        var document = new HtmlDocument();
        document.LoadHtml(pageContent);

        var firstList = document.DocumentNode
            .SelectNodes("//body/nfj-root/nfj-layout/nfj-main-content/div/nfj-postings-search/div/div[3]/common-main-loader/div/nfj-search-results/nfj-postings-list/div[2]/a");

        var secondList = document.DocumentNode
            .SelectNodes("//body/nfj-root/nfj-layout/nfj-main-content/div/nfj-postings-search/div/div[3]/common-main-loader/div/nfj-search-results/nfj-postings-list[2]/div/a");

        var offers = (firstList is null ? 0 : firstList.Count) + (secondList is null ? 0 : secondList.Count);

        return offers;
    }

    public static async Task<List<OfferGroup>> ScrapingNoFluffJobs(string[] categories, string[] experiences, ILogger logger) {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        page.SetDefaultNavigationTimeout(600_000);

        string url = "https://nofluffjobs.com/pl/?lang=en";

        var offerGroups = new List<OfferGroup>();

        foreach(var category in categories) {
            foreach(var experience in experiences) {
                var offers = await CountOffers(url + WebUtility.UrlEncode(category) + "?lang=en&criteria=seniority%3D" + experience, page);

                string rowKey = Guid.NewGuid().ToString();

                offerGroups.Add(new OfferGroup() {
                    ExperienceLevel = experience,
                    Stack = category,
                    OfferCount = offers,
                    PartitionKey = url.Split("/")[2],
                    RowKey = rowKey
                });

                logger.LogInformation("Function: " + nameof(ScrapingNoFluffJobs) + " || Stack: " + category + " || Experience: " + experience + " || Offers: " + offers);
            }
        }

        return offerGroups;
    }
}

