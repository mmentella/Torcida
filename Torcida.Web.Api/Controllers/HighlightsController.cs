using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

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

            var boundary = MultipartRequestHelper.GetBoundary(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse(HttpContext.Request.ContentType),
                                                              FormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            if (section != null)
            {
                var hasContentDisposition = Microsoft.Net.Http.Headers.ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

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

                        await SendNotificationAsync();
                        var id = Path.GetFileNameWithoutExtension(name);
                        return Created($"{Request.Path.Value}/{id}", new { @id = id });
                    }
                }
            }

            return BadRequest("No content disposition found.");
        }

        private async Task SendNotificationAsync()
        {
            var credential = GoogleCredential
                .FromFile("torcida-admin.json")
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            var token = await credential.UnderlyingCredential
                                        .GetAccessTokenForRequestAsync();

            var proxy = new HttpClient();
            proxy.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            var data = new {
                message = new
                {
                    topic = "juventus",
                    notification = new
                    {
                        body = "La tua squadra preferita ha realizzato un goal",
                        title = "Goal!!"
                    }
                }
            };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await proxy.PostAsync("https://fcm.googleapis.com/v1/projects/torcida-free/messages:send", content);
            var message = await response.Content.ReadAsStringAsync();
        }
    }
}