using iMask.EF;
using iMask.Extensions;
using iMask.Models;
using Line.Messaging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iMask
{
    [Route("api/linebot")]
    public class LineBotController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _webRootPath;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpContext _httpContext;
        private readonly LineBotConfig _lineBotConfig;
        private readonly ILogger _logger;
        private readonly CoreDbContext _db;

        public LineBotController(IWebHostEnvironment webHostEnvironment,
            IServiceProvider serviceProvider,
            LineBotConfig lineBotConfig,
            ILogger<LineBotController> logger,
            CoreDbContext db)
        {
            _webHostEnvironment = webHostEnvironment;
            _webRootPath = webHostEnvironment.WebRootPath;
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            _httpContext = _httpContextAccessor.HttpContext;
            _lineBotConfig = lineBotConfig;
            _logger = logger;
            _db = db;
        }

        [HttpPost("run")]
        public async Task<IActionResult> Post()
        {
            try
            {
                var events = await _httpContext.Request.GetWebhookEventsAsync(_lineBotConfig.channelSecret);
                var lineMessagingClient = new LineMessagingClient(_lineBotConfig.accessToken);

                var lineBotApp = new LineBotApp(lineMessagingClient, _db);
                await lineBotApp.RunAsync(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(JsonConvert.SerializeObject(ex));
            }
            return Ok();
        }

        [HttpGet("wakeUp")]
        public string WakeUp()
        {
            return "I wake up!!";
        }
    }
}
