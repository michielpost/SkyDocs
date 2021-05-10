using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SkyDocs.Blazor.Models;
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
        private Modal modalRef;
        private bool centered = false;
        private ModalSize modalSize = ModalSize.Default;

        private bool LoginDisabled = true;
        private bool IsSaving = false;
        public string Username { get; set; }
        public string Password { get; set; }
        private Dictionary<string, object> at = new Dictionary<string, object>() { { "id", "rte" } };

        [Inject]
        public IJSRuntime JsRuntime { get; set; }

        [Inject]
        public NavigationManager MyNavigationManager { get; set; }
        [Inject]
        public SkyDocsService skyDocsService { get; set; }

        [Inject]
        public HttpClient httpClient { get; set; }

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
                    modalRef.Show();

                if (skyDocsService.CurrentDocument != null)
                {
                    await Task.Delay(100);
                    await InitDocument();
                }

            }

        }

        void OnTextChanged(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password))
                LoginDisabled = false;
            else
                LoginDisabled = true;
        }

        private void OnModalClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!skyDocsService.IsLoggedIn)
            {
                // just set Cancel to true to prevent modal from closing
                e.Cancel = true;
            }
        }

        private void OnLoadingModalClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (skyDocsService.IsLoading)
            {
                // just set Cancel to true to prevent modal from closing
                e.Cancel = true;
            }
        }

        private void OnSavingModalClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (IsSaving)
            {
                // just set Cancel to true to prevent modal from closing
                e.Cancel = true;
            }
        }

        private void OnErrorModalClosing(System.ComponentModel.CancelEventArgs e)
        {
            skyDocsService.Error = null;
        }


        private async Task Login()
        {
            skyDocsService.Login(Username, Password);
            modalRef.Hide();

            await skyDocsService.LoadDocumentList();
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
