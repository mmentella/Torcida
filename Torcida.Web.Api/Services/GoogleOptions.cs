using System.Collections.Generic;

namespace Torcida.Web.Api.Services
{
    public class GoogleOptions
    {
        public string FilePath { get; internal set; } = "torcida-admin.json";
        public string[] Scope { get; internal set; } = new string[] { "https://www.googleapis.com/auth/firebase.messaging" };
        public string NotificationUri { get; internal set; } = "https://fcm.googleapis.com/v1/projects/torcida-free/messages:send";
    }
}