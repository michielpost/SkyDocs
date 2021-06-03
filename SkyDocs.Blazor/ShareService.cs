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
using TheGraph;

namespace SkyDocs.Blazor
{
    public class ShareService
    {
        private readonly NavigationManager navigationManager;
        private readonly string baseUrl = "https://skydocs-shareserver.azurewebsites.net";

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

            var shareUrl = QueryHelpers.AddQueryString(navigationManager.ToAbsoluteUri("/").ToString(), query);
            return shareUrl;
        }

        public async Task<string?> StoreShareMessage(string ethAddress, DocumentSummary currentSum)
        {
            string url = baseUrl + "/share/add";

            ShareModel shareMsg = new ShareModel()
            {
                Sum = currentSum
            };
            string json = JsonSerializer.Serialize(shareMsg);

            AddMessageRequest reqModel = new AddMessageRequest()
            {
                 Address = ethAddress,
                 Message = json
            };

            HttpClient client = new HttpClient();
            var result = await client.PostAsJsonAsync(url, reqModel);

            if(result.IsSuccessStatusCode)
            {
                var skylink = await result.Content.ReadAsStringAsync();
                return "sia://" + skylink;
            }

            return null;
        }

        public async Task<ShareModel?> GetMessage(string ethAddress, string hash, string skylink)
        {
            string url = baseUrl + "/share/get";
            skylink = skylink.Replace("sia://", string.Empty);

            GetMessageRequest reqModel = new GetMessageRequest()
            {
                Address = ethAddress,
                SecretHash = hash,
                Skylink = skylink
            };

            HttpClient client = new HttpClient();
            var result = await client.PostAsJsonAsync(url, reqModel);

            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();
                var shareModel  = JsonSerializer.Deserialize<ShareModel>(json);

                if (shareModel?.Sum?.ShareOrigin != null)
                    shareModel.Sum.ShareOrigin = null;

                return shareModel;
            }

            return null;
        }

        /// <summary>
        /// Get shared documents from The Graph
        /// </summary>
        /// <param name="ethAddress"></param>
        /// <returns></returns>
        public async Task<List<TheGraphShare>> GetSharedDocuments(string ethAddress)
        {
            var client = new TheGraphClient("michielpost/the-shareit-network");

            string query = @"
                        {
                          shares(where: { appId:""SkyDocs"", receiver: ""0x92b143f46c3f8b4242ba85f800579cdf73882e98"" }) {
                            id
                            sender
                            shareData
                          }
                        }
                        ";

            var result = await client.SendQueryAsync<TheGraphShareResultModel>(query);

            return result.Data.Shares.Where(x => x.Skylink?.Length > 10).ToList();
        }
    }
}
