using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dfinity.Blazor
{
    // This class provides JavaScript functionality for Dfinity wrapped
    // in a .NET class for easy consumption. The associated JavaScript module is
    // loaded on demand when first needed.
    //
    // This class can be registered as scoped DI service and then injected into Blazor
    // components for use.

    public class DfinityService
    {
        private readonly IJSRuntime jsRuntime;

        public DfinityService(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public async ValueTask Test()
        {
            try
            {
                await jsRuntime.InvokeVoidAsync("EntryPoint.test");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ValueTask SetValue(string key, string value)
        {
            return jsRuntime.InvokeVoidAsync("EntryPoint.setValue", key, value);
        }

        public async ValueTask<string?> GetValue(string key)
        {
            var result = await jsRuntime.InvokeAsync<string[]>("EntryPoint.getValue", key);
            return result.FirstOrDefault();
        }

        public ValueTask SetValueForUser(string key, string value)
        {
            return jsRuntime.InvokeVoidAsync("EntryPoint.setValueForUser", key, value);
        }

        public async ValueTask<string?> GetValueForUser(string key)
        {
            var result = await jsRuntime.InvokeAsync<string[]>("EntryPoint.getValueForUser", key);
            return result.FirstOrDefault();
        }


        public ValueTask<bool> IsLoggedIn()
        {
            return jsRuntime.InvokeAsync<bool>("EntryPoint.isLoggedIn");
        }

        public ValueTask Login()
        {
            return jsRuntime.InvokeVoidAsync("EntryPoint.login");
        }

        public ValueTask Logout()
        {
            return jsRuntime.InvokeVoidAsync("EntryPoint.logout");
        }

        public ValueTask<string?> WhoAmI()
        {
            return jsRuntime.InvokeAsync<string?>("EntryPoint.whoami");
        }

    }
}
