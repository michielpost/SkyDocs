using Blazored.LocalStorage;
using MetaMask.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Radzen;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toolbelt.Blazor.Extensions.DependencyInjection;

namespace SkyDocs.Blazor
{
    public class Program
    {
        public static string? Version { get; set; }

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddScoped<DialogService>();
            builder.Services.AddScoped<NotificationService>();
            builder.Services.AddScoped<TooltipService>();
            builder.Services.AddScoped<ContextMenuService>();

            builder.Services.AddHeadElementHelper();

            builder.Services.AddMetaMaskBlazor();
            builder.Services.AddBlazoredLocalStorage();

            builder.RootComponents.Add<App>("#app");

            builder.Services.AddSingleton<SkyDocsService>();
            builder.Services.AddScoped<MetaMaskStorageService>();
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            var host = builder.Build();

            Version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            Version = "2.0.0-2eeb71b";

            await host.RunAsync();

        }

        public static string? GetVersionHash()
        {
            if(Version != null)
            {
                int sep = Version.LastIndexOf('-');
                if(sep < Version.Length)
                    return Version.Substring(sep+1);
            }
            return null;
        }

        public static string? GetVersionWithoutHash()
        {
            if (Version != null)
            {
                int sep = Version.LastIndexOf('-');
                return Version.Substring(0, sep);
            }
            return null;
        }
    }
}
