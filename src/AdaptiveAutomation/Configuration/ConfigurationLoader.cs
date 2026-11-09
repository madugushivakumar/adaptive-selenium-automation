using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace AdaptiveAutomation.Configuration;

public static class ConfigurationLoader
{
    public static AppSettings Load(string[]? args = null, string? basePath = null)
    {
        basePath ??= AppDomain.CurrentDomain.BaseDirectory;

        // Fallback search paths for appsettings.json
        string configPath = Path.Combine(basePath, "appsettings.json");
        if (!File.Exists(configPath))
        {
            string altPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "AdaptiveAutomation", "appsettings.json");
            if (File.Exists(altPath))
            {
                configPath = altPath;
                basePath = Path.GetDirectoryName(altPath)!;
            }
            else
            {
                altPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                if (File.Exists(altPath))
                {
                    configPath = altPath;
                    basePath = Directory.GetCurrentDirectory();
                }
            }
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "AUTOMATION_");

        if (args != null && args.Length > 0)
        {
            builder.AddCommandLine(args);
        }

        IConfigurationRoot root = builder.Build();
        var settings = new AppSettings();
        root.Bind(settings);

        // Command-line flag parsing overrides
        if (args != null)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "--url", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    settings.Application.BaseUrl = args[i + 1];
                }
                else if (string.Equals(args[i], "--headless", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length && bool.TryParse(args[i + 1], out bool hl))
                    {
                        settings.Browser.Headless = hl;
                    }
                    else
                    {
                        settings.Browser.Headless = true;
                    }
                }
                else if (string.Equals(args[i], "--firstname", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    settings.TestData.FirstName = args[i + 1];
                }
                else if (string.Equals(args[i], "--gender", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    settings.TestData.Gender = args[i + 1];
                }
                else if (string.Equals(args[i], "--state", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    settings.TestData.State = args[i + 1];
                }
            }
        }

        return settings;
    }
}

