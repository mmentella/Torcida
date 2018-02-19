using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Torcida.Web.Api.Models;

namespace Torcida.Web.Api.Services
{
    public interface IProviderService
    {
        Task<Provider> CreateAsync(Provider model);
    }
}
