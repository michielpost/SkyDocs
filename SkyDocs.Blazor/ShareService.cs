using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using SkyDocs.Blazor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
