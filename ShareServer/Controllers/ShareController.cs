using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShareServer.Models;
using SiaSkynet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShareController : ControllerBase
    {
        private readonly ILogger<ShareController> _logger;
        private readonly SecretConfig secretConfig;
        private readonly SiaSkynetClient client;

        public ShareController(ILogger<ShareController> logger, IOptions<SecretConfig> secretConfig)
        {
            _logger = logger;
            this.secretConfig = secretConfig.Value;
            client = new SiaSkynetClient();
        }

        [HttpPost]
        [Route("add")]
        [EnableCors("MyPolicy")]
        public async Task<string> StoreMessage([FromBody] AddMessageRequest req)
        {
            string address = req.Address.ToLowerInvariant();

            string seed = $"{secretConfig.SkynetSeed}-{address}";
            var key = SiaSkynetClient.GenerateKeys(seed);

            var encrypted = Utils.Encrypt(System.Text.Encoding.UTF8.GetBytes(req.Message), key.privateKey);

            using (Stream stream = new MemoryStream(encrypted))
            {
                //Save data to Skynet file
                var response = await client.UploadFileAsync("secret.dat", stream);

                var link = response.Skylink;

                return link;
            }
        }

        [HttpPost]
        [Route("get")]
        [EnableCors("MyPolicy")]
        public async Task<string> GetMessage([FromBody] GetMessageRequest req)
        {
            //Check hash
            var hashKey = SiaSkynetClient.GenerateKeys(req.Address);

            var cipherData = Utils.HexStringToByteArray(req.SecretHash);
            var decrypt = Utils.Decrypt(cipherData, hashKey.privateKey);

            if (decrypt == null)
                throw new Exception("Invalid hash.");


            string address = req.Address.ToLowerInvariant();
            string seed = $"{secretConfig.SkynetSeed}-{address}";
            var key = SiaSkynetClient.GenerateKeys(seed);

            var encryptedData = await client.DownloadFileAsByteArrayAsync(req.Skylink);

            var data = Utils.Decrypt(encryptedData.file, key.privateKey);

            return Encoding.UTF8.GetString(data);
        }
    }
}
