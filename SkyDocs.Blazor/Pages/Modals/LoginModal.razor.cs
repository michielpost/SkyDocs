using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Pages.Modals
{
    public partial class LoginModal
    {
        public LoginModel loginModel { get; set; } = new LoginModel();

        [Inject]
        public DialogService DialogService { get; set; }

        [Inject]
        public SkyDocsService SkyDocsService { get; set; } = default!;

        private async Task Login()
        {
            SkyDocsService.Login(loginModel.Username, loginModel.Password);

            DialogService.Close();

            
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
