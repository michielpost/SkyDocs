using HtmlAgilityPack;
using MetaMask.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using Radzen;
using SiaSkynet;
using SkyDocs.Blazor.Models;
using SkyDocs.Blazor.Pages.Modals;
using SkyDocs.Blazor.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Pages
{
    public partial class Index : IDisposable
    {
        private Dictionary<string, object> at = new Dictionary<string, object>() { { "id", "rte" } };

        public bool HasMetaMask { get; set; }
        public string? SelectedAddress { get; set; }

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public SkyDocsService skyDocsService { get; set; }

        [Inject]
        public HttpClient httpClient { get; set; }

        [Inject]
        public DialogService DialogService { get; set; }

        [Inject]
        public ShareService ShareService { get; set; }

        [Inject]
        public MetaMaskService MetaMaskService { get; set; } = default!;

        [CascadingParameter]
        public MainLayout Layout { get; set; }

        private void NavigationManager_LocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            CheckUriAndOpenDocument();
        }

        protected override async Task OnInitializedAsync()
        {
            NavigationManager.LocationChanged += NavigationManager_LocationChanged;

            //HasMetaMask = await MetaMaskService.HasMetaMask();
            //bool isSiteConnected = await MetaMaskService.IsSiteConnected();
            //if (isSiteConnected)
            //    SelectedAddress = await MetaMaskService.GetSelectedAddress();

            base.OnInitialized();

#if RELEASE
        string baseUrl = NavigationManager.BaseUri;
        var uri = new Uri(baseUrl);
        skyDocsService.SetPortalDomain(uri.Scheme, uri.Authority);
#endif

            await CheckUriAndOpenDocument();

            //No document open, ask user to login
            if(skyDocsService.CurrentDocument == null)
            {
                if (!skyDocsService.IsLoggedIn)
                {
                    await Login();
                }
            }
        }

        void IDisposable.Dispose()
        {
            // Unsubscribe from the event when our component is disposed
            NavigationManager.LocationChanged -= NavigationManager_LocationChanged;
        }

        private async Task Login()
        {
            await DialogService.OpenAsync<LoginModal>("Log in to SkyDocs", options: new DialogOptions() { ShowClose = false, Width = "450px" });

            DialogService.Open<LoadingModal>("Loading...", new Dictionary<string, object>() { { "Msg", "Loading files from Skynet..." } }, options: new DialogOptions() { ShowClose = false, ShowTitle = false, Width = "200px" });
            await skyDocsService.LoadDocumentList();
            DialogService.Close();
            StateHasChanged();

            if(skyDocsService.IsMetaMaskLogin)
            {
                var address = await MetaMaskService.GetSelectedAddress();
                var shares = await ShareService.GetSharedDocuments(address);
                var myshares = await ShareService.GetDocumentsIShared(address);

                shares = shares.Union(myshares).ToList();

                var shareData = skyDocsService.SetShares(shares);
                Layout.SetNewShares(shareData.total, shareData.newShares);
            }
        }

        private async Task CheckUriAndOpenDocument()
        {
            var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
            string? documentId = GetQueryParam(uri, "id");
            string? pub = GetQueryParam(uri, "pub");
            string? priv = GetQueryParam(uri, "priv");
            string? contentSeed = GetQueryParam(uri, "c");
            byte[]? pubKey = null;
            byte[]? privKey = null;

            if (!string.IsNullOrEmpty(pub))
                pubKey = Utils.HexStringToByteArray(pub);
            if (!string.IsNullOrEmpty(priv))
                privKey = Utils.HexStringToByteArray(priv);

            if (Guid.TryParse(documentId, out Guid docId) && pubKey != null && !string.IsNullOrEmpty(contentSeed))
            {
                skyDocsService.AddDocumentSummary(docId, pubKey, privKey, contentSeed);

                await OpenDocument(docId);
            }
            else
            {
                skyDocsService.CurrentDocument = null;
            }

            StateHasChanged();
        }

        private string? GetQueryParam(Uri uri, string paramName)
        {
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(paramName, out var param))
            {
                var value = param.First();
                //Console.WriteLine($"QueryParam: {paramName}: {value}");
                return value;
            }

            return null;
        }

        private async Task OpenDocument(Guid id)
        {
            DialogService.Open<LoadingModal>("Loading...", new Dictionary<string, object>() { { "Msg", "Loading..." } }, options: new DialogOptions() { ShowClose = false, ShowTitle = false, Width = "200px" });
            await skyDocsService.LoadDocument(id);
            DialogService.Close();

            if (skyDocsService.CurrentDocument == null || string.IsNullOrEmpty(skyDocsService.CurrentDocument.Content))
            {
                GoToList();
                DialogService.Open<ErrorModal>("Error loading document from Skynet. Please try again.");
            }
        }

        private void NavigateToDocument(Guid id)
        {
            var sum = skyDocsService.DocumentList.Where(x => x.Id == id).FirstOrDefault();
            if (sum != null)
            {
                //Share url:
                string? shareUrl = ShareService.GetShareUrl(sum, true);
                if (!string.IsNullOrEmpty(shareUrl))
                    NavigationManager.NavigateTo(shareUrl);
            }
        }

        

        private void NewDocument()
        {
            skyDocsService.StartNewDocument();

            StateHasChanged();
        }

        private async Task AddTemplates()
        {
            var samples = await httpClient.GetFromJsonAsync<List<DocumentSummary>>("templates.json");

            if (samples != null)
                skyDocsService.DocumentList.AddRange(samples);
        }

        public void GoToList()
        {
            skyDocsService.CurrentDocument = null;
            NavigationManager.NavigateTo("/");
        }

        public async Task OnSave()
        {

            if (skyDocsService.CurrentDocument != null)
            {
                DialogService.Open<LoadingModal>("Saving to Skynet...", new Dictionary<string, object>() { { "Msg", "Saving to Skynet..." } }, options: new DialogOptions() { ShowClose = false, ShowTitle = false, Width = "200px" });

                var htmlContent = skyDocsService.CurrentDocument.Content;
                var textContent = StripHtml(htmlContent) ?? string.Empty;

                var fallbackTitle = textContent.Substring(0, Math.Min(textContent.Length, 15));

                var imageBytes = await JsRuntime.InvokeAsync<string>("Snap", "rte", "image/jpeg");
                var data = Convert.FromBase64String(imageBytes); // get the image as byte array
                Console.WriteLine("Image captured: " + data.Length);

                await skyDocsService.SaveCurrentDocument(fallbackTitle, data);
                DialogService.Close();

                if(skyDocsService.CurrentDocument == null)
                    GoToList();

                if(!string.IsNullOrEmpty(SkyDocsService.Error))
                    DialogService.Open<ErrorModal>(SkyDocsService.Error);

            }


        }

        public static string StripHtml(string value)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(value);

            if (htmlDoc == null)
                return value;

            return htmlDoc.DocumentNode.InnerText;
        }

        public async Task OnDelete()
        {

            if (skyDocsService.CurrentDocument != null)
            {
                DialogService.Open<LoadingModal>("Deleting from Skynet...", new Dictionary<string, object>() { { "Msg", "Deleting from Skynet..." } }, options: new DialogOptions() { ShowClose = false, ShowTitle = false, Width = "200px" });
                await skyDocsService.DeleteCurrentDocument();
                DialogService.Close();

                if (skyDocsService.CurrentDocument != null)
                {
                    DialogService.Open<ErrorModal>("Unable to delete document.Please try again.");
                }
                else
                {
                    GoToList();
                }
            }

            StateHasChanged();

        }

        public void OnShare()
        {
            if (skyDocsService.CurrentDocument != null && skyDocsService.CurrentSum != null)
            {
                ShareService.CurrentShareUrl = ShareService.GetShareUrl(skyDocsService.CurrentSum, true);
                DialogService.Open<ShareModal>("Share document: " + skyDocsService.CurrentDocument.Title, options: new DialogOptions() { ShowClose = true });
            }
        }
    }
}
