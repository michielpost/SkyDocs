using MetaMask.Blazor;
using MetaMask.Blazor.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Radzen;
using SkyDocs.Blazor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        public IMetaMaskService MetaMaskService { get; set; } = default!;

        [Inject]
        public ShareService ShareService { get; set; } = default!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        public bool ShareReadOnly { get; set; } = true;

        public string ShareText => ShareReadOnly ? "Anyone with the link can view the document" : "Anyone with the link can edit the document";

        public ShareFormModel ShareFormModel { get; set; } = new ShareFormModel();

        public string CopyText { get; set; } = "Copy";
        public string? TxInfo { get; set; }

        public string? Error { get; set; }
        public string? Progress { get; set; }

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
            CopyText = "Copy";
            StateHasChanged();
        }

        private async Task OnMetaMaskShare()
        {
            Error = null;

            var chain = await MetaMaskService.GetSelectedChain();
            if (chain.chain != MetaMask.Blazor.Enums.Chain.Kovan)
            {
                Error = "Please select the Kovan network in MetaMask. Sharing currently only works on the Kovan testnet.";
                Progress = null;
                return;
            }

            if (SkyDocsService.CurrentSum == null)
            {
                Progress = null;
                return;
            }

            //Store data to share and get URL
            Progress = "Saving sharing secrets to Skynet...";
            StateHasChanged();

            try
            {
                var existing = SkyDocsService.CurrentSum;
                DocumentSummary shareSum = new DocumentSummary()
                {
                    ContentSeed = existing.ContentSeed,
                    CreatedDate = existing.CreatedDate,
                    Id = existing.Id,
                    ShareOrigin = existing.ShareOrigin,
                    Title = existing.Title,
                    PublicKey = existing.PublicKey,
                    ModifiedDate = existing.ModifiedDate,
                    PreviewImage = existing.PreviewImage,
                    PrivateKey = ShareReadOnly ? null : existing.PrivateKey,
                    StorageSource = SkyDocsService.IsDfinityLogin ? StorageSource.Dfinity : StorageSource.Skynet
                };

                string? url = await ShareService.StoreShareMessage(ShareFormModel.EthAddress, shareSum);
                if (string.IsNullOrEmpty(url))
                {
                    Error = "Error storing shared data. Please try again";
                }
                else
                    Console.WriteLine(url);

                //Smart contract has a function called "share"
                FunctionABI function = new FunctionABI("share", false);

                //With 4 inputs
                var inputsParameters = new[] {
                    new Parameter("address", "receiver"),
                    new Parameter("string", "appId"),
                    new Parameter("string", "shareType"),
                    new Parameter("string", "data")
                };
                function.InputParameters = inputsParameters;

                var functionCallEncoder = new FunctionCallEncoder();

                var data = functionCallEncoder.EncodeRequest(function.Sha3Signature, inputsParameters,
                    ShareFormModel.EthAddress,
                    "SkyDocs",
                    string.Empty,
                    url);

                //Using The Share It Network: https://github.com/michielpost/TheShareItNetwork
                string address = "0x6E8c5AFd3CFf5f6Ec85c032B68eF2997323a00FD";
                BigInteger weiValue = 0;

                data = data[2..]; //Remove the 0x from the generated string

                Progress = "Sending transaction using MetaMask...";
                StateHasChanged();

                var result = await MetaMaskService.SendTransaction(address, weiValue, data);
                TxInfo = $"TX Hash: {result}";
            }
            catch (UserDeniedException)
            {
                Error = "User cancelled MetaMask transaction.";
            }
            catch
            {
                Error = "Error sharing document.";
            }
            finally
            {
                Progress = null;
                StateHasChanged();
            }
        }

        private async Task CopyTextToClipboard()
        {
            await JSRuntime.InvokeVoidAsync("clipboardCopy.copyText", ShareService.CurrentShareUrl);
            CopyText = "Copied!";
        }
    }

    public class ShareFormModel
    {
        public string EthAddress { get; set; } = default!;
    }
}
