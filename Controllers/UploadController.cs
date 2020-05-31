using System.Runtime.CompilerServices;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ImageWebp.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _host;

        public UploadController(IWebHostEnvironment host)
        {
            _host = host;
        }

        [HttpPost]
        public async Task<IActionResult> File(IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName);
            string newName = Guid.NewGuid().ToString("N");

            string saveFilePath = Path.Combine(_host.WebRootPath, "upload");

            if (!Directory.Exists(saveFilePath))
            {
                Directory.CreateDirectory(saveFilePath);
            }

            using (var stream = System.IO.File.Create(Path.Combine(saveFilePath, newName + ext)))
            {
                await file.CopyToAsync(stream);
            }

            return Ok();
        }

    }
}