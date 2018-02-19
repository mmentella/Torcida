using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Torcida.Web.Api.Models;
using Torcida.Web.Api.Services;

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
        public HighlightsController(IHostingEnvironment hostingEnvironment,
                                    INotificationService notificationService)
        {
            FormOptions = new FormOptions();
            HostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            NotificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        /// <summary>
        /// 
        /// </summary>
        public FormOptions FormOptions { get; }
        /// <summary>
        /// 
        /// </summary>
        public IHostingEnvironment HostingEnvironment { get; }
        public INotificationService NotificationService { get; }

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
            string videoId = string.Empty;

            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                switch (section.ContentType)
                {
                    case "video/mp4":
                        videoId = await SaveVideoAsync(section);
                        break;
                    case "application/json":
                        await SaveVideoMetadataAsync(section, videoId);
                        break;
                    default:
                        break;
                }

                section = await reader.ReadNextSectionAsync();
            }

            var message = await SendNotificationAsync(videoId);

            return Created($"{Request.Scheme}://{Request.Host}{Request.Path}/{videoId}", new {  message });
        }

        private async Task SaveVideoMetadataAsync(MultipartSection section, string videoId)
        {
            var filename = $"{HostingEnvironment.WebRootPath}/{videoId}.json";
            if (System.IO.File.Exists(filename))
            {
                System.IO.File.Delete(filename);
            }

            using (var targetStream = System.IO.File.Create(filename))
            {
                await section.Body.CopyToAsync(targetStream);
            }
        }

        public async Task<string> SaveVideoAsync(MultipartSection section)
        {
            var hasContentDisposition = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

            if (!hasContentDisposition) throw new ArgumentNullException();
            if(!MultipartRequestHelper.HasFileContentDisposition(contentDisposition)) throw new ArgumentNullException();

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

            var id = Path.GetFileNameWithoutExtension(name);
            return id;

            // var message = await SendNotificationAsync(id);
            // return Created($"{Request.Scheme}://{Request.Host}{Request.Path}/{id}", new { message = message });
        }

        private async Task<string> SendNotificationAsync(string highlightId)
        {
            return await NotificationService.SendAsync(new Notification
            {
                Body = "La tua squadra preferita ha realizzato un goal",
                Data = new
                {
                    videoToken = $"{highlightId}-uniqueToken",
                    videoUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}/{highlightId}"
                },
                Title = "Goal!!",
                Topic = "juventus"
            });
            
        }
    }
}