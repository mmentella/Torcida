using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Torcida.Web.Api.Models;

namespace Torcida.Web.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/Teams")]
    public class TeamsController : Controller
    {
        [HttpGet]
        public Task<IActionResult> Get()
        {
            var teams = new Team[]
            {
                new Team{ Id = "atalanta", name = "Atalanta"},
                new Team{ Id = "benevento", name = "Benevento"},
                new Team{ Id = "bologna", name = "Bologna"},
                new Team{ Id = "cagliari", name = "Cagliari"},
                new Team{ Id = "chievo", name = "Chievo"},
                new Team{ Id = "crotone", name = "Crotone"},
                new Team{ Id = "fiorentina", name = "Fiorentina"},
                new Team{ Id = "genoa", name = "Genoa"},
                new Team{ Id = "verona", name = "Verona"},
                new Team{ Id = "inter", name = "Inter"},
                new Team{ Id = "milan", name = "Milan"},
                new Team{ Id = "napoli", name = "Napoli"},
                new Team{ Id = "roma", name = "Roma"},
                new Team{ Id = "sampdoria", name = "Sampdoria"},
                new Team{ Id = "sassuolo", name = "Sassuolo"},
                new Team{ Id = "spal", name = "Spal"},
                new Team{ Id = "torino", name = "Torino"},
                new Team{ Id = "udinese", name = "Udinese"},
            };

            return Task.FromResult((IActionResult)Ok(teams));
        }
    }
}