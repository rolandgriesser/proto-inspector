using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ProtoInspector.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InspectController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<InspectController> _logger;

        public InspectController(ILogger<InspectController> logger)
        {
            _logger = logger;
        }

        public class InspectRequestDto
        {
            public string ProtoDescription { get; set; }
            public string Data { get; set; }
        }

        [HttpPost]
        public object Post(InspectRequestDto request)
        {
            System.IO.File.WriteAllText("temp.proto", request.ProtoDescription);
            Process.Start("protoc --csharp_out temp.proto");

            return null;
        }
    }
}
