using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Torcida.Web.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/highlights")]
    public class HighlightsController
        : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public HighlightsController(IHostingEnvironment hostingEnvironment)
        {
            FormOptions = new FormOptions();
            HostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        /// <summary>
        /// 
        /// </summary>
        public FormOptions FormOptions { get; }
        /// <summary>
        /// 
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public IActionResult Get([FromRoute]string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var name = $"{id}.mp4";
            var targetPath = Path.Combine(HostingEnvironment.WebRootPath, name);
            if (System.IO.File.Exists(targetPath))
            {
                var stream = System.IO.File.OpenRead(targetPath);
                stream.Seek(0, SeekOrigin.Begin);

                return File(stream, "video/mp4");
            }

            return NotFound(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(HttpContext.Request.ContentType),
                                                              FormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            if (section != null)
            {
                var hasContentDisposition = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue contentDisposition);

                if (hasContentDisposition)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        var name = HeaderUtilities.RemoveQuotes(contentDisposition.FileName).Value.Trim();
                        var targetFilePath = $"{HostingEnvironment.WebRootPath}/{name}";

                        if (System.IO.File.Exists(targetFilePath))
                        {
                            System.IO.File.Delete(targetFilePath);
                        }

                        using (var targetStream = System.IO.File.Create(targetFilePath))
                        {
                            await section.Body.CopyToAsync(targetStream);
                        }

                        return Created($"{Request.Path.Value}/{Path.GetFileNameWithoutExtension(name)}", new { });
                    }
                }
            }

            return BadRequest("No content disposition found.");
        }
    }
}