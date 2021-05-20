using Blazored.LocalStorage;
using SiaSkynet;
using SkyDocs.Blazor.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkyDocs.Blazor
{
    public class MetaMaskStorageService
    {
        private readonly ILocalStorageService localStorageService;

        private readonly string MetaMaskLocalStorageKey = "metamask";


        public MetaMaskStorageService(ILocalStorageService localStorageService)
        {
            this.localStorageService = localStorageService;
        }

        public async Task<MetaMaskLogin?> GetStoredhash(string address)
        {
            string encryptedStoredLogin = await localStorageService.GetItemAsStringAsync(MetaMaskLocalStorageKey);
            if (!string.IsNullOrEmpty(encryptedStoredLogin))
            {
                try
                {
                    var key = SiaSkynetClient.GenerateKeys(address);

                    var cipherData = Utils.HexStringToByteArray(encryptedStoredLogin);
                    var decrypt = Utils.Decrypt(cipherData, key.privateKey);
                    var dString = System.Text.Encoding.UTF8.GetString(decrypt);

                    MetaMaskLogin? storedLogin = JsonSerializer.Deserialize<MetaMaskLogin>(dString);

                    if (storedLogin?.address != address)
                        return null;

                    return storedLogin;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return null;

        }

        public async Task SaveStoredHash(MetaMaskLogin storedLogin)
        {
            try
            {
                var key = SiaSkynetClient.GenerateKeys(storedLogin.address);

                string jsonString = JsonSerializer.Serialize(storedLogin);
                var encrypted = Utils.Encrypt(System.Text.Encoding.UTF8.GetBytes(jsonString), key.privateKey);
                var hexString = BitConverter.ToString(encrypted).Replace("-", "");

                await localStorageService.SetItemAsStringAsync(MetaMaskLocalStorageKey, hexString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
