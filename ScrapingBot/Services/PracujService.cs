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

public class PracujService {
    private const string _baseUrl = "https://it.pracuj.pl/praca";

    private async Task<List<Category>> GetCategories(IPage page) {
        await page.GotoAsync(_baseUrl);

        await page
            .Locator("//*[@id=\"filters\"]/div/div[4]/div[2]/div/div/div[1]/div[1]")
            .WaitForAsync();

        var pageContent = await page.ContentAsync();
        var document = new HtmlDocument();
        document.LoadHtml(pageContent);

        var divs = document.DocumentNode
            .SelectNodes("//*[@id=\"filters\"]/div/div[4]/div[2]/div/div/div[1]/div");

        if(divs is null) {
            throw new NullCollectionException(nameof(divs), nameof(GetCategories), _baseUrl);
        }

        var categories = new List<Category>();

        foreach(var div in divs) {
            string categoryName = div.InnerText;

            if(categoryName != String.Empty) {
                int categoryId = div.GetAttributeValue("data-test", string.Empty).ToInt();
                categories.Add(new() { CategoryId = categoryId, CategoryName = categoryName });
            }
        }

        if(categories.Count == 0) {
            throw new EmptyCollectionException(nameof(categories), nameof(GetCategories), _baseUrl);
        }

        return categories;
    }

    private async IAsyncEnumerable<(string experience, int offers)> CountOffers(string url, IPage page) {
        await page.GotoAsync(url);

        await page
            .Locator("//*[@id=\"filters\"]/div[2]/div[6]/div[2]/div/ul/li[1]/label/span[2]/span/span[2]")
            .WaitForAsync();

        var pageContent = await page.ContentAsync();

        var document = new HtmlDocument();
        document.LoadHtml(pageContent);

        var experienceLevels = document.DocumentNode
            .SelectNodes("//*[@id=\"filters\"]/div/div[6]/div[2]/div/ul/li/label/span[2]/span/span[1]");

        var experienceOffers = document.DocumentNode
            .SelectNodes("//*[@id=\"filters\"]/div/div[6]/div[2]/div/ul/li/label/span[2]/span/span[2]");

        if(experienceLevels is null) {
            throw new NullCollectionException(nameof(experienceLevels), nameof(CountOffers), _baseUrl);
        }
        else if(experienceOffers is null) {
            throw new NullCollectionException(nameof(experienceOffers), nameof(CountOffers), _baseUrl);
        }

        for(int i = 0; i < experienceLevels.Count; i++) {
            string level = experienceLevels[i].InnerText;
            string offers = experienceOffers[i].InnerText;

            if(level != String.Empty && offers != String.Empty) {
                int offersToReturn = offers.ToInt();
                yield return (level, offersToReturn);
            }
            else if(level != String.Empty) {
                yield return (level, 0);
            }
        }
    }

    public async Task<List<OfferGroup>> ScrapingPracuj(ILogger logger) {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

        var page = await browser.NewPageAsync();

        await page.SetViewportSizeAsync(1440, 840);

        var categories = await GetCategories(page);

        var offerGroups = new List<OfferGroup>();

        foreach(var category in categories) {
            var offers = CountOffers(_baseUrl + "?itth=" + category.CategoryId, page);

            await foreach(var item in offers) {

                string rowKey = Guid.NewGuid().ToString();

                offerGroups.Add(new OfferGroup() {
                    ExperienceLevel = item.experience,
                    Stack = category.CategoryName,
                    OfferCount = item.offers,
                    PartitionKey = _baseUrl.Split("/")[2],
                    RowKey = rowKey
                });

                logger.LogInformation("Function: " + nameof(ScrapingPracuj) + " || Stack: " + category.CategoryName + " || Experience: " + item.experience + " || Offers: " + item.offers);
            }
        }

        return offerGroups;
    }
}
