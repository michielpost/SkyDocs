using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkyDocs.Blazor
{
    /// <summary>
    /// https://stackoverflow.com/questions/63831943/httpclient-doesnt-include-cookies-with-requests-in-blazor-webassembly-app
    /// </summary>
    public class CookieHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
