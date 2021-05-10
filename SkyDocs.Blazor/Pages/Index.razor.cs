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
        private bool LoginDisabled = true;
        private bool IsSaving = false;

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

        public string? DocumentSource { get; set; }


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
                    Console.WriteLine("Show login");
                    await DialogService.OpenAsync<LoginModal>("Login", options: new DialogOptions() { ShowClose = false });

                    DialogService.Open<MessageModal>("Loading...");
                    await skyDocsService.LoadDocumentList();
                    DialogService.Close();
                    Console.WriteLine("Loading finished");
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
            DialogService.Open<MessageModal>("Login");
        }

        private async Task OpenDocument(Guid id)
        {
            await skyDocsService.LoadDocument(id);

            await InitDocument();
        }

        private async Task InitDocument()
        {
            Console.WriteLine("Init document");

            if (skyDocsService.CurrentDocument != null && !string.IsNullOrEmpty(skyDocsService.CurrentDocument.Content))
            {
                skyDocsService.CurrentDocument.Title = skyDocsService.CurrentDocument.Title;

                DocumentSource = skyDocsService.CurrentDocument.Content;

                Console.WriteLine("Value set: " + DocumentSource);
            }
        }

        private void NewDocument()
        {
            skyDocsService.StartNewDocument();

            DocumentSource = string.Empty;
        }

        private async Task AddTemplates()
        {
            var samples = await httpClient.GetFromJsonAsync<List<DocumentSummary>>("templates.json");

            if (samples != null)
                skyDocsService.DocumentList.AddRange(samples);
        }

        public async Task GoToList()
        {
            DocumentSource = null;
            skyDocsService.CurrentDocument = null;
        }

        public async Task OnSave()
        {
            IsSaving = true;

            var htmlContent = DocumentSource;
            var textContent = DocumentSource ?? string.Empty; //TODO: Strip html

            if (skyDocsService.CurrentDocument != null)
            {
                skyDocsService.CurrentDocument.Content = htmlContent ?? string.Empty;

                var fallbackTitle = textContent.Substring(0, Math.Min(textContent.Length, 15));

                var imageBytes = await JsRuntime.InvokeAsync<string>("Snap", "rte", "image/jpeg");
                var data = Convert.FromBase64String(imageBytes); // get the image as byte array
                Console.WriteLine("Image captured: " + data.Length);

                await skyDocsService.SaveCurrentDocument(textContent, data);

            }

            DocumentSource = null;

            IsSaving = false;

        }

        public async Task OnDelete()
        {
            IsSaving = true;

            if (skyDocsService.CurrentDocument != null)
            {
                await skyDocsService.DeleteCurrentDocument();

            }

            DocumentSource = null;

            IsSaving = false;

        }
    }
}
