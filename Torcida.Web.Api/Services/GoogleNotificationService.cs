using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Torcida.Web.Api.Models;

namespace Torcida.Web.Api.Services
{
    public class GoogleNotificationService
        : INotificationService

    {
        public GoogleNotificationService(GoogleOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            var credential = GoogleCredential
                .FromFile(options.FilePath)
                .CreateScoped(Options.Scope);
            Token = credential.UnderlyingCredential
                              .GetAccessTokenForRequestAsync()
                              .ConfigureAwait(false)
                              .GetAwaiter()
                              .GetResult();
        }

        private string Token { get; }
        public GoogleOptions Options { get; }

        public async Task<string> SendAsync(Notification notification)
        {
            var proxy = new HttpClient();
            proxy.DefaultRequestHeaders.Add("Authorization", $"Bearer {Token}");

            var data = new
            {
                message = new
                {
                    topic = notification.Topic,
                    notification = new
                    {
                        body = notification.Body,
                        title = notification.Title
                    },
                    data = notification.Data
                }
            };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await proxy.PostAsync(Options.NotificationUri, content);
            var message = await response.Content.ReadAsStringAsync();

            return message;
        }
    }
}
