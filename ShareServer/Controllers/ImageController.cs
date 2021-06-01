using Microsoft.AspNetCore.Http;
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
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private readonly SiaSkynetClient client;


        public ImageController(ILogger<ImageController> logger)
        {
            _logger = logger;
            client = new SiaSkynetClient();
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                var result = await client.UploadFileAsync(file.FileName, file.OpenReadStream());
                return Ok(new { Url = "https://siasky.net/" + result.Skylink });

                //using (var stream = new MemoryStream())
                //{
                //    // Save the file
                //    file.CopyTo(stream);

                //    client.UploadFileAsync(file.FileName, file.OpenReadStream());

                //    return Ok(new { Url = url });
                //}
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


    }
}
