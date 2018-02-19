using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Torcida.Web.Api.Models;

namespace Torcida.Web.Api.Services
{
    public class ProviderService
        : IProviderService
    {
        public Task<Provider> CreateAsync(Provider model)
        {
            return Task.FromResult(model);
        }
    }
}
