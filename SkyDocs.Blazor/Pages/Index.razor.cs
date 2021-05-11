using HtmlAgilityPack;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using SkyDocs.Blazor.Models;
using SkyDocs.Blazor.Pages.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Pages
{
    public partial class Index
    {
        private Dictionary<string, object> at = new Dictionary<string, object>() { { "id", "rte" } };

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Inject]
        public NavigationManager MyNavigationManager { get; set; }
        [Inject]
        public SkyDocsService skyDocsService { get; set; }

        [Inject]
        public HttpClient httpClient { get; set; }

        [Inject]
        public DialogService DialogService { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

#if RELEASE
        string baseUrl = MyNavigationManager.BaseUri;
        var uri = new Uri(baseUrl);
        var portalDomain = $"{uri.Scheme}://{uri.Authority}/";
        skyDocsService.SetPortalDomain(portalDomain);
#endif

        }

        protected override async void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                if (!skyDocsService.IsLoggedIn)
                {
                    await DialogService.OpenAsync<LoginModal>("Login", options: new DialogOptions() { ShowClose = false });

                    DialogService.Open<LoadingModal>("Loading...", options: new DialogOptions() { ShowClose = false });
                    await skyDocsService.LoadDocumentList();
                    DialogService.Close();
                    StateHasChanged();

                }

                if (skyDocsService.CurrentDocument != null)
                {
                    await Task.Delay(100);
                    await InitDocument();
                }

            }

        }

        public void TestButton()
        {
            DialogService.Open<LoadingModal>("Login", options: new DialogOptions() { ShowClose = false });
        }

        private async Task OpenDocument(Guid id)
        {
            DialogService.Open<LoadingModal>("Loading...", options: new DialogOptions() { ShowClose = false });
            await skyDocsService.LoadDocument(id);
            DialogService.Close();

            await InitDocument();
        }

        private async Task InitDocument()
        {
            if (skyDocsService.CurrentDocument != null && !string.IsNullOrEmpty(skyDocsService.CurrentDocument.Content))
            {
                skyDocsService.CurrentDocument.Title = skyDocsService.CurrentDocument.Title;
            }
            else
            {
                DialogService.Open<ErrorModal>("Error loading document from Skynet. Please try again.");
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

        public async Task GoToList()
        {
            skyDocsService.CurrentDocument = null;
        }

        public async Task OnSave()
        {

            if (skyDocsService.CurrentDocument != null)
            {
                DialogService.Open<LoadingModal>("Save to Skynet...", options: new DialogOptions() { ShowClose = false });

                var htmlContent = skyDocsService.CurrentDocument.Content;
                var textContent = StripHtml(htmlContent) ?? string.Empty;

                var fallbackTitle = textContent.Substring(0, Math.Min(textContent.Length, 15));

                var imageBytes = await JsRuntime.InvokeAsync<string>("Snap", "rte", "image/jpeg");
                var data = Convert.FromBase64String(imageBytes); // get the image as byte array
                Console.WriteLine("Image captured: " + data.Length);

                await skyDocsService.SaveCurrentDocument(fallbackTitle, data);
                DialogService.Close();

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
                DialogService.Open<LoadingModal>("Deleting from Skynet...", options: new DialogOptions() { ShowClose = false });
                await skyDocsService.DeleteCurrentDocument();
                DialogService.Close();

                if (skyDocsService.CurrentDocument != null)
                {
                    DialogService.Open<ErrorModal>("Unable to delete document.Please try again.");
                }
            }

            StateHasChanged();

        }
    }
}
