using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using ScrapingBot.Entities;
using ScrapingBot.Exceptions;
using ScrapingBot.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScrapingBot.Services;

public class JustJoinService {
    private const string _baseUrl = "https://justjoin.it";

    private List<string> GetCategories() {
        var web = new HtmlWeb();

        var document = web.Load(_baseUrl);

        var anchors = document.DocumentNode
            .SelectNodes("//*[@id=\"__next\"]/div[2]/div[1]/div/div[1]/div[1]/div[2]/a");

        if(anchors is null) {
            throw new NullCollectionException(nameof(anchors), nameof(GetCategories), _baseUrl);
        }

        var categories = new List<string>();

        foreach(var anchor in anchors) {
            string text = anchor.GetAttributeValue("href", String.Empty);
            if(text != String.Empty) {
                categories.Add(text);
            }
        }

        if(categories.Count == 0) {
            throw new EmptyCollectionException(nameof(categories), nameof(GetCategories), _baseUrl);
        }

        return categories;
    }

    private async Task<List<string>> GetExperiences(IPage page) {
        await page
            .GetByRole(AriaRole.Button, new() { Name = "Decline all" })
            .ClickAsync();

        await page
            .GetByRole(AriaRole.Button, new() { Name = "More filters" })
            .ClickAsync();

        await page
            .Locator("//*[@id=\"filters-more\"]/div[3]/fieldset/div/div[1]")
            .WaitForAsync();

        var pageContent = await page.ContentAsync();

        var document = new HtmlDocument();
        document.LoadHtml(pageContent);

        var labels = document.DocumentNode
            .SelectNodes("//*[@id=\"filters-more\"]/div[3]/fieldset/div/div/label");

        if(labels is null) {
            throw new NullCollectionException(nameof(labels), nameof(GetExperiences), _baseUrl);
        }

        var experienceLevels = new List<string>();

        foreach(var label in labels) {
            var text = label.InnerText;
            if(text != String.Empty) {
                experienceLevels.Add(text);
            }
        }

        if(experienceLevels.Count == 0) {
            throw new EmptyCollectionException(nameof(experienceLevels), nameof(GetExperiences), _baseUrl);
        }

        return experienceLevels;
    }

    private int CountOffers(string url) {
        var web = new HtmlWeb();
        var document = web.Load(url);

        var header = document.DocumentNode
            .SelectSingleNode("//body/div[1]/div[2]/div/div/div[2]/div/div/div[1]/h1");

        if(header is not null) {
            string text = header.InnerText;

            int offers = text != String.Empty ? text.ToInt() : 0;
            return offers;
        }
        else {
            return 0;
        }
    }

    public async Task<List<OfferGroup>> ScrapingJustJoin(ILogger logger) {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() {
            Headless = true
        });

        var page = await browser.NewPageAsync();

        await page.SetViewportSizeAsync(425, 858);

        await page.GotoAsync(_baseUrl);

        var categories = GetCategories();

        var experiences = await GetExperiences(page);

        var offerGroups = new List<OfferGroup>();

        foreach(var experience in experiences) {
            foreach(var category in categories) {

                var exp = experience.ToLower();

                var offers = CountOffers(_baseUrl + category + "/experience-level_" + exp);

                string rowKey = Guid.NewGuid().ToString();

                offerGroups.Add(new OfferGroup() {
                    ExperienceLevel = exp,
                    Stack = category,
                    OfferCount = offers,
                    PartitionKey = _baseUrl["https://".Length..],
                    RowKey = rowKey
                });

                logger.LogInformation("Function: " + nameof(ScrapingJustJoin) + " || Stack: " + category.Split("/")[2] + " || Experience: " + exp + " || Offers: " + offers);
            }
        }

        return offerGroups;
    }
}
