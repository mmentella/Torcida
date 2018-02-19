using System.Threading.Tasks;
using Torcida.Web.Api.Models;

namespace Torcida.Web.Api.Services
{
    public interface INotificationService
    {
        Task<string> SendAsync(Notification notification);
    }
}
