using MetaMask.Blazor;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Pages.Modals
{
    public partial class ShareModal
    {
        [Inject]
        public DialogService DialogService { get; set; } = default!;

        [Inject]
        public SkyDocsService SkyDocsService { get; set; } = default!;

        [Inject]
        public MetaMaskService MetaMaskService { get; set; } = default!;

        [Inject]
        public ShareService ShareService { get; set; } = default!;

        public bool ShareReadOnly { get; set; } = true;

        public string ShareText  => ShareReadOnly ? "Anyone with the link can view the document" : "Anyone with the link can edit the document";

        public string? ShareAddress { get; set; }
        public string? Error { get; set; }

        private void SetShareUrl(bool readOnly)
        {
            var sum = SkyDocsService.CurrentSum;
            if (sum != null)
            {
                ShareService.CurrentShareUrl = ShareService.GetShareUrl(sum, readOnly);
            }
        }

        void OnRadioButtonChange(bool value)
        {
            SetShareUrl(value);
            StateHasChanged();
        }

        private void OnMetaMaskShare()
        {

        }
    }
}
