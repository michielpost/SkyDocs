using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Shared
{
    public partial class MainLayout
    {
        public int NewShares { get; set; }

        public void SetNewShares(int newShares)
        {
            NewShares = newShares;
            StateHasChanged();
        }
    }
}
