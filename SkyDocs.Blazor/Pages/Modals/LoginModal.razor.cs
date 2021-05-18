using MetaMask.Blazor;
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
        //Changing these values invalidates logins
        private readonly string SignLabel = "SkyDocs login";
        private readonly string SignValue = "Sign this message to login with SkyDocs";

        public LoginModel loginModel { get; set; } = new LoginModel();

        [Inject]
        public DialogService DialogService { get; set; }

        [Inject]
        public SkyDocsService SkyDocsService { get; set; } = default!;

        [Inject]
        public MetaMaskService MetaMaskService { get; set; } = default!;

        private async Task Login()
        {
            SkyDocsService.Login(loginModel.Username, loginModel.Password);

            DialogService.Close();

            
        }

        private async Task MetaMaskLogin()
        {
            bool hasMetaMask = await MetaMaskService.HasMetaMask();
            if (!hasMetaMask)
            {
                DialogService.Open<ErrorModal>("Please install MetaMask.");
            }
            else
            {
                bool isSiteConnected = await MetaMaskService.IsSiteConnected();
                if (isSiteConnected)
                {
                    //TODO: Check if there is a hash in a cookie

                   
                }
                else
                {
                    //
                }

                string signHash = await MetaMaskService.SignTypedData(SignLabel, SignValue);

                //TODO: Store hash in cookie

                SkyDocsService.Login("metamask", signHash);
                DialogService.Close();

            }

        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
