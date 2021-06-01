using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using ShareServer.Models;
using SkyDocs.Blazor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SkyDocs.Blazor
{
    public class ShareService
    {
        private readonly NavigationManager navigationManager;

        public string CurrentShareUrl { get; set; } = string.Empty;

        public ShareService(NavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }

        public string GetShareUrl(DocumentSummary sum, bool readOnly)
        {
            var pubString = BitConverter.ToString(sum.PublicKey).Replace("-", "");
            var privString = sum.PrivateKey != null ? BitConverter.ToString(sum.PrivateKey).Replace("-", "") : null;

            var query = new Dictionary<string, string> {
                        { "id", sum.Id.ToString() },
                        { "pub", pubString },
                        { "c", sum.ContentSeed },
                    };

            if (!string.IsNullOrEmpty(privString) && !readOnly)
                query.Add("priv", privString);

            var shareUrl = QueryHelpers.AddQueryString(navigationManager.Uri, query);
            return shareUrl;
        }

        public async Task<string?> StoreShareMessage(string ethAddress, DocumentSummary currentSum)
        {
            string url = "https://skydocs-shareserver.azurewebsites.net/share/add";
            //string url = "https://localhost:44392/share/add";

            ShareModel shareMsg = new ShareModel()
            {
                Sum = currentSum
            };
            string json = JsonSerializer.Serialize(shareMsg);

            AddMessageRequest shareModel = new AddMessageRequest()
            {
                 Address = ethAddress,
                 Message = json
            };

            HttpClient client = new HttpClient();
            var result = await client.PostAsJsonAsync(url, shareModel);

            if(result.IsSuccessStatusCode)
            {
                var skylink = await result.Content.ReadAsStringAsync();
                return "sia://" + skylink;
            }

            return null;
        }
    }
}
