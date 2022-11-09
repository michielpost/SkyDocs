using MetaMask.Blazor;
using Microsoft.AspNetCore.Components;
using Radzen;
using SkyDocs.Blazor.Models;
using SkyDocs.Blazor.Pages.Modals;
using SkyDocs.Blazor.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Pages
{
    public partial class Shares
    {
        [Inject]
        public SkyDocsService skyDocsService { get; set; } = default!;

        [Inject]
        public ShareService shareService { get; set; } = default!;

        [Inject]
        public IMetaMaskService metaMaskService { get; set; } = default!;

        [Inject]
        public MetaMaskStorageService metaMaskStorageService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        public DialogService DialogService { get; set; } = default!;

        [CascadingParameter]
        public MainLayout Layout { get; set; } = default!;

        public async Task Open(string? graphShareId)
        {
            var share = skyDocsService.Shares.Where(x => x.Id == graphShareId).FirstOrDefault();
            if (share == null || string.IsNullOrEmpty(share.Skylink))
                return;

            var existing = skyDocsService.DocumentList.Where(x => x.ShareOrigin == graphShareId).FirstOrDefault();
            if (existing != null)
            {
                NavigateToDocument(existing.Id);
                return;
            }

            var address = await metaMaskService.GetSelectedAddress();
            var hash = await metaMaskStorageService.GetEncryptedMetamaskHash();

            DialogService.Open<LoadingModal>("Loading...", new Dictionary<string, object>() { { "Msg", "Loading..." } }, options: new DialogOptions() { ShowClose = false, ShowTitle = false, Width = "200px" });

            ShareModel? shareModel = await shareService.GetMessage(address, hash, share.Skylink);

            if (shareModel?.Sum != null)
            {
                existing = skyDocsService.DocumentList.Where(x => x.Id == shareModel.Sum.Id).FirstOrDefault();
                if (existing == null)
                {
                    shareModel.Sum.ShareOrigin = graphShareId;
                    skyDocsService.DocumentList.Add(shareModel.Sum);

                    //Save documentlist
                    bool success = await skyDocsService.SaveDocumentList(skyDocsService.DocumentList);
                    Console.WriteLine(skyDocsService.DocumentList.Count);
                    Console.WriteLine("Save document list " + success);

                    Layout.SetNewShares(Layout.TotalShares, Layout.NewShares - 1);

                }
                else
                {
                    existing.ShareOrigin = graphShareId;
                }

                //Open document
                NavigateToDocument(shareModel.Sum.Id);
            }

            DialogService.Close();
        }

        private void NavigateToDocument(Guid id)
        {
            var sum = skyDocsService.DocumentList.Where(x => x.Id == id).FirstOrDefault();
            if (sum != null)
            {
                //Share url:
                string? shareUrl = shareService.GetShareUrl(sum, true);
                if (!string.IsNullOrEmpty(shareUrl))
                    NavigationManager.NavigateTo(shareUrl);
            }
        }
    }
}
