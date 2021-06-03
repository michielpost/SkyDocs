using MetaMask.Blazor;
using Microsoft.AspNetCore.Components;
using SkyDocs.Blazor.Models;
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
        public MetaMaskService metaMaskService { get; set; } = default!;

        [Inject]
        public MetaMaskStorageService metaMaskStorageService { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [CascadingParameter]
        public MainLayout Layout { get; set; }

        public async Task Open(string? graphShareId)
        {
            var share = skyDocsService.Shares.Where(x => x.Id == graphShareId).FirstOrDefault();
            if (share == null)
                return;

            var existing = skyDocsService.DocumentList.Where(x => x.ShareOrigin == graphShareId).FirstOrDefault();
            if (existing != null)
            {
                NavigateToDocument(existing.Id);
                return;
            }

            var address = await metaMaskService.GetSelectedAddress();
            var hash = await metaMaskStorageService.GetEncryptedMetamaskHash();

            ShareModel? shareModel = await shareService.GetMessage(address, hash, share.Skylink);

            if (shareModel?.Sum != null)
            {
                existing = skyDocsService.DocumentList.Where(x => x.Id == shareModel.Sum.Id).FirstOrDefault();
                if (existing == null)
                {
                    shareModel.Sum.ShareOrigin = graphShareId;
                    skyDocsService.DocumentList.Add(shareModel.Sum);

                    //Save documentlist
                    await skyDocsService.SaveDocumentList(skyDocsService.DocumentList);

                    Layout.SetNewShares(Layout.TotalShares, Layout.NewShares - 1);

                }
                else
                {
                    existing.ShareOrigin = graphShareId;
                }

                //Open document
                NavigateToDocument(shareModel.Sum.Id);
            }
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
