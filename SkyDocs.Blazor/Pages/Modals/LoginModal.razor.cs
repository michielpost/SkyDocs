using Blazored.LocalStorage;
using MetaMask.Blazor;
using MetaMask.Blazor.Exceptions;
using Microsoft.AspNetCore.Components;
using Radzen;
using SkyDocs.Blazor.Models;
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
        private readonly string MetaMaskLocalStorageKey = "metamask";

        public LoginModel loginModel { get; set; } = new LoginModel();

        public string? Error { get; set; }
        public bool ShowMetaMaskMessage { get; set; }

        [Inject]
        public DialogService DialogService { get; set; }

        [Inject]
        public SkyDocsService SkyDocsService { get; set; } = default!;

        [Inject]
        public MetaMaskService MetaMaskService { get; set; } = default!;

        [Inject]
        public ILocalStorageService LocalStorageService { get; set; } = default!;

        private async Task Login()
        {
            SkyDocsService.Login(loginModel.Username, loginModel.Password);

            DialogService.Close();
        }

        private async Task MetaMaskLogin()
        {
            ShowMetaMaskMessage = false;
            Error = null;

            bool hasMetaMask = await MetaMaskService.HasMetaMask();
            if (!hasMetaMask)
            {
                ShowMetaMaskMessage = true;
            }
            else
            {
                try
                {
                    bool isSiteConnected = await MetaMaskService.IsSiteConnected();
                    var storedLogin = await LocalStorageService.GetItemAsync<MetaMaskLogin>(MetaMaskLocalStorageKey);
                    if (!isSiteConnected || storedLogin == null)
                    {
                        string address = await MetaMaskService.GetSelectedAddress();

                        storedLogin = await GetAndStoreHash(storedLogin, address);
                    }
                    else
                    {
                        string address = await MetaMaskService.GetSelectedAddress();
                        if(storedLogin.address != address)
                        {
                            storedLogin = await GetAndStoreHash(storedLogin, address);
                        }
                    }

                    SkyDocsService.Login(storedLogin.address, storedLogin.hash);
                    DialogService.Close();

                }
                catch (NoMetaMaskException)
                {
                    ShowMetaMaskMessage = true;
                }
                catch (UserDeniedException)
                {
                    Error = "MetaMask not allowed to connect to SkyDocs. Please try again.";
                }
                catch
                {
                    Error = "Failed to sign message. Please try again.";
                }

            }

        }

        private async Task<MetaMaskLogin> GetAndStoreHash(MetaMaskLogin? storedLogin, string address)
        {
            string signHash = await MetaMaskService.SignTypedData(SignLabel, SignValue);
            storedLogin = new MetaMaskLogin(address, signHash);

            //Store hash in cookie
            await LocalStorageService.SetItemAsync(MetaMaskLocalStorageKey, storedLogin);
            return storedLogin;
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
