// HTML -> PNG converter using PuppeteerSharp (headless Chromium)
// Requirements:
//  - .NET 6.0+ SDK
//  - NuGet package: PuppeteerSharp
//    Install: dotnet add package PuppeteerSharp
// Usage examples:
//  dotnet run -- input.html output.png --width 1024 --height 768 --fullpage false
//  dotnet run -- https://example.com screenshot.png --fullpage true
//  dotnet run -- "<html><body><h1>Hi</h1></body></html>" out.png --inline

using System;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: HtmlToPng <input> <output> [--width N] [--height N] [--fullpage true|false] [--inline]");
            Console.WriteLine("  input: path to .html file, a URL (https://...) or HTML string if --inline is set");
            return 1;
        }

        string input = args[0];
        string output = args[1];

        int width = 1280;
        int height = 720;
        bool fullPage = false;
        bool inline = false;

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--width":
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var w)) { width = w; i++; }
                    break;
                case "--height":
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var h)) { height = h; i++; }
                    break;
                case "--fullpage":
                    if (i + 1 < args.Length && bool.TryParse(args[i + 1], out var f)) { fullPage = f; i++; }
                    break;
                case "--inline":
                    inline = true;
                    break;
            }
        }

        try
        {
            // Ensure output directory exists
            var outDir = Path.GetDirectoryName(Path.GetFullPath(output));
            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            // Download Chromium if needed (stored in user profile; only first run)
            Console.WriteLine("Checking/downloading Chromium (this may take a while on first run)...");
            // Download Chromium using new API
            await new BrowserFetcher().DownloadAsync();

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            };

            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();

            await page.SetViewportAsync(new ViewPortOptions { Width = width, Height = height });

            if (inline)
            {
                Console.WriteLine("Setting page content from inline HTML...");
                await page.SetContentAsync(input, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
            }
            else if (Uri.TryCreate(input, UriKind.Absolute, out var uri) && (uri.Scheme == "http" || uri.Scheme == "https"))
            {
                Console.WriteLine($"Navigating to URL: {input}");
                await page.GoToAsync(input, WaitUntilNavigation.Networkidle0);
            }
            else if (File.Exists(input))
            {
                Console.WriteLine($"Loading HTML file: {input}");
                var html = await File.ReadAllTextAsync(input);
                await page.SetContentAsync(html, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Load } });
            }
            else
            {
                Console.WriteLine("Input looks like a string but --inline was not specified. To treat input as raw HTML string, add --inline flag.");
                return 2;
            }

            Console.WriteLine($"Taking screenshot -> {output} (fullPage={fullPage}, width={width}, height={height})");

            var screenshotOptions = new ScreenshotOptions
            {
                FullPage = fullPage
            };

            await page.ScreenshotAsync(output, screenshotOptions);

            Console.WriteLine("Done.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex.Message);
            return 3;
        }
    }
}
