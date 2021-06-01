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
        public MetaMaskStorageService MetaMaskStorageService { get; set; } = default!;

        private void Login()
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
                    MetaMaskLogin? storedLogin = null;
                    if (!isSiteConnected)
                    {
                        storedLogin = await GetAndStoreHash();
                    }
                    else
                    {
                        string address = await MetaMaskService.GetSelectedAddress();
                        storedLogin = await MetaMaskStorageService.GetStoredhash(address);

                        if (storedLogin == null)
                        {
                            storedLogin = await GetAndStoreHash();
                        }
                    }

                    SkyDocsService.Login(storedLogin.address, storedLogin.hash, isMetaMaskLogin: true);
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
                catch(Exception ex)
                {
                    Error = "Failed to sign message. Please try again.";
                    Console.WriteLine(ex);
                }

            }

        }

        private async Task<MetaMaskLogin> GetAndStoreHash()
        {
            string signHash = await MetaMaskService.SignTypedData(SignLabel, SignValue);
            string address = await MetaMaskService.GetSelectedAddress();

            MetaMaskLogin storedLogin = new MetaMaskLogin(address, signHash);

            //Store hash in cookie
            await MetaMaskStorageService.SaveStoredHash(storedLogin);
            return storedLogin;
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
