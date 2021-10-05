using Blazored.LocalStorage;
using Dfinity.Blazor;
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
            builder.RootComponents.Add<App>("#app");

            ConfigureServices(builder.Services, builder.HostEnvironment.BaseAddress);

            await builder.Build().RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services, string baseAddress)
        {
            services.AddScoped<DialogService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<TooltipService>();
            services.AddScoped<ContextMenuService>();

            services.AddHeadElementHelper();

            services.AddMetaMaskBlazor();
            services.AddDfinityBlazor();

            services.AddBlazoredLocalStorage();

            services.AddSingleton<SkyDocsService>();
            services.AddScoped<MetaMaskStorageService>();
            services.AddScoped<ShareService>();
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

            Version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        }

        public static string? GetVersionHash()
        {
            if(Version != null)
            {
                int sep = Version.LastIndexOf('-');
                if(sep >= 0 && sep < Version.Length)
                    return Version.Substring(sep+1);
            }
            return null;
        }

        public static string? GetVersionWithoutHash()
        {
            if (Version != null)
            {
                int sep = Version.LastIndexOf('-');
                if(sep >= 0)
                    return Version.Substring(0, sep);
            }
            return Version;
        }
    }
}
