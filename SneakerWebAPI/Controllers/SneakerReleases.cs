using HtmlAgilityPack;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System;
using SneakerWebAPI.Services.SneakerReleaseService;
using SneakerWebAPI.DTOs;
namespace SneakerWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SneakerReleases : ControllerBase
    {
        private readonly ISneakerReleaseService _sneakerReleasesService;

        public SneakerReleases(ISneakerReleaseService sneakerReleasesService)
        {
            _sneakerReleasesService = sneakerReleasesService;
        }

        [HttpGet]
        public async Task<ActionResult<List<SneakerReleaseResponse>>> GetReleases()
        {
            return Ok(await _sneakerReleasesService.GetReleases());
        }

    }
}
