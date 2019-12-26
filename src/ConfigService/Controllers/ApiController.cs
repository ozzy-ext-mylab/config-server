﻿using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ConfigService.Models;
using ConfigService.Services;
using ConfigService.Services.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthorizationService = ConfigService.Services.Authorization.IAuthorizationService;

namespace ConfigService.Controllers
{
    [Route("api/config")]
    public class ApiController : Controller
    {
        public IConfigProvider ConfigProvider { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="ApiController"/>
        /// </summary>
        public ApiController(IConfigProvider configProvider)
        {
            ConfigProvider = configProvider;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = BasicAuthSchemaName.Name)]
        public async Task<IActionResult> Get()
        {
            return Ok(await ConfigProvider.LoadConfig(Request.HttpContext.User.Identity.Name, true));
        }
    }
}