using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Pages.Modals
{
    public partial class ErrorModal
    {
        [Inject]
        public DialogService DialogService { get; set; }

        public void Close()
        {
            DialogService.Close();
        }
    }
}
