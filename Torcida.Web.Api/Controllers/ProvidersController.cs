using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Torcida.Web.Api.Models;
using Torcida.Web.Api.Services;

namespace Torcida.Web.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/Providers")]
    public class ProvidersController 
        : Controller
    {
        public ProvidersController(IProviderService providerService)
        {
            ProviderService = providerService ?? throw new ArgumentNullException(nameof(providerService));
        }

        public IProviderService ProviderService { get; }

        [HttpPost]
        public async Task<IActionResult> Post(RegisterProviderViewmodel model)
        {
            var provider = await ProviderService.CreateAsync(new Provider { Description = model.Description });
            return Created($"{Request.Scheme}://{Request.Host}{Request.Path}/{provider.Id}", provider);
        }
    }
}