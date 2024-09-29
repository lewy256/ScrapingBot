using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(ScrapingBot.Startup))]

namespace ScrapingBot;

public class Startup : FunctionsStartup {
    public override void Configure(IFunctionsHostBuilder builder) {
        Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", Environment.GetEnvironmentVariable("HOME_EXPANDED"));
        Microsoft.Playwright.Program.Main(["install", "chromium", "--with-deps",]);
    }
}
