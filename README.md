# Converter-Html-Image

Simple HTML to PNG converter in .NET 6+ using PuppeteerSharp (headless Chromium).

## 📦 Installation

1. **Clone the repository:**

```bash
git clone https://github.com/KokeKoke1/Converter-Html-Image.git
cd Converter-Html-Image
```

2. **Install PuppeteerSharp:**

```bash
dotnet add package PuppeteerSharp
```

3. **Download Chromium:**

```csharp
using var browserFetcher = new BrowserFetcher();
await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
```

## 🚀 Usage

Run the application with parameters:

```bash
dotnet run -- input.html output.png --width 1024 --height 768 --fullpage false
dotnet run -- https://example.com screenshot.png --fullpage true
dotnet run -- "<html><body><h1>Hi</h1></body></html>" out.png --inline
```

### Parameters:

* `input.html` or URL: path to an HTML file or a URL to capture.
* `output.png`: path to save the PNG file.
* `--width`: page width in pixels (default 1280).
* `--height`: page height in pixels (default 1024).
* `--fullpage`: whether to capture the full page (default `false`).
* `--inline`: if set, HTML is provided directly in the command line.

## 🧪 C# Example Code

```csharp
using PuppeteerSharp;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var input = args[0];
        var output = args[1];
        var width = int.Parse(args[2]);
        var height = int.Parse(args[3]);
        var fullpage = bool.Parse(args[4]);

        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        using var page = await browser.NewPageAsync();

        if (input.StartsWith("http"))
        {
            await page.GoToAsync(input);
        }
        else if (input.StartsWith("<"))
        {
            await page.SetContentAsync(input);
        }
        else
        {
            await page.GoToAsync($"file:///{input}");
        }

        await page.SetViewportAsync(new ViewPortOptions { Width = width, Height = height });
        await page.ScreenshotAsync(output, new ScreenshotOptions { FullPage = fullpage });
    }
}
```

## 📄 Documentation

More information about PuppeteerSharp and its
